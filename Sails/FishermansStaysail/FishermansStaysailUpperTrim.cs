using System;
using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail
{
    internal static class FishermansStaysailUpperTrim
    {
        // Travel along the head-span sphere toward the pulley. A straight pull
        // could stretch the head; this arc preserves its existing chord length.
        internal static Vector3 Target(Vector3 head, Vector3 foreHead, Vector3 pulley, float travel)
        {
            var radial = head - foreHead;
            float radius = radial.magnitude;
            if (radius < 1e-6f || travel <= 0 || !Finite(travel))
                return head;
            radial /= radius;
            var guide = pulley - foreHead;
            float along = Vector3.Dot(guide, radial);
            var tangent = guide - radial * along;
            float across = tangent.magnitude;
            if (!Finite(radius) || !Finite(along) || !Finite(across) || across < radius * 1e-6f)
                return head;
            float angle = Math.Min(travel / radius, (float)Math.Atan2(across, along));
            return foreHead
                + radius
                    * (radial * (float)Math.Cos(angle) + tangent / across * (float)Math.Sin(angle));
        }

        internal static bool Fit(
            Vector3 baselineHead,
            Vector3 foreHead,
            Vector3 pulley,
            Vector3 clew,
            Vector3 tack,
            Vector3 normal,
            Vector3 down,
            float width,
            float load,
            float unroll,
            float trimFraction,
            float leechLength,
            float footLength,
            Vector3[] points,
            out Vector3 head
        )
        {
            float deployment = FishermansStaysailBillow.Deployment(unroll);
            float travel =
                width * Math.Max(0, trimFraction) * Math.Min(1, Math.Abs(load)) * deployment;
            head = Target(baselineHead, foreHead, pulley, travel);
            if (TryFit(head))
                return true;

            // Keep the original finite failure behavior for an invalid fit.
            // When only the trim is infeasible, keep as much as the edges allow.
            head = baselineHead;
            if (!TryFit(head))
                return false;
            float low = 0,
                high = 1;
            for (int i = 0; i < 16; i++)
            {
                float amount = (low + high) * 0.5f;
                var candidate = Target(baselineHead, foreHead, pulley, travel * amount);
                if (TryFit(candidate))
                    low = amount;
                else
                    high = amount;
            }
            head = Target(baselineHead, foreHead, pulley, travel * low);
            return TryFit(head);

            bool TryFit(Vector3 candidate)
            {
                var bow = FishermansStaysailBillow.SupportBow(
                    clew,
                    candidate,
                    normal,
                    down,
                    width,
                    load
                );
                return FishermansStaysailTension.Fit(
                    clew,
                    candidate,
                    tack,
                    bow * deployment,
                    leechLength,
                    footLength,
                    deployment,
                    points
                );
            }
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
