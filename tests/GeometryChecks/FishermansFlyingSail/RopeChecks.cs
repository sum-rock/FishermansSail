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
        var reversal = FishermansFlyingSailRopeGeometry.Tangent(
            Vector3.zero,
            Vector3.forward,
            Vector3.zero
        );
        Check(reversal.sqrMagnitude > 0, "A reversed route lost its join direction.");
        Console.WriteLine(
            "PASS: sheet anchors, continuous join tangents, bounded handles, native-slack sag, degenerate spans and moved/heeled routes."
        );
    }

    private static void CheckRoute(Vector3[] nodes, float slack)
    {
        var directions = new Vector3[nodes.Length];
        directions[0] = directions[1] = (nodes[1] - nodes[0]).normalized;
        directions[nodes.Length - 1] = (
            nodes[nodes.Length - 1] - nodes[nodes.Length - 2]
        ).normalized;
        for (int i = 2; i < nodes.Length - 1; i++)
            directions[i] = FishermansFlyingSailRopeGeometry.Tangent(
                nodes[i - 1],
                nodes[i],
                nodes[i + 1]
            );
        var shift = new Vector3(19, -8, 31);
        Vector3 previousDirection = directions[1];
        for (int span = 1; span < nodes.Length - 1; span++)
        {
            var start = nodes[span];
            var end = nodes[span + 1];
            float length = (end - start).magnitude;
            float before = (start - nodes[span - 1]).magnitude;
            float after = span + 2 < nodes.Length ? (nodes[span + 2] - end).magnitude : length;
            var a = FishermansFlyingSailRopeGeometry.Handle(directions[span], before, length);
            var b = FishermansFlyingSailRopeGeometry.Handle(directions[span + 1], after, length);
            Vector3 Point(float t) =>
                FishermansFlyingSailRopeGeometry.Point(start, end, a, b, slack, t);
            Near(Point(0), start, 0.0001f);
            Near(Point(1), end, 0.0001f);
            var departing = (Point(0.001f) - start).normalized;
            var arriving = (end - Point(0.999f)).normalized;
            Check(
                Vector3.Dot(previousDirection, departing) > 0.98f,
                "Adjacent spans form a sharp corner."
            );
            Check(
                Vector3.Dot(arriving, directions[span + 1]) > 0.98f,
                "Rope missed its arrival tangent."
            );
            previousDirection = arriving;
            for (int sample = 0; sample <= 32; sample++)
            {
                float t = sample / 32f;
                Near(
                    FishermansFlyingSailRopeGeometry.Point(
                        start + shift,
                        end + shift,
                        a,
                        b,
                        slack,
                        t
                    ),
                    Point(t) + shift,
                    0.0001f
                );
                var delta = Point(t) - Vector3.Lerp(start, end, t);
                Check(
                    delta.magnitude <= length * 0.36f,
                    "A smoothed rope looped away from its attachments."
                );
            }
        }
    }

    private static void Near(Vector3 a, Vector3 b, float tolerance) =>
        Check((a - b).magnitude <= tolerance, "Rope attachment or translation changed.");

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
