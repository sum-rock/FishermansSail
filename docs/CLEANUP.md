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

- [ ] Implementation and applicable automated checks complete.

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

**Progress:** Not started. Checks run: none for this item. In-game validation:
pending. Implementation completed: —.

### CLEANUP-2 — Extend behavior checks to both staysail marks

- [ ] Implementation and applicable automated checks complete.

**Finding:** The detailed fixed-head, tack, sheet-angle and partial-reef sweeps
instantiate Mk.A. Mk.B has cut and geometry checks, but its different foot and
leech lengths warrant the same behavioral sweeps. Some shared sheet checks still
describe the older upper-head response. The Flying Sail assembly check reports
control-list restoration after checking for a finalizer attribute, rather than
exercising restoration after an exception.

**References:** [Mk.A fixed-head checks](../tests/GeometryChecks/FishermansStaysail/MkA/FixedHeadChecks.cs),
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

**Progress:** Not started. Checks run: none for this item. In-game validation:
not established by these tests. Implementation completed: —.

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
