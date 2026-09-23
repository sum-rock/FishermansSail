using System;
using UnityEngine;

namespace FishermansSail
{
    internal static class StayGeometry
    {
        // A separate index namespace from sail prefabs. Source indices remain
        // stable even when different mast options are installed or inactive.
        internal const int MountIndexOffset = 128;
        internal const int SourceIndexLimit = 128;
        internal const int MountCapacity = MountIndexOffset + SourceIndexLimit;
        internal const float AttachmentTolerance = 0.15f;

        internal static int MountIndex(int sourceIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= SourceIndexLimit)
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            return MountIndexOffset + sourceIndex;
        }

        // All inputs are in the boat frame, never world space. Mast endpoints
        // describe the physical spar, whose tip can extend beyond its sail mount.
        internal static void ForemastAttachments(
            Vector3 foreMount,
            Vector3 foreBottom,
            Vector3 foreTop,
            Vector3 aftBottom,
            Vector3 aftTop,
            out Vector3 fore,
            out Vector3 aft
        )
        {
            // Use the forward mast's upper sail mount, below the spar tip.
            // Intersect both axes so a raked aft mast still meets the stay.
            fore = AtHeight(foreBottom, foreTop, foreMount.y);
            aft = AtHeight(aftBottom, aftTop, foreMount.y);
        }

        internal static Vector3 AtHeight(Vector3 bottom, Vector3 top, float height)
        {
            if (!Finite(bottom) || !Finite(top) || !Finite(height) || top.y - bottom.y < 0.01f)
                throw new ArgumentException("Expected a finite, upward mast segment.");
            float t = (height - bottom.y) / (top.y - bottom.y);
            var point = bottom + (top - bottom) * t;
            point.y = height;
            return point;
        }

        internal static bool SupportsHeight(Vector3 bottom, Vector3 top, float height) =>
            Finite(height)
            && height >= bottom.y - AttachmentTolerance
            && height <= top.y + AttachmentTolerance;

        internal static bool AdjoiningSections(
            Vector3 bottom,
            Vector3 top,
            Vector3 otherBottom,
            Vector3 otherTop
        )
        {
            // Allow the small fore/aft offset of overlapping topmast sections.
            // A separate mast or a genuine vertical gap is not a continuation.
            float low = Math.Max(bottom.y, otherBottom.y);
            float high = Math.Min(top.y, otherTop.y);
            if (low > high + AttachmentTolerance)
                return false;
            float joinHeight = (low + high) * 0.5f;
            return HorizontalDistanceSquared(
                    AtHeight(bottom, top, joinHeight),
                    AtHeight(otherBottom, otherTop, joinHeight)
                ) <= 1f;
        }

        internal static float Span(Vector3 aft, Vector3 fore)
        {
            if (!Finite(aft) || !Finite(fore) || Math.Abs(aft.y - fore.y) > 0.001f)
                throw new ArgumentException("Stay endpoints must have the same boat-local height.");
            float span = (aft - fore).magnitude;
            if (!Finite(span) || span < 0.25f)
                throw new ArgumentException("Stay endpoints must be on separate masts.");
            return span;
        }

        internal static float HorizontalDistanceSquared(Vector3 a, Vector3 b) =>
            (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);

        internal static Vector3 FrameUp(Vector3 sourceUp, Vector3 forward)
        {
            if (!Finite(sourceUp) || !Finite(forward) || forward.sqrMagnitude < 0.001f)
                throw new ArgumentException("The stay needs a finite orientation.");
            var up = sourceUp - forward * (Vector3.Dot(sourceUp, forward) / forward.sqrMagnitude);
            if (up.sqrMagnitude < 0.001f)
                throw new ArgumentException(
                    "The source stay frame is parallel to the new stay axis."
                );
            return up.normalized;
        }

        private static bool Finite(Vector3 value) =>
            Finite(value.x) && Finite(value.y) && Finite(value.z);

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
