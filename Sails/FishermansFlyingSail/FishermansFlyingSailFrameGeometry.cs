using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishermansSail.Sails.FishermansFlyingSail
{
    internal readonly struct FishermansFlyingSailSheetGuideState
    {
        internal readonly Vector3 Position;
        internal readonly bool MastActive;
        internal readonly bool AttachmentActive;

        internal FishermansFlyingSailSheetGuideState(
            Vector3 position,
            bool mastActive,
            bool attachmentActive
        )
        {
            Position = position;
            MastActive = mastActive;
            AttachmentActive = attachmentActive;
        }
    }

    internal static class FishermansFlyingSailFrameGeometry
    {
        internal static Vector3 ModelOffset(Vector3 pivot, Vector3 alignedHead) =>
            pivot - alignedHead;

        internal static bool PositionChanged(Vector3 a, Vector3 b) =>
            (a - b).sqrMagnitude > 0.000004f;

        // Positions are relative to the boat. Measure height along its up axis,
        // so heel cannot select a different pulley. Keep the first equal-height guide.
        internal static int HighestGuideIndex(
            IReadOnlyList<FishermansFlyingSailSheetGuideState> guides,
            Vector3 boatUp
        )
        {
            int best = -1;
            float height = float.NegativeInfinity;
            for (int i = 0; i < guides.Count; i++)
            {
                var guide = guides[i];
                if (!guide.MastActive || !guide.AttachmentActive)
                    continue;
                float candidate = Vector3.Dot(guide.Position, boatUp);
                if (float.IsNaN(candidate) || float.IsInfinity(candidate) || candidate <= height)
                    continue;
                best = i;
                height = candidate;
            }
            return best;
        }

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
                angle * 0.85f * FishermansFlyingSailBillow.Deployment(unroll)
            );
        }

        internal static float SheetSlack(float currentLength, float totalLength)
        {
            if (
                currentLength <= 1e-8f
                || float.IsNaN(currentLength)
                || float.IsInfinity(currentLength)
                || float.IsNaN(totalLength)
                || float.IsInfinity(totalLength)
            )
                return 0;
            return Math.Max(0, Math.Min(1, 1 - totalLength / currentLength));
        }

        // Two spans meet exactly at the mast guide (t = 0.5). These are visual
        // sheets driven by native slack, not additional physical constraints.
        internal static Vector3 UpperSheetPoint(
            Vector3 head,
            Vector3 guide,
            Vector3 control,
            float slack,
            float t
        )
        {
            var from = t <= 0.5f ? head : guide;
            var to = t <= 0.5f ? guide : control;
            float along = t <= 0.5f ? t * 2 : (t - 0.5f) * 2;
            float sag = (to - from).magnitude * (0.005f + 0.08f * Math.Max(0, Math.Min(1, slack)));
            return FishermansFlyingSailBillow.SupportPoint(
                from,
                to,
                new Vector3(0, -sag, 0),
                along
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
