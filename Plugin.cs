using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace FishermansSail
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.nandbrew.shipyardexpansion")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.august.fishermanssail";
        public const string PluginName = "Fisherman's Sail";
        public const string PluginVersion = "0.8.3";

        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            new Harmony(PluginGuid).PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded!");
        }
    }
}
