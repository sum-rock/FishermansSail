using System;
using System.Collections.Generic;
using System.Linq;
using FishermansSail.BoatRigs;
using FishermansSail.Stays.FishermansStay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FishermansSail.Sails.FishermansStaysail
{
    // Save ownership stays on the registered stay; the physical pivot is the fore mast.
    internal sealed class FishermansStaysailRigging : MonoBehaviour
    {
        internal sealed class MountPair
        {
            internal BoatRefs Boat;
            internal Mast Mount,
                Fore,
                Aft,
                ForeBase,
                SheetControlSource;
            internal FishermansStay Stay;
            internal Mast[] ForeSections;
            internal Transform AftGuide;
            internal bool Active => Mount && Mount.gameObject.activeInHierarchy && Stay.Available;
        }

        internal MountPair Pair { get; private set; }
        private Sail sail;
        private GameObject controlsRoot;
        private GPButtonRopeWinch[] controls;
        private Transform mastGuide,
            upperGuide;
        private bool bindingDirty;
        private int controlSlot = -1;

        internal void Invalidate() => bindingDirty = true;

        internal static FishermansStaysailRigging For(Sail sail)
        {
            var rig =
                sail.GetComponent<FishermansStaysailRigging>()
                ?? sail.gameObject.AddComponent<FishermansStaysailRigging>();
            rig.sail = sail;
            return rig;
        }

        internal static bool TryResolve(Mast mount, out MountPair pair)
        {
            pair = null;
            if (
                !FishermansStayRegistry.TryFind(mount, out var stay)
                || !stay.Available
                || !mount.gameObject.activeInHierarchy
            )
                return false;
            var refs = stay.References;
            var boat = mount.GetComponentInParent<BoatRefs>();
            var profile = BoatRigCatalog.Find(boat.name);
            int baseId = FishermansStaysailDefinitions.ForeBase(
                profile.BoatName,
                refs.Fore.orderIndex
            );
            var foreSections = FishermansStaysailDefinitions
                .ForeSections(profile.BoatName, refs.Fore.orderIndex)
                .Select(id => boat.masts[id])
                .ToArray();
            var foreBase = boat.masts[baseId];
            if (
                foreSections.Any(m => !m || !m.gameObject.activeInHierarchy)
                || !foreBase
                || !foreBase.gameObject.activeInHierarchy
                || !FirstControl(foreBase.reefWinch)
                || !FirstControl(refs.Donor.leftAngleWinch)
                || !FirstControl(refs.Donor.rightAngleWinch)
            )
                return false;
            pair = new MountPair
            {
                Boat = boat,
                Mount = mount,
                Stay = stay,
                Fore = refs.Fore,
                Aft = refs.Aft,
                ForeBase = foreBase,
                ForeSections = foreSections,
                SheetControlSource = refs.Donor,
                AftGuide = refs.Guide,
            };
            return true;
        }

        private static GPButtonRopeWinch FirstControl(GPButtonRopeWinch[] winches) =>
            winches?.FirstOrDefault(w =>
                w && w.GetComponent<Renderer>() && w.GetComponent<Collider>()
            );

        internal bool Bind(Mast mast)
        {
            // A removal preview temporarily enables two mutually exclusive options.
            // Keep the current support until that order finishes, so the removal
            // guard cannot be evaded by silently rebinding to the preview option.
            if (
                Pair != null
                && Pair.Mount == mast
                && Pair.Active
                && (!bindingDirty || GameState.currentShipyard)
            )
                return true;
            if (!TryResolve(mast, out var resolved))
                return false;
            bool changed =
                Pair == null
                || Pair.Mount != resolved.Mount
                || Pair.Aft != resolved.Aft
                || Pair.AftGuide != resolved.AftGuide;
            Pair = resolved;
            bindingDirty = false;
            if (changed)
                Plugin.Log.LogInfo(
                    $"FishermansStaysail mast rig: boat={Pair.Boat.name}, fore={Pair.Fore.orderIndex}, aft={Pair.Aft.orderIndex}, upperGuide={Pair.AftGuide.parent.name}/{Pair.AftGuide.name}."
                );
            return true;
        }

        internal static string InstallError(Sail sail, Mast mast)
        {
            if (!TryResolve(mast, out var pair))
                return "(REQUIRES AN ACTIVE FISHERMAN'S STAY)";
            var rig = sail.GetComponent<FishermansStaysailRig>();
            if (!rig || !rig.RefreshFrame())
                return "(STAYSAIL RIG NOT READY)";
            var binding = For(sail);
            binding.ForeSailFrame(out var head, out var axis);
            var scale = sail.cloth.transform.parent.localScale;
            float width = -rig.Corners[0].z * scale.z;
            var bottom = binding.SectionBottom;
            float room = Vector3.Dot(head - bottom, axis);
            float span = Vector3
                .ProjectOnPlane(binding.AftPoint - binding.ForePoint, axis)
                .magnitude;
            return FishermansStaysailInstallationGeometry.FitError(
                width,
                -rig.Corners[2].x * scale.x,
                mast.mastHeight - sail.GetCurrentInstallHeight(),
                room,
                span
            );
        }

        internal Vector3 ForePoint =>
            Pair.Fore.transform.TransformPoint(Pair.Stay.References.Definition.ForePoint);
        internal Vector3 AftPoint =>
            Pair.Aft.transform.TransformPoint(Pair.Stay.References.Definition.AftPoint);
        internal Vector3 ForeAxis
        {
            get
            {
                var c = Pair.Fore.GetComponent<CapsuleCollider>();
                var axis = c.transform.TransformDirection(
                    c.direction == 0 ? Vector3.right
                    : c.direction == 1 ? Vector3.up
                    : Vector3.forward
                );
                return Vector3.Dot(axis, Pair.Boat.transform.up) < 0 ? -axis : axis;
            }
        }
        internal Vector3 SectionBottom
        {
            get
            {
                var c = Pair.Fore.GetComponent<CapsuleCollider>();
                var center = c.transform.TransformPoint(c.center);
                var direction =
                    c.direction == 0 ? Vector3.right
                    : c.direction == 1 ? Vector3.up
                    : Vector3.forward;
                return center
                    - ForeAxis * c.transform.TransformVector(direction * c.height * 0.5f).magnitude;
            }
        }
        internal float HeadSlope =>
            FishermansStaysailInstallationGeometry.HeadSlope(AftPoint - ForePoint, ForeAxis);

        internal void ForeSailFrame(out Vector3 point, out Vector3 axis)
        {
            axis = ForeAxis;
            point =
                ForePoint
                - axis
                    * (
                        FishermansStaysailInstallationGeometry.HeadClearance
                        + Pair.Mount.mastHeight
                        - sail.GetCurrentInstallHeight()
                    );
        }

        internal Vector3 AftReference => AftPoint;

        // Reefing stays on the fitted section: the foot travels up to the head.
        internal Vector3 LuffPoint(Vector3 requested) =>
            ForePoint + ForeAxis * Vector3.Dot(requested - ForePoint, ForeAxis);

        internal void AttachControls()
        {
            var mast = sail.transform.parent ? sail.transform.parent.GetComponent<Mast>() : null;
            if (!Bind(mast))
                return;
            if (!controlsRoot)
            {
                var used = Pair
                    .Mount.sails.Where(s => s)
                    .Select(s => s.GetComponent<FishermansStaysailRigging>())
                    .Where(r => r && r != this && r.controlSlot >= 0)
                    .Select(r => r.controlSlot)
                    .ToArray();
                controlSlot = 0;
                while (used.Contains(controlSlot))
                    controlSlot++;
                controlsRoot = new GameObject("FishermansStaysail independent controls");
                controlsRoot.SetActive(false);
                controlsRoot.transform.SetParent(Pair.Boat.transform, false);
                controls = new[]
                {
                    CopyWinch(FirstControl(Pair.ForeBase.reefWinch), "Hoist"),
                    CopyWinch(FirstControl(Pair.SheetControlSource.leftAngleWinch), "Port sheet"),
                    CopyWinch(
                        FirstControl(Pair.SheetControlSource.rightAngleWinch),
                        "Starboard sheet"
                    ),
                };
                mastGuide = new GameObject("FishermansStaysail halyard guide").transform;
                mastGuide.SetParent(controlsRoot.transform, false);
                upperGuide = new GameObject("FishermansStaysail upper halyard guide").transform;
                upperGuide.SetParent(controlsRoot.transform, false);
                controlsRoot.SetActive(true);
            }
            var connections = sail.GetComponent<SailConnections>();
            controls[0].AttachToController(connections.reefController);
            controls[1].AttachToController(connections.angleControllerLeft);
            controls[2].AttachToController(connections.angleControllerRight);
            foreach (var control in controls)
                control.ShowWinch(true);
            connections.colChecker.RegisterBoatWalkCol(mast.walkColMast);
            // Old saves can restore the wider donor range; keep the checker,
            // shipyard description and native sail limits in agreement.
            connections.colChecker.colAngleMin = FishermansStaysailTravel.Clamp(
                connections.colChecker.colAngleMin
            );
            connections.colChecker.colAngleMax = FishermansStaysailTravel.Clamp(
                connections.colChecker.colAngleMax
            );
            sail.minAngle = connections.colChecker.colAngleMin;
            sail.maxAngle = connections.colChecker.colAngleMax;
        }

        private GPButtonRopeWinch CopyWinch(GPButtonRopeWinch source, string label)
        {
            var clone = Object.Instantiate(source.gameObject, controlsRoot.transform, false);
            clone.name = "FishermansStaysail " + label;
            var towardsFore = Vector3
                .ProjectOnPlane(
                    Pair.Fore.transform.position - Pair.Aft.transform.position,
                    Pair.Boat.transform.up
                )
                .normalized;
            clone.transform.SetPositionAndRotation(
                source.transform.position + towardsFore * (0.35f * (controlSlot + 2)),
                source.transform.rotation
            );
            clone.transform.localScale = source.transform.lossyScale;
            var winch = clone.GetComponent<GPButtonRopeWinch>();
            winch.rope = null;
            if (winch.rotHandle && !winch.rotHandle.transform.IsChildOf(clone.transform))
            {
                winch.rotHandle = Object.Instantiate(winch.rotHandle, clone.transform, false);
                winch.rotHandle.transform.localPosition = Vector3.zero;
            }
            if (winch.rotHandle)
                winch.rotHandle.rotatable = clone.transform;
            clone.SetActive(true);
            return winch;
        }

        internal void UpdateHalyard(Transform[] attachments, bool struck)
        {
            if (!controlsRoot)
                AttachControls();
            if (!controlsRoot)
                return;
            mastGuide.position = ForePoint;
            upperGuide.position = ForePoint + ForeAxis * 0.05f;
            var connections = sail.GetComponent<SailConnections>();
            var guide = connections.mastReefAttachment;
            var upper = connections.mastReefAttExtension;
            guide.SetParent(mastGuide, false);
            guide.localPosition = Vector3.zero;
            upper.SetParent(upperGuide, false);
            upper.localPosition = Vector3.zero;
            connections.reefController.GetComponent<RopeEffect>().attachment = guide;
            guide.GetComponent<RopeEffect>().attachment = upper;
            upper.GetComponent<RopeEffect>().attachment = attachments[0];
        }

        internal bool DependsOn(BoatPartOption option)
        {
            if (Pair == null)
                return false;
            return new[] { Pair.Mount, Pair.Fore, Pair.Aft, Pair.ForeBase }.Any(m =>
                m && (m.GetComponent<BoatPartOption>() == option || option.childMast == m)
            );
        }

        private void OnDestroy()
        {
            if (controlsRoot)
                Object.Destroy(controlsRoot);
        }
    }
}
