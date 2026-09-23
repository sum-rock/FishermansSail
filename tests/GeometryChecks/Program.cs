using System;
using FishermansSail;
using UnityEngine;

internal static class Program
{
    private static void Main()
    {
        RigChecks.Run();
        OrderTextChecks.Run();
        FlyingSailChecks.Run();
        MastInstallationChecks.Run();
        BillowChecks.Run();
        ShapingChecks.Run();
        AerodynamicChecks.Run();
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        {
            CheckSail(width);
        }
        Require(
            PrototypeGeometry.RenderState(0) == 0 && PrototypeGeometry.RenderState(0.02f) == 0,
            "A fully struck sail must select the hidden state."
        );
        Require(
            PrototypeGeometry.RenderState(0.03f) == 1 && PrototypeGeometry.RenderState(0.5f) == 1,
            "Partly furled sails must use the procedural renderer without cloth simulation."
        );
        Require(
            PrototypeGeometry.RenderState(0.98f) == 2 && PrototypeGeometry.RenderState(1) == 2,
            "Only fully set sails should use the cloth renderer."
        );
        foreach (float width in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, 101f })
        {
            try
            {
                PrototypeGeometry.Create(width);
            }
            catch (ArgumentException)
            {
                continue;
            }
            throw new Exception("Invalid width accepted.");
        }
        Console.WriteLine(
            "PASS: four-corner outline, revised cut, spare top-edge cloth, area, winding, UVs, bone weights, cloth pins, render states, and independent mesh arrays."
        );
    }

    private static void CheckSail(float width)
    {
        var d = PrototypeGeometry.Create(width);
        var second = PrototypeGeometry.Create(width);
        Require(
            !ReferenceEquals(d.Vertices, second.Vertices)
                && !ReferenceEquals(d.Constraints, second.Constraints),
            "Sails share mutable geometry."
        );
        var c = d.Corners;
        Angle(c[2], c[0], c[1], 90);
        Angle(c[0], c[1], c[3], 90);
        Require(Math.Abs(c[3].x / width + 1f) < 1e-5, "Wrong revised aft depth.");
        Require(
            Math.Abs(c[2].x / width + 1.6917536f) < 1e-5,
            "The original forward depth changed."
        );
        Require(c[2].x < c[3].x && c[2].z == -width, "The lowest corner must be forward.");
        double area = 0,
            projectedArea = 0;
        var centroid = Vector3.zero;
        for (int i = 0; i < d.Triangles.Length; i += 3)
        {
            var a = d.Vertices[d.Triangles[i]];
            var b = d.Vertices[d.Triangles[i + 1]];
            var e = d.Vertices[d.Triangles[i + 2]];
            var cross = Vector3.Cross(b - a, e - a);
            Require(cross.y > 0, "A triangle collapsed or inverted.");
            area += cross.magnitude * 0.5;
            projectedArea += cross.y * 0.5;
            centroid += (a + b + e) * (cross.magnitude / 6);
        }
        double expected = width * -(c[2].x + c[3].x) / 2;
        Require(Math.Abs(projectedArea / expected - 1) < 1e-5, "Projected cut area is wrong.");
        Require(
            area > expected * 1.005 && area < expected * 1.05,
            "Camber must add actual cloth area."
        );
        Require(
            (centroid / (float)area - d.Center).magnitude < width * 1e-5f,
            "Wind center must use the revised surface centroid."
        );
        double topLength = 0;
        for (int col = 1; col <= PrototypeGeometry.Columns; col++)
            topLength += (d.Vertices[col] - d.Vertices[col - 1]).magnitude;
        Require(
            topLength > width * 1.03 && topLength < width * 1.04,
            "The head needs spare cloth between its fixed endpoints."
        );
        Require(
            Math.Abs(d.Vertices[PrototypeGeometry.Columns / 2].y - width * 0.12f) < width * 1e-5f,
            "Top camber must peak at twelve percent of width."
        );
        double luffLength = 0;
        for (int row = 1; row <= PrototypeGeometry.Rows; row++)
            luffLength += (
                d.Vertices[row * (PrototypeGeometry.Columns + 1)]
                - d.Vertices[(row - 1) * (PrototypeGeometry.Columns + 1)]
            ).magnitude;
        double luffChord = (c[2] - c[0]).magnitude;
        Require(
            luffLength > luffChord * 1.002 && luffLength < luffChord * 1.004,
            "Luff needs a small amount of actual extra cloth length, not just movement permission."
        );
        var boneSeen = new bool[PrototypeGeometry.BoneCount];
        for (int row = 0; row <= PrototypeGeometry.Rows; row++)
        for (int col = 0; col <= PrototypeGeometry.ShapeColumns; col++)
        {
            int bone = PrototypeGeometry.ShapeBone(row, col);
            Require(
                bone >= 0 && bone < boneSeen.Length && !boneSeen[bone],
                "Invalid or duplicate shaping bone index."
            );
            boneSeen[bone] = true;
        }
        Require(Array.TrueForAll(boneSeen, b => b), "Uninitialized shaping bone.");
        Require(
            d.Constraints[PrototypeGeometry.Columns / 2].maxDistance < width * 0.12f,
            "Top travel must keep the loaded peak on the target side."
        );
        int pins = 0;
        for (int i = 0; i < d.Vertices.Length; i++)
        {
            var w = d.Weights[i];
            Require(
                w.weight0 >= w.weight1 && w.weight1 >= w.weight2 && w.weight2 >= w.weight3,
                "Unity skin weights must be sorted largest first."
            );
            Require(w.weight0 > 0, "A vertex must start with a nonzero influence.");
            Require(
                w.weight0 >= 0 && w.weight1 >= 0 && w.weight2 >= 0 && w.weight3 >= 0,
                "Negative skin weight."
            );
            Require(
                Math.Abs(w.weight0 + w.weight1 + w.weight2 + w.weight3 - 1) < 1e-5,
                "Skin weights do not sum to one."
            );
            var reconstructed =
                d.BonePositions[w.boneIndex0] * w.weight0
                + d.BonePositions[w.boneIndex1] * w.weight1
                + d.BonePositions[w.boneIndex2] * w.weight2
                + d.BonePositions[w.boneIndex3] * w.weight3;
            Require(
                (reconstructed - d.Vertices[i]).magnitude < width * 1e-5f,
                "Shaping bones must reconstruct the whole rest mesh without a one-sided residual."
            );
            var uv = d.UV[i];
            Require(uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1, "Invalid UV.");
            bool pinned = d.Constraints[i].maxDistance == 0;
            if (pinned)
                pins++;
            Require(
                pinned
                    == (
                        i == 0
                        || i == PrototypeGeometry.Columns
                        || i == PrototypeGeometry.Rows * (PrototypeGeometry.Columns + 1)
                        || i == d.Vertices.Length - 1
                    ),
                "Wrong cloth attachment."
            );
        }
        Require(pins == 4, "Only the four sail corners should be pinned.");
        for (int row = 1; row < PrototypeGeometry.Rows; row++)
        {
            int fore = row * (PrototypeGeometry.Columns + 1);
            float travel = d.Constraints[fore].maxDistance;
            Require(
                travel > 0 && travel <= width * 0.04001f,
                "Intermediate luff vertices must be free with a modest travel limit."
            );
            int mirrored = (PrototypeGeometry.Rows - row) * (PrototypeGeometry.Columns + 1);
            Require(
                Math.Abs(travel - d.Constraints[mirrored].maxDistance) < width * 1e-6f,
                "Luff travel must taper symmetrically toward its fixed corners."
            );
            Require(
                travel < d.Vertices[fore].y,
                "Loaded forward-edge travel must stay within its moving camber target."
            );
            int i = row * (PrototypeGeometry.Columns + 1) + PrototypeGeometry.Columns;
            Require(
                d.Constraints[i].maxDistance > 0
                    && d.Constraints[i].maxDistance <= width * 0.06001f,
                "Intermediate leech vertices need bounded movement away from their skin targets."
            );
        }
        Require(
            Math.Abs(
                d.Constraints[
                    (PrototypeGeometry.Rows / 2) * (PrototypeGeometry.Columns + 1)
                ].maxDistance
                    - width * 0.04f
            )
                < width * 1e-6f,
            "Luff travel must allow flex around the moving six-percent curve."
        );
        float leechMiddle = d.Constraints[
            (PrototypeGeometry.Rows / 2) * (PrototypeGeometry.Columns + 1)
                + PrototypeGeometry.Columns
        ].maxDistance;
        Require(
            Math.Abs(leechMiddle - width * 0.06f) < width * 1e-6f,
            "Mid-leech must have six percent of width to flex naturally."
        );
        float nearClew = d.Constraints[
            (PrototypeGeometry.Rows - 1) * (PrototypeGeometry.Columns + 1)
                + PrototypeGeometry.Columns
        ].maxDistance;
        Require(
            nearClew > 0 && nearClew < width * 0.002f,
            "The free leech must retain reinforcement beside the clew."
        );
        Require(
            d.Center.x < 0 && d.Center.z > -width && d.Center.z < 0,
            "Wind center lies outside the sail."
        );
    }

    private static void Angle(Vector3 a, Vector3 b, Vector3 c, double expected)
    {
        var u = (a - b).normalized;
        var v = (c - b).normalized;
        double actual = Math.Acos(Math.Max(-1, Math.Min(1, Vector3.Dot(u, v)))) * 180 / Math.PI;
        Require(Math.Abs(actual - expected) < 0.001, "Wrong corner angle: " + actual);
    }

    private static void Require(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
