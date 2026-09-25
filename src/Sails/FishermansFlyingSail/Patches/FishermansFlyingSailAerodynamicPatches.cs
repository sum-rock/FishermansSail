using HarmonyLib;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail.Patches
{
    [HarmonyPatch(typeof(Sail), "UpdateWindForceOnSail")]
    internal static class FishermansFlyingSailWindFramePatch
    {
        [HarmonyPrefix]
        private static void Prefix(Sail __instance)
        {
            var rig = __instance.GetComponent<FishermansFlyingSailRig>();
            if (rig)
                rig.RefreshAerodynamics();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetSailForceDirection")]
    internal static class FishermansFlyingSailForceDirectionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref Vector3 __result)
        {
            var rig = __instance.GetComponent<FishermansFlyingSailRig>();
            if (!rig || !rig.RefreshAerodynamics())
                return true;
            __result = FishermansFlyingSailAerodynamics.ForceDirection(
                __instance.apparentWind,
                __instance.windcenter.up
            );
            return false;
        }
    }
}
