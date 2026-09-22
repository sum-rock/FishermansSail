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
                foreach (float unroll in new[] { 0f, 0.5f, 1f })
                {
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
        Console.WriteLine(
            "PASS: mast-fixed luff, moving aft head/clew, raked masts, scaling, furling, unstretched sheeting and free top-edge cloth."
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
