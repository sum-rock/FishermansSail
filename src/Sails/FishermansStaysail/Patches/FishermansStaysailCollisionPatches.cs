using HarmonyLib;

namespace FishermansSail.Sails.FishermansStaysail.Patches
{
    [HarmonyPatch(typeof(ShipyardSailColChecker), "UpdateRotation")]
    internal static class FishermansStaysailCollisionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            ShipyardSailColChecker __instance,
            Sail ___sail,
            int ___checkDelay,
            ref UnityEngine.Quaternion ___initialRot
        )
        {
            var rig = ___sail ? ___sail.GetComponent<FishermansStaysailRig>() : null;
            if (!rig || !rig.RefreshFrame())
                return true;
            // The native checker first waits above the ship for contacts to
            // clear. After that, sweep around the same mast axis as the sail.
            if (___checkDelay <= 2)
            {
                // The final step ends the sweep. Return both position and
                // rotation to neutral before native code publishes the result.
                float angle =
                    __instance.currentAngle > __instance.startMaxAngle
                        ? 0
                        : __instance.currentAngle;
                if (!rig.PositionCollisionChecker(__instance.transform, angle, out var neutral))
                    return true;
                // FixedUpdate restores initialRot after the final sweep. Preserve
                // our aligned neutral panel instead of the donor's old axes.
                ___initialRot = neutral;
            }
            return false;
        }
    }
}
