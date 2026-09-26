using ShipyardExpansion.Scripts;
using UnityEngine;

namespace MoreSailwindSails.Compatibility
{
    internal static class ShipyardExpansionTextureCatalog
    {
        private const string PlainTextureName = "ParticleCloudWhite";

        internal static bool HasPlainFirst =>
            SailTextureChanger.sailTextures.Count > 0
            && SailTextureChanger.sailTextures[0]
            && SailTextureChanger.sailTextures[0].name == PlainTextureName;

        internal static void SeedPlain()
        {
            if (SailTextureChanger.sailTextures.Count != 0)
                return;
            var directory = PrefabsDirectory.instance;
            // The brig jib's cloth is painted, but its native furled bundle uses
            // the plain texture. Read the asset without instantiating a material.
            var source = directory && directory.sails.Length > 110 ? directory.sails[110] : null;
            var sail = source ? source.GetComponent<Sail>() : null;
            var reef = source ? source.GetComponent<ReefEffectAnimUniversal>() : null;
            var material = reef && reef.furledSail ? reef.furledSail.sharedMaterial : null;
            var texture =
                material && material.HasProperty("_MainTex")
                    ? material.GetTexture("_MainTex")
                    : null;
            if (!sail || sail.prefabIndex != 110 || !texture || texture.name != PlainTextureName)
            {
                Plugin.Log.LogError(
                    "Could not seed SE's plain sail texture: native brig jib bundle changed."
                );
                return;
            }
            TextureCatalogOrder.SeedPlain(SailTextureChanger.sailTextures, texture);
            Plugin.Log.LogInfo(
                "Seeded SE sail texture 0 with native ParticleCloudWhite before texture discovery."
            );
        }
    }
}
