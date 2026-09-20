using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishermansSail
{
    // One registry per boat instance, including shipyard/test copies. No static
    // scene-object cache survives a scene change.
    internal sealed class FishermanStayRegistry : MonoBehaviour
    {
        internal readonly List<FishermanStay> Stays = new List<FishermanStay>();
        internal readonly List<BoatPart> Parts = new List<BoatPart>();
        internal readonly List<GameObject> AuxiliaryObjects = new List<GameObject>();
        internal bool Registered;

        private void OnDestroy()
        {
            foreach (var stay in Stays)
                if (stay.WalkObject)
                    Object.Destroy(stay.WalkObject);
            foreach (var item in AuxiliaryObjects)
                if (item)
                    Object.Destroy(item);
        }
    }

    internal sealed class FishermanStay
    {
        internal const string ForeDisplayName = "Formast Triatic Stay";
        internal const string MizzenDisplayName = "Mizzenmast Triatic Stay";
        internal string DisplayName => isMizzen ? MizzenDisplayName : ForeDisplayName;
        private bool isMizzen;
        internal Mast Mount;
        internal BoatPartOption Option;
        internal GameObject WalkObject;
        internal bool Fits;
        internal string UnavailableReason;
        private Mast source;
        private Mast foreMast;
        private Mast aftMast;
        private BoatRefs boat;
        private Transform visual;
        private Transform walkVisual;
        private readonly List<Tuple<Transform, Transform>> anchors =
            new List<Tuple<Transform, Transform>>();
        private readonly List<Tuple<GPButtonRopeWinch, GPButtonRopeWinch>> controls =
            new List<Tuple<GPButtonRopeWinch, GPButtonRopeWinch>>();

        internal static void Register(SaveableBoatCustomization customization)
        {
            var boat = customization.GetComponent<BoatRefs>();
            var parts = customization.GetComponent<BoatCustomParts>();
            if (!boat || !parts || parts.availableParts == null)
                return;
            var registry =
                boat.GetComponent<FishermanStayRegistry>()
                ?? boat.gameObject.AddComponent<FishermanStayRegistry>();
            if (registry.Registered)
                return;
            registry.Registered = true;

            // Keep source parts/options intact. Each new part mirrors the source
            // group's mutually exclusive mast configurations, not its on/off state.
            var sourceParts = parts.availableParts.ToArray();
            var upperPairs = sourceParts
                .SelectMany(p => p.partOptions)
                .Where(o => o && StayGeometry.IsUpperStay(o.optionName + " " + o.name))
                .Where(o => o.GetComponent<Mast>() && o.GetComponent<Mast>().onlyStaysails)
                .Select(o => OrderedPhysicalMasts(o.GetComponent<Mast>(), boat))
                .Where(pair => pair.Length == 2)
                .ToArray();
            foreach (var group in DonorGroups(sourceParts))
            {
                var sourcePart = group.Item1;
                var candidates = group.Item2;
                var added = new List<FishermanStay>();
                GameObject empty = null;
                GameObject emptyWalk = null;
                try
                {
                    foreach (var source in candidates)
                    {
                        var option = source.GetComponent<BoatPartOption>();
                        var dependencies = OrderedPhysicalMasts(source, boat);
                        if (dependencies.Length != 2)
                            continue; // A forestay to a bowsprit is not an inter-mast stay.
                        if (!source.walkColMast || !boat.walkCol || source.mastHeight < 0.25f)
                            throw new InvalidOperationException(
                                $"{source.name} has no usable stay geometry."
                            );
                        int index = StayGeometry.MountIndex(source.orderIndex);
                        if (boat.masts.Length > index && boat.masts[index])
                            throw new InvalidOperationException(
                                $"Mount index {index} is occupied; no mount was replaced."
                            );
                        var stay = new FishermanStay
                        {
                            source = source,
                            boat = boat,
                            isMizzen = StayGeometry.IsMizzenPair(
                                option.optionName
                                    + " "
                                    + string.Join(" ", dependencies.Select(m => m.name)),
                                upperPairs.Any(pair =>
                                    pair[0] == dependencies[1] && pair[1] != dependencies[0]
                                )
                            ),
                        };
                        added.Add(stay); // Include partially constructed objects in rollback.
                        stay.Create(dependencies);
                    }
                    if (added.Count == 0)
                        continue;

                    empty = NewInactive("FishermansStay None", added[0].Mount.transform.parent);
                    emptyWalk = NewInactive(
                        "FishermansStay None Walk",
                        added[0].WalkObject.transform.parent
                    );
                    var none = empty.AddComponent<BoatPartOption>();
                    none.optionName = "(no " + added[0].DisplayName + ")";
                    none.requires = new List<BoatPartOption>();
                    none.requiresDisabled = new List<BoatPartOption>();
                    none.childOptions = new GameObject[0];
                    none.walkColObject = emptyWalk;
                    none.canInstall = true;
                    var options = new List<BoatPartOption> { none };
                    options.AddRange(added.Select(s => s.Option));
                    var part = new BoatPart
                    {
                        category = sourcePart.category,
                        activeOption = 0,
                        partOptions = options,
                    };
                    if (boat.masts.Length < StayGeometry.MountCapacity)
                        Array.Resize(ref boat.masts, StayGeometry.MountCapacity);
                    // Awake must run with complete references, before any save is
                    // loaded, even though the default part option is None.
                    foreach (var stay in added)
                    {
                        stay.Mount.gameObject.SetActive(true);
                        if (boat.masts[stay.Mount.orderIndex] != stay.Mount)
                            throw new InvalidOperationException(
                                "The new mount did not register during Awake."
                            );
                        stay.Mount.gameObject.SetActive(false);
                        Plugin.Log.LogInfo(
                            $"Registered {stay.DisplayName}: boat={boat.name}, source={stay.source.orderIndex}, "
                                + $"mount={stay.Mount.orderIndex}, span={stay.Mount.mastHeight:F2}, available={stay.Fits}."
                        );
                    }
                    parts.availableParts.Add(part);
                    registry.Stays.AddRange(added);
                    registry.Parts.Add(part);
                    registry.AuxiliaryObjects.Add(emptyWalk);
                    empty.SetActive(true);
                    emptyWalk.SetActive(true);
                }
                catch (Exception exception)
                {
                    foreach (var stay in added)
                    {
                        if (stay.Mount)
                        {
                            int index = stay.Mount.orderIndex;
                            if (
                                index >= 0
                                && index < boat.masts.Length
                                && boat.masts[index] == stay.Mount
                            )
                                boat.masts[index] = null;
                            stay.Mount.gameObject.SetActive(false);
                            Object.Destroy(stay.Mount.gameObject);
                        }
                        if (stay.WalkObject)
                            Object.Destroy(stay.WalkObject);
                    }
                    if (empty)
                        Object.Destroy(empty);
                    if (emptyWalk)
                        Object.Destroy(emptyWalk);
                    Plugin.Log.LogError(
                        $"Could not register fisherman stay group on {boat.name}: {exception}"
                    );
                }
            }
        }

        private static IEnumerable<Tuple<BoatPart, Mast[]>> DonorGroups(BoatPart[] parts)
        {
            // Preserve every previously appended part position. Newly supported
            // unqualified mizzen groups go after all existing upper-stay groups.
            for (int pass = 0; pass < 2; pass++)
                foreach (var part in parts)
                {
                    var mounts = part
                        .partOptions.Where(o => o)
                        .Select(o => o.GetComponent<Mast>())
                        .Where(m =>
                            m
                            && m.onlyStaysails
                            && m.orderIndex >= 0
                            && m.orderIndex < StayGeometry.SourceIndexLimit
                        )
                        .OrderBy(m => m.orderIndex)
                        .ToArray();
                    var upper = mounts
                        .Where(m =>
                            StayGeometry.IsUpperStay(
                                m.name + " " + m.GetComponent<BoatPartOption>().optionName
                            )
                        )
                        .ToArray();
                    if (pass == 0)
                    {
                        if (upper.Length > 0)
                            yield return Tuple.Create(part, upper);
                        continue;
                    }
                    if (upper.Length > 0)
                        continue;
                    var fallback = mounts
                        .Where(m =>
                        {
                            var option = m.GetComponent<BoatPartOption>();
                            var dependencies = PhysicalMasts(option).Distinct().ToArray();
                            return dependencies.Length == 2
                                && StayGeometry.IsFallbackMizzenStay(
                                    m.name + " " + option.optionName,
                                    string.Join(" ", dependencies.Select(d => d.name))
                                );
                        })
                        .ToArray();
                    if (fallback.Length > 0)
                        yield return Tuple.Create(part, fallback);
                }
        }

        private static IEnumerable<Mast> PhysicalMasts(BoatPartOption option)
        {
            foreach (var required in option.requires ?? new List<BoatPartOption>())
            {
                if (!required)
                    continue;
                var mast = required.GetComponent<Mast>();
                // Square-only physical masts are valid too; exclude horizontal
                // spars/bowsprits geometrically, not by their names/categories.
                if (
                    mast
                    && !mast.onlyStaysails
                    && Math.Abs(
                        Vector3.Dot(
                            mast.transform.forward,
                            option.GetComponent<Mast>().shipRigidbody.transform.up
                        )
                    ) > 0.8f
                )
                    yield return mast;
            }
        }

        // Ordered aft-to-forward by proximity to the donor's upper endpoint.
        private static Mast[] OrderedPhysicalMasts(Mast source, BoatRefs boat)
        {
            var origin = boat.transform.InverseTransformPoint(source.transform.position);
            return PhysicalMasts(source.GetComponent<BoatPartOption>())
                .Distinct()
                .OrderBy(m =>
                    StayGeometry.HorizontalDistanceSquared(
                        origin,
                        boat.transform.InverseTransformPoint(m.transform.position)
                    )
                )
                .ToArray();
        }

        private void Create(Mast[] dependencies)
        {
            aftMast = dependencies[0];
            foreMast = dependencies[1];
            // Parent directly to the boat: an optional donor rigging container
            // must not disable this independent stay. This also makes the native
            // Mast's localPosition-based hinge anchor a boat-local coordinate.
            var root = NewInactive($"FishermansStay_{source.orderIndex}", boat.transform);
            Mount = root.AddComponent<Mast>();
            Mount.orderIndex = StayGeometry.MountIndex(source.orderIndex);
            Mount.maxSails = 1;
            Mount.onlyStaysails = true;
            Mount.shipRigidbody = source.shipRigidbody;
            Mount.startSailPrefab = null;
            Mount.startSailPrefabs = new GameObject[0];
            Mount.startSailsHeightOffsets = new float[0];
            Mount.sails = new List<GameObject>();
            Mount.mastCols = (source.mastCols ?? new CapsuleCollider[0]).ToArray();
            Mount.startingSailColor = source.startingSailColor;
            Mount.leftAngleWinch = CloneWinches(source.leftAngleWinch, "Port sheet");
            Mount.rightAngleWinch = CloneWinches(source.rightAngleWinch, "Starboard sheet");
            Mount.midAngleWinch = CloneWinches(source.midAngleWinch, "Sheet");
            var halyardMast = isMizzen ? aftMast : foreMast;
            Mount.reefWinch = CloneWinches(halyardMast.reefWinch, "Furl");
            if (
                Mount.reefWinch.Length == 0
                || (
                    Mount.midAngleWinch.Length == 0
                    && (Mount.leftAngleWinch.Length == 0 || Mount.rightAngleWinch.Length == 0)
                )
            )
                throw new InvalidOperationException(
                    "The source stay does not provide a complete set of sail controls."
                );
            Mount.midRopeAtt = CloneAnchors(source.midRopeAtt, "Sheet attachment");
            Mount.mastReefAtt = new[] { NewHalyardAnchor("Halyard mast guide") };
            Mount.mastReefAttExtension = new[] { NewHalyardAnchor("Halyard upper guide") };

            visual = CopyGeometry(source.transform, root.transform, true);
            if (visual.GetComponentsInChildren<MeshRenderer>(true).Length == 0)
                throw new InvalidOperationException(
                    "The source stay has no static rope mesh to copy."
                );
            WalkObject = NewInactive(root.name + " Walk", boat.walkCol);
            walkVisual = CopyGeometry(source.walkColMast, WalkObject.transform);
            Mount.walkColMast = WalkObject.transform;
            var original = source.GetComponent<BoatPartOption>();
            Option = root.AddComponent<BoatPartOption>();
            Option.optionName = DisplayName + " (" + original.optionName + ")";
            Option.basePrice = original.basePrice;
            Option.installCost = original.installCost;
            Option.mass = original.mass;
            Option.requires = FilterDependencies(original.requires);
            Option.requiresDisabled = FilterDependencies(original.requiresDisabled);
            Option.childOptions = new GameObject[0];
            Option.childMast = Mount;
            Option.walkColObject = WalkObject;
            Refresh();
        }

        private static List<BoatPartOption> FilterDependencies(List<BoatPartOption> dependencies) =>
            (dependencies ?? new List<BoatPartOption>())
                .Where(o => o && !(o.GetComponent<Mast>() && o.GetComponent<Mast>().onlyStaysails))
                .ToList();

        private Transform NewHalyardAnchor(string name)
        {
            var anchor = new GameObject(name).transform;
            anchor.SetParent(Mount.transform, false);
            return anchor;
        }

        internal void UpdateHalyard(Sail sail, Transform[] attachments)
        {
            var connections = sail.GetComponent<SailConnections>();
            var guide = connections.mastReefAttachment;
            var upperGuide = connections.mastReefAttExtension;
            if (!connections.reefController || !guide || !upperGuide)
                return;
            // Set an explicit winch -> mast guides -> upper sail corner route.
            // Shipyard Expansion can swap the two donor guide references in Awake.
            guide.SetParent(Mount.mastReefAtt[0], false);
            guide.localPosition = Vector3.zero;
            upperGuide.SetParent(Mount.mastReefAttExtension[0], false);
            upperGuide.localPosition = Vector3.zero;
            connections.reefController.GetComponent<RopeEffect>().attachment = guide;
            guide.GetComponent<RopeEffect>().attachment = upperGuide;
            upperGuide.GetComponent<RopeEffect>().attachment = attachments[isMizzen ? 1 : 0];
            var winch = Mount.reefWinch[0].transform;
            connections.reefController.transform.position = winch.position + winch.right * 0.06f;
        }

        internal Vector3 ForeAttachment(Vector3 requestedWorld)
        {
            PhysicalSegment(foreMast, out var bottom, out var top);
            float height = boat.transform.InverseTransformPoint(requestedWorld).y;
            return boat.transform.TransformPoint(StayGeometry.AtHeight(bottom, top, height));
        }

        internal void Refresh()
        {
            try
            {
                Vector3 foreBottom,
                    foreTop,
                    aftBottom,
                    aftTop;
                PhysicalSegment(foreMast, out foreBottom, out foreTop);
                PhysicalSegment(aftMast, out aftBottom, out aftTop);
                Vector3 fore,
                    aft;
                StayGeometry.ForemastAttachments(
                    boat.transform.InverseTransformPoint(
                        (isMizzen ? aftMast : foreMast).transform.position
                    ),
                    foreBottom,
                    foreTop,
                    aftBottom,
                    aftTop,
                    out fore,
                    out aft
                );
                float span = StayGeometry.Span(aft, fore);
                Fits =
                    StayGeometry.SupportsHeight(foreBottom, foreTop, aft.y)
                    && StayGeometry.SupportsHeight(aftBottom, aftTop, aft.y);
                UnavailableReason = Fits
                    ? null
                    : "Both masts must reach the "
                        + (isMizzen ? "mizzenmast" : "foremast")
                        + " upper sail-mount height.";
                var forward = boat.transform.TransformDirection((aft - fore).normalized);
                // Preserve the donor mount's roll. Cloth mesh axes are not the
                // mount axes: forcing mount +X downward flips the stock rig.
                var rotation = Quaternion.LookRotation(
                    forward,
                    StayGeometry.FrameUp(source.transform.up, forward)
                );
                var worldAft = boat.transform.TransformPoint(aft);
                bool moved =
                    Mount.transform.position != worldAft || Mount.transform.rotation != rotation;
                // Winch rotations are live input state. Preserve them while the
                // mount moves; never synchronize them to the donor's sheet input.
                var controlRotations = controls.Select(p => p.Item2.transform.rotation).ToArray();
                Mount.transform.SetPositionAndRotation(worldAft, rotation);
                Mount.transform.localScale = Vector3.one;
                Mount.mastHeight = span;
                visual.localScale = new Vector3(
                    source.transform.localScale.x,
                    source.transform.localScale.y,
                    span / source.mastHeight
                );
                WalkObject.transform.SetPositionAndRotation(
                    source.walkColMast.TransformPoint(
                        source.transform.InverseTransformPoint(worldAft)
                    ),
                    source.walkColMast.rotation
                        * Quaternion.Inverse(source.transform.rotation)
                        * rotation
                );
                WalkObject.transform.localScale = source.walkColMast.localScale;
                walkVisual.localScale = new Vector3(1f, 1f, span / source.mastHeight);
                foreach (var pair in anchors)
                    pair.Item2.SetPositionAndRotation(pair.Item1.position, pair.Item1.rotation);
                var halyardMast = isMizzen ? aftMast : foreMast;
                var guidePoint = isMizzen ? aft : fore;
                PhysicalSegment(halyardMast, out var guideBottom, out var guideTop);
                Mount.mastReefAtt[0].position = boat.transform.TransformPoint(guidePoint);
                Mount.mastReefAttExtension[0].position = boat.transform.TransformPoint(
                    StayGeometry.AtHeight(guideBottom, guideTop, guidePoint.y + 0.15f)
                );
                var towardsFore = Vector3
                    .ProjectOnPlane(
                        foreMast.transform.position - aftMast.transform.position,
                        boat.transform.up
                    )
                    .normalized;
                for (int i = 0; i < controls.Count; i++)
                    controls[i]
                        .Item2.transform.SetPositionAndRotation(
                            controls[i].Item1.transform.position + towardsFore * 0.35f,
                            controlRotations[i]
                        );
                if (moved && Mount.sails.Count > 0)
                    Mount.UpdateControllerAttachments();
                Option.canInstall = Fits;
            }
            catch (Exception exception)
            {
                Fits = false;
                UnavailableReason = exception.Message;
                if (Option)
                    Option.canInstall = false;
                Plugin.Log.LogWarning(
                    $"Fisherman stay {source.name} is unavailable: {exception.Message}"
                );
            }
        }

        private void PhysicalSegment(Mast mast, out Vector3 bottom, out Vector3 top)
        {
            var collider = mast.GetComponent<CapsuleCollider>();
            if (!collider)
                throw new InvalidOperationException(
                    $"{mast.name} has no physical spar capsule to locate its tip."
                );
            var axis =
                collider.direction == 0 ? Vector3.right
                : collider.direction == 1 ? Vector3.up
                : Vector3.forward;
            // Use the physical spar's full extent, not mastHeight (sail capacity).
            bottom = boat.transform.InverseTransformPoint(
                collider.transform.TransformPoint(collider.center - axis * collider.height * 0.5f)
            );
            top = boat.transform.InverseTransformPoint(
                collider.transform.TransformPoint(collider.center + axis * collider.height * 0.5f)
            );
            if (bottom.y > top.y)
            {
                var swap = bottom;
                bottom = top;
                top = swap;
            }
        }

        private GPButtonRopeWinch[] CloneWinches(GPButtonRopeWinch[] sources, string label)
        {
            if (sources == null || sources.Length == 0)
                return new GPButtonRopeWinch[0];
            var sourceWinch = sources[0];
            if (
                !sourceWinch
                || !sourceWinch.GetComponent<Renderer>()
                || !sourceWinch.GetComponent<Collider>()
            )
                throw new InvalidOperationException(
                    "A source winch has no usable renderer/collider."
                );
            var clone = Object.Instantiate(sourceWinch.gameObject, Mount.transform, false);
            clone.name = "Fisherman's " + label;
            // Keep the new controls beside their donor controls on deck. The
            // inactive mount prevents Awake from seeing the donor's live rope.
            var towardsFore = foreMast.transform.position - aftMast.transform.position;
            towardsFore = Vector3.ProjectOnPlane(towardsFore, boat.transform.up).normalized;
            clone.transform.SetPositionAndRotation(
                sourceWinch.transform.position + towardsFore * 0.35f,
                sourceWinch.transform.rotation
            );
            clone.transform.localScale = sourceWinch.transform.lossyScale;
            var winch = clone.GetComponent<GPButtonRopeWinch>();
            winch.rope = null;
            controls.Add(Tuple.Create(sourceWinch, winch));
            if (winch.rotHandle && !winch.rotHandle.transform.IsChildOf(clone.transform))
            {
                winch.rotHandle = Object.Instantiate(winch.rotHandle, clone.transform, false);
                winch.rotHandle.transform.localPosition = Vector3.zero;
            }
            if (winch.rotHandle)
                winch.rotHandle.rotatable = clone.transform;
            clone.SetActive(true);
            return new[] { winch };
        }

        private Transform[] CloneAnchors(Transform[] sources, string label)
        {
            if (sources == null || sources.Length == 0 || !sources[0])
                return new Transform[0];
            var clone = new GameObject("Fisherman's " + label).transform;
            clone.SetParent(Mount.transform, false);
            clone.SetPositionAndRotation(sources[0].position, sources[0].rotation);
            anchors.Add(Tuple.Create(sources[0], clone));
            return new[] { clone };
        }

        // Copy static geometry only. Instantiating a fitted Mast would also copy
        // live sails, script state, and references to the original winches.
        private static Transform CopyGeometry(
            Transform source,
            Transform parent,
            bool mastVisual = false
        )
        {
            var root = new GameObject("Stay geometry").transform;
            root.SetParent(parent, false);
            var nodes = new Dictionary<Transform, Transform> { [source] = root };
            foreach (var old in source.GetComponentsInChildren<Transform>(true))
            {
                if (old != source && old.GetComponentInParent<Sail>())
                    continue;
                if (!nodes.TryGetValue(old, out var node))
                {
                    if (!nodes.TryGetValue(old.parent, out var parentNode))
                        continue;
                    node = new GameObject(old.name).transform;
                    node.SetParent(parentNode, false);
                    node.localPosition = old.localPosition;
                    node.localRotation = old.localRotation;
                    node.localScale = old.localScale;
                    node.gameObject.SetActive(old.gameObject.activeSelf);
                    nodes.Add(old, node);
                }
                node.gameObject.layer = old.gameObject.layer;
                var filter = old.GetComponent<MeshFilter>();
                var renderer = old.GetComponent<MeshRenderer>();
                if (filter && renderer)
                {
                    node.gameObject.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                    var copy = node.gameObject.AddComponent<MeshRenderer>();
                    copy.sharedMaterials = renderer.sharedMaterials;
                    copy.shadowCastingMode = renderer.shadowCastingMode;
                    copy.receiveShadows = renderer.receiveShadows;
                }
                foreach (var collider in old.GetComponents<Collider>())
                {
                    Collider copy;
                    if (collider is CapsuleCollider capsule)
                    {
                        var c = node.gameObject.AddComponent<CapsuleCollider>();
                        c.center = capsule.center;
                        c.radius = capsule.radius;
                        c.height = capsule.height;
                        c.direction = capsule.direction;
                        copy = c;
                    }
                    else if (collider is BoxCollider box)
                    {
                        var c = node.gameObject.AddComponent<BoxCollider>();
                        c.center = box.center;
                        c.size = box.size;
                        copy = c;
                    }
                    else if (collider is MeshCollider mesh)
                    {
                        var c = node.gameObject.AddComponent<MeshCollider>();
                        c.sharedMesh = mesh.sharedMesh;
                        c.convex = mesh.convex;
                        copy = c;
                    }
                    else if (collider is SphereCollider sphere)
                    {
                        var c = node.gameObject.AddComponent<SphereCollider>();
                        c.center = sphere.center;
                        c.radius = sphere.radius;
                        copy = c;
                    }
                    else
                        throw new InvalidOperationException(
                            $"Unsupported stay collider: {collider.GetType().Name}."
                        );
                    copy.sharedMaterial = collider.sharedMaterial;
                    // Mast.Awake normally makes the source's root collider a
                    // trigger. Our visual is a child of the new mount instead.
                    copy.isTrigger = collider.isTrigger || (mastVisual && old == source);
                    copy.enabled = collider.enabled;
                }
            }
            return root;
        }

        private static GameObject NewInactive(string name, Transform parent)
        {
            var result = new GameObject(name);
            result.SetActive(false);
            result.transform.SetParent(parent, false);
            return result;
        }
    }
}
