using HarmonyLib;
using MoreSailwindSails.Sails.FishermansStaysail.MkA;
using MoreSailwindSails.Sails.FishermansStaysail.MkB;

namespace MoreSailwindSails.Sails.FishermansStaysail.Patches
{
    [HarmonyPatch(typeof(PrefabsDirectory), "Start")]
    internal static class FishermansStaysailRegistrationPatch
    {
        // Clone after SE has configured the source components, but before
        // All Sails captures the prefab array for its cached menu pages.
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        [HarmonyBefore("NatoriusG.AllSailsAllShipyards")]
        private static void Postfix(PrefabsDirectory __instance)
        {
            FishermansStaysailMkA.Register(__instance);
            FishermansStaysailMkB.Register(__instance);
        }
    }

    [HarmonyPatch(typeof(Shipyard), "Awake")]
    internal static class FishermansStaysailShipyardPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Shipyard __instance)
        {
            FishermansStaysailMkA.AddToShipyard(__instance);
            FishermansStaysailMkB.AddToShipyard(__instance);
        }
    }

    [HarmonyPatch(typeof(Shipyard), "ActivateDocuments")]
    internal static class FishermansStaysailShipyardFallbackPatch
    {
        // Covers shipyards that awakened before PrefabsDirectory.Start.
        [HarmonyPrefix]
        private static void Prefix(Shipyard __instance)
        {
            FishermansStaysailMkA.AddToShipyard(__instance);
            FishermansStaysailMkB.AddToShipyard(__instance);
        }
    }
}
