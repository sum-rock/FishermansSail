using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MoreSailwindSails
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.nandbrew.shipyardexpansion")]
    [BepInDependency("pr0skynesis.sailinfo", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.august.moresailwindsails";
        public const string PluginName = "MoreSailwindSails";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(Plugin).Assembly);
            Sails.FishermansStaysail.Patches.FishermansStaysailSailInfoPatch.Install(harmony);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded!");
        }
    }
}
