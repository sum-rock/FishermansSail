# Architecture cleanup checklist

Recorded after the architecture review on 2026-09-23 for version **0.1.0**.
This is a work queue for focused changes before release and future bug fixes.
Recording this plan does not implement any cleanup or change release metadata.

## Working through the checklist

Use the stable IDs below when requesting work, for example “tackle CLEANUP-2.”
Work on one requested item at a time; this document does not authorize executing
the entire queue. Prioritize CLEANUP-1 and CLEANUP-2 before release.

Check an item only when its implementation and applicable automated checks are
complete. Record in-game validation separately: a checked implementation box
does not mean the behavior has been observed in Unity. For each handoff, update
the entry with the changes, checks actually run, remaining validation and
implementation completion date. Preserve findings and decisions for context.

Preserve successful sail mechanics, authored boat attachment references, Unity
serialized fields, plugin identity and prefab IDs **400**, **401** and **402**.
Do not swap the live Cloth mesh or merge the sail families' distinct mounting,
reefing and lifecycle behavior. Follow the safeguards in [AGENTS.md](../AGENTS.md).

## Review baseline

The review found a clean working tree and made no code changes. The Release
build, CSharpier check, GeometryChecks, AssemblyChecks and `git diff --check`
passed during that review. These are historical results, not validation of
future cleanup changes. Neither suite simulates Unity Cloth. Mixed-sail winch
placement and live cloth behavior were not validated in-game during the review.

## Active items

### CLEANUP-1 — Coordinate winch placement and cloning

- [x] Implementation and applicable automated checks complete.

**Finding:** Flying sails, custom staysails and stay-owned controls allocate
positions independently. For a shared donor and mast direction, the second
Flying Sail control and first custom staysail control both use a 0.70 m offset.
Stay-owned controls use 0.35 m, matching the first Flying Sail control. This is
a code-supported overlap risk, not an in-game reproduction. Clone setup also
differs: both custom sail paths reset inherited outlines, while the stay path
does not. Different initialization timing means this alone does not prove an
outline bug in the stay path.

**References:** [Flying Sail rigging](../Sails/FishermansFlyingSail/FishermansFlyingSailRigging.cs),
[staysail rigging](../Sails/FishermansStaysail/FishermansStaysailRigging.cs),
[stay controls](../Stays/FishermansStay/FishermansStay.cs).

**Change:** Use a boat-owned allocator keyed by the actual donor winch, with
reservations released on teardown or source changes. Consolidate common clone
initialization, including rope ownership, external rotation handles and outline
reset. Keep family-specific routing and lifecycle behavior. Account for inactive
stay variants and shipyard previews so they do not exhaust control positions.

**Acceptance:** Test allocation, release and donor changes. In-game, verify mixed
sail families, multiple sails, vanilla sails on Fisherman's Stays, removal and
reinstallation, save/reload and outlines after boat movement. Controls should
remain accessible, distinct and attached to the correct sail.

**Progress:** Implemented **2026-09-24** in version **0.1.0**. All three control
paths now use a boat-owned allocator and inactive clone factory in
[`Controls/`](../Controls/). Reservations use actual donor identity, reject nearby
native/owned controls, retain occupied slots and release unused/removed controls.
Donor replacement preserves sail-owned controllers; mount transforms are separate
from the wheel rotation used by native input. Stay variants without bound ropes
consume no slots. External handles and cloned outlines share the same ownership
and initialization policy.

The user also reported floating and inward-facing staysail winches on several
boats. The previous horizontal offsets could leave a mast surface or cross it
without rotating the face. Authored boat-local placement directions now cover
**151 donor/role mappings across seven boats**, measured from installed assets.
Mast controls prefer vertical stacks, then other faces around the authored mast
axis, rotating position and facing together. Native interaction sizes determine
spacing; height and surface travel are bounded. Deck/rail donors retain their
native mounting orientation and use surface tangents. This is a correction based
on code and asset measurements, not a confirmed reproduction of every report.
Exhausted candidates leave the control hidden with a diagnostic and retry;
sail-owned controllers remain active rather than being destroyed or disabled.

