using System;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansStaysail
{
    internal static class FishermansStaysailInstallationGeometry
    {
        internal const float HeadClearance = 0.15f;
        internal const float AftClearance = 0.15f;

        internal static float HeadSlope(Vector3 stay, Vector3 mastAxis)
        {
            float along = Vector3.Dot(stay, mastAxis.normalized);
            float span = Vector3.ProjectOnPlane(stay, mastAxis).magnitude;
            if (!Finite(along) || !Finite(span) || span < 0.25f || along < 0)
                throw new ArgumentException("Invalid rising stay frame.");
            return (float)(Math.Atan2(along, span) * 180 / Math.PI);
        }

        internal static string FitError(float width, float luff, float drop, float room, float span)
        {
            if (
                !Finite(width)
                || !Finite(luff)
                || !Finite(drop)
                || !Finite(room)
                || !Finite(span)
                || width <= 0
                || luff <= 0
                || span <= 0
            )
                return "(INVALID STAYSAIL SUPPORT GEOMETRY)";
            if (drop < -0.001f)
                return "(SAIL HEAD ABOVE FISHERMAN'S STAY)";
            if (luff > room + 0.02f)
                return "(LUFF EXCEEDS FORWARD MAST SECTION)";
            if (width > span - AftClearance)
                return "(SAIL TOO WIDE FOR MAST PAIR)";
            return null;
        }

        internal static bool CollisionStrip(
            Vector3[] corners,
            int column,
            float clearance,
            out Vector3 center,
            out Vector3 size
        )
        {
            float width = -corners[0].z;
            float step = width / FishermansStaysailGeometry.Columns;
            float start = Math.Max((column + 0.05f) * step, clearance);
            float end = (column + 0.95f) * step;
            float u = (start + end) / (2 * width);
            // Inscribe in both sloping edges instead of filling the billow envelope.
            float top = Vector3.Lerp(corners[0], corners[1], start / width).x;
            float bottom = Vector3.Lerp(corners[2], corners[3], start / width).x;
            center = new Vector3((top + bottom) * 0.5f, 0, -width * (1 - u));
            size = new Vector3(
                Math.Max(0.01f, top - bottom - 0.1f),
                0.05f,
                Math.Max(0.001f, end - start)
            );
            return end > start;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
