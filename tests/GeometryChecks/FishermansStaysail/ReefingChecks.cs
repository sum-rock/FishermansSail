using System;
using FishermansSail.Sails.FishermansStaysail;
using FishermansSail.Sails.FishermansStaysail.MkA;
using FishermansSail.Tests.GeometryChecks.FishermansStaysail.MkA;
using UnityEngine;

namespace FishermansSail.Tests.GeometryChecks.FishermansStaysail;

internal static class ReefingChecks
{
    internal static void Run()
    {
        // Installed brig jib reef clip's fold-bone endpoint scales.
        var open = Vector3.one * 0.9988177f;
        var closed = Vector3.one * 0.03956151f;
        foreach (float slope in new[] { 20f, 35f, 55f })
        foreach (float width in new[] { 3f, 6.9f, 13.8f })
        foreach (float t in new[] { 0f, 0.2f, 0.5f, 0.8f, 1f, 0.8f, 0.5f, 0.2f, 0f })
        {
            var corners = FishermansStaysailMkAGeometry.Create(width, slope).Corners;
            float amount = FishermansStaysailReefingGeometry.Progress(
                Vector3.Lerp(closed, open, t),
                closed,
                open
            );
            CutChecks.Near(amount, t, 1e-5f, "native channel normalization/reversal");
            foreach (float along in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
            {
                var head = Vector3.Lerp(corners[0], corners[1], along);
                var foot = Vector3.Lerp(corners[2], corners[3], along);
                var posedHead = FishermansStaysailReefingGeometry.Pose(
                    head,
                    corners[0],
                    corners[1],
                    amount,
                    amount
                );
                var posedFoot = FishermansStaysailReefingGeometry.Pose(
                    foot,
                    corners[0],
                    corners[1],
                    amount,
                    amount
                );
                CutChecks.Near((posedHead - head).magnitude, 0, 1e-5f, "reef head stays in place");
                CutChecks.Near(posedFoot.z, foot.z, 1e-5f, "reef retains head span");
                CutChecks.Near(posedFoot.y, 0, 1e-6f, "reef retains mast plane");
                if (posedFoot.x < foot.x - 1e-5f || posedFoot.x > head.x)
                    throw new Exception("Reefed foot must rise toward its head.");
                if (t == 1)
                    CutChecks.Near(
                        (posedFoot - foot).magnitude,
                        0,
                        1e-5f,
                        "full deployment unchanged"
                    );
                if (t == 0)
                    CutChecks.Near(
                        (head - posedFoot).magnitude,
                        (head - foot).magnitude * 0.015f,
                        1e-5f,
                        "gathers to narrow head strip"
                    );
            }
            foreach (float angle in new[] { -40f, 0f, 40f })
            {
                float deployment = FishermansStaysailBillow.Deployment(t);
                var tack = FishermansStaysailReefingGeometry.Pose(
                    corners[2],
                    corners[0],
                    corners[1],
                    amount,
                    amount
                );
                var clew = FishermansStaysailReefingGeometry.Pose(
                    corners[3],
                    corners[0],
                    corners[1],
                    amount,
                    amount
                );
                clew = FishermansStaysailFrameGeometry.RotateAroundMast(
                    clew,
                    corners[0],
                    Vector3.right,
                    angle * deployment
                );
                var head = FishermansStaysailFrameGeometry.RotateAroundMast(
                    corners[1],
                    corners[0],
                    Vector3.right,
                    angle * 0.85f * deployment
                );
                var points = new Vector3[33];
                if (
                    !FishermansStaysailTension.Fit(
                        clew,
                        head,
                        tack,
                        Vector3.zero,
                        (corners[1] - corners[3]).magnitude * Math.Max(0.015f, amount),
                        (clew - tack).magnitude,
                        deployment,
                        points
                    )
                )
                    throw new Exception(
                        $"Reef edge budgets fail at {slope} slope, {t} deployment, {angle} sheet."
                    );
            }
        }
        if (
            FishermansStaysailGeometry.RenderState(0) != 0
            || FishermansStaysailGeometry.RenderState(0.039f) != 0
            || FishermansStaysailGeometry.RenderState(0.04f) != 1
            || FishermansStaysailGeometry.RenderState(0.5f) != 1
            || FishermansStaysailGeometry.RenderState(1) != 2
        )
            throw new Exception("Native furled threshold or renderer selection changed.");
        Console.WriteLine(
            "PASS: native reef-channel normalization, halyard reversals, upward gathering on nominal/steep stays, partial-reef edge budgets and visible-bundle threshold."
        );
    }
}
