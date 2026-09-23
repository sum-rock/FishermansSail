using HarmonyLib;

namespace FishermansSail
{
    [HarmonyPatch(typeof(ShipyardSailColChecker), "UpdateRotation")]
    internal static class FlyingSailCollisionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            ShipyardSailColChecker __instance,
            Sail ___sail,
            int ___checkDelay
        )
        {
            var rig = ___sail ? ___sail.GetComponent<FishermanSailRig>() : null;
            if (!rig || !rig.RefreshFlyingFrame())
                return true;
            // The native checker first waits above the ship for contacts to
            // clear. After that, sweep around the same mast axis as the sail.
            if (___checkDelay <= 2)
                return !rig.PositionCollisionChecker(__instance.transform, __instance.currentAngle);
            return false;
        }
    }
}
