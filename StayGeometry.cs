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

        internal static bool IsUpperStay(string name)
        {
            var words = (name ?? "")
                .ToLowerInvariant()
                .Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return Array.IndexOf(words, "top") >= 0 || Array.IndexOf(words, "upper") >= 0;
        }

        // All inputs are in the boat frame, never world space. Mast endpoints
        // describe the physical spar, whose tip can extend beyond its sail mount.
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
