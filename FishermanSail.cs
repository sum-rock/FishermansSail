using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishermansSail
{
    internal static class FishermanSail
    {
        internal const int SourceIndex = 110;

        // Stable across launches: Sailwind stores this index in boat saves.
        internal const int PrefabIndex = 400;
        internal const string DisplayName = "Fisherman's Sail";
        private static GameObject prefab;

        internal static void Register(PrefabsDirectory directory)
        {
            GameObject container = null;
            Mesh mesh = null;
            Mesh shadowMesh = null;
            try
            {
                if (
                    prefab
                    && directory.sails.Length > PrefabIndex
                    && directory.sails[PrefabIndex] == prefab
                )
                    return;
                prefab = null;
                if (directory.sails.Length > PrefabIndex && directory.sails[PrefabIndex])
                    throw new InvalidOperationException(
                        $"Sail index {PrefabIndex} is already occupied; no sail was replaced."
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
                var geometry = FishermanGeometry.Create(sourceSail.installHeight);

                // An inactive parent prevents Awake/Start from running on our
                // template. Installed copies retain activeSelf=true and initialize normally.
                container = new GameObject("FishermansSail Prefabs");
                container.SetActive(false);
                container.transform.SetParent(directory.transform, false);
                var clone = Object.Instantiate(source, container.transform, false);
                clone.name = $"{PrefabIndex} SAIL {DisplayName}";
                var sail = clone.GetComponent<Sail>();
                sail.prefabIndex = PrefabIndex;
                sail.sailName = DisplayName;
                sail.category = SailCategory.other;
                sail.obsolete = false;
                sail.minAngle = -FishermanTravel.MaximumAngle;
                sail.maxAngle = FishermanTravel.MaximumAngle;
                var hinge = sail.GetComponent<HingeJoint>();
                var limits = hinge.limits;
                limits.min = sail.minAngle;
                limits.max = sail.maxAngle;
                hinge.limits = limits;
                hinge.useLimits = true;
                // Changing the menu category must not change the donor's trim dynamics.
                var body = sail.GetComponent<Rigidbody>();
                body.mass = 0.1f;
                body.angularDrag = 1f;

                mesh = new Mesh { name = "FishermansSail Trapezoid Cloth" };
                mesh.vertices = geometry.Vertices;
                mesh.triangles = geometry.Triangles;
                mesh.uv = geometry.UV;
                mesh.boneWeights = geometry.Weights;
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                shadowMesh = new Mesh { name = "FishermansSail Shadow Samples" };
                // Native shadow checking casts one ray per vertex per frame.
                // Use a coarse 3x3 sample grid, not all 825 cloth vertices.
                var shadowPoints = new Vector3[9];
                for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                {
                    shadowPoints[row * 3 + col] = geometry.Vertices[
                        row * (FishermanGeometry.Rows / 2) * (FishermanGeometry.Columns + 1)
                            + col * (FishermanGeometry.Columns / 2)
                    ];
                    // Fixed center-plane samples are neutral between tacks.
                    shadowPoints[row * 3 + col].y = 0;
                }
                shadowMesh.vertices = shadowPoints;
                shadowMesh.triangles = new[]
                {
                    0,
                    3,
                    1,
                    1,
                    3,
                    4,
                    1,
                    4,
                    2,
                    2,
                    4,
                    5,
                    3,
                    6,
                    4,
                    4,
                    6,
                    7,
                    4,
                    7,
                    5,
                    5,
                    7,
                    8,
                };
                shadowMesh.RecalculateBounds();
                FishermanSailRig.Configure(sail, geometry, mesh, shadowMesh);
                FishermanAppearance.Configure(sail);
                var renderer = sail.cloth.GetComponent<SkinnedMeshRenderer>();
                clone.SetActive(true);
                sail.SetSailArea();

                // Verify that the source still points at its original mesh.
                if (sourceRenderer.sharedMesh != sourceMesh || renderer.sharedMesh == sourceMesh)
                    throw new InvalidOperationException(
                        "The fisherman sail must own a separate cloth mesh."
                    );

                string registrationMessage =
                    $"Registered {DisplayName}: source={SourceIndex}, index={PrefabIndex}, "
                    + $"vertices={mesh.vertexCount}, corners=4, aftDepthRatio=1, headCamber=0.12, "
                    + $"area={sourceSail.GetSailArea():F2}->{sail.sailArea:F2}. Original brig jib preserved.";

                if (directory.sails.Length <= PrefabIndex)
                    Array.Resize(ref directory.sails, PrefabIndex + 1);
                directory.sails[PrefabIndex] = clone;
                container.AddComponent<FishermanSailAssets>().Meshes = new[] { mesh, shadowMesh };
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
    internal static class RegisterFishermanSailPatch
    {
        // Clone after SE has configured the source components, but before
        // All Sails captures the prefab array for its cached menu pages.
        [HarmonyPostfix]
        [HarmonyAfter("com.nandbrew.shipyardexpansion")]
        [HarmonyBefore("NatoriusG.AllSailsAllShipyards")]
        private static void Postfix(PrefabsDirectory __instance) =>
            FishermanSail.Register(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "Awake")]
    internal static class ShipyardFishermanSailPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Shipyard __instance) => FishermanSail.AddToShipyard(__instance);
    }

    [HarmonyPatch(typeof(Shipyard), "ActivateDocuments")]
    internal static class ShipyardFishermanSailFallbackPatch
    {
        // Covers shipyards that awakened before PrefabsDirectory.Start.
        [HarmonyPrefix]
        private static void Prefix(Shipyard __instance) => FishermanSail.AddToShipyard(__instance);
    }
}
