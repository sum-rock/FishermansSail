using System;

namespace MoreSailwindSails.Sails.FishermansFlyingSail
{
    internal static class FishermansFlyingSailTravel
    {
        // Degrees either side of the neutral, fore-and-aft mast frame.
        internal const int MaximumAngle = 40;

        internal static float Clamp(float angle) =>
            Math.Max(-MaximumAngle, Math.Min(MaximumAngle, angle));

        internal static void ConstrainHinge(
            ref float min,
            ref float max,
            float allowedMin,
            float allowedMax
        )
        {
            allowedMin = Clamp(allowedMin);
            allowedMax = Clamp(allowedMax);
            min = Math.Max(allowedMin, Math.Min(allowedMax, min));
            max = Math.Max(allowedMin, Math.Min(allowedMax, max));
            // Tight opposing sheets plus negative native sway can cross the
            // endpoints. Meet halfway instead of assigning an inverted joint range.
            if (min > max)
                min = max = (min + max) * 0.5f;
        }
    }
}
