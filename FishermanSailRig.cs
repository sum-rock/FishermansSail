using System;
using UnityEngine;

namespace FishermansSail
{
    // Only the inactive template container owns meshes. Installed copies share
    // these read-only assets and must not destroy them when an individual sail is removed.
    internal sealed class FishermanSailAssets : MonoBehaviour
    {
        public Mesh[] Meshes;

        private void OnDestroy()
        {
            if (Meshes == null)
                return;
            foreach (var mesh in Meshes)
                if (mesh)
                    UnityEngine.Object.Destroy(mesh);
        }
    }

    // Serialized fields are intentionally copied into each installed prefab instance.
    internal sealed class FishermanSailRig : MonoBehaviour
    {
        public Sail Sail;
        public Transform[] Bones;
        public Vector3[] Corners;
        public Transform SheetAttachment;
        public Transform[] HalyardAttachments;
        public SkinnedMeshRenderer ReefedRenderer;
        public MeshRenderer BundleRenderer;
        private int lastRenderState = -1;
        private FishermanStay stay;
        private Mast lastMount;
        private bool refreshRequested;

        internal void RefreshCloth() => refreshRequested = true;

        internal static void Configure(
            Sail sail,
            SailMeshData data,
            Mesh mesh,
            Mesh shadowMesh,
            Mesh bundleMesh
        )
        {
            var cloth = sail.cloth;
            var renderer = cloth.GetComponent<SkinnedMeshRenderer>();
            var scaleRoot = cloth.transform.parent;
            var reef = sail.GetComponent<ReefEffectAnimUniversal>();
            if (!reef || !reef.overrideAnimator)
                throw new InvalidOperationException("Expected the brig jib's furl animator.");
            reef.enabled = false;
            reef.overrideAnimator.enabled = false;
            if (reef.furledSail)
                reef.furledSail.enabled = false;
            // Retain the disabled Animator as Shipyard Expansion's scaling reference.
            scaleRoot.localPosition = Vector3.zero;
            scaleRoot.localRotation = Quaternion.identity;
            cloth.transform.localPosition = Vector3.zero;
            cloth.transform.localRotation = Quaternion.identity;
            cloth.transform.localScale = Vector3.one;

            var rig = sail.gameObject.AddComponent<FishermanSailRig>();
            rig.Sail = sail;
            rig.Corners = data.Corners;
            rig.Bones = new Transform[4];
            var poses = new Matrix4x4[4];
            for (int i = 0; i < 4; i++)
            {
                var bone = new GameObject("Fisherman corner " + i).transform;
                bone.SetParent(cloth.transform, false);
                bone.localPosition = data.Corners[i];
                rig.Bones[i] = bone;
                poses[i] = bone.worldToLocalMatrix * cloth.transform.localToWorldMatrix;
            }
            // RopeEffect.LookAt rotates both endpoint transforms. Never give
            // it a skin bone directly: use independent leaves beneath the bones.
            rig.HalyardAttachments = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var attachment = new GameObject("Fisherman head halyard " + i).transform;
                attachment.SetParent(rig.Bones[i], false);
                rig.HalyardAttachments[i] = attachment;
            }
            mesh.bindposes = poses;
            // The donor Cloth contains serialized simulation data for a different
            // topology. Recreate only that component on the inactive clone, after
            // saving its physical settings; WindCloth resolves it in Awake later.
            var clothObject = cloth.gameObject;
            float bending = cloth.bendingStiffness,
                stretching = cloth.stretchingStiffness;
            float damping = cloth.damping,
                friction = cloth.friction;
            bool gravity = cloth.useGravity;
            cloth.enabled = false;
            UnityEngine.Object.DestroyImmediate(cloth);

