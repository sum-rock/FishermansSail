using System;
using MoreSailwindSails.Sails.FishermansFlyingSail;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;

internal static class RopeChecks
{
    internal static void Run()
    {
        foreach (float scale in new[] { 0.3f, 1f, 2f })
        foreach (float trim in new[] { -40f, 0f, 40f })
        foreach (float heel in new[] { -35f, 0f, 35f })
        foreach (float slack in new[] { 0f, 0.5f, 1f })
        foreach (int side in new[] { -1, 1 })
        {
            Vector3 Heel(Vector3 p) =>
                FishermansFlyingSailFrameGeometry.RotateAroundMast(
                    p,
                    Vector3.zero,
                    Vector3.forward,
                    heel
                );
            Vector3 Trim(Vector3 p) =>
                FishermansFlyingSailFrameGeometry.RotateAroundMast(
                    p,
                    Vector3.zero,
                    Vector3.up,
                    trim
                );
            var aft = Heel(Vector3.forward);
            var top = Heel(new Vector3(0, 14, 0));
            var tack = top - Heel(Vector3.up * (8 * scale));
            var head = top + Heel(Trim(new Vector3(0, 1.46f, 4) * scale));
            var clew = tack + Heel(Trim(new Vector3(0, -1.46f, 4) * scale));
            var guide = Heel(new Vector3(0, 22, 10));
            var control = Heel(new Vector3(side * 2, 1, 9));
            CheckRoute(new[] { top - aft * 0.4572f, top, head, guide, control }, slack);
            CheckRoute(new[] { tack - aft * 0.4572f, tack, clew, control }, slack);
        }
        // Collinear lines isolate the specified 1–2% gravity sag from corner easing.
        foreach (float length in new[] { 0.001f, 1f, 40f })
        {
            var end = Vector3.forward * length;
            var handle = end / 3;
            foreach (float slack in new[] { 0f, 1f })
            {
                var midpoint = FishermansFlyingSailRopeGeometry.Point(
                    Vector3.zero,
                    end,
                    handle,
                    handle,
                    slack,
                    0.5f
                );
                Near(
                    midpoint,
                    end / 2 + Vector3.down * (length * (0.01f + 0.01f * slack)),
                    length * 1e-5f
                );
            }
        }
        // Collapsed and reversing paths must stay finite and bounded.
        foreach (var end in new[] { Vector3.zero, Vector3.forward * 0.00001f, Vector3.forward })
            for (int i = 0; i <= 32; i++)
            {
                var point = FishermansFlyingSailRopeGeometry.Point(
                    Vector3.zero,
                    end,
                    Vector3.back * 100,
                    Vector3.forward * 100,
                    1,
                    i / 32f
                );
                Check(
                    float.IsFinite(point.x) && float.IsFinite(point.y) && float.IsFinite(point.z),
                    "Degenerate rope is not finite."
                );
                Check(
                    point.magnitude <= end.magnitude * 1.36f,
                    "Rope handles escaped the local span."
                );
            }
        Console.WriteLine(
            "PASS: direct external sheets, gravity-only sag, local tie easing, bounded handles, degenerate spans and moved/heeled routes."
        );
    }

    private static void CheckRoute(Vector3[] nodes, float slack)
    {
        var shift = new Vector3(19, -8, 31);
        for (int span = 1; span < nodes.Length - 1; span++)
        {
            var start = nodes[span];
            var end = nodes[span + 1];
            float length = (end - start).magnitude;
            var a = FishermansFlyingSailRopeGeometry.Handle(nodes[1] - nodes[0], 0.4572f, length);
            var b = (end - start) / 3;
            Vector3 Point(float t) =>
                span == 1
                    ? FishermansFlyingSailRopeGeometry.Point(start, end, a, b, slack, t)
                    : FishermansFlyingSailRopeGeometry.DirectPoint(start, end, slack, t);
            Near(Point(0), start, 0.0001f);
            Near(Point(1), end, 0.0001f);
            if (span == 1)
                Check(
                    Vector3.Dot(
                        (Point(0.001f) - start).normalized,
                        (nodes[1] - nodes[0]).normalized
                    ) > 0.98f,
                    "The fabric-edge extension must ease into the short straight mast tie."
                );
            for (int sample = 0; sample <= 32; sample++)
            {
                float t = sample / 32f;
                var moved =
                    span == 1
                        ? FishermansFlyingSailRopeGeometry.Point(
                            start + shift,
                            end + shift,
                            a,
                            b,
                            slack,
                            t
                        )
                        : FishermansFlyingSailRopeGeometry.DirectPoint(
                            start + shift,
                            end + shift,
                            slack,
                            t
                        );
                Near(moved, Point(t) + shift, 0.0001f);
                var delta = Point(t) - Vector3.Lerp(start, end, t);
                if (span == 1)
                    Check(delta.magnitude <= length * 0.36f, "Tie easing escaped its local span.");
                else
                {
                    Check(
                        Math.Abs(delta.x) < 1e-5f && Math.Abs(delta.z) < 1e-5f,
                        "External ropes must not bow sideways or inherit a corner tangent."
                    );
                    Check(
                        delta.y <= 1e-5f && delta.y >= -length * 0.025f - 1e-5f,
                        "External rope must sag down by at most 2.5 percent of span length."
                    );
                    var tight = FishermansFlyingSailRopeGeometry.DirectPoint(start, end, 0, t);
                    var eased = FishermansFlyingSailRopeGeometry.DirectPoint(start, end, 1, t);
                    Check(eased.y <= tight.y + 1e-5f, "Easing a sheet must increase downward sag.");
                }
            }
        }
        foreach (float amount in new[] { 0f, 1f })
        {
            var midpoint = FishermansFlyingSailRopeGeometry.DirectPoint(
                nodes[1],
                nodes[2],
                amount,
                0.5f
            );
            float expectedSag = (nodes[2] - nodes[1]).magnitude * (amount == 0 ? 0.005f : 0.025f);
            Near(midpoint, (nodes[1] + nodes[2]) / 2 + Vector3.down * expectedSag, 0.0001f);
        }
        Near(
            FishermansFlyingSailRopeGeometry.DirectPoint(nodes[1], nodes[1], slack, 0.5f),
            nodes[1],
            0.0001f
        );
    }

    private static void Near(Vector3 a, Vector3 b, float tolerance) =>
        Check((a - b).magnitude <= tolerance, "Rope attachment or translation changed.");

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
