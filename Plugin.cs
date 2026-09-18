using BepInEx;

namespace FishermansSail
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.august.fishermanssail";
        public const string PluginName = "Fisherman's Sail";
        public const string PluginVersion = "0.1.0";

        private void Awake()
        {
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded!");
        }
    }
}
