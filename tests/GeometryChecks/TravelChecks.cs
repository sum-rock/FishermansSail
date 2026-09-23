using System;
using FishermansSail;

internal static class TravelChecks
{
    internal static void Run()
    {
        Check(
            FishermanTravel.Clamp(-89) == -40 && FishermanTravel.Clamp(89) == 40,
            "Old saved travel limits must become 40 degrees on each side."
        );
        foreach (float angle in new[] { -40f, -25f, 0f, 15f, 40f })
            Check(
                FishermanTravel.Clamp(angle) == angle,
                "Existing narrower collision limits must remain unchanged."
            );

        Range(-89, 89, -89, 89, -40, 40);
        Range(-40.5f, 40.5f, -40, 40, -40, 40);
        Range(-89, 89, -20, 30, -20, 30);
        Range(-89, 89, -89, 25, -40, 25);
        Range(-10.5f, 15.5f, -40, 40, -10.5f, 15.5f);
        // Stale sheet targets entirely beyond a new limit must collapse at
        // the boundary, not push the sail out or invert the joint limits.
        Range(60, 80, -40, 40, 40, 40);
        Range(-80, -60, -40, 40, -40, -40);
        Range(10.5f, 9.5f, -40, 40, 10, 10);

        // Native Update combines the two sheet ranges, then adds signed sway.
        // Exercise independently tightened/eased sheets on both tacks, including
        // one controller still using the previous frame's wider saved limits.
        foreach (float savedMin in new[] { -89f, -40f, -20f })
        foreach (float savedMax in new[] { 89f, 40f, 30f })
        foreach (float left in new[] { 0f, 0.25f, 0.75f, 1f })
        foreach (float right in new[] { 0f, 0.25f, 0.75f, 1f })
        foreach (float sway in new[] { -0.5f, 0f, 0.5f })
        {
            float min = Math.Max(Lerp(-1, savedMin, left), Lerp(-1, -89, right)) - sway;
            float max = Math.Min(Lerp(1, savedMax, left), Lerp(1, 89, right)) + sway;
            FishermanTravel.ConstrainHinge(ref min, ref max, savedMin, savedMax);
            Check(
                min >= -40 && max <= 40 && min <= max,
                "Sheet/sway combinations must produce an ordered range within 40 degrees."
            );
            Check(
                min >= savedMin && max <= savedMax,
                "Sway must not widen a tighter obstruction limit."
            );
        }
        Console.WriteLine(
            "PASS: 40-degree travel, old saved limits, asymmetric obstructions, independent sheets and bounded native sway."
        );
    }

    private static float Lerp(float from, float to, float t) => from + (to - from) * t;

    private static void Range(
        float min,
        float max,
        float allowedMin,
        float allowedMax,
        float expectedMin,
        float expectedMax
    )
    {
        FishermanTravel.ConstrainHinge(ref min, ref max, allowedMin, allowedMax);
        Check(
            min == expectedMin && max == expectedMax,
            $"Unexpected constrained hinge range: [{min}, {max}]."
        );
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
