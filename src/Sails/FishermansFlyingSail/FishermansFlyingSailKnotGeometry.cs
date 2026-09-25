using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal static class FishermansFlyingSailKnotGeometry
    {
        // The native jib rope has two disconnected sections: its long tube and
        // the compact knot at bone zero. Select by topology and proximity, not
        // native vertex ordering, and compact away every unused rope vertex.
        internal static int[] Select(
            Vector3[] vertices,
            int[] triangles,
            Vector3 attachment,
            out int[] indices
        )
        {
            if (vertices.Length == 0 || triangles.Length == 0 || triangles.Length % 3 != 0)
                throw new ArgumentException("Expected a triangulated native jib rope.");
            var parents = new int[vertices.Length];
            var used = new bool[vertices.Length];
            for (int i = 0; i < parents.Length; i++)
                parents[i] = i;
            foreach (int vertex in triangles)
            {
                if (vertex < 0 || vertex >= vertices.Length)
                    throw new ArgumentException("Native jib rope has an invalid triangle index.");
                used[vertex] = true;
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int root = Find(parents, triangles[i]);
                parents[Find(parents, triangles[i + 1])] = root;
                parents[Find(parents, triangles[i + 2])] = root;
            }
            var radii = new Dictionary<int, float>();
            for (int i = 0; i < vertices.Length; i++)
            {
                if (!used[i])
                    continue;
                int root = Find(parents, i);
                float radius = (vertices[i] - attachment).sqrMagnitude;
                if (float.IsNaN(radius) || float.IsInfinity(radius))
                    throw new ArgumentException("Native jib rope contains nonfinite vertices.");
                radii.TryGetValue(root, out float previous);
                radii[root] = Math.Max(previous, radius);
            }
            if (radii.Count != 2)
                throw new ArgumentException("Expected separate native rope and knot sections.");
            int selected = -1;
            float smallest = float.MaxValue,
                largest = 0;
            foreach (var component in radii)
            {
                if (component.Value < smallest)
                {
                    selected = component.Key;
                    smallest = component.Value;
                }
                largest = Math.Max(largest, component.Value);
            }
            if (smallest <= 1e-10f || smallest >= largest * 0.25f)
                throw new ArgumentException("Native knot is not compact around its attachment.");
            var originals = new List<int>();
            var map = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                if (used[i] && Find(parents, i) == selected)
                {
                    map[i] = originals.Count;
                    originals.Add(i);
                }
            var compact = new List<int>();
            for (int i = 0; i < triangles.Length; i += 3)
                if (Find(parents, triangles[i]) == selected)
                {
                    compact.Add(map[triangles[i]]);
                    compact.Add(map[triangles[i + 1]]);
                    compact.Add(map[triangles[i + 2]]);
                }
            indices = compact.ToArray();
            return originals.ToArray();
        }

        private static int Find(int[] parents, int vertex)
        {
            while (parents[vertex] != vertex)
            {
                parents[vertex] = parents[parents[vertex]];
                vertex = parents[vertex];
            }
            return vertex;
        }
    }
}
