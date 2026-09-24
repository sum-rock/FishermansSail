using System;
using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail
{
    internal static class FishermansStaysailFrameGeometry
    {
        internal static Vector3 ModelOffset(Vector3 pivot, Vector3 alignedHead) =>
            pivot - alignedHead;

        internal static bool PositionChanged(Vector3 a, Vector3 b) =>
            (a - b).sqrMagnitude > 0.000004f;

        internal static Vector3 UpperHead(
            Vector3 neutralHead,
            Vector3 requestedHead,
            Vector3 foreHead,
            Vector3 mastAxis,
            float unroll
        )
        {
            var axis = mastAxis.normalized;
            var neutral = neutralHead - foreHead;
            var requested = requestedHead - foreHead;
            neutral -= axis * Vector3.Dot(neutral, axis);
            requested -= axis * Vector3.Dot(requested, axis);
            float angle = (float)(
                Math.Atan2(
                    Vector3.Dot(axis, Vector3.Cross(neutral, requested)),
                    Vector3.Dot(neutral, requested)
                )
                * 180
                / Math.PI
            );
            return RotateAroundMast(
                neutralHead,
                foreHead,
                axis,
                angle * 0.85f * FishermansStaysailBillow.Deployment(unroll)
            );
        }

        internal static Vector3 RotateAroundMast(
            Vector3 point,
            Vector3 pivot,
            Vector3 axis,
            float degrees
        )
        {
            if (axis.sqrMagnitude < 0.000001f || float.IsNaN(degrees) || float.IsInfinity(degrees))
                throw new ArgumentException("Expected a mast axis and a finite sheet angle.");
            var direction = axis.normalized;
            var relative = point - pivot;
            float radians = degrees * (float)Math.PI / 180f;
            float cosine = (float)Math.Cos(radians),
                sine = (float)Math.Sin(radians);
            return pivot
                + relative * cosine
                + Vector3.Cross(direction, relative) * sine
                + direction * Vector3.Dot(direction, relative) * (1 - cosine);
        }
    }
}
