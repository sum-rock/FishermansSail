using System;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal static class FishermansFlyingSailMastInstallationGeometry
    {
        internal static Vector3 AtHeight(Vector3 bottom, Vector3 top, float height)
        {
            if (
                !Finite(bottom.x)
                || !Finite(bottom.y)
                || !Finite(bottom.z)
                || !Finite(top.x)
                || !Finite(top.y)
                || !Finite(top.z)
                || !Finite(height)
                || top.y - bottom.y < 0.01f
            )
                throw new ArgumentException("Expected a finite, upward mast segment.");
            float t = (height - bottom.y) / (top.y - bottom.y);
            var point = bottom + (top - bottom) * t;
            point.y = height;
            return point;
        }

        internal static string FitError(
            float reach,
            float foreHeadHeight,
            float aftHeadHeight,
            float foreGuideHeight,
            float aftGuideHeight,
            float span
        )
        {
            if (
                !Finite(reach)
                || !Finite(foreHeadHeight)
                || !Finite(aftHeadHeight)
                || !Finite(foreGuideHeight)
                || !Finite(aftGuideHeight)
                || !Finite(span)
                || reach <= 0
                || span <= 0
            )
                return "(INVALID MAST SUPPORT GEOMETRY)";
            if (foreHeadHeight > foreGuideHeight + 0.05f || aftHeadHeight > aftGuideHeight + 0.05f)
                return "(SAIL HEAD ABOVE SUPPORT PULLEY)";
            if (reach > span - 0.15f)
                return "(SAIL TOO WIDE FOR MAST PAIR)";
            return null;
        }

        // Check the neutral sheet, not a solid volume filled to maximum billow
        // on both tacks. The offset luff normally needs no mast clipping.
        internal static bool CollisionStrip(
            float width,
            int column,
            float mastClearance,
            out Vector3 center,
            out Vector3 size
        )
        {
            float step = width / FishermansFlyingSailGeometry.Columns;
            float start = Math.Max((column + 0.05f) * step, mastClearance);
            float end = (column + 0.95f) * step;
            // Both edges expand aft: use the narrow end to keep each strip
            // inscribed below the rising head and above the falling foot.
            float head = start * FishermansFlyingSailGeometry.EdgeSlope;
            float foot = -width * FishermansFlyingSailGeometry.ForeDepthRatio - head;
            center = new Vector3((head + foot) * 0.5f, 0, -width + (start + end) * 0.5f);
            size = new Vector3(
                Math.Max(0.01f, head - foot - 0.1f),
                0.05f,
                Math.Max(0.001f, end - start)
            );
            return end > start;
        }

        internal static float HoistAmount(float unroll) =>
            Math.Max(0, Math.Min(1, (unroll - 0.02f) / 0.96f));

        internal static float HoistScale(float unroll) => 0.015f + 0.985f * HoistAmount(unroll);

        internal static Vector3 HoistCorner(
            Vector3 corner,
            Vector3 foreHead,
            Vector3 deck,
            float unroll
        )
        {
            var gathered = deck + (corner - foreHead) * 0.015f;
            return Vector3.Lerp(gathered, corner, HoistAmount(unroll));
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
