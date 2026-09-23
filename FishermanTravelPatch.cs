using HarmonyLib;

namespace FishermansSail
{
    [HarmonyPatch(typeof(JibAngleMaster), "Update")]
    internal static class FishermanTravelPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Sail ___sail, out bool __state)
        {
            __state = ___sail && ___sail.GetComponent<FishermanSailRig>();
            if (!__state)
                return;
            // Native Update caches these for both sheet controllers. Saved
            // limits and later mast refreshes may still contain the old range.
            ___sail.minAngle = FishermanTravel.Clamp(___sail.minAngle);
            ___sail.maxAngle = FishermanTravel.Clamp(___sail.maxAngle);
        }

        [HarmonyPostfix]
        private static void Postfix(JibAngleMaster __instance, Sail ___sail, bool __state)
        {
            if (!__state)
                return;
            var hinge = __instance.sailHinge;
            var limits = hinge.limits;
            float min = limits.min,
                max = limits.max;
            // ApplySway runs inside native Update after the sheet limits are
            // assigned. Bound its final result, including stale sheet values,
            // without changing the sail pose or resetting Cloth.
            FishermanTravel.ConstrainHinge(ref min, ref max, ___sail.minAngle, ___sail.maxAngle);
            limits.min = min;
            limits.max = max;
            hinge.limits = limits;
        }
    }
}
