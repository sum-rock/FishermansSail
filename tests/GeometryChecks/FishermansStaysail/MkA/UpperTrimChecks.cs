using System;
using FishermansSail.Sails.FishermansStaysail;
using FishermansSail.Sails.FishermansStaysail.MkA;
using UnityEngine;

namespace FishermansSail.Tests.GeometryChecks.FishermansStaysail.MkA;

internal static class UpperTrimChecks
{
    internal static void Run()
    {
        foreach (float slope in new[] { 20f, 35f, 55f })
        foreach (float width in new[] { 3f, 6.9f, 13.8f })
        foreach (float angle in new[] { -40f, -15f, 0f, 15f, 40f })
        foreach (float unroll in new[] { 0f, 0.5f, 0.75f, 0.8f, 0.95f, 1f })
        foreach (float load in new[] { -1f, -0.1f, 0f, 0.1f, 1f })
        {
            var c = FishermansStaysailMkAGeometry.Create(width, slope).Corners;
            float deployment = FishermansStaysailBillow.Deployment(unroll);
            var baseline = FishermansStaysailFrameGeometry.RotateAroundMast(
                c[1],
                c[0],
                Vector3.right,
                angle * 0.85f * deployment
            );
            var tack = FishermansStaysailReefingGeometry.Pose(c[2], c[0], c[1], unroll, unroll);
            var clew = FishermansStaysailReefingGeometry.Pose(c[3], c[0], c[1], unroll, unroll);
            clew = FishermansStaysailFrameGeometry.RotateAroundMast(
                clew,
                c[0],
                Vector3.right,
                angle * deployment
            );
            var pulley = c[0] + (c[1] - c[0]) * 1.5f + Vector3.right * (width * 0.03f);
            float foot = (clew - tack).magnitude;
            float leech = (c[1] - c[3]).magnitude * Math.Max(0.015f, unroll);
            var points = new Vector3[33];
            if (
                !FishermansStaysailUpperTrim.Fit(
                    baseline,
                    c[0],
                    pulley,
                    clew,
                    tack,
                    Vector3.up,
                    -Vector3.right,
                    width,
                    load,
                    unroll,
                    0.025f,
                    leech,
                    foot,
                    points,
                    out var head
                )
            )
                throw new Exception(
                    $"Trim failed: slope {slope}, width {width}, angle {angle}, unroll {unroll}, load {load}."
                );
            float tolerance = width * 1e-5f;
            CutChecks.Near(
                (head - c[0]).magnitude,
                (baseline - c[0]).magnitude,
                tolerance,
                "trim preserves head span"
            );
            float maxTravel = width * 0.025f * Math.Abs(load) * deployment;
            if ((head - baseline).magnitude > maxTravel + tolerance)
                throw new Exception("Upper trim exceeds its travel budget.");
            if ((pulley - head).magnitude > (pulley - baseline).magnitude + tolerance)
                throw new Exception("Upper trim lengthens the control line.");
            if (maxTravel == 0)
                CutChecks.Near(
                    (head - baseline).magnitude,
                    0,
                    tolerance,
                    "unloaded/furled baseline retained"
                );
            else if (angle != 0 && unroll == 1 && Math.Abs(load) == 1)
                if ((pulley - head).magnitude >= (pulley - baseline).magnitude - tolerance)
                    throw new Exception("Loaded upper corner did not move toward the aft pulley.");
            float reserve = 1 - 0.01f * deployment;
            CutChecks.Near(
                (points[0] - tack).magnitude,
                foot * reserve,
                tolerance,
                "trim respects foot budget"
            );
            CutChecks.Near(
                (points[32] - head).magnitude,
                0,
                tolerance,
                "leech ends at trimmed head"
            );
            float arc = 0;
            for (int i = 1; i < points.Length; i++)
                arc += (points[i] - points[i - 1]).magnitude;
            if (arc > leech * reserve + tolerance)
                throw new Exception("Upper trim stretches the leech.");
        }
        CheckArc();
        CheckLimitedFit();
        Console.WriteLine(
            "PASS (executed, dormant helper using Mk.A cut): optional wind-driven upper trim, shorter pulley line, fixed head span, reef fade and coupled edge budgets."
        );
    }

