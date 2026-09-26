using HarmonyLib;
using ShipyardExpansion.Scripts;

namespace MoreSailwindSails.Compatibility.Patches
{
    [HarmonyPatch(typeof(SailTextureChanger), "Setup")]
    internal static class ShipyardExpansionTextureCatalogPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix() => ShipyardExpansionTextureCatalog.SeedPlain();
    }
}
