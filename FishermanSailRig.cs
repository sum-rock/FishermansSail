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
    [DefaultExecutionOrder(100)]
    internal sealed class FishermanSailRig : MonoBehaviour
    {
        public Sail Sail;
        public Transform[] Bones;
        public Vector3[] Corners;
        public Transform SheetAttachment;
        public Transform[] HalyardAttachments;
        public SkinnedMeshRenderer ReefedRenderer;
        public MeshRenderer BundleRenderer;
        public Transform FlyingFrame;
        public Vector3 OriginalHingeAxis;
        public Vector3 OriginalHingeAnchor;
        public bool OriginalAutoAnchor;
        public FishermanSupportLine SupportLine;
        public Transform Shadow;
        private float clothLoad;
        private float camber = 1;
        private int camberSide = 1;
        private bool tensionWarning;
        private readonly Vector3[] leechPoints = new Vector3[PrototypeGeometry.Rows + 1];
        private int lastRenderState = -1;
        private FishermanRigging rigging;
        private Mast lastMount;
        private bool refreshRequested;
        private bool bindingDirty = true;
        private bool boundToMast;
        private Vector3 pivot;
        private Vector3 pivotAxis;
        private Vector3 lastScale;
        private Vector3 lastFramePosition;
        private Quaternion lastFrameRotation;

        private void OnEnable() => bindingDirty = true;

        private void FixedUpdate() => RefreshFlyingFrame();

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
            var originalHinge = sail.GetComponent<HingeJoint>();
            rig.OriginalHingeAxis = originalHinge.axis;
            rig.OriginalHingeAnchor = originalHinge.anchor;
            rig.OriginalAutoAnchor = originalHinge.autoConfigureConnectedAnchor;
            rig.FlyingFrame = new GameObject("Fisherman mast pivot frame").transform;
            rig.FlyingFrame.SetParent(sail.transform, false);
            scaleRoot.SetParent(rig.FlyingFrame, false);
            rig.Bones = new Transform[data.BonePositions.Length];
            var poses = new Matrix4x4[rig.Bones.Length];
            for (int i = 0; i < rig.Bones.Length; i++)
            {
                var bone = new GameObject("Fisherman corner " + i).transform;
                bone.SetParent(cloth.transform, false);
                bone.localPosition = data.BonePositions[i];
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
            cloth.bendingStiffness = 0.4f;
            cloth.stretchingStiffness = 0.99f;
            cloth.damping = damping;
            cloth.friction = friction;
            cloth.useGravity = gravity;
            cloth.worldVelocityScale = 0;
            cloth.worldAccelerationScale = 0;
            cloth.clothSolverFrequency = 120;
            cloth.coefficients = data.Constraints;
            if (cloth.coefficients.Length != mesh.vertexCount)
                throw new InvalidOperationException(
                    "The new Cloth does not match the trapezoid vertex count."
                );
            cloth.enabled = true;
            var wind = clothObject.GetComponent<WindCloth>();
            if (wind)
            {
                // Keep the donor's serialized wind response (5 on the brig
                // jib). The previous 0.6 override let gravity dominate it.
                wind.minClothDamping = 0.25f;
                wind.maxClothDamping = 0.8f;
            }

            var connections = sail.GetComponent<SailConnections>();
            var left = connections.angleControllerLeft.GetComponent<RopeEffect>();
            var right = connections.angleControllerRight.GetComponent<RopeEffect>();
            rig.SupportLine = FishermanSupportLine.Create(sail.transform, left, right);
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
                // SailShadowCol.Awake resolves its Sail exactly two parents up.
                // Keep that contract despite the added mast-pivot frame.
                shadow.transform.SetParent(rig.FlyingFrame, false);
                rig.Shadow = shadow.transform;
                shadow.transform.localPosition = Vector3.zero;
                shadow.transform.localRotation = Quaternion.identity;
                shadow.transform.localScale = scaleRoot.localScale;
                shadow.GetComponent<MeshFilter>().sharedMesh = shadowMesh;
                var box = shadow.GetComponent<BoxCollider>();
                if (box)
                {
                    box.center = new Vector3(mesh.bounds.center.x, 0, mesh.bounds.center.z);
                    box.size = new Vector3(
                        mesh.bounds.size.x,
                        mesh.bounds.size.y * 2 + 0.1f,
                        mesh.bounds.size.z
                    );
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
                MastInstallationGeometry.CollisionStrip(width, i, 0, out var center, out var size);
                box.center = center;
                box.size = size;
                box.isTrigger = true;
                var visual = box.GetComponent<MeshRenderer>();
                if (visual)
                    visual.enabled = false;
            }
        }

        internal bool RefreshFlyingFrame()
        {
            if (
                !Sail
                || !FlyingFrame
                || Bones == null
                || Bones.Length != PrototypeGeometry.BoneCount
            )
                return false;
            var mount = Sail.transform.parent ? Sail.transform.parent.GetComponent<Mast>() : null;
            if (mount != lastMount)
            {
                bool wasBound = boundToMast;
                lastMount = mount;
                rigging = mount ? FishermanRigging.For(Sail) : null;
                if (wasBound && !rigging)
                {
                    FlyingFrame.localPosition = Vector3.zero;
                    FlyingFrame.localRotation = Quaternion.identity;
                    var originalHinge = Sail.GetComponent<HingeJoint>();
                    originalHinge.axis = OriginalHingeAxis;
                    originalHinge.anchor = OriginalHingeAnchor;
                    originalHinge.autoConfigureConnectedAnchor = OriginalAutoAnchor;
                    refreshRequested = true;
                }
                boundToMast = false;
                bindingDirty = true;
            }
            if (!rigging || !rigging.Bind(mount))
                return false;

            rigging.ForeSailFrame(out var forePoint, out var foreAxis);
            var scaleRoot = Sail.cloth.transform.parent;
            // Preserve the native saved installation coordinate. Offset the
            // model so its whole luff sits on the physical forward mast even
            // when the sail is narrower than the distance between the masts.
            var origin = new Vector3(0, 0, Sail.GetCurrentInstallHeight() - mount.mastHeight);
            var nextPivot = mount.transform.InverseTransformPoint(forePoint) - origin;
            var nextAxis = mount.transform.InverseTransformDirection(foreAxis).normalized;
            var aftDirection = mount.transform.InverseTransformDirection(
                Vector3.ProjectOnPlane(rigging.AftReference - forePoint, foreAxis).normalized
            );
            var alignment =
                Quaternion.LookRotation(aftDirection, Vector3.Cross(aftDirection, nextAxis))
                * Quaternion.Inverse(scaleRoot.localRotation);
            FlyingFrame.localRotation = alignment;
            FlyingFrame.localPosition = FlyingSailGeometry.ModelOffset(
                nextPivot,
                alignment
                    * (
                        scaleRoot.localPosition
                        + scaleRoot.localRotation * Vector3.Scale(Corners[0], scaleRoot.localScale)
                    )
            );
            bool changed =
                bindingDirty
                || !boundToMast
                || FlyingSailGeometry.PositionChanged(nextPivot, pivot)
                || FlyingSailGeometry.PositionChanged(nextAxis, pivotAxis)
                || FlyingSailGeometry.PositionChanged(lastScale, scaleRoot.localScale)
                || FlyingSailGeometry.PositionChanged(lastFramePosition, FlyingFrame.localPosition)
                || Quaternion.Angle(lastFrameRotation, FlyingFrame.localRotation) > 0.05f;
            if (changed)
            {
                var body = Sail.GetComponent<Rigidbody>();
                var hinge = Sail.GetComponent<HingeJoint>();
                if (!boundToMast || GameState.currentShipyard)
                    body.rotation = mount.transform.rotation;
                body.position = forePoint - body.rotation * nextPivot;
                RefreshCollisionStrips();
                // Hoisting can start well below the fully set panel's original
                // bounds, especially after fitting a smaller sail high on a mast.
                var clothRenderer = Sail.cloth.GetComponent<SkinnedMeshRenderer>();
                var bounds = clothRenderer.sharedMesh.bounds;
                bounds.Encapsulate(Sail.cloth.transform.InverseTransformPoint(rigging.DeckPoint));
                bounds.Expand(
                    new Vector3(-Corners[0].z * 0.3f, -Corners[0].z * 2.5f, -Corners[0].z * 0.4f)
                );
                clothRenderer.localBounds = bounds;
                ReefedRenderer.localBounds = bounds;
                hinge.autoConfigureConnectedAnchor = false;
                hinge.connectedBody = mount.shipRigidbody;
                hinge.axis = nextAxis;
                hinge.anchor = nextPivot;
                hinge.connectedAnchor = mount.shipRigidbody.transform.InverseTransformPoint(
                    forePoint
                );
                if (!boundToMast)
                    Sail.GetComponent<JibAngleMaster>().UpdateInitialAngle();
                pivot = nextPivot;
                pivotAxis = nextAxis;
                lastScale = scaleRoot.localScale;
                lastFramePosition = FlyingFrame.localPosition;
                lastFrameRotation = FlyingFrame.localRotation;
                boundToMast = true;
                bindingDirty = false;
                refreshRequested = true;
            }
            return true;
        }

        private void RefreshCollisionStrips()
        {
            // Cloth attaches to the supporting mast's axis. Its contact with the
            // mast and the roots of its fittings is intentional, not obstruction.
            var mastCollider = lastMount.GetComponent<CapsuleCollider>();
            var mastScale = mastCollider.transform.lossyScale;
            float radius =
                mastCollider.radius
                * Mathf.Max(
                    mastCollider.direction == 0 ? mastScale.y : mastScale.x,
                    mastCollider.direction == 2 ? mastScale.y : mastScale.z
                );
            float widthScale = Sail.cloth.transform.TransformVector(Vector3.forward).magnitude;
            float clearance = (radius + 0.02f) / Mathf.Max(0.0001f, widthScale);
            var boxes = Sail.GetComponent<SailConnections>()
                .colChecker.GetComponentsInChildren<BoxCollider>(true);
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].enabled = MastInstallationGeometry.CollisionStrip(
                    -Corners[0].z,
                    i,
                    clearance,
                    out var center,
                    out var size
                );
                boxes[i].center = center;
                boxes[i].size = size;
            }
        }

        internal bool PositionCollisionChecker(
            Transform checker,
            float angle,
            out Quaternion neutralRotation
        )
        {
            neutralRotation = Quaternion.identity;
            var walk = lastMount.walkColMast;
            if (!walk || checker.parent != walk)
                return false;
            var scaleRoot = Sail.cloth.transform.parent;
            var rotation = Quaternion.AngleAxis(angle, pivotAxis);
            var origin = new Vector3(0, 0, Sail.GetCurrentInstallHeight() - lastMount.mastHeight);
            var modelOffset =
                FlyingFrame.localPosition + FlyingFrame.localRotation * scaleRoot.localPosition;
            var position =
                origin + FlyingSailGeometry.RotateAroundMast(modelOffset, pivot, pivotAxis, angle);
            neutralRotation = FlyingFrame.localRotation * scaleRoot.localRotation;
            checker.SetPositionAndRotation(
                walk.TransformPoint(position),
                walk.rotation * rotation * neutralRotation
            );
            checker.localScale = scaleRoot.localScale;
            return true;
        }

        internal bool RefreshAerodynamics()
        {
            if (!Sail || !Sail.windcenter || Bones == null || Bones.Length < 4)
                return false;
            if (
                !FishermanAerodynamics.TryFrame(
                    Bones[0].position,
                    Bones[2].position,
                    Bones[1].position,
                    Bones[3].position,
                    out var frame
                )
            )
                return false;
            Sail.windcenter.SetPositionAndRotation(
                frame.Center,
                Quaternion.LookRotation(frame.MastAxis, frame.Normal)
            );
            return true;
        }

        private void UpdateShapeBones()
        {
            var normal = FishermanBillow.CamberNormal(
                Bones[0].localPosition,
                Bones[2].localPosition,
                Bones[1].localPosition,
                Bones[3].localPosition
            );
            var flow = Sail.cloth.transform.InverseTransformDirection(Sail.apparentWind);
            camberSide = FishermanBillow.CamberSide(
                camberSide,
                Vector3.Dot(flow, normal) * Sail.GetCurrentShadowMult()
            );
            camber = FishermanBillow.SmoothLoad(camber, camberSide, Time.deltaTime);
            float deployedCamber = camber * FishermanBillow.Deployment(Sail.currentUnroll);
            for (int row = 0; row <= PrototypeGeometry.Rows; row++)
            {
                float v = (float)row / PrototypeGeometry.Rows;
                var fore = Vector3.Lerp(Bones[0].localPosition, Bones[2].localPosition, v);
                var aft = Bones[PrototypeGeometry.LeechBone(row)].localPosition;
                for (int column = 0; column < PrototypeGeometry.ShapeColumns; column++)
                {
                    int bone = PrototypeGeometry.ShapeBone(row, column);
                    if (bone == 0 || bone == 2)
                        continue;
                    Bones[bone].localPosition = FishermanBillow.ShapePoint(
                        fore,
                        aft,
                        normal,
                        -Corners[0].z,
                        (float)column / PrototypeGeometry.ShapeColumns,
                        v,
                        deployedCamber
                    );
                }
            }
        }

        private void LateUpdate()
        {
            if (!Sail || Bones == null || Bones.Length != PrototypeGeometry.BoneCount)
                return;
            bool supported = RefreshFlyingFrame();
            if (Shadow)
            {
                var scale = Sail.cloth.transform.parent;
                Shadow.localPosition = scale.localPosition;
                Shadow.localRotation = scale.localRotation;
                Shadow.localScale = scale.localScale;
            }
            float targetLoad = !supported
                ? 0
                : Sail.cloth.transform.InverseTransformDirection(Sail.apparentWind).y
                    / 8f
                    * Sail.GetCurrentShadowMult();
            clothLoad = FishermanBillow.SmoothLoad(clothLoad, targetLoad, Time.deltaTime);
            var deck = supported
                ? Sail.cloth.transform.InverseTransformPoint(rigging.DeckPoint)
                : Corners[2];
            for (int i = 0; i < Corners.Length; i++)
                Bones[i].localPosition = MastInstallationGeometry.HoistCorner(
                    Corners[i],
                    Corners[0],
                    deck,
                    Sail.currentUnroll
                );

            int state = PrototypeGeometry.RenderState(Sail.currentUnroll);
            for (int row = 1; row < PrototypeGeometry.Rows; row++)
                Bones[PrototypeGeometry.LeechBone(row)].localPosition = Vector3.Lerp(
                    Bones[1].localPosition,
                    Bones[3].localPosition,
                    (float)row / PrototypeGeometry.Rows
                );
            if (supported)
            {
                var scaleRoot = Sail.cloth.transform.parent;
                var origin = new Vector3(
                    0,
                    0,
                    Sail.GetCurrentInstallHeight() - lastMount.mastHeight
                );
                var neutralHead = lastMount.transform.TransformPoint(
                    origin
                        + FlyingFrame.localPosition
                        + FlyingFrame.localRotation
                            * (
                                scaleRoot.localPosition
                                + scaleRoot.localRotation
                                    * Vector3.Scale(
                                        MastInstallationGeometry.HoistCorner(
                                            Corners[1],
                                            Corners[0],
                                            deck,
                                            Sail.currentUnroll
                                        ),
                                        scaleRoot.localScale
                                    )
                            )
                );
                var clothTransform = Sail.cloth.transform;
                rigging.ForeSailFrame(out var forePoint, out var mastAxis);
                var head = FlyingSailGeometry.UpperHead(
                    neutralHead,
                    clothTransform.TransformPoint(Corners[1]),
                    clothTransform.TransformPoint(Bones[0].localPosition),
                    mastAxis,
                    Sail.currentUnroll
                );
                var localHead = clothTransform.InverseTransformPoint(head);
                var normal = Vector3.Cross(mastAxis, rigging.AftReference - forePoint).normalized;
                var clew = Bones[3].localPosition;
                var bow = FishermanBillow.SupportBow(
                    clew,
                    localHead,
                    clothTransform.InverseTransformDirection(normal),
                    clothTransform.InverseTransformDirection(Vector3.down),
                    -Corners[0].z,
                    clothLoad
                );
                var tack = Bones[2].localPosition;
                bool fitted = FishermanTension.Fit(
                    clew,
                    localHead,
                    tack,
                    bow * FishermanBillow.Deployment(Sail.currentUnroll),
                    -Corners[3].x * MastInstallationGeometry.HoistScale(Sail.currentUnroll),
                    (clew - tack).magnitude,
                    FishermanBillow.Deployment(Sail.currentUnroll),
                    leechPoints
                );
                if (!fitted && !tensionWarning && state == 2)
                {
                    Plugin.Log.LogWarning(
                        "Fisherman corner span exceeds available foot/leech lengths; check sail fit."
                    );
                    tensionWarning = true;
                }
                for (int row = 0; row <= PrototypeGeometry.Rows; row++)
                    Bones[PrototypeGeometry.LeechBone(row)].localPosition = leechPoints[
                        PrototypeGeometry.Rows - row
                    ];
                HalyardAttachments[0].localPosition = Vector3.zero;
                SheetAttachment.localPosition = Vector3.zero;
                rigging.UpdateHalyard(HalyardAttachments, state == 0);
                if (state == 0)
                    SheetAttachment.position = rigging.Pair.ForeGuide.position;
                if (!GameState.currentlyLoading)
                    SupportLine.Draw(
                        state == 0 ? rigging.Pair.AftGuide.position : Bones[1].position,
                        rigging.Pair.AftGuide.position
                    );
                else
                    SupportLine.Hide();
            }
            else
            {
                SupportLine.Hide();
            }
            UpdateShapeBones();
            RefreshAerodynamics();
            if (refreshRequested || state != lastRenderState)
            {
                // Refresh after applying our corner poses. The donor animator's
                // Start never runs, so its RefreshCloth must not be invoked.
                Sail.cloth.enabled = false;
                Sail.cloth.ClearTransformMotion();
                refreshRequested = false;
            }
            Sail.cloth.enabled = supported && state == 2;
            var clothRenderer = Sail.cloth.GetComponent<SkinnedMeshRenderer>();
            bool visible = supported && !GameState.currentlyLoading;
            // WindCloth writes renderer.enabled in Update; select the correct
            // renderer here in LateUpdate so the disabled solver cannot leave
            // stale full-size triangles visible when the sail is struck.
            clothRenderer.enabled = visible && state == 2;
            ReefedRenderer.sharedMaterial = clothRenderer.sharedMaterial;
            BundleRenderer.sharedMaterial = clothRenderer.sharedMaterial;
            ReefedRenderer.enabled = visible && state == 1;
            // Kept only as the native ChangeSailColor/SE reference; never display it.
            BundleRenderer.enabled = false;
            lastRenderState = state;
        }
    }
}
