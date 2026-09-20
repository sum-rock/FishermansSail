using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using FishermansSail;
using UnityEngine;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--stay-fixture")
        {
            StayChecks.Run(args[1]);
            args = Array.Empty<string>();
        }
        else
            StayChecks.Run();
        // A triangular sail in X/Z, with a pinned luff and a free clew.
        var vertices = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 14),
            new Vector3(7, 0, 7),
            new Vector3(3.5f, 0, 3.5f),
            new Vector3(3.5f, 0, 10.5f),
            new Vector3(2, 0, 7),
            new Vector3(0, 0, 7),
        };
        var constraints = Constraints(
            new[] { 0f, 0f, float.MaxValue, float.MaxValue, float.MaxValue, 0f, 0f }
        );
        var output = CheckInvariants(vertices, constraints);
        Require(
            output[3].x > vertices[3].x && output[4].x > vertices[4].x,
            "Both free edges must visibly widen."
        );
        Require(Same(output[2], vertices[2]), "The clew must remain at its rope attachment.");
        Require(Same(output[5], vertices[5]), "Even interior pinned vertices must remain fixed.");
        ExpectFailure(() => PrototypeGeometry.Deform(vertices, new ClothSkinningCoefficient[1]));
        ExpectFailure(() =>
            PrototypeGeometry.Deform(new Vector3[3], new ClothSkinningCoefficient[3])
        );
        var invalid = (Vector3[])vertices.Clone();
        invalid[1].x = float.NaN;
        ExpectFailure(() => PrototypeGeometry.Deform(invalid, constraints));
        Console.WriteLine(
            "PASS: mesh isolation, pinned vertices, corner positions, visible deformation, bounds, and invalid inputs."
        );

        // Optional local game fixture; proprietary geometry is never committed.
        if (args.Length > 0)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(args[0]));
            var root = document.RootElement;
            var source = root.GetProperty("vertices")
                .EnumerateArray()
                .Select(v => new Vector3(v[0].GetSingle(), v[1].GetSingle(), v[2].GetSingle()))
                .ToArray();
            var coefficients = Constraints(
                root.GetProperty("maxDistances")
                    .EnumerateArray()
                    .Select(v => v.GetSingle())
                    .ToArray()
            );
            var changed = CheckInvariants(source, coefficients);
            double before = 0,
                after = 0;
            foreach (var triangle in root.GetProperty("triangles").EnumerateArray())
            {
                int a = triangle[0].GetInt32(),
                    b = triangle[1].GetInt32(),
                    c = triangle[2].GetInt32();
                double oldArea = SignedArea(source[a], source[b], source[c]);
                double newArea = SignedArea(changed[a], changed[b], changed[c]);
                if (Math.Abs(oldArea) > 1e-7)
                    Require(oldArea * newArea > 0, "A triangle collapsed or inverted.");
                before += Math.Abs(oldArea);
                after += Math.Abs(newArea);
            }
            Require(after > before, "The deformed sail should have greater area.");
            Console.WriteLine(
                $"PASS: actual brig mesh ({source.Length} vertices, {coefficients.Count(c => c.maxDistance == 0)} pinned); "
                    + $"no inverted triangles; projected area {before:F2} -> {after:F2}."
            );
        }
    }

    private static Vector3[] CheckInvariants(
        Vector3[] source,
        ClothSkinningCoefficient[] constraints
    )
    {
        var snapshot = (Vector3[])source.Clone();
        var originalConstraints = constraints.Select(c => c.maxDistance).ToArray();
        var output = PrototypeGeometry.Deform(source, constraints);
        Require(
            !ReferenceEquals(source, output) && source.Length == output.Length,
            "Return a separate array with the original topology."
        );
        float minX = source.Min(v => v.x),
            maxX = source.Max(v => v.x);
        bool anyChanged = false;
        for (int i = 0; i < source.Length; i++)
        {
            Require(Same(source[i], snapshot[i]), "The input geometry was mutated.");
            Require(
                constraints[i].maxDistance == originalConstraints[i],
                "The cloth constraints were mutated."
            );
            Require(
                output[i].y == source[i].y && output[i].z == source[i].z,
                "Deformation must stay in the existing cloth plane."
            );
            Require(
                float.IsFinite(output[i].x) && output[i].x >= minX && output[i].x <= maxX,
                "The deformation escaped its original bounds."
            );
            if (constraints[i].maxDistance <= 0)
                Require(Same(output[i], source[i]), "An attachment vertex moved.");
            anyChanged |= !Same(output[i], source[i]);
        }
        Require(anyChanged, "The deformation must change the mesh.");
        return output;
    }

    private static double SignedArea(Vector3 a, Vector3 b, Vector3 c) =>
        ((double)(b.x - a.x) * (c.z - a.z) - (double)(b.z - a.z) * (c.x - a.x)) * 0.5;

    private static ClothSkinningCoefficient[] Constraints(float[] values) =>
        values.Select(v => new ClothSkinningCoefficient { maxDistance = v }).ToArray();

    private static bool Same(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }

    private static void ExpectFailure(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }
        throw new Exception("Expected an invalid mesh to be rejected.");
    }
}
