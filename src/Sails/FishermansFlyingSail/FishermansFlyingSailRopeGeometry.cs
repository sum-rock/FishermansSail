using System;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal static class FishermansFlyingSailRopeGeometry
    {
        // All adjacent spans use this same direction at their shared attachment.
        internal static Vector3 Tangent(Vector3 before, Vector3 at, Vector3 after)
        {
            var incoming = (at - before).normalized;
            var outgoing = (after - at).normalized;
            var sum = incoming + outgoing;
            return sum.sqrMagnitude > 1e-8f ? sum.normalized : incoming;
        }

        internal static Vector3 Handle(Vector3 direction, float adjacentLength, float length) =>
            direction.normalized * (Math.Max(0, Math.Min(adjacentLength, length)) / 3);

        internal static Vector3 Point(
            Vector3 start,
            Vector3 end,
            Vector3 startHandle,
            Vector3 endHandle,
            float slack,
            float t
        )
        {
            t = Math.Max(0, Math.Min(1, t));
            float s = 1 - t;
            float length = (end - start).magnitude;
            // Bound even caller-supplied handles; a collapsed span stays a point.
            startHandle = Handle(startHandle, startHandle.magnitude * 3, length);
            endHandle = Handle(endHandle, endHandle.magnitude * 3, length);
            float sag = length * (0.01f + 0.01f * Math.Max(0, Math.Min(1, slack)));
            // Zero sag derivative at attachments preserves the cubic join tangents.
            return start * (s * s * s)
                + (start + startHandle) * (3 * s * s * t)
                + (end - endHandle) * (3 * s * t * t)
                + end * (t * t * t)
                + Vector3.down * (sag * 16 * t * t * s * s);
        }
    }
}