**Checks:** CSharpier formatting/check, Release build, GeometryChecks,
AssemblyChecks and `git diff --check` pass. New executed checks cover shared
allocation, release/reuse, donor changes, exhaustion, cross-donor overlap,
cancellation-style release, all profile references and 151 numeric donor fixtures
(including native sizes, mast radii, outward-face preservation and boat rotation).
Each measured donor supports at least three additional controls in isolation.
Assembly checks inspect clone initialization, input-rotation separation and the
three lifecycle integrations; they do not execute Unity activation or previews.

**In-game validation:** Pending on all seven boats. Start with Brig, then check
both marks and mixed families, vanilla sails on Fisherman's Stays, multiple sails,
boat movement, removal/reinstallation, valid/invalid previews and cancellation,
save/reload, mouse/VR handles, outlines and rope continuity. Surface accessibility,
clearance from other boat geometry and the reported placement corrections need
visual confirmation; the fixture checks do not establish those outcomes.
Implementation completed: **2026-09-24**.

**Organizational follow-up (2026-09-24):** Consolidated authored supports, stay
variants, mast ancestry and winch mounts into one static class/file per boat in
[`BoatRigs/`](../BoatRigs/). Shared types and the small catalog now live in
[`Definitions.cs`](../BoatRigs/Definitions.cs); placement calculations live in
[`WinchPlacementGeometry.cs`](../Controls/WinchPlacementGeometry.cs). Runtime
consumers use the selected complete boat profile. Removed the scattered tables
and redundant lookup wrappers. Before/after snapshots match exactly for all
seven boats, 97 stays, 151 winch mappings, 69 mast section chains and 453 placement
cases, preserving registration order and numeric values. CSharpier, the Release
build, GeometryChecks, AssemblyChecks and `git diff --check` pass. Added profile
lookup and malformed-ancestry checks. Version remains **0.1.0**; the in-game
validation above remains pending.

### CLEANUP-2 — Extend behavior checks to both staysail marks

- [x] Implementation and applicable automated checks complete.

**Finding:** The detailed fixed-head, tack, sheet-angle and partial-reef sweeps
instantiate Mk.A. Mk.B has cut and geometry checks, but its different foot and
leech lengths warrant the same behavioral sweeps. Some shared sheet checks still
describe the older upper-head response. The Flying Sail assembly check reports
control-list restoration after checking for a finalizer attribute, rather than
exercising restoration after an exception.

**References:** [family fixed-head checks](../tests/GeometryChecks/FishermansStaysail/FixedHeadChecks.cs),
[Mk.B cut checks](../tests/GeometryChecks/FishermansStaysail/MkB/CutChecks.cs),
[Flying Sail patch checks](../tests/AssemblyChecks/FishermansFlyingSail/PatchChecks.cs).

**Change:** Move shared behavior checks to the staysail family directory and
parameterize them over both cut factories. Keep mark-specific geometry and
identity checks in the mark directories. Cover the deployed 14° head policy,
reef-dependent reduction, both tacks, sheet travel and coupled edge budgets.
Strengthen exception-path list restoration checks where feasible in the
standalone environment; otherwise narrow the reported claims to what is checked.

**Acceptance:** Both marks run the same applicable behavior matrix. Current
policy assertions replace obsolete expectations. Test output distinguishes
structural checks from executed behavior. Both suites pass without claiming
Unity Cloth stability or in-game validation.

**Progress:** Complete. A family fixture runs both cut factories through the same
fixed-head, sheet, reef and weighted-skin sweeps at widths 3/6.9/13.8, slopes
20°/35°/55°, both tacks and sheets from −40° to +40° in 5° steps. Reef cases
include the bundle threshold, partial/full deployment and halyard reversals.
Current-policy assertions cover the 14° × unroll head, its fixed height/span,
sheet independence, constrained edges and mirrored skin/triangle lengths.
Geometry-independent tack, travel and billow helpers run once. Cut/identity
checks remain mark-specific; retained optional trim tests explicitly identify
dormant helper coverage, pending CLEANUP-3.

**Checks:** CSharpier formatting/check, Release build (zero warnings/errors),
GeometryChecks, AssemblyChecks and `git diff --check` pass. Both Flying Sail and
staysail control-finalizer checks now inspect the target/signature, assignment
of the saved list to `Mast.sails` and subsequent native order refresh. Output
distinguishes executed geometry from structural assembly checks. The populated
list's order refresh accesses Unity transforms/components, so standalone checks
do not execute exception recovery. Version **0.1.0** and production code are
unchanged. Implementation completed: **2026-09-24**.

