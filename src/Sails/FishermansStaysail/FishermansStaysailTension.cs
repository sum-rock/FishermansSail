using System;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansStaysail
{
    internal static class FishermansStaysailTension
    {
        // The clew lies on the intersection of two spheres: distance from the
        // tack preserves foot tension, distance from the head budgets leech
        // length. Select the point nearest the native sheet-requested pose.
        internal static bool Fit(
            Vector3 requested,
            Vector3 head,
            Vector3 tack,
            Vector3 bow,
            float leechLength,
            float footLength,
            float deployment,
            Vector3[] points
        )
        {
            float distance = (tack - head).magnitude;
            float reserve = 1 - 0.01f * Math.Max(0, Math.Min(1, deployment));
            float footRadius = footLength * reserve;
            float arcLength = leechLength * reserve;
            if (distance <= 1e-8f || arcLength <= 1e-8f || footRadius <= 1e-8f)
            {
                Fill(head, head, Vector3.zero, points);
                return false;
            }
            var axis = (tack - head) / distance;
            float low = Math.Abs(distance - footRadius);
            float high = Math.Min(arcLength, distance + footRadius);
            if (low > high)
            {
                // Impossible installation geometry: retain the fixed head and
                // a finite leech. Caller can report this without breaking load.
                Fill(head + axis * Math.Min(arcLength, distance), head, Vector3.zero, points);
                return false;
            }
            var radial = requested - head;
            radial -= axis * Vector3.Dot(radial, axis);
            if (radial.sqrMagnitude < distance * distance * 1e-12f)
            {
                radial = Vector3.Cross(axis, Vector3.up);
                if (radial.sqrMagnitude < 1e-8f)
                    radial = Vector3.Cross(axis, Vector3.forward);
            }
            radial = radial.normalized;
            var nearest = OnCircle(head, axis, radial, distance, low, footRadius);
            // Curve shape yields to available cloth, never foot tension. This
            // reduction is normally unnecessary but handles extreme bow inputs.
            for (int i = 0; i < 16 && Length(nearest, head, bow) > arcLength; i++)
                bow *= 0.5f;
            if (Length(nearest, head, bow) > arcLength)
                bow = Vector3.zero;
            for (int i = 0; i < 24; i++)
            {
                float radius = (low + high) * 0.5f;
                var candidate = OnCircle(head, axis, radial, distance, radius, footRadius);
                if (Length(candidate, head, bow) > arcLength)
                    high = radius;
                else
                    low = radius;
            }
            var clew = OnCircle(head, axis, radial, distance, low, footRadius);
            Fill(clew, head, bow, points);
            return true;
        }

        private static Vector3 OnCircle(
            Vector3 head,
            Vector3 axis,
            Vector3 radial,
            float distance,
            float leechRadius,
            float footRadius
        )
        {
            double along =
                (
                    (double)leechRadius * leechRadius
                    - (double)footRadius * footRadius
                    + (double)distance * distance
                ) / (2 * distance);
            float radius = (float)
                Math.Sqrt(Math.Max(0, (double)leechRadius * leechRadius - along * along));
            return head + axis * (float)along + radial * radius;
        }

        private static float Length(Vector3 clew, Vector3 head, Vector3 bow)
        {
            float length = 0;
            var previous = clew;
            for (int i = 1; i <= 64; i++)
            {
                var point = FishermansStaysailBillow.SupportPoint(clew, head, bow, i / 64f);
                length += (point - previous).magnitude;
                previous = point;
            }
            return length;
        }

        private static void Fill(Vector3 clew, Vector3 head, Vector3 bow, Vector3[] points)
        {
            float length = Length(clew, head, bow);
            points[0] = clew;
            var previous = clew;
            float travelled = 0;
            int next = 1;
            for (int step = 1; step <= 64 && next < points.Length - 1; step++)
            {
                var point = FishermansStaysailBillow.SupportPoint(clew, head, bow, step / 64f);
                float segment = (point - previous).magnitude;
                while (
                    next < points.Length - 1
                    && length * next / (points.Length - 1) <= travelled + segment
                )
                {
                    float fraction =
                        segment > 1e-8f
                            ? (length * next / (points.Length - 1) - travelled) / segment
                            : 0;
                    points[next++] = FishermansStaysailBillow.SupportPoint(
                        clew,
                        head,
                        bow,
                        (step - 1 + fraction) / 64
                    );
                }
                travelled += segment;
                previous = point;
            }
            while (next < points.Length)
                points[next++] = head;
        }
    }
}
