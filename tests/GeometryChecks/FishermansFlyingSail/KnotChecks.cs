using System;
using System.Linq;
using MoreSailwindSails.Sails.FishermansFlyingSail;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;

internal static class KnotChecks
{
    internal static void Run()
    {
        // Synthetic disconnected tube and compact attachment; no game mesh is
        // copied into the repository. Include an unused vertex near the knot.
        var vertices = new[]
        {
            new Vector3(0.02f, 0, 0),
            new Vector3(-0.02f, 0, 0),
            new Vector3(0, 0.02f, 1),
            new Vector3(0.08f, 0, 0),
            new Vector3(0, 0.08f, 0),
            new Vector3(0, 0, -0.08f),
            new Vector3(-0.08f, 0, 0),
            Vector3.zero,
        };
        var triangles = new[] { 0, 1, 2, 3, 4, 5, 4, 6, 5 };
        foreach (float scale in new[] { 0.1f, 1f, 10f })
        foreach (bool reversed in new[] { false, true })
        {
            var offset = new Vector3(9, -3, 18);
            var posed = vertices
                .Select(v =>
                    offset
                    + FishermansFlyingSailFrameGeometry.RotateAroundMast(
                        v * scale,
                        Vector3.zero,
                        new Vector3(1, 2, 3),
                        57
                    )
                )
                .ToArray();
            var ordered = reversed ? posed.Reverse().ToArray() : posed;
            var inputTriangles = triangles
                .Select(i => reversed ? vertices.Length - 1 - i : i)
                .ToArray();
            var originalTriangles = (int[])inputTriangles.Clone();
            var selected = FishermansFlyingSailKnotGeometry.Select(
                ordered,
                inputTriangles,
                offset,
                out var compact
            );
            Check(
                selected.Length == 4 && compact.Length == 6,
                "Knot extraction retained tube or unused vertices."
            );
            Check(
                selected.All(i =>
                {
                    int original = reversed ? vertices.Length - 1 - i : i;
                    return original >= 3 && original <= 6;
                }),
                "Knot extraction depends on source vertex order."
            );
            Check(
                inputTriangles.SequenceEqual(originalTriangles),
                "Extraction modified donor triangle indices."
            );
            for (int i = 0; i < compact.Length; i++)
                Check(
                    selected[compact[i]] == inputTriangles[i + 3],
                    "Compaction changed triangle winding or UV/normal mapping."
                );
        }
        Reject(vertices, new[] { 0, 1, 2 }); // Missing knot.
        Reject(vertices, new[] { 0, 1, 2, 2, 3, 4 }); // One connected section.
        Reject(vertices, new[] { 0, 1, 99 });
        Reject(vertices, new[] { 0, 1 });
        Reject(
            new[]
            {
                Vector3.zero,
                Vector3.one,
                Vector3.right,
                Vector3.one,
                Vector3.right,
                Vector3.up,
            },
            new[] { 0, 1, 2, 3, 4, 5 }
        ); // Equally large sections are ambiguous.
        var invalid = (Vector3[])vertices.Clone();
        invalid[3] = new Vector3(float.NaN, 0, 0);
        Reject(invalid, triangles);
        Console.WriteLine(
            "PASS: isolated knot selection, compact indices, preserved winding, transformed/reordered inputs and incompatible donor rejection."
        );
    }

    private static void Reject(Vector3[] vertices, int[] triangles)
    {
        try
        {
            FishermansFlyingSailKnotGeometry.Select(vertices, triangles, Vector3.zero, out _);
        }
        catch (ArgumentException)
        {
            return;
        }
        throw new Exception(
            "Incompatible donor should omit its decoration instead of including the rope tube."
        );
    }

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
