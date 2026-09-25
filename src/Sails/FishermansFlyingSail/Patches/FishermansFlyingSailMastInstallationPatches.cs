using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ShipyardExpansion;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail.Patches
{
    [HarmonyPatch(typeof(ReefEffectAnimUniversal), "RefreshCloth")]
    internal static class FishermansFlyingSailClothRefreshPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ReefEffectAnimUniversal __instance)
        {
            var rig = __instance.GetComponent<FishermansFlyingSailRig>();
            if (!rig)
                return true;
            rig.RefreshCloth();
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardUI), "SailMastCompatible")]
    internal static class FishermansFlyingSailMastCompatiblePatch
    {
        [HarmonyPostfix]
        private static void Postfix(GameObject sailPrefab, ref bool __result)
        {
            if (__result && sailPrefab.GetComponent<FishermansFlyingSailRig>())
                __result = FishermansFlyingSailRigging.TryResolve(
                    GameState.currentShipyard.sailInstaller.GetCurrentMast(),
                    out _
                );
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "GetInstallError")]
    internal static class FishermansFlyingSailInstallErrorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail sail, Mast mast, ref bool error, ref string __result)
        {
            if (!sail.GetComponent<FishermansFlyingSailRig>())
                return true;
            string reason = FishermansFlyingSailRigging.InstallError(sail, mast);
            if (reason == null)
                return true;
            error = true;
            __result = reason;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "InstallSail")]
    internal static class FishermansFlyingSailInstallGuardPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Mast mast, Sail sail)
        {
            if (!sail.GetComponent<FishermansFlyingSailRig>())
                return true;
            string error = FishermansFlyingSailRigging.InstallError(sail, mast);
            if (error == null)
                return true;
            Plugin.Log.LogWarning("FishermansFlyingSail installation rejected: " + error);
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipyardSailInstaller), "AddNewSail")]
    internal static class FishermansFlyingSailNewSailPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(ShipyardSailInstaller __instance, GameObject sailObject)
        {
            var rig = sailObject.GetComponent<FishermansFlyingSailRig>();
            if (!rig)
                return;
            var mast = __instance.GetCurrentMast();
            var sail = rig.Sail;
            // Native AddNewSail chooses the shipyard's first palette entry.
            // Use the existing white swatch for this sail's initial selection.
            sail.ChangeSailColor(FishermansFlyingSailAppearance.WhiteColorIndex);
            // Start at 100% of the smaller base mesh.
            sail.GetComponent<SailScaler>().SetScaleAbs(1f, 1f);
            sail.ChangeInstallHeight(mast.mastHeight - sail.GetCurrentInstallHeight());
            sail.UpdateInstallPosition();
            sail.currentUnroll = 1f;
            rig.RefreshFlyingFrame();
            mast.UpdateControllerAttachments();
            ShipyardUI.instance.UpdateDescriptionText();
            GameState.currentShipyard.UpdateOrder();
        }
    }

    [HarmonyPatch(typeof(Sail), "GetScaledHeight")]
    internal static class FishermansFlyingSailMastHeightPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Sail __instance, ref float __result)
        {
            var rig = __instance.GetComponent<FishermansFlyingSailRig>();
            if (!rig || rig.Corners == null)
                return true;
            __result = -rig.Corners[2].x * __instance.cloth.transform.parent.localScale.x;
            return false;
        }
    }

    // Let native binding process only native-controlled sails. Restore the actual
    // list even on exceptions; saves, mast capacity and overlap checks see all sails.
    [HarmonyPatch(typeof(Mast), "UpdateControllerAttachments")]
    internal static class FishermansFlyingSailControlsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Mast __instance, out List<GameObject> __state)
        {
            __state = null;
            if (
                __instance.sails == null
                || !__instance.sails.Any(s => s && s.GetComponent<FishermansFlyingSailRig>())
            )
                return;
            __state = __instance.sails;
            __instance.sails = __state
                .Where(s => s && !s.GetComponent<FishermansFlyingSailRig>())
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
                if (item && item.GetComponent<FishermansFlyingSailRig>())
                    FishermansFlyingSailRigging.For(item.GetComponent<Sail>()).AttachControls();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "CanUninstall")]
    internal static class FishermansFlyingSailSupportRemovalPatch
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
                    .GetComponentsInChildren<FishermansFlyingSailRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                return;
            __result = false;
            dependentOptionNames = ": supports a Fisherman's Flying Sail; remove the sail first.";
        }
    }

    [HarmonyPatch(typeof(BoatPart), "SetOptionEnabled")]
    internal static class FishermansFlyingSailSupportPreviewPatch
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
                && boat.GetComponentsInChildren<FishermansFlyingSailRigging>(true)
                    .Any(r => r.DependsOn(option))
            )
                state = true;
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshParts")]
    internal static class FishermansFlyingSailPartsRefreshPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(BoatCustomParts __instance)
        {
            foreach (
                var rig in __instance.GetComponentsInChildren<FishermansFlyingSailRigging>(true)
            )
                rig.Invalidate();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshPartsWithOrder")]
    internal static class FishermansFlyingSailOrderRefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BoatCustomParts __instance) =>
            FishermansFlyingSailPartsRefreshPatch.Postfix(__instance);
    }
}
