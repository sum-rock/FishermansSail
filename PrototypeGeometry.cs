using System;
using UnityEngine;

namespace FishermansSail
{
    internal static class PrototypeGeometry
    {
        // The brig jib lies in local X/Z. Bow its free edges outward, while
        // retaining the luff, corner positions, and the original bounding box.
        internal static Vector3[] Deform(Vector3[] source, ClothSkinningCoefficient[] constraints)
        {
            if (
                source == null
                || constraints == null
                || source.Length < 3
                || source.Length != constraints.Length
            )
                throw new ArgumentException("The cloth must have one constraint per mesh vertex.");

            float minX = source[0].x,
                maxX = minX;
            float minZ = source[0].z,
                maxZ = minZ;
            foreach (var vertex in source)
            {
                if (!IsFinite(vertex.x) || !IsFinite(vertex.y) || !IsFinite(vertex.z))
                    throw new ArgumentException("The source mesh has a non-finite vertex.");
                minX = Math.Min(minX, vertex.x);
                maxX = Math.Max(maxX, vertex.x);
                minZ = Math.Min(minZ, vertex.z);
                maxZ = Math.Max(maxZ, vertex.z);
            }

            float width = maxX - minX;
            float height = maxZ - minZ;
            if (width < 0.001f || height < 0.001f)
                throw new ArgumentException(
                    "Expected a brig jib mesh spanning the local X/Z plane."
                );

            var result = (Vector3[])source.Clone();
            for (int i = 0; i < result.Length; i++)
            {
                if (constraints[i].maxDistance <= 0f)
                    continue;

                float u = (source[i].x - minX) / width;
                float v = (source[i].z - minZ) / height;
                // Zero displacement at all four bounds also preserves the clew
                // and its rope attachment. No vertices or bone weights are added.
                if (u <= 0f || u >= 1f || v <= 0f || v >= 1f)
                    continue;
                result[i].x += 0.20f * width * (4f * u * (1f - u)) * (float)Math.Sin(Math.PI * v);
            }
            return result;
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
