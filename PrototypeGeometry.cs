using System;
using UnityEngine;

namespace FishermansSail
{
    internal sealed class SailMeshData
    {
        internal Vector3[] Vertices;
        internal Vector2[] UV;
        internal int[] Triangles;
        internal BoneWeight[] Weights;
        internal ClothSkinningCoefficient[] Constraints;
        internal Vector3[] Corners;
        internal Vector3 Center;
    }

    internal static class PrototypeGeometry
    {
        internal const int Columns = 24;
        internal const int Rows = 32;
        internal const float AftDepthRatio = 0.5f;
        internal static readonly float ForeDepthRatio =
            AftDepthRatio + 1f / (float)Math.Tan(40 * Math.PI / 180);

        // Sail mount frame: +X is up, -Z points forward along the stay.
        // Corner order: top fore, top aft, bottom fore, bottom aft.
        internal static SailMeshData Create(float width)
        {
            if (float.IsNaN(width) || float.IsInfinity(width) || width < 0.25f || width > 100f)
                throw new ArgumentException(
                    "Expected a finite sail width between 0.25 and 100 metres."
                );
            var data = new SailMeshData
            {
                Vertices = new Vector3[(Columns + 1) * (Rows + 1)],
                UV = new Vector2[(Columns + 1) * (Rows + 1)],
                Weights = new BoneWeight[(Columns + 1) * (Rows + 1)],
                Constraints = new ClothSkinningCoefficient[(Columns + 1) * (Rows + 1)],
                Triangles = new int[Columns * Rows * 6],
                Corners = new[]
                {
                    new Vector3(0, 0, -width),
                    Vector3.zero,
                    new Vector3(-width * ForeDepthRatio, 0, -width),
                    new Vector3(-width * AftDepthRatio, 0, 0),
                },
            };
            for (int row = 0; row <= Rows; row++)
            for (int col = 0; col <= Columns; col++)
            {
                float u = (float)col / Columns,
                    v = (float)row / Rows;
                int i = row * (Columns + 1) + col;
                data.Vertices[i] = new Vector3(
                    -width * (ForeDepthRatio + (AftDepthRatio - ForeDepthRatio) * u) * v,
                    0,
                    -width * (1 - u)
                );
                data.UV[i] = new Vector2(u, 1 - v);
                data.Weights[i] = SortedWeights((1 - u) * (1 - v), u * (1 - v), (1 - u) * v, u * v);
                bool pinned = row == 0 || (row == Rows && (col == 0 || col == Columns));
                data.Constraints[i] = new ClothSkinningCoefficient
                {
                    maxDistance = pinned ? 0 : width * 0.06f * v,
                    collisionSphereDistance = 0,
                };
                if (row == Rows || col == Columns)
                    continue;
                int t = (row * Columns + col) * 6;
                data.Triangles[t] = i;
                data.Triangles[t + 1] = i + Columns + 1;
                data.Triangles[t + 2] = i + 1;
                data.Triangles[t + 3] = i + 1;
                data.Triangles[t + 4] = i + Columns + 1;
                data.Triangles[t + 5] = i + Columns + 2;
            }
            // Area-weighted centroid, also used for the aerodynamic force point.
            float area = 0;
            for (int i = 0; i < data.Triangles.Length; i += 3)
            {
                var a = data.Vertices[data.Triangles[i]];
                var b = data.Vertices[data.Triangles[i + 1]];
                var c = data.Vertices[data.Triangles[i + 2]];
                float weight = Vector3.Cross(b - a, c - a).magnitude;
                data.Center += (a + b + c) * (weight / 3);
                area += weight;
            }
            data.Center /= area;
            return data;
        }

        // Unity requires influences in descending order, with indices moving
        // alongside weights. In particular, a fully pinned corner must put its
        // one nonzero influence first rather than behind three zero weights.
        private static BoneWeight SortedWeights(float a, float b, float c, float d)
        {
            var weights = new[] { a, b, c, d };
            var indices = new[] { 0, 1, 2, 3 };
            for (int i = 1; i < 4; i++)
            for (int j = i; j > 0 && weights[j] > weights[j - 1]; j--)
            {
                float weight = weights[j - 1];
                weights[j - 1] = weights[j];
                weights[j] = weight;
                int index = indices[j - 1];
                indices[j - 1] = indices[j];
                indices[j] = index;
            }
            return new BoneWeight
            {
                weight0 = weights[0],
                weight1 = weights[1],
                weight2 = weights[2],
                weight3 = weights[3],
                boneIndex0 = indices[0],
                boneIndex1 = indices[1],
                boneIndex2 = indices[2],
                boneIndex3 = indices[3],
            };
        }

        internal static Vector3 ReefCorner(Vector3 corner, float unroll)
        {
            float amount = Math.Max(0.015f, Math.Min(1f, unroll));
            return new Vector3(corner.x * amount, corner.y, corner.z);
        }
    }
}
