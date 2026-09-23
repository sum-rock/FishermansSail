using System;
using System.Linq;
using FishermansSail;
using UnityEngine;

internal static class FlyingSailChecks
{
    internal static void Run()
    {
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        foreach (float scale in new[] { 0.55f, 1f, 1.5f })
        foreach (float rake in new[] { 0f, 8f, -12f })
        {
            var data = PrototypeGeometry.Create(width);
            var pivot = new Vector3(0, 0, -width * 2);
            var axis = FlyingSailGeometry.RotateAroundMast(
                Vector3.right,
                Vector3.zero,
                Vector3.up,
                rake
            );
            var rest = data
                .Corners.Select(c =>
                    FlyingSailGeometry.RotateAroundMast(c * scale, Vector3.zero, Vector3.up, rake)
                )
                .ToArray();
            var offset = FlyingSailGeometry.ModelOffset(pivot, rest[0]);
            rest = rest.Select(c => offset + c).ToArray();
            Near(rest[0], pivot, "Scaled sail head must meet the mast despite a wider stay.");
            foreach (float angle in new[] { -70f, -25f, 0f, 25f, 70f })
            {
                var posed = rest.Select(c =>
                        FlyingSailGeometry.RotateAroundMast(c, pivot, axis, angle)
                    )
                    .ToArray();
                Near(posed[0], rest[0], "Forward head moved away from the mast.");
                Near(posed[2], rest[2], "Forward tack moved away from the mast.");
                Near(
                    posed[1] - rest[1],
                    posed[3] - rest[3],
                    "Aft head must swing out with the clew."
                );
                for (int i = 0; i < 4; i++)
                for (int j = i + 1; j < 4; j++)
                    Check(
                        Math.Abs((posed[i] - posed[j]).magnitude - (rest[i] - rest[j]).magnitude)
                            < 0.001f,
                        "Sheeting stretched the four-corner outline."
                    );
                if (angle != 0)
                    Check(
                        posed[1].y * angle < 0 && posed[3].y * angle < 0,
                        "Head and clew moved to different sides."
                    );
                foreach (float unroll in new[] { 0f, 0.5f, 0.75f, 0.8f, 0.9f, 0.98f, 1f })
                {
                    var upper = FlyingSailGeometry.UpperHead(
                        rest[1],
                        posed[1],
                        pivot,
                        axis,
                        unroll
                    );
                    Near(
                        upper,
                        FlyingSailGeometry.RotateAroundMast(
                            rest[1],
                            pivot,
                            axis,
                            angle * 0.85f * FishermanBillow.Deployment(unroll)
                        ),
                        "Upper head lost its angle, pivot or neutral frame on a scaled/raked mast."
                    );
                    Check(
                        Math.Abs((upper - pivot).magnitude - (rest[1] - pivot).magnitude) < 0.001f,
                        "The moving upper corner changed the top span."
                    );
                    var reefed =
                        pivot - axis * ((rest[2] - pivot).magnitude * Math.Max(0.015f, unroll));
                    Near(
                        FlyingSailGeometry.RotateAroundMast(reefed, pivot, axis, angle),
                        reefed,
                        "Furling tack must rise along the mast at every sheet angle."
                    );
                }
            }
            for (int col = 1; col < PrototypeGeometry.Columns; col++)
                Check(
                    data.Constraints[col].maxDistance > 0,
                    "Top-edge cloth is still locked to the stay."
                );
        }
        Check(
            !FlyingSailGeometry.PositionChanged(Vector3.zero, new Vector3(0.0002f, 0, 0)),
            "Roundoff from boat movement must not reset the cloth."
        );
        Check(
            FlyingSailGeometry.PositionChanged(Vector3.zero, new Vector3(0.01f, 0, 0)),
            "Actual rig changes must update the hinge."
        );
        CheckUpperSheets();
        CheckUpperGuideSelection();
        Console.WriteLine(
            "PASS: mast-fixed luff, moving aft head/clew, raked masts, scaling, furling, unstretched sheeting, upper pulley routes and free top-edge cloth."
        );
    }

