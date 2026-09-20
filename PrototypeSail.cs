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
            Mesh shadowMesh = null;
            Mesh bundleMesh = null;
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
                var geometry = PrototypeGeometry.Create(sourceSail.installHeight);

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

                mesh = new Mesh { name = "FishermansSail Trapezoid Cloth" };
                mesh.vertices = geometry.Vertices;
                mesh.triangles = geometry.Triangles;
                mesh.uv = geometry.UV;
                mesh.boneWeights = geometry.Weights;
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                shadowMesh = new Mesh { name = "FishermansSail Shadow Samples" };
                shadowMesh.vertices = geometry.Corners;
                shadowMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
                shadowMesh.RecalculateBounds();
                var bundle = PrototypeGeometry.CreateBundle(sourceSail.installHeight);
                bundleMesh = new Mesh { name = "FishermansSail Furled Bundle" };
                bundleMesh.vertices = bundle.Vertices;
                bundleMesh.triangles = bundle.Triangles;
                bundleMesh.uv = bundle.UV;
                bundleMesh.RecalculateNormals();
                bundleMesh.RecalculateBounds();
                FishermanSailRig.Configure(sail, geometry, mesh, shadowMesh, bundleMesh);
                var renderer = sail.cloth.GetComponent<SkinnedMeshRenderer>();
                clone.SetActive(true);
                sail.SetSailArea();

                // Verify that the source still points at its original mesh.
                if (sourceRenderer.sharedMesh != sourceMesh || renderer.sharedMesh == sourceMesh)
                    throw new InvalidOperationException(
                        "The prototype must own a separate cloth mesh."
                    );

                string registrationMessage =
                    $"Registered {DisplayName}: source={SourceIndex}, index={PrototypeIndex}, "
                    + $"vertices={mesh.vertexCount}, corners=4, forwardAngle=40, "
                    + $"area={sourceSail.GetSailArea():F2}->{sail.sailArea:F2}. Original brig jib preserved.";

                if (directory.sails.Length <= PrototypeIndex)
                    Array.Resize(ref directory.sails, PrototypeIndex + 1);
                directory.sails[PrototypeIndex] = clone;
                container.AddComponent<FishermanSailAssets>().Meshes = new[]
                {
                    mesh,
                    shadowMesh,
                    bundleMesh,
                };
                prefab = clone;
                Plugin.Log.LogInfo(registrationMessage);
            }
            catch (Exception exception)
            {
                if (container)
                    Object.Destroy(container);
                if (mesh)
                    Object.Destroy(mesh);
                if (shadowMesh)
                    Object.Destroy(shadowMesh);
                if (bundleMesh)
                    Object.Destroy(bundleMesh);
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
