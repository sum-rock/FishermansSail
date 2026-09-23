using System;
using System.Collections.Generic;
using HarmonyLib;
using ShipyardExpansion.Scripts;
using UnityEngine;

namespace FishermansSail
{
    internal static class FishermanAppearance
    {
        // Existing PrefabsDirectory palette swatch, not a new RGB color.
        internal const int WhiteColorIndex = 11;

        // SE registers the unpainted stock square sail's texture first.
        internal const int PlainTextureIndex = 0;

        internal static void Configure(Sail sail)
        {
            var changer = sail.GetComponent<SailTextureChanger>();
            if (!changer || SailTextureChanger.sailTextures.Count <= PlainTextureIndex)
                throw new InvalidOperationException(
                    "Shipyard Expansion's plain sail texture is unavailable."
                );
            // Replace the clone's list, leaving the brig jib's options intact.
            changer.allowedTextures = new List<int> { PlainTextureIndex };
            changer.SetTexture(PlainTextureIndex);
            sail.ChangeSailColor(WhiteColorIndex);
            var rig = sail.GetComponent<FishermanSailRig>();
            var material = sail.cloth.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            rig.ReefedRenderer.sharedMaterial = material;
            rig.FurledColorReference.sharedMaterial = material;
        }
    }

    [HarmonyPatch(typeof(SailTextureChanger), "UpdateMaterial")]
    internal static class FishermanPlainTexturePatch
    {
        [HarmonyPrefix]
        private static void Prefix(SailTextureChanger __instance)
        {
            if (__instance.GetComponent<FishermanSailRig>())
                // Covers saved patterns, SetTexture and NextTexture using the
                // original material update and the existing plain texture.
                __instance.textureIndex = FishermanAppearance.PlainTextureIndex;
        }
    }

    [HarmonyPatch(typeof(ShipyardUI), "UpdateMoveButtons")]
    internal static class FishermanTextureButtonPatch
    {
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        private static void Postfix(GameObject ___moveUpButton)
        {
            var shipyard = GameState.currentShipyard;
            var sail = shipyard ? shipyard.sailInstaller.GetCurrentSail() : null;
            if (!sail || !sail.GetComponent<FishermanSailRig>())
                return;
            // SE restores the button for other sails on each refresh. Find the
            // component so either SE color-page layout is handled.
            foreach (
                var button in ___moveUpButton.transform.parent.GetComponentsInChildren<TextureButton>(
                    true
                )
            )
                button.gameObject.SetActive(false);
        }
    }
}
