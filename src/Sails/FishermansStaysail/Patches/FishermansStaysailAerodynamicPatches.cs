using HarmonyLib;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansStaysail.Patches
{
    [HarmonyPatch(typeof(Sail), "UpdateWindForceOnSail")]
    internal static class FishermansStaysailWindFramePatch
    {
        [HarmonyPrefix]
        private static void Prefix(Sail __instance)
        {
            var rig = __instance.GetComponent<FishermansStaysailRig>();
            if (rig)
                rig.RefreshAerodynamics();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetSailForceDirection")]
    internal static class FishermansStaysailForceDirectionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref Vector3 __result)
        {
            var rig = __instance.GetComponent<FishermansStaysailRig>();
            if (!rig || !rig.RefreshAerodynamics())
                return true;
            __result = FishermansStaysailAerodynamics.ForceDirection(
                __instance.apparentWind,
                __instance.windcenter.up
            );
            return false;
        }
    }
}