    private static void CheckArc()
    {
        var head = new Vector3(2, 0, 5);
        var fore = new Vector3(0, 0, -2);
        var guide = new Vector3(4, 2, 12);
        var expected = FishermansStaysailUpperTrim.Target(head, fore, guide, 0.1f);
        var axis = new Vector3(1, 2, 3).normalized;
        var shift = new Vector3(30, -6, 11);
        Vector3 Transform(Vector3 p) =>
            FishermansStaysailFrameGeometry.RotateAroundMast(p, Vector3.zero, axis, 57) + shift;
        var posed = FishermansStaysailUpperTrim.Target(
            Transform(head),
            Transform(fore),
            Transform(guide),
            0.1f
        );
        CutChecks.Near(
            (posed - Transform(expected)).magnitude,
            0,
            1e-5f,
            "trim follows raked/heeling frame"
        );
        CutChecks.Near(
            (
                FishermansStaysailUpperTrim.Target(head * 2, fore * 2, guide * 2, 0.2f)
                - expected * 2
            ).magnitude,
            0,
            1e-5f,
            "trim scales with sail"
        );
        CutChecks.Near(
            (FishermansStaysailUpperTrim.Target(head, head, guide, 0.1f) - head).magnitude,
            0,
            0,
            "zero-span fallback"
        );
        CutChecks.Near(
            (FishermansStaysailUpperTrim.Target(head, fore, guide, float.NaN) - head).magnitude,
            0,
            0,
            "invalid-travel fallback"
        );
        var nearest = fore + (guide - fore).normalized * (head - fore).magnitude;
        CutChecks.Near(
            (FishermansStaysailUpperTrim.Target(head, fore, guide, 100) - nearest).magnitude,
            0,
            1e-5f,
            "no pulley overshoot"
        );
        float load = 0;
        foreach (float target in new[] { 1f, -1f, 1f, 0f })
            for (int frame = 0; frame < 60; frame++)
            {
                var previous = FishermansStaysailUpperTrim.Target(
                    head,
                    fore,
                    guide,
                    0.1f * Math.Abs(load)
                );
                load = FishermansStaysailBillow.SmoothLoad(load, target, 1f / 60);
                var current = FishermansStaysailUpperTrim.Target(
                    head,
                    fore,
                    guide,
                    0.1f * Math.Abs(load)
                );
                if ((current - previous).magnitude > 0.011f)
                    throw new Exception("Upper trim jumps across a load reversal.");
            }
    }

    private static void CheckLimitedFit()
    {
        // Baseline is just inside the available edge budget; pulling upward
        // would separate head and tack too far. Back off only the new trim.
        var head = new Vector3(1, 0, 0);
        var tack = new Vector3(0, -1, 0);
        var clew = new Vector3(1, -1, 0);
        var guide = new Vector3(1, 10, 0);
        var points = new Vector3[33];
        float foot = 1 / 0.99f,
            leech = 0.42f / 0.99f;
        var original = new Vector3[33];
        FishermansStaysailTension.Fit(clew, head, tack, Vector3.zero, leech, foot, 1, original);
        if (
            !FishermansStaysailUpperTrim.Fit(
                head,
                Vector3.zero,
                guide,
                clew,
                tack,
                Vector3.forward,
                Vector3.zero,
                1,
                1,
                1,
                0,
                leech,
                foot,
                points,
                out var disabled
            )
        )
            throw new Exception("A mark with zero trim must preserve its baseline fit.");
        CutChecks.Near((disabled - head).magnitude, 0, 0, "disabled trim head");
        for (int i = 0; i < points.Length; i++)
            CutChecks.Near((points[i] - original[i]).magnitude, 0, 1e-6f, "disabled trim leech");
        var full = FishermansStaysailUpperTrim.Target(head, Vector3.zero, guide, 0.015f);
        if (FishermansStaysailTension.Fit(clew, full, tack, Vector3.zero, leech, foot, 1, points))
            throw new Exception("The limiting test must reject the full trim.");
        if (
            !FishermansStaysailUpperTrim.Fit(
                head,
                Vector3.zero,
                guide,
                clew,
                tack,
                Vector3.forward,
                Vector3.zero,
                1,
                1,
                1,
                0.015f,
                leech,
                foot,
                points,
                out var limited
            )
        )
            throw new Exception("Feasible baseline must survive a rejected trim.");
        if (
            (limited - head).magnitude >= (full - head).magnitude
            || (limited - head).magnitude < 1e-5f
        )
            throw new Exception("Edge constraints should retain a smaller feasible trim.");
        if (
            FishermansStaysailUpperTrim.Fit(
                head,
                Vector3.zero,
                guide,
                clew,
                tack,
                Vector3.forward,
                Vector3.zero,
                1,
                1,
                1,
                0.015f,
                0.1f,
                0.1f,
                points,
                out var fallback
            )
        )
            throw new Exception("Invalid baseline must still report failure.");
        CutChecks.Near((fallback - head).magnitude, 0, 0, "invalid baseline keeps original head");
        foreach (var point in points)
            if (float.IsNaN(point.sqrMagnitude) || float.IsInfinity(point.sqrMagnitude))
                throw new Exception("Rejected fit must leave finite points.");
    }
}