            renderer.sharedMesh = mesh;
            renderer.bones = rig.Bones;
            renderer.rootBone = clothObject.transform;
            renderer.quality = SkinQuality.Bone4;
            renderer.localBounds = new Bounds(
                mesh.bounds.center,
                new Vector3(
                    mesh.bounds.size.x + sail.installHeight * 0.25f,
                    mesh.bounds.size.x * 2.25f,
                    sail.installHeight * 1.25f
                )
            );
            renderer.updateWhenOffscreen = true;
            cloth = clothObject.AddComponent<Cloth>();
            cloth.enabled = false;
            sail.cloth = cloth;
            cloth.bendingStiffness = bending;
            cloth.stretchingStiffness = stretching;
            cloth.damping = damping;
            cloth.friction = friction;
            cloth.useGravity = gravity;
            cloth.worldVelocityScale = 0;
            cloth.worldAccelerationScale = 0;
            cloth.coefficients = data.Constraints;
            if (cloth.coefficients.Length != mesh.vertexCount)
                throw new InvalidOperationException(
                    "The new Cloth does not match the trapezoid vertex count."
                );
            cloth.enabled = true;

            var connections = sail.GetComponent<SailConnections>();
            var left = connections.angleControllerLeft.GetComponent<RopeEffect>();
            var right = connections.angleControllerRight.GetComponent<RopeEffect>();
            rig.SheetAttachment = left.attachment;
            if (!rig.SheetAttachment || right.attachment != rig.SheetAttachment)
                throw new InvalidOperationException("Expected a shared brig jib sheet attachment.");
            rig.SheetAttachment.SetParent(rig.Bones[3], false);
            rig.SheetAttachment.localPosition = Vector3.zero;
            sail.windcenter.SetParent(scaleRoot, false);
            sail.windcenter.localPosition = data.Center;
            connections.colChecker.transform.SetParent(scaleRoot, false);
            ConfigureCollision(connections.colChecker, sail.installHeight);
            foreach (var visual in scaleRoot.GetComponentsInChildren<MeshRenderer>(true))
                visual.enabled = false;
            var shadow = sail.GetComponentInChildren<SailShadowCol>(true);
            if (shadow)
            {
                shadow.transform.SetParent(scaleRoot, false);
                shadow.transform.localPosition = Vector3.zero;
                shadow.transform.localRotation = Quaternion.identity;
                shadow.transform.localScale = Vector3.one;
                shadow.GetComponent<MeshFilter>().sharedMesh = shadowMesh;
                var box = shadow.GetComponent<BoxCollider>();
                if (box)
                {
                    box.center = mesh.bounds.center;
                    box.size = new Vector3(mesh.bounds.size.x, 0.1f, mesh.bounds.size.z);
                }
            }
            var reefed = new GameObject("Fisherman reefing cloth");
            reefed.transform.SetParent(scaleRoot, false);
            rig.ReefedRenderer = reefed.AddComponent<SkinnedMeshRenderer>();
            rig.ReefedRenderer.sharedMesh = mesh;
            rig.ReefedRenderer.bones = rig.Bones;
            rig.ReefedRenderer.rootBone = cloth.transform;
            rig.ReefedRenderer.quality = SkinQuality.Bone4;
            rig.ReefedRenderer.localBounds = renderer.localBounds;
            rig.ReefedRenderer.sharedMaterials = renderer.sharedMaterials;
            rig.ReefedRenderer.enabled = false;
            var bundle = new GameObject("Fisherman furled bundle");
            bundle.transform.SetParent(scaleRoot, false);
            bundle.AddComponent<MeshFilter>().sharedMesh = bundleMesh;
            rig.BundleRenderer = bundle.AddComponent<MeshRenderer>();
            rig.BundleRenderer.sharedMaterials = renderer.sharedMaterials;
            rig.BundleRenderer.enabled = false;
            reef.furledSail = rig.BundleRenderer;
        }