**In-game validation:** Not established by these tests. Cloth stability,
control recovery after an exception and actual sail behavior remain pending
in-game validation; no installed game files or saves were changed.

### CLEANUP-3 — Retire dormant upper-corner strategies

- [ ] Implementation and applicable automated checks complete.

**Finding:** Both shipped marks select the fixed 14° upper-head policy, reduced
during reefing. The alternative sheet-following branch and optional upper-corner
shortening machinery remain for hypothetical future policies. Their retention
was intentional, but makes the current runtime harder to follow. Tests also
enforce some of this dormant plumbing.

**References:** [staysail rig](../Sails/FishermansStaysail/FishermansStaysailRig.cs),
[shape policy](../Sails/FishermansStaysail/FishermansStaysailShape.cs),
[upper trim helper](../Sails/FishermansStaysail/FishermansStaysailUpperTrim.cs).

**Change:** Make the current head policy explicit and remove unused strategies
and their obsolete tests. Preserve the active support-bow and coupled tension
calculations inside the trim helper; do not delete the helper wholesale without
retaining that work. Update historical AGENTS.md guidance that retained these
experiments. Git history remains the reference for discarded approaches.

**Acceptance:** Both marks retain their existing head position, reef response,
lower-sheet behavior and finite tension fallback. The expanded behavior tests
pass. In-game, check both tacks, eased/tight sheets, partial/full reef and
redeployment without resetting Cloth or changing the mesh.

**Progress:** Not started. Checks run: none for this item. In-game validation:
pending. Implementation completed: —.

### CLEANUP-4 — Extract identical shared calculations

- [ ] Implementation and applicable automated checks complete.

**Finding:** The two sail families' tension solvers and aerodynamic frame
helpers are identical after accounting for type names. Appearance and order-text
handling also contain substantial duplication, creating opportunities for fixes
to diverge.

**References:** [Flying Sail helpers](../Sails/FishermansFlyingSail/),
[staysail helpers](../Sails/FishermansStaysail/).

**Change:** Start with small helpers under `Sails/Shared/` for the tension solver
and aerodynamic frame construction. Review appearance and text wrapping for
similarly exact extraction, documenting any duplication intentionally retained.
Keep feature-specific Harmony filters and ordering/input protections. Avoid a
generic rig hierarchy or merging distinct mounting, reefing and cloth behavior.

**Acceptance:** Preserve outputs for existing geometry and wind-frame cases,
finite fallback behavior, appearance defaults and text-guard scope. Run relevant
geometry checks, the Release build and assembly checks. Validate both sail
families in-game for propulsion, appearance and shipyard interactions affected
by the extracted code.

**Progress:** Not started. Checks run: none for this item. In-game validation:
pending. Implementation completed: —.

### CLEANUP-5 — Consolidate staysail variant definitions

- [ ] Implementation and applicable automated checks complete.

**Finding:** Prefab registration accepts a shape component, geometry factory and
object prefix separately, although the shape component also supplies geometry
and its prefix. A future mark could combine mismatched definitions. Mk.B also
obtains the shared donor index from Mk.A.

**References:** [prefab registration](../Sails/FishermansStaysail/FishermansStaysailPrefab.cs),
[shape definition](../Sails/FishermansStaysail/FishermansStaysailShape.cs),
[Mk.B registration](../Sails/FishermansStaysail/MkB/FishermansStaysailMkB.cs).

**Change:** Derive initial template and installed-instance geometry and object
prefixes from the same authoritative shape definition. Move the common donor
index into family code. Retain per-mark names and prefab identities without
introducing a general-purpose plugin registry.

**Acceptance:** Both marks register with their correct names, cuts and IDs;
template geometry and instance initialization agree. Preserve scaling, native
appearance and save compatibility. Run registration and geometry checks and
verify fitting both marks plus existing-save reload in-game.

**Progress:** Not started. Checks run: none for this item. In-game validation:
pending. Implementation completed: —.

## Deferred work

Consider separating inactive-template construction from the large runtime rig
classes after coverage improves. This is outside the active checklist and is
not a release prerequisite. Keep runtime MonoBehaviour state and serialized
references intact; the two families' differing lifecycle requirements are a
reason to avoid merging their entire rigs.
