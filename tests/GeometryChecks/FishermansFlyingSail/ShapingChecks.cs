using System;
using MoreSailwindSails.Sails.FishermansFlyingSail;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;

internal static class ShapingChecks
{
    internal static void Run()
    {
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        {
            var data = FishermansFlyingSailGeometry.Create(width);
            // The two signed target surfaces mirror about the same attached
            // outline; this checks the actual weighted skin, not just the bones.
            var positive = Pose(data, width, 1, 1);
            var negative = Pose(data, width, -1, 1);
            var neutral = Pose(data, width, 0, 1);
            for (int i = 0; i < data.Vertices.Length; i++)
            {
                Near(positive[i], data.Vertices[i], width, "Positive pose changed the rest cut.");
                Near(
                    negative[i],
                    new Vector3(positive[i].x, -positive[i].y, positive[i].z),
                    width,
                    "Opposite camber retained a one-sided skin offset."
                );
                Near(
                    neutral[i],
                    new Vector3(positive[i].x, 0, positive[i].z),
                    width,
                    "Mid-transition camber must cross the panel plane."
                );
                if (data.Constraints[i].maxDistance == 0)
                    Near(negative[i], positive[i], width, "Billow moved a pinned attachment.");
            }
            for (int i = 0; i < data.Triangles.Length; i++)
            {
                int a = data.Triangles[i],
                    b = data.Triangles[i / 3 * 3 + (i + 1) % 3];
                Check(
                    Math.Abs(
                        (positive[a] - positive[b]).magnitude
                            - (negative[a] - negative[b]).magnitude
                    )
                        < width * 1e-5f,
                    "Mirroring camber changed the required fabric length."
                );
            }
            int top = FishermansFlyingSailGeometry.Columns / 2;
            int luff =
                FishermansFlyingSailGeometry.Rows / 2 * (FishermansFlyingSailGeometry.Columns + 1);
            foreach (int peak in new[] { top, luff })
            {
                float travel = data.Constraints[peak].maxDistance;
                Check(
                    positive[peak].y - travel > 0 && negative[peak].y + travel < 0,
                    "Settled top/luff peaks can still billow on the wrong side within their travel sphere."
                );
            }
            foreach (float unroll in new[] { 0f, 0.02f, 0.5f, 0.75f, 0.9f, 0.98f, 1f })
            {
                var a = Pose(data, width, -1, unroll);
                var b = Pose(data, width, 1, unroll);
                for (int i = 0; i < a.Length; i++)
                    Near(
                        a[i],
                        new Vector3(b[i].x, -b[i].y, b[i].z),
                        width,
                        "Furling failed to gather either signed curve through the bones."
                    );
            }
            foreach (int fps in new[] { 15, 30, 60, 144 })
            {
                float camber = 1;
                int side = 1,
                    flips = 0;
                var previous = positive;
                for (int tack = 0; tack < 4; tack++)
                {
                    int desired = tack % 2 == 0 ? -1 : 1;
                    for (int frame = 0; frame < fps * 3; frame++)
                    {
                        int next = FishermansFlyingSailBillow.CamberSide(side, desired * 8);
                        if (next != side)
                            flips++;
                        side = next;
                        camber = FishermansFlyingSailBillow.SmoothLoad(camber, side, 1f / fps);
                        Check(camber >= -1 && camber <= 1, "Camber interpolation overshot.");
                        var pose = Pose(data, width, camber, 1);
                        for (int i = 0; i < pose.Length; i++)
                            Check(
                                (pose[i] - previous[i]).magnitude < width * 0.8f / fps,
                                "A tack abruptly displaced the shaping surface."
                            );
                        previous = pose;
                    }
                    Check(
                        Math.Abs(camber - desired) < 0.001f,
                        "Billow failed to settle on the new side."
                    );
                }
                Check(flips == 4, "Sustained tacks must select one side each.");
            }
        }
        foreach (int side in new[] { -1, 1 })
        foreach (float flow in new[] { -0.6f, -0.1f, 0f, 0.1f, 0.6f, float.NaN })
            Check(
                FishermansFlyingSailBillow.CamberSide(side, flow) == side,
                "Weak wind must retain the previous target side."
            );
        // Apparent flow and the panel normal rotate together on a heeled/raked boat.
        var head = new Vector3(0, 0, 6);
        var fore = Vector3.zero;
        var tackPoint = new Vector3(-10, 0, 0);
        var clew = new Vector3(-6, 0, 6);
        var normal = FishermansFlyingSailBillow.CamberNormal(fore, tackPoint, head, clew);
        Vector3 Rotate(Vector3 p) =>
            FishermansFlyingSailFrameGeometry.RotateAroundMast(
                p,
                Vector3.zero,
                new Vector3(1, 2, 3),
                53
            );
        var offset = new Vector3(13, -8, 25);
        var moved = FishermansFlyingSailBillow.CamberNormal(
            Rotate(fore) + offset,
            Rotate(tackPoint) + offset,
            Rotate(head) + offset,
            Rotate(clew) + offset
        );
        Near(moved, Rotate(normal), 1, "Heel/rake changed the camber normal incorrectly.");
        Check(
            FishermansFlyingSailBillow.CamberSide(1, Vector3.Dot(Rotate(-normal * 8), moved)) == -1,
            "Boat rotation changed which side the apparent wind selects."
        );
        Console.WriteLine(
            "PASS: exact signed skin targets, fixed corners, loaded edge limits, bone-driven furling, repeated smooth tacks and rotated wind frames."
        );
    }

    private static Vector3[] Pose(
        FishermansFlyingSailMeshData data,
        float width,
        float camber,
        float unroll
    )
    {
        var bones = new Vector3[FishermansFlyingSailGeometry.BoneCount];
        var foreHead = HoistPose.Corner(data.Corners, 0, unroll);
        var tack = HoistPose.Corner(data.Corners, 2, unroll);
        var head = HoistPose.Corner(data.Corners, 1, unroll);
        var clew = HoistPose.Corner(data.Corners, 3, unroll);
        float amount = camber * FishermansFlyingSailBillow.Deployment(unroll);
        for (int row = 0; row <= FishermansFlyingSailGeometry.Rows; row++)
        for (int col = 0; col <= FishermansFlyingSailGeometry.ShapeColumns; col++)
        {
            float v = (float)row / FishermansFlyingSailGeometry.Rows,
                u = (float)col / FishermansFlyingSailGeometry.ShapeColumns;
            bones[FishermansFlyingSailGeometry.ShapeBone(row, col)] =
                FishermansFlyingSailBillow.ShapePoint(
                    Vector3.Lerp(foreHead, tack, v),
                    Vector3.Lerp(head, clew, v),
                    Vector3.up,
                    width,
                    u,
                    v,
                    amount
                );
        }
        var result = new Vector3[data.Vertices.Length];
        for (int i = 0; i < result.Length; i++)
        {
            var w = data.Weights[i];
            result[i] =
                data.Vertices[i]
                + (bones[w.boneIndex0] - data.BonePositions[w.boneIndex0]) * w.weight0
                + (bones[w.boneIndex1] - data.BonePositions[w.boneIndex1]) * w.weight1;
        }
        return result;
    }

    private static void Near(Vector3 a, Vector3 b, float width, string message) =>
        Check((a - b).magnitude < width * 1e-5f, message);

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
