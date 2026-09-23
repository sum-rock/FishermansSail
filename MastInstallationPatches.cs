using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace FishermansSail
{
    [HarmonyPatch(typeof(ReefEffectAnimUniversal), "RefreshCloth")]
    internal static class FishermanClothRefreshPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ReefEffectAnimUniversal __instance)
        {
            var rig = __instance.GetComponent<FishermanSailRig>();
            if (!rig)
                return true;
            rig.RefreshCloth();
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardUI), "SailMastCompatible")]
    internal static class FishermanMastCompatiblePatch
    {
        [HarmonyPostfix]
        private static void Postfix(GameObject sailPrefab, ref bool __result)
        {
            if (__result && sailPrefab.GetComponent<FishermanSailRig>())
                __result = FishermanRigging.TryResolve(
                    GameState.currentShipyard.sailInstaller.GetCurrentMast(),
                    out _
                );
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "GetInstallError")]
    internal static class FishermanInstallErrorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail sail, Mast mast, ref bool error, ref string __result)
        {
            if (!sail.GetComponent<FishermanSailRig>())
                return true;
            string reason = FishermanRigging.InstallError(sail, mast);
            if (reason == null)
                return true;
            error = true;
            __result = reason;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "InstallSail")]
    internal static class FishermanInstallGuardPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Mast mast, Sail sail)
        {
            if (!sail.GetComponent<FishermanSailRig>())
                return true;
            string error = FishermanRigging.InstallError(sail, mast);
            if (error == null)
                return true;
            Plugin.Log.LogWarning("Fisherman installation rejected: " + error);
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "AddNewSail")]
    internal static class FishermanNewSailPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ShipyardSailInstaller __instance, GameObject sailObject)
        {
            var rig = sailObject.GetComponent<FishermanSailRig>();
            if (!rig)
                return;
            var mast = __instance.GetCurrentMast();
            var sail = rig.Sail;
            sail.ChangeInstallHeight(mast.mastHeight - sail.GetCurrentInstallHeight());
            sail.UpdateInstallPosition();
            sail.currentUnroll = 1f;
            rig.RefreshFlyingFrame();
            mast.UpdateControllerAttachments();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetScaledHeight")]
    internal static class FishermanMastHeightPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref float __result)
        {
            var rig = __instance.GetComponent<FishermanSailRig>();
            if (!rig || rig.Corners == null)
                return true;
            __result = -rig.Corners[2].x * __instance.cloth.transform.parent.localScale.x;
            return false;
        }
    }

    // Let native binding process only native-controlled sails. Restore the actual
    // list even on exceptions; saves, mast capacity and overlap checks see all sails.
    [HarmonyPatch(typeof(Mast), "UpdateControllerAttachments")]
    internal static class FishermanControlsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Mast __instance, out List<GameObject> __state)
        {
            __state = null;
            if (
                __instance.sails == null
                || !__instance.sails.Any(s => s && s.GetComponent<FishermanSailRig>())
            )
                return;
            __state = __instance.sails;
            __instance.sails = __state
                .Where(s => s && !s.GetComponent<FishermanSailRig>())
                .ToList();
        }

        [HarmonyFinalizer]
        private static void Finalizer(
            Mast __instance,
            List<GameObject> __state,
            Exception __exception
        )
        {
            if (__state == null)
                return;
            __instance.sails = __state;
            __instance.UpdateSailOrder();
            if (__exception != null)
                return;
            foreach (var item in __state)
                if (item && item.GetComponent<FishermanSailRig>())
                    FishermanRigging.For(item.GetComponent<Sail>()).AttachControls();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "CanUninstall")]
    internal static class FishermanSupportRemovalPatch
    {
        [HarmonyPostfix]
        private static void Postfix(
            BoatCustomParts __instance,
            int partIndex,
            int optionIndex,
            ref bool __result,
            ref string dependentOptionNames
        )
        {
            var option = __instance.availableParts[partIndex].partOptions[optionIndex];
            if (
                !__instance
                    .GetComponentsInChildren<FishermanRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                return;
            __result = false;
            dependentOptionNames = ": supports a Fisherman's Sail; remove the sail first.";
        }
    }

    [HarmonyPatch(typeof(BoatPart), "SetOptionEnabled")]
    internal static class FishermanSupportPreviewPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BoatPart __instance, int i, ref bool state)
        {
            if (state)
                return;
            var option = __instance.partOptions[i];
            var boat = option.GetComponentInParent<BoatRefs>();
            if (
                boat
                && boat.GetComponentsInChildren<FishermanRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                state = true;
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshParts")]
    internal static class FishermanPartsRefreshPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(BoatCustomParts __instance)
        {
            foreach (var rig in __instance.GetComponentsInChildren<FishermanRigging>(true))
                rig.Invalidate();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshPartsWithOrder")]
    internal static class FishermanOrderRefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BoatCustomParts __instance) =>
            FishermanPartsRefreshPatch.Postfix(__instance);
    }
}