    private static void CheckUpperGuideSelection()
    {
        // Lateral offsets and rake can put a lower fitting higher in world space
        // when heeled. Selection must still follow the boat's mast height.
        var guides = new[]
        {
            new Vector3(-4, 12, 10),
            new Vector3(1, 18, 11),
            new Vector3(1, 18, 11),
            new Vector3(0, 15, 10.5f),
        };
        foreach (float rake in new[] { -12f, 0f, 8f })
        foreach (float heel in new[] { -75f, 0f, 75f })
        {
            var up = FlyingSailGeometry.RotateAroundMast(
                Vector3.up,
                Vector3.zero,
                Vector3.forward,
                heel
            );
            var posed = guides
                .Select(g =>
                    FlyingSailGeometry.RotateAroundMast(
                        FlyingSailGeometry.RotateAroundMast(g, Vector3.zero, Vector3.right, rake),
                        Vector3.zero,
                        Vector3.forward,
                        heel
                    )
                )
                .ToArray();
            Check(
                FlyingSailGeometry.HighestGuideIndex(ActiveGuides(posed), up) == 1,
                "Rake, heel or duplicate guides changed the selected upper pulley."
            );
            // The higher guide remains in the profile when the topmast is absent.
            // Reuse the same candidates as options change, as the live rig does.
            var candidates = new[]
            {
                new SheetGuideState(posed[1], false, false),
                new SheetGuideState(posed[3], true, true),
                new SheetGuideState(posed[1], false, false),
            };
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == 1,
                "Absent topmast stole the route from the active lower-mast pulley."
            );
            candidates[0] = new SheetGuideState(posed[1], true, true);
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == 0,
                "Enabling the topmast did not select its higher pulley."
            );
            candidates[0] = new SheetGuideState(posed[1], true, false);
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == 1,
                "An inactive pulley child must not receive the control line."
            );
            candidates[0] = new SheetGuideState(posed[1], false, true);
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == 1,
                "An active referenced guide cannot make an inactive owning mast eligible."
            );
            candidates[1] = new SheetGuideState(posed[3], false, false);
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == -1,
                "Loading with every mast inactive must leave no pulley selected."
            );
            candidates[1] = new SheetGuideState(posed[3], true, true);
            Check(
                FlyingSailGeometry.HighestGuideIndex(candidates, up) == 1,
                "Reactivating the lower mast must recover its pulley without rebuilding candidates."
            );
        }
        Check(
            FlyingSailGeometry.HighestGuideIndex(Array.Empty<SheetGuideState>(), Vector3.up) == -1,
            "Missing mast guides must allow the original route fallback."
        );
        var invalid = new[]
        {
            new Vector3(0, float.NaN, 0),
            new Vector3(float.PositiveInfinity, 1, 0),
        };
        Check(
            FlyingSailGeometry.HighestGuideIndex(ActiveGuides(invalid), Vector3.up) == -1
                && FlyingSailGeometry.HighestGuideIndex(
                    ActiveGuides(invalid.Concat(new[] { guides[1] }).ToArray()),
                    Vector3.up
                ) == 2,
            "Invalid guide positions must not mask a valid pulley or prevent fallback."
        );
        Check(
            FlyingSailGeometry.HighestGuideIndex(
                ActiveGuides(new[] { guides[0], guides[3] }),
                Vector3.up
            ) == 1,
            "Changing mast guides must refresh the selection."
        );
    }

    private static SheetGuideState[] ActiveGuides(Vector3[] positions) =>
        positions.Select(position => new SheetGuideState(position, true, true)).ToArray();

    private static void CheckUpperSheets()
    {
        var head = new Vector3(4, 12, 7);
        var guide = new Vector3(0, 18, 10);
        var controls = new[] { new Vector3(-2, 1, 12), new Vector3(2, 1, 12) };
        foreach (var control in controls)
        foreach (float slack in new[] { 0f, 0.25f, 1f })
        {
            Near(
                FlyingSailGeometry.UpperSheetPoint(head, guide, control, slack, 0),
                head,
                "Upper sheet detached from the moving head."
            );
            Near(
                FlyingSailGeometry.UpperSheetPoint(head, guide, control, slack, 0.5f),
                guide,
                "Upper sheet missed the aft mast's existing upper pulley."
            );
            Near(
                FlyingSailGeometry.UpperSheetPoint(head, guide, control, slack, 1),
                control,
                "Upper sheet failed to join its existing sheet control."
            );
            for (int i = 0; i <= 32; i++)
            {
                var point = FlyingSailGeometry.UpperSheetPoint(
                    head,
                    guide,
                    control,
                    slack,
                    i / 32f
                );
                Check(
                    float.IsFinite(point.x) && float.IsFinite(point.y) && float.IsFinite(point.z),
                    "Upper sheet generated invalid positions."
                );
            }
            for (int i = 1; i <= 16; i++)
                Check(
                    FlyingSailGeometry.UpperSheetPoint(head, guide, control, slack, i / 32f).y
                        > FlyingSailGeometry
                            .UpperSheetPoint(head, guide, control, slack, (i - 1) / 32f)
                            .y,
                    "The upper sheet must rise from the sail corner to the elevated pulley."
                );
            foreach (float t in new[] { 0.25f, 0.75f })
                Check(
                    FlyingSailGeometry.UpperSheetPoint(head, guide, control, 1, t).y
                        < FlyingSailGeometry.UpperSheetPoint(head, guide, control, 0, t).y,
                    "Slack sheets must sag farther than tensioned sheets on both spans."
                );
        }
        Check(
            FlyingSailGeometry.SheetSlack(1, 1) == 0
                && Math.Abs(FlyingSailGeometry.SheetSlack(1, 0.44f) - 0.56f) < 1e-6f
                && FlyingSailGeometry.SheetSlack(1, 2) == 0
                && FlyingSailGeometry.SheetSlack(0, 0) == 0
                && FlyingSailGeometry.SheetSlack(float.NaN, 1) == 0,
            "Native sheet slack must map independently to bounded visual sag."
        );
    }

    private static void Near(Vector3 a, Vector3 b, string message) =>
        Check((a - b).magnitude < 0.001f, message);

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
