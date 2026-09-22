using HarmonyLib;
using UnityEngine;

namespace FishermansSail
{
    [HarmonyPatch(typeof(Sail), "UpdateWindForceOnSail")]
    internal static class FishermanWindFramePatch
    {
        [HarmonyPrefix]
        private static void Prefix(Sail __instance)
        {
            var rig = __instance.GetComponent<FishermanSailRig>();
            if (rig)
                rig.RefreshAerodynamics();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetSailForceDirection")]
    internal static class FishermanForceDirectionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref Vector3 __result)
        {
            var rig = __instance.GetComponent<FishermanSailRig>();
            if (!rig || !rig.RefreshAerodynamics())
                return true;
            __result = FishermanAerodynamics.ForceDirection(
                __instance.apparentWind,
                __instance.windcenter.up
            );
            return false;
        }
    }
}
