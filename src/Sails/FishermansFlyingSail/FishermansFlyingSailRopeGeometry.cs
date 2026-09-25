using System;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal static class FishermansFlyingSailRopeGeometry
    {
        // Guides and knots terminate a span: they do not impose a tangent on
        // the next span. Gravity supplies the only bow in these external ropes.
        internal static Vector3 DirectPoint(Vector3 start, Vector3 end, float slack, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            float sag =
                (end - start).magnitude * (0.005f + 0.02f * Math.Max(0, Math.Min(1, slack)));
            return Vector3.Lerp(start, end, t) + Vector3.down * (sag * 4 * t * (1 - t));
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
