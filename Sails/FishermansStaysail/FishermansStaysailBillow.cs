using System;
using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail
{
    internal static class FishermansStaysailBillow
    {
        internal static float ClothTravel(float width, float u, float v)
        {
            float taper = ClewTaper(u, v);
            float freeTravel =
                width
                * taper
                * (
                    (0.08f * (1 - v) + 0.13f * v) * (float)Math.Sin(Math.PI * u)
                    + 0.015f * u
                    + 0.045f * u * (float)Math.Sin(Math.PI * v)
                    + 0.04f * (float)Math.Pow(1 - u, 4) * (float)Math.Sin(Math.PI * v)
                );
            // The old flying-sail allowances exceeded this sail's shallow belly
            // depth, permitting folds across the intended billow surface. Bound
            // the interior relative to its own camber on either tack; retain
            // free motion near the foot/leech and the existing clew taper.
            float edgeFreedom = width * 0.015f * (float)(Math.Pow(u, 8) + Math.Pow(v, 8));
            float shapedTravel =
                taper * (0.6f * FishermansStaysailGeometry.RestCamber(width, u, v) + edgeFreedom);
            return Math.Min(freeTravel, shapedTravel);
        }

        internal static float ClewTaper(float u, float v)
        {
            float du = (1 - u) / 0.2f,
                dv = (1 - v) / 0.2f;
            float distance = Math.Min(1, (float)Math.Sqrt(du * du + dv * dv));
            return distance * distance * (3 - 2 * distance);
        }

        internal static float SmoothLoad(float previous, float target, float seconds)
        {
            if (float.IsNaN(target) || float.IsInfinity(target))
                target = 0;
            target = Math.Max(-1, Math.Min(1, target));
            float blend = 1 - (float)Math.Exp(-3 * Math.Max(0, Math.Min(0.1f, seconds)));
            return previous + (target - previous) * blend;
        }

        internal static float Deployment(float unroll) =>
            Math.Max(0, Math.Min(1, (unroll - 0.75f) / 0.23f));

        internal static int CamberSide(int previous, float normalFlow) =>
            normalFlow > 0.6f ? 1
            : normalFlow < -0.6f ? -1
            : previous;

        // Positive normal matches +Y of the neutral mesh. Only positions move;
        // the mesh, bind poses, bone rotations and scales remain initialized.
        internal static Vector3 CamberNormal(
            Vector3 foreHead,
            Vector3 tack,
            Vector3 head,
            Vector3 clew
        )
        {
            var normal = Vector3.Cross((head + clew - foreHead - tack) * 0.5f, foreHead - tack);
            return normal.sqrMagnitude > 1e-8f ? normal.normalized : Vector3.up;
        }

        internal static Vector3 ShapePoint(
            Vector3 fore,
            Vector3 aft,
            Vector3 normal,
            float width,
            float u,
            float v,
            float camber
        ) =>
            Vector3.Lerp(fore, aft, u)
            + normal * (FishermansStaysailGeometry.RestCamber(width, u, v) * camber);

        internal static Vector3 SupportPoint(Vector3 clew, Vector3 head, Vector3 bow, float t) =>
            clew + (head - clew) * t + bow * (4 * t * (1 - t));

        internal static Vector3 SupportBow(
            Vector3 clew,
            Vector3 head,
            Vector3 normal,
            Vector3 down,
            float width,
            float load
        )
        {
            float pressure = Math.Min(1, Math.Abs(load));
            // A modest outward arch, bounded by the clew's actual off-center
            // displacement. Its lateral derivative always points back inboard.
            return normal * (Vector3.Dot(clew - head, normal) * 0.12f * pressure)
                + down * (width * 0.02f * pressure);
        }
    }
}