        private static void ConfigureCollision(ShipyardSailColChecker checker, float width)
        {
            // Use narrow inscribed strips rather than the old triangular clew box.
            // Keep the checker outside the animated bones: it measures the fully set sail.
            var root = checker.transform;
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;
            var old = checker.GetComponentsInChildren<BoxCollider>(true);
            if (old.Length != 1 || old[0].transform.parent != root)
                throw new InvalidOperationException("Unexpected brig jib collision hierarchy.");
            for (int i = 0; i < PrototypeGeometry.Columns; i++)
            {
                var box =
                    i == 0
                        ? old[0]
                        : new GameObject(
                            "Fisherman collision strip " + i
                        ).AddComponent<BoxCollider>();
                box.transform.SetParent(root, false);
                box.transform.localPosition = Vector3.zero;
                box.transform.localRotation = Quaternion.identity;
                box.transform.localScale = Vector3.one;
                float u = (i + 1f) / PrototypeGeometry.Columns;
                float depth =
                    width
                    * (
                        PrototypeGeometry.ForeDepthRatio
                        + (PrototypeGeometry.AftDepthRatio - PrototypeGeometry.ForeDepthRatio) * u
                    );
                box.center = new Vector3(
                    -depth * 0.5f,
                    0,
                    -width + (i + 0.5f) * width / PrototypeGeometry.Columns
                );
                box.size = new Vector3(
                    Math.Max(0.01f, depth - 0.1f),
                    0.05f,
                    width / PrototypeGeometry.Columns * 0.9f
                );
                box.isTrigger = true;
                var visual = box.GetComponent<MeshRenderer>();
                if (visual)
                    visual.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!Sail || Bones == null || Bones.Length != 4)
                return;
            var mount = Sail.transform.parent ? Sail.transform.parent.GetComponent<Mast>() : null;
            if (mount != lastMount)
            {
                lastMount = mount;
                stay = null;
                var registry = mount ? mount.GetComponentInParent<FishermanStayRegistry>() : null;
                if (registry)
                    stay = registry.Stays.Find(s => s.Mount == mount);
            }
            Bones[2].localPosition = PrototypeGeometry.ReefCorner(Corners[2], Sail.currentUnroll);
            Bones[3].localPosition = PrototypeGeometry.ReefCorner(Corners[3], Sail.currentUnroll);
            if (stay != null)
            {
                // The forward lower corner belongs to the physical mast, not the
                // rotating sail. It rises along that mast as the sail is struck.
                var cloth = Sail.cloth.transform;
                var neutral = cloth.localToWorldMatrix;
                var world = neutral.MultiplyPoint3x4(Bones[2].localPosition);
                // Height must come from the resting sail frame, independent of sheet angle.
                var local = Sail.transform.InverseTransformPoint(world);
                world = mount.transform.TransformPoint(Sail.transform.localPosition + local);
                Bones[2].position = stay.ForeAttachment(world);
            }
            if (stay != null)
                stay.UpdateHalyard(Sail, HalyardAttachments);
            int state = PrototypeGeometry.RenderState(Sail.currentUnroll);
            if (refreshRequested || state != lastRenderState)
            {
                // Refresh after applying our corner poses. The donor animator's
                // Start never runs, so its RefreshCloth must not be invoked.
                Sail.cloth.enabled = false;
                Sail.cloth.ClearTransformMotion();
                refreshRequested = false;
            }
            Sail.cloth.enabled = state == 2;
            var clothRenderer = Sail.cloth.GetComponent<SkinnedMeshRenderer>();
            bool visible = !GameState.currentlyLoading;
            // WindCloth writes renderer.enabled in Update; select the correct
            // renderer here in LateUpdate so the disabled solver cannot leave
            // stale full-size triangles visible when the sail is struck.
            clothRenderer.enabled = visible && state == 2;
            ReefedRenderer.sharedMaterial = clothRenderer.sharedMaterial;
            BundleRenderer.sharedMaterial = clothRenderer.sharedMaterial;
            ReefedRenderer.enabled = visible && state == 1;
            BundleRenderer.enabled = visible && state == 0;
            lastRenderState = state;
        }
    }
}
