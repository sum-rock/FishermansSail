using System;
using UnityEngine;

namespace FishermansSail
{
    internal static class MastInstallationGeometry
    {
        internal static string FitError(
            float width,
            float headHeight,
            float guideHeight,
            float span
        )
        {
            if (
                !Finite(width)
                || !Finite(headHeight)
                || !Finite(guideHeight)
                || !Finite(span)
                || width <= 0
                || span <= 0
            )
                return "(INVALID MAST SUPPORT GEOMETRY)";
            if (headHeight > guideHeight + 0.05f)
                return "(SAIL HEAD ABOVE SUPPORT PULLEY)";
            if (width > span - 0.15f)
                return "(SAIL TOO WIDE FOR MAST PAIR)";
            return null;
        }

        // Check the neutral sheet, not a solid volume filled to maximum billow
        // on both tacks. Clip only the intentional luff contact inside its mast.
        internal static bool CollisionStrip(
            float width,
            int column,
            float mastClearance,
            out Vector3 center,
            out Vector3 size
        )
        {
            float step = width / PrototypeGeometry.Columns;
            float start = Math.Max((column + 0.05f) * step, mastClearance);
            float end = (column + 0.95f) * step;
            float depth =
                width
                * (
                    PrototypeGeometry.ForeDepthRatio
                    + (PrototypeGeometry.AftDepthRatio - PrototypeGeometry.ForeDepthRatio)
                        * ((column + 1f) / PrototypeGeometry.Columns)
                );
            center = new Vector3(-depth * 0.5f, 0, -width + (start + end) * 0.5f);
            size = new Vector3(Math.Max(0.01f, depth - 0.1f), 0.05f, Math.Max(0.001f, end - start));
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
