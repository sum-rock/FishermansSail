using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ShipyardExpansion;
using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail.Patches
{
    [HarmonyPatch(typeof(ReefEffectAnimUniversal), "RefreshCloth")]
    internal static class FishermansStaysailClothRefreshPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ReefEffectAnimUniversal __instance)
        {
            var rig = __instance.GetComponent<FishermansStaysailRig>();
            if (!rig)
                return true;
            rig.RefreshCloth();
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardUI), "SailMastCompatible")]
    internal static class FishermansStaysailMastCompatiblePatch
    {
        [HarmonyPostfix]
        private static void Postfix(GameObject sailPrefab, ref bool __result)
        {
            if (__result && sailPrefab.GetComponent<FishermansStaysailRig>())
                __result = FishermansStaysailRigging.TryResolve(
                    GameState.currentShipyard.sailInstaller.GetCurrentMast(),
                    out _
                );
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "GetInstallError")]
    internal static class FishermansStaysailInstallErrorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail sail, Mast mast, ref bool error, ref string __result)
        {
            if (!sail.GetComponent<FishermansStaysailRig>())
                return true;
            string reason = FishermansStaysailRigging.InstallError(sail, mast);
            if (reason == null)
                return true;
            error = true;
            __result = reason;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "InstallSail")]
    internal static class FishermansStaysailInstallGuardPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Mast mast, Sail sail)
        {
            if (!sail.GetComponent<FishermansStaysailRig>())
                return true;
            string error = FishermansStaysailRigging.InstallError(sail, mast);
            if (error == null)
                return true;
            Plugin.Log.LogWarning("FishermansStaysail installation rejected: " + error);
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "AddNewSail")]
    internal static class FishermansStaysailNewSailPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(ShipyardSailInstaller __instance, GameObject sailObject)
        {
            var rig = sailObject.GetComponent<FishermansStaysailRig>();
            if (!rig)
                return;
            var mast = __instance.GetCurrentMast();
            var sail = rig.Sail;
            // Native AddNewSail chooses the shipyard's first palette entry.
            // Use the existing white swatch for this sail's initial selection.
            sail.ChangeSailColor(FishermansStaysailAppearance.WhiteColorIndex);
            // Only new shipyard selections get the smaller initial size.
            // Loaded sails continue to restore their saved SE dimensions.
            sail.GetComponent<SailScaler>().SetScaleAbs(0.5f, 0.5f);
            sail.ChangeInstallHeight(mast.mastHeight - sail.GetCurrentInstallHeight());
            sail.UpdateInstallPosition();
            sail.currentUnroll = 1f;
            rig.RefreshFrame();
            mast.UpdateControllerAttachments();
            ShipyardUI.instance.UpdateDescriptionText();
            GameState.currentShipyard.UpdateOrder();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetScaledHeight")]
    internal static class FishermansStaysailMastHeightPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref float __result)
        {
            var rig = __instance.GetComponent<FishermansStaysailRig>();
            if (!rig || rig.Corners == null)
                return true;
            __result = -rig.Corners[2].x * __instance.cloth.transform.parent.localScale.x;
            return false;
        }
    }

    // Let native binding process only native-controlled sails. Restore the actual
    // list even on exceptions; saves, mast capacity and overlap checks see all sails.
    [HarmonyPatch(typeof(Mast), "UpdateControllerAttachments")]
    internal static class FishermansStaysailControlsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Mast __instance, out List<GameObject> __state)
        {
            __state = null;
            if (
                __instance.sails == null
                || !__instance.sails.Any(s => s && s.GetComponent<FishermansStaysailRig>())
            )
                return;
            __state = __instance.sails;
            __instance.sails = __state
                .Where(s => s && !s.GetComponent<FishermansStaysailRig>())
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
                if (item && item.GetComponent<FishermansStaysailRig>())
                    FishermansStaysailRigging.For(item.GetComponent<Sail>()).AttachControls();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "CanUninstall")]
    internal static class FishermansStaysailSupportRemovalPatch
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
                    .GetComponentsInChildren<FishermansStaysailRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                return;
            __result = false;
            dependentOptionNames = ": supports a Fisherman's Staysail; remove the sail first.";
        }
    }

    [HarmonyPatch(typeof(BoatPart), "SetOptionEnabled")]
    internal static class FishermansStaysailSupportPreviewPatch
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
                && boat.GetComponentsInChildren<FishermansStaysailRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                state = true;
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshParts")]
    internal static class FishermansStaysailPartsRefreshPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(BoatCustomParts __instance)
        {
            foreach (var rig in __instance.GetComponentsInChildren<FishermansStaysailRigging>(true))
                rig.Invalidate();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshPartsWithOrder")]
    internal static class FishermansStaysailOrderRefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BoatCustomParts __instance) =>
            FishermansStaysailPartsRefreshPatch.Postfix(__instance);
    }
}
