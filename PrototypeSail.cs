using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishermansSail
{
    internal static class PrototypeSail
    {
        internal const int SourceIndex = 110;

        // Stable across launches: Sailwind stores this index in boat saves.
        internal const int PrototypeIndex = 400;
        internal const string DisplayName = "Fisherman's Sail Prototype";
        private static GameObject prefab;

        internal static void Register(PrefabsDirectory directory)
        {
            GameObject container = null;
            Mesh mesh = null;
            try
            {
                if (
                    prefab
                    && directory.sails.Length > PrototypeIndex
                    && directory.sails[PrototypeIndex] == prefab
                )
                    return;
                prefab = null;
                if (directory.sails.Length > PrototypeIndex && directory.sails[PrototypeIndex])
                    throw new InvalidOperationException(
                        $"Sail index {PrototypeIndex} is already occupied; no sail was replaced."
                    );

                var source =
                    directory.sails.Length > SourceIndex ? directory.sails[SourceIndex] : null;
                var sourceSail = source ? source.GetComponent<Sail>() : null;
                if (
                    !sourceSail
                    || sourceSail.prefabIndex != SourceIndex
                    || sourceSail.category != SailCategory.staysail
                    || sourceSail.sailName != "brig jib"
                )
                    throw new InvalidOperationException(
                        $"Expected the brig jib staysail at index {SourceIndex}."
                    );
                var sourceRenderer = sourceSail.cloth
                    ? sourceSail.cloth.GetComponent<SkinnedMeshRenderer>()
                    : null;
                if (
                    !sourceRenderer
                    || !sourceRenderer.sharedMesh
                    || !sourceRenderer.sharedMesh.isReadable
                )
                    throw new InvalidOperationException(
                        "The brig jib has no readable skinned cloth mesh."
                    );

                var sourceMesh = sourceRenderer.sharedMesh;
                var originalVertices = sourceMesh.vertices;
                var constraints = sourceSail.cloth.coefficients;
                var deformed = PrototypeGeometry.Deform(originalVertices, constraints);
                int changed = 0,
                    pinned = 0;
                for (int i = 0; i < originalVertices.Length; i++)
                {
                    if (deformed[i].x != originalVertices[i].x)
                        changed++;
                    if (constraints[i].maxDistance <= 0f)
                        pinned++;
                }
                if (changed == 0)
                    throw new InvalidOperationException(
                        "The prototype deformation did not move any cloth vertices."
                    );

                // An inactive parent prevents Awake/Start from running on our
                // template. Installed copies retain activeSelf=true and initialize normally.
                container = new GameObject("FishermansSail Prefabs");
                container.SetActive(false);
                container.transform.SetParent(directory.transform, false);
                var clone = Object.Instantiate(source, container.transform, false);
                clone.name = $"{PrototypeIndex} SAIL Fishermans Prototype";
                var sail = clone.GetComponent<Sail>();
                sail.prefabIndex = PrototypeIndex;
                sail.sailName = DisplayName;
                sail.obsolete = false;

                // Instantiate(GameObject) still shares asset meshes. Clone the
                // mesh explicitly before changing any vertex data.
                mesh = Object.Instantiate(sourceMesh);
                mesh.name = "FishermansSail Prototype Cloth";
                mesh.vertices = deformed;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                var renderer = sail.cloth.GetComponent<SkinnedMeshRenderer>();
                bool clothEnabled = sail.cloth.enabled;
                sail.cloth.enabled = false;
                renderer.sharedMesh = mesh;
                // Preserve the source's animation bounds as well as the new mesh bounds.
                var bounds = renderer.localBounds;
                bounds.Encapsulate(mesh.bounds);
                renderer.localBounds = bounds;
                sail.cloth.coefficients = constraints;
                sail.cloth.enabled = clothEnabled;
                clone.SetActive(true);
                sail.SetSailArea();

                // Verify that the source still points at its original mesh.
                if (sourceRenderer.sharedMesh != sourceMesh || renderer.sharedMesh == sourceMesh)
                    throw new InvalidOperationException(
                        "The prototype must own a separate cloth mesh."
                    );

                string registrationMessage =
                    $"Registered {DisplayName}: source={SourceIndex}, index={PrototypeIndex}, "
                    + $"vertices={mesh.vertexCount}, changed={changed}, pinned={pinned}, "
                    + $"area={sourceSail.GetSailArea():F2}->{sail.sailArea:F2}. Original brig jib preserved.";

                if (directory.sails.Length <= PrototypeIndex)
                    Array.Resize(ref directory.sails, PrototypeIndex + 1);
                directory.sails[PrototypeIndex] = clone;
                prefab = clone;
                Plugin.Log.LogInfo(registrationMessage);
            }
            catch (Exception exception)
            {
                if (container)
                    Object.Destroy(container);
                if (mesh)
                    Object.Destroy(mesh);
                Plugin.Log.LogError($"Could not register {DisplayName}: {exception}");
            }
        }

        internal static void AddToShipyard(Shipyard shipyard)
        {
            if (!prefab || !shipyard || shipyard.sailPrefabs == null)
                return;
            // All Sails in All Shipyards may already have included our prefab.
            foreach (var existing in shipyard.sailPrefabs)
                if (existing == prefab)
                    return;

            int index = shipyard.sailPrefabs.Length;
            Array.Resize(ref shipyard.sailPrefabs, index + 1);
            shipyard.sailPrefabs[index] = prefab;
            Plugin.Log.LogInfo($"Added {DisplayName} to {shipyard.name}.");
        }
    }

    [HarmonyPatch(typeof(PrefabsDirectory), "Start")]
    internal static class RegisterPrototypePatch
    {
        // Clone after SE has configured the source components, but before
        // All Sails captures the prefab array for its cached menu pages.
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        [HarmonyBefore("NatoriusG.AllSailsAllShipyards")]
        private static void Postfix(PrefabsDirectory __instance) =>
            PrototypeSail.Register(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "Awake")]
    internal static class ShipyardPrototypePatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Shipyard __instance) => PrototypeSail.AddToShipyard(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "ActivateDocuments")]
    internal static class ShipyardPrototypeFallbackPatch
    {
        // Covers shipyards that awakened before PrefabsDirectory.Start.
        [HarmonyPrefix]
        private static void Prefix(Shipyard __instance) => PrototypeSail.AddToShipyard(__instance);
    }
}
