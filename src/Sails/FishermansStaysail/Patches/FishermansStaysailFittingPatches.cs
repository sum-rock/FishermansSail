using HarmonyLib;
using ShipyardExpansion;
using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail.Patches
{
    [HarmonyPatch(typeof(ShipyardSailInstaller), "MastNotTallEnough")]
    internal static class FishermansStaysailFitPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Mast mast, Sail sail, ref bool __result)
        {
            if (!sail.GetComponent<FishermansStaysailRig>())
                return true;
            __result = FishermansStaysailRigging.InstallError(sail, mast) != null;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "MoveHeldSail")]
    internal static class FishermansStaysailMovePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ShipyardSailInstaller __instance, float distance)
        {
            var sail = __instance.GetCurrentSail();
            var rig = sail ? sail.GetComponent<FishermansStaysailRig>() : null;
            if (!rig)
                return true;
            if (sail.IsInstalled() || !rig.RefreshFrame())
                return false;
            var binding = FishermansStaysailRigging.For(sail);
            var mount = __instance.GetCurrentMast();
            float room =
                Vector3.Dot(binding.ForePoint - binding.SectionBottom, binding.ForeAxis)
                - FishermansStaysailInstallationGeometry.HeadClearance;
            float maxDrop = Mathf.Max(0, room - sail.GetScaledHeight());
            float height = Mathf.Clamp(
                sail.GetCurrentInstallHeight() + distance,
                mount.mastHeight - maxDrop,
                mount.mastHeight
            );
            sail.ChangeInstallHeight(height - sail.GetCurrentInstallHeight());
            AccessTools
                .Method(typeof(ShipyardSailInstaller), "ApplySailPosition")
                .Invoke(__instance, null);
            return false;
        }
    }

    [HarmonyPatch(typeof(SailScaler), "Awake")]
    internal static class FishermansStaysailScalerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(SailScaler __instance)
        {
            if (!__instance.GetComponent<FishermansStaysailRig>())
                return;
            __instance.scaleType = ScaleType.Uniform;
            __instance.rotatablePart = null;
            __instance.flippable = false;
        }
    }
}
