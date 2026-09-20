using System;
using FishermansSail;
using UnityEngine;

internal static class Program
{
    private static void Main(string[] args)
    {
        StayChecks.Run(args.Length == 2 && args[0] == "--stay-fixture" ? args[1] : null);
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        {
            CheckSail(width);
            CheckBundle(width);
        }
        Require(
            PrototypeGeometry.RenderState(0) == 0 && PrototypeGeometry.RenderState(0.02f) == 0,
            "A fully struck sail must show only the bundle."
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
            "PASS: four-corner outline, 90/90/40/140 degree angles, area, winding, UVs, bone weights, cloth pins, furling, bundle geometry, render states, and independent mesh arrays."
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
        Angle(c[0], c[2], c[3], 40);
        Angle(c[2], c[3], c[1], 140);
        Require(Math.Abs(c[3].x / width + 0.5f) < 1e-5, "Wrong aft depth.");
        Require(c[2].x < c[3].x && c[2].z == -width, "The lowest corner must be forward.");
        double area = 0;
        for (int i = 0; i < d.Triangles.Length; i += 3)
        {
            var a = d.Vertices[d.Triangles[i]];
            var b = d.Vertices[d.Triangles[i + 1]];
            var e = d.Vertices[d.Triangles[i + 2]];
            var cross = Vector3.Cross(b - a, e - a);
            Require(cross.y > 0, "A triangle collapsed or inverted.");
            area += cross.magnitude * 0.5;
        }
        double expected = width * -(c[2].x + c[3].x) / 2;
        Require(Math.Abs(area / expected - 1) < 1e-5, "Mesh area does not equal trapezoid area.");
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
                c[w.boneIndex0] * w.weight0
                + c[w.boneIndex1] * w.weight1
                + c[w.boneIndex2] * w.weight2
                + c[w.boneIndex3] * w.weight3;
            Require(
                (reconstructed - d.Vertices[i]).magnitude < width * 1e-5,
                "Skinning does not reproduce the rest mesh."
            );
            var uv = d.UV[i];
            Require(uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1, "Invalid UV.");
            bool pinned = d.Constraints[i].maxDistance == 0;
            if (pinned)
                pins++;
            Require(
                pinned
                    == (
                        d.Vertices[i].x == 0
                        || i == PrototypeGeometry.Rows * (PrototypeGeometry.Columns + 1)
                        || i == d.Vertices.Length - 1
                    ),
                "Wrong cloth attachment."
            );
        }
        Require(
            pins == PrototypeGeometry.Columns + 3,
            "Expected pinned top edge and both lower corners."
        );
        foreach (var corner in c)
        {
            var full = PrototypeGeometry.ReefCorner(corner, 1);
            var half = PrototypeGeometry.ReefCorner(corner, 0.5f);
            var furled = PrototypeGeometry.ReefCorner(corner, 0);
            Require((full - corner).magnitude < 1e-6, "Unfurling changes the rest shape.");
            Require(
                half.x == corner.x * 0.5f && half.z == corner.z,
                "Furling must raise corners without moving along the stay."
            );
            Require(
                Math.Abs(furled.x) <= Math.Abs(corner.x) * 0.016f && furled.z == corner.z,
                "Struck sail must gather at the top."
            );
        }
        Require(
            d.Center.x < 0 && d.Center.z > -width && d.Center.z < 0,
            "Wind center lies outside the sail."
        );
    }

    private static void CheckBundle(float width)
    {
        var data = PrototypeGeometry.CreateBundle(width);
        foreach (var vertex in data.Vertices)
            Require(
                float.IsFinite(vertex.x)
                    && float.IsFinite(vertex.y)
                    && float.IsFinite(vertex.z)
                    && vertex.x >= -width * 0.01601f
                    && vertex.x <= 0
                    && Math.Abs(vertex.y) <= width * 0.00801f
                    && vertex.z >= -width
                    && vertex.z <= 0,
                "Furled mesh must stay within a narrow bundle along the head."
            );
        double volume = 0;
        for (int i = 0; i < data.Triangles.Length; i += 3)
        {
            var a = data.Vertices[data.Triangles[i]];
            var b = data.Vertices[data.Triangles[i + 1]];
            var c = data.Vertices[data.Triangles[i + 2]];
            var normal = Vector3.Cross(b - a, c - a);
            Require(normal.magnitude > 1e-9, "Furled bundle has a collapsed triangle.");
            volume += Vector3.Dot(a, Vector3.Cross(b, c)) / 6;
        }
        Require(volume > 0, "Bundle faces must point outward.");
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
