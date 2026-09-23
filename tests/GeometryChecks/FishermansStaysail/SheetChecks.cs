using System;
using FishermansSail.Sails.FishermansStaysail;
using FishermansSail.Sails.FishermansStaysail.MkA;
using FishermansSail.Tests.GeometryChecks.FishermansStaysail.MkA;
using UnityEngine;

namespace FishermansSail.Tests.GeometryChecks.FishermansStaysail;

internal static class SheetChecks
{
    internal static void Run()
    {
        foreach (float slope in new[] { 20f, 35f, 55f })
        foreach (float width in new[] { 3f, 6f, 13.8f })
        foreach (float angle in new[] { -40f, 0f, 40f, -20f, 20f, -40f, 40f })
        {
            var c = FishermansStaysailMkAGeometry.Create(width, slope).Corners;
            var requestedHead = FishermansStaysailFrameGeometry.RotateAroundMast(
                c[1],
                c[0],
                Vector3.right,
                angle
            );
            var head = FishermansStaysailFrameGeometry.UpperHead(
                c[1],
                requestedHead,
                c[0],
                Vector3.right,
                1
            );
            var expected = FishermansStaysailFrameGeometry.RotateAroundMast(
                c[1],
                c[0],
                Vector3.right,
                angle * 0.85f
            );
            CutChecks.Near((head - expected).magnitude, 0, width * 1e-5f, "85% upper sheeting");
            var clew = FishermansStaysailFrameGeometry.RotateAroundMast(
                c[3],
                c[2],
                Vector3.right,
                angle
            );
            var points = new Vector3[33];
            float foot = (c[3] - c[2]).magnitude,
                leech = (c[1] - c[3]).magnitude;
            if (
                !FishermansStaysailTension.Fit(
                    clew,
                    head,
                    c[2],
                    new Vector3(0, -width * 0.02f, 0),
                    leech,
                    foot,
                    1,
                    points
                )
            )
                throw new Exception("Mk.A tension failed on a legal tack.");
            CutChecks.Near(
                (points[0] - c[2]).magnitude,
                foot * 0.99f,
                width * 1e-4f,
                "coupled foot budget"
            );
            float length = 0;
            for (int i = 1; i < points.Length; i++)
                length += (points[i] - points[i - 1]).magnitude;
            if (length > leech * 0.9901f)
                throw new Exception("Mk.A stretched the leech.");
            if (
                !FishermansStaysailAerodynamics.TryFrame(c[0], c[2], head, points[0], out var frame)
            )
                throw new Exception("Mk.A lost its aerodynamic frame on tack.");
            CutChecks.Near(frame.Normal.magnitude, 1, 1e-5f, "wind normal");
        }
        // Tilt the entire frame for mast rake and boat heel, and give the
        // reference stay its own pitch. Readout must show actual sheet travel.
        foreach (var axis in new[] { Vector3.right, new Vector3(0.2f, 1, 0.3f).normalized })
        foreach (float pitch in new[] { 20f, 55f })
        foreach (float angle in new[] { -60f, -40f, -15f, 0f, 15f, 40f, 60f })
        {
            var neutral = Vector3.Cross(axis, Vector3.forward).normalized;
            var stay = neutral + axis * (float)Math.Tan(pitch * Math.PI / 180);
            var current = FishermansStaysailFrameGeometry.RotateAroundMast(
                neutral,
                Vector3.zero,
                axis,
                angle
            );
            CutChecks.Near(
                FishermansStaysailTravel.SignedAngle(stay, current, axis),
                angle,
                1e-4f,
                "mast-relative angle (unclipped)"
            );
        }
        float fullMin = -85,
            fullMax = 85;
        FishermansStaysailTravel.ConstrainHinge(ref fullMin, ref fullMax, -85, 85);
        if (fullMin != -40 || fullMax != 40)
            throw new Exception("Mk.A final hinge exceeds 40 degrees.");
        float crossedMin = 30,
            crossedMax = -20;
        FishermansStaysailTravel.ConstrainHinge(ref crossedMin, ref crossedMax, -40, 40);
        if (crossedMin != crossedMax || crossedMin < -40 || crossedMax > 40)
            throw new Exception("Crossed sheet limits must remain a legal hinge range.");
        float min = -85,
            max = 85;
        FishermansStaysailTravel.ConstrainHinge(ref min, ref max, -25, 30);
        if (min != -25 || max != 30)
            throw new Exception("Mk.A lost tighter collision restrictions.");
        Console.WriteLine(
            "PASS: Mk.A repeated sheeting, 85% aft-head motion, coupled edge budgets, aerodynamic frames mast-relative readout and final 40-degree/tighter travel limits."
        );
    }
}
