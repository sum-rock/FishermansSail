using HarmonyLib;

namespace FishermansSail.Sails.FishermansFlyingSail.Patches
{
    [HarmonyPatch(typeof(PrefabsDirectory), "Start")]
    internal static class FishermansFlyingSailRegistrationPatch
    {
        // Clone after SE has configured the source components, but before
        // All Sails captures the prefab array for its cached menu pages.
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        [HarmonyBefore("NatoriusG.AllSailsAllShipyards")]
        private static void Postfix(PrefabsDirectory __instance) =>
            FishermansFlyingSail.Register(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "Awake")]
    internal static class FishermansFlyingSailShipyardPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Shipyard __instance) =>
            FishermansFlyingSail.AddToShipyard(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "ActivateDocuments")]
    internal static class FishermansFlyingSailShipyardFallbackPatch
    {
        // Covers shipyards that awakened before PrefabsDirectory.Start.
        [HarmonyPrefix]
        private static void Prefix(Shipyard __instance) =>
            FishermansFlyingSail.AddToShipyard(__instance);
    }
}
