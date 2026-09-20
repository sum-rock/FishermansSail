using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishermansSail
{
    [HarmonyPatch(typeof(SaveableBoatCustomization), "Awake")]
    internal static class RegisterFishermanStaysPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(SaveableBoatCustomization __instance) =>
            FishermanStay.Register(__instance);
    }

    [HarmonyPatch(typeof(SaveBoatCustomizationData), MethodType.Constructor)]
    internal static class StaySaveCapacityPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(SaveBoatCustomizationData __instance)
        {
            if (__instance.masts.Length < StayGeometry.MountCapacity)
                Array.Resize(ref __instance.masts, StayGeometry.MountCapacity);
        }
    }

    [HarmonyPatch(typeof(SaveableBoatCustomization), "LoadData")]
    internal static class StayLoadPatch
    {
        [HarmonyPrefix]
        [HarmonyBefore("com.nandbrew.shipyardexpansion")]
        private static void Prefix(
            SaveableBoatCustomization __instance,
            SaveBoatCustomizationData data
        )
        {
            var registry = __instance.GetComponent<FishermanStayRegistry>();
            if (!registry || data == null)
                return;
            var refs = __instance.GetComponent<BoatRefs>();
            // LoadData removes existing sails using this array's length, including
            // when a shipyard cancellation restores an older customization snapshot.
            if (data.masts.Length < refs.masts.Length)
                Array.Resize(ref data.masts, refs.masts.Length);
            var parts = __instance.GetComponent<BoatCustomParts>();
            foreach (var part in registry.Parts)
            {
                int index = parts.availableParts.IndexOf(part);
                if (data.partActiveOptions == null || index >= data.partActiveOptions.Count)
                    part.activeOption = 0;
            }
            foreach (var stay in registry.Stays)
                stay.Refresh();
        }
    }

    [HarmonyPatch(typeof(ShipyardUI), "Awake")]
    internal static class StayMountButtonsPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(ref GameObject[] ___mastButtons)
        {
            // Shipyard Expansion supplies the first 128 buttons. Add the same
            // native buttons for the reserved fisherman mount range.
            if (___mastButtons == null || ___mastButtons.Length == 0 || !___mastButtons[0])
                return;
            int start = ___mastButtons.Length;
            if (start >= StayGeometry.MountCapacity)
                return;
            Array.Resize(ref ___mastButtons, StayGeometry.MountCapacity);
            for (int i = start; i < ___mastButtons.Length; i++)
            {
                var button = Object.Instantiate(
                    ___mastButtons[0],
                    ___mastButtons[0].transform.parent,
                    false
                );
                button.name = $"FishermansStay mount button {i}";
                button.GetComponent<ShipyardButton>().index = i;
                button.SetActive(false);
                ___mastButtons[i] = button;
            }
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshParts")]
    internal static class StayRefreshPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(BoatCustomParts __instance)
        {
            var registry = __instance.GetComponent<FishermanStayRegistry>();
            if (!registry)
                return;
            foreach (var stay in registry.Stays)
                stay.Refresh();
        }
    }

    [HarmonyPatch(typeof(BoatCustomParts), "RefreshPartsWithOrder")]
    internal static class StayOrderRefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(BoatCustomParts __instance) =>
            StayRefreshPatch.Postfix(__instance);
    }

    [HarmonyPatch(typeof(BoatCustomParts), "CanInstall")]
    internal static class StayCanInstallPatch
    {
        [HarmonyPostfix]
        private static void Postfix(
            BoatCustomParts __instance,
            int partIndex,
            int optionIndex,
            ref bool __result,
            ref string requiredOptionNames
        )
        {
            var registry = __instance.GetComponent<FishermanStayRegistry>();
            if (!registry)
                return;
            var option = __instance.availableParts[partIndex].partOptions[optionIndex];
            var stay = registry.Stays.FirstOrDefault(s => s.Option == option);
            if (stay == null)
                return;
            stay.Refresh();
            if (!stay.Fits)
            {
                __result = false;
                requiredOptionNames = stay.UnavailableReason;
            }
        }
    }
}
