# MoreSailwindSails development

MoreSailwindSails is a collection of additional sail types for Sailwind.
Fisherman's Staysails (Mk.A/Mk.B/Mk.C) and Fisherman's Flying Sails are the two
current sail families; Fisherman's Stays supply supporting rigging. Additional
families can be developed within the same mod as their own features.

The repository and local checkout are named `MoreSailwindSails`. The project
file is `src/MoreSailwindSails.csproj` and its assembly is `MoreSailwindSails.dll`.
The root `MoreSailwindSails.sln` includes the plugin and both check projects.
The mod is unreleased. The C# namespace is `MoreSailwindSails`, with plugin
GUID `com.august.moresailwindsails` and BepInEx display name `MoreSailwindSails`.

## Environment and build

The Nix flake supports **x86_64 Linux** and supplies the .NET 8 SDK. The plugin
targets Sailwind's `netstandard2.0` API. A local game installation, BepInEx 5 and
Shipyard Expansion are required; no Unity editor or separate asset bundle is
needed.

For a fresh checkout:

```sh
nix develop
dotnet tool restore
dotnet restore
dotnet build -c Release --no-restore
```

The first Nix and NuGet restores require network access. Keep `flake.lock` and
the tool manifest pinned. CSharpier is configured in `.config/dotnet-tools.json`;
run `pre-commit install` inside the development shell to enable the formatting
hook. The hook invokes Nix itself on later commits.

The default game directory is
`~/.local/share/Steam/steamapps/common/Sailwind`. Override it when building with:

```sh
nix develop -c dotnet build -c Release -p:SailwindDir="/path/to/Sailwind"
```

The build references the installed BepInEx, HarmonyX, Shipyard Expansion, game
and Unity assemblies. They are not bundled with the plugin or committed to Git.
The output is `src/bin/Release/netstandard2.0/MoreSailwindSails.dll`.

## Automated checks

After restoring dependencies, run from the repository root:

```sh
nix develop -c bash -c 'dotnet csharpier check . && dotnet build -c Release --no-restore && dotnet run --project tests/GeometryChecks -c Release --no-restore && dotnet run --project tests/AssemblyChecks -c Release --no-restore'
git diff --check
```

Use `dotnet csharpier format .` inside the development shell to fix formatting.
It changes files; `check` only reports differences.

- **GeometryChecks** covers mesh and skinning geometry, coupled tension, shaping,
  travel limits, wind frames, boat profiles, collision bounds, hoisting and text
  wrapping, authored stay endpoints, topmast exclusions, shorter-foremast
  fallbacks, geometry alignment and older shipyard snapshots. All three staysail
  marks run the same fixed-head, sheet, reef and mirrored-skin behavior matrix.
- **AssemblyChecks** validates Harmony targets against installed assemblies,
  texture load paths, patch ordering, control-list restoration structure and restrictions
  on the live Cloth lifecycle, plus stay registration ordering and save capacity.
  Control-finalizer checks inspect the saved-list assignment and native order
  refresh; they do not execute exception recovery against live Unity objects.

For a custom game directory, pass `-p:SailwindDir` to the test builds too.
AssemblyChecks also needs that directory as a runtime argument:

```sh
nix develop -c dotnet run --project tests/AssemblyChecks -c Release -p:SailwindDir="/path/to/Sailwind" -- "/path/to/Sailwind"
```

Neither suite simulates Unity rendering, Cloth, hinge physics or the live
shipyard. Passing checks do not establish stable in-game cloth motion.

## Local installation and in-game verification

Close Sailwind, build, then run from the repository root:

```sh
./scripts/install-local.sh
```

Use `./scripts/install-local.sh "/path/to/Sailwind"` for another installation. This
script copies only the built DLL; it does not build it. Builds and checks do
not replace the installed plugin or change saves.

Confirm the startup message `MoreSailwindSails 0.1.0 loaded!` in
`BepInEx/LogOutput.log`. Flying Sail registration should report donor index
**110**, sail index **400** and **825** vertices. Staysail registrations use
indices **401** (Mk.A), **402** (Mk.B) and **403** (Mk.C). Read
Unity's `Player.log` as well when diagnosing warnings or cloth problems; this
machine's paths are recorded in [AGENTS.md](../AGENTS.md).

Start with the Brig, then repeat relevant checks on another supported boat:

1. Fit the sail under **Other** on the foremast. Check resizing, vertical overlap,
   shroud clearance, ordinary panel obstructions and topmasts present or absent.
2. Check independent sheet and hoist controls alongside other sails and with
   multiple Fisherman's Flying Sails. Canceling shipyard orders must not leave extra
   winches. Removing either supporting mast must be rejected while occupied.
3. Hoist, pause, reverse, partially furl, fully lower and redeploy. Keep all four
   corners and ropes attached, with invisible cloth and parked rope ends when
   fully lowered.
4. Repeat port/starboard tacks, eased/tight sheets and weak-wind conditions.
   Check useful forward force, a flexible leech and smooth camber reversal.
   Fully eased travel should stop near 40° on each side, or sooner if obstructed.
5. Confirm the name **Fisherman's Flying Sail**, white/plain defaults, consistent color
   during partial hoists and a hidden texture selector. Switching to another
   sail must restore its texture options. Test recoloring and save/reload;
   existing colors remain while old patterns become plain.

The mast-mounted rig and 40° travel limit received positive in-game feedback
during development. A fresh 0.1.0 check, including the appearance and naming
changes, remains pending. Record observed results separately from automated
checks.

## Source organization

`src/Plugin.cs` owns the plugin metadata and assembly-wide Harmony registration for
MoreSailwindSails. Each sail family owns its mechanics and game-facing names;
the project rename does not change existing menus or saved sail identities.

- `src/Sails/FishermansFlyingSail/` contains the mast-mounted sail's registration,
  geometry, appearance, cloth rig and controls, using the namespace
  `MoreSailwindSails.Sails.FishermansFlyingSail` and `FishermansFlyingSail` type prefix.
- Its `Patches/` subdirectory contains all feature-specific Harmony patches in
  the corresponding `.Patches` namespace, including registration, appearance
  and the order-text freeze guard.
- `src/BoatRigs/` contains one static class/file per boat in `MoreSailwindSails.BoatRigs`.
  Each exposes a complete `BoatRigDefinition` through `Definition`, with private
  factories for Flying Sail supports, stay variants, mast ancestry and winch mounts.
  `Definitions.cs` holds the shared data types, validation and `BoatRigCatalog`.
  Resolve ancestry and winches through the selected profile (`Sections`, `Base`,
  `WinchMount`); individual winch records inherit boat identity from that profile.
- `src/Controls/` owns shared winch allocation, cloning and placement calculations.
  `WinchPlacementGeometry.cs` uses the authored mounting data without owning any
  boat tables.
- `src/Stays/FishermansStay/` owns the new stays, their independent controls, native
  mount registration, save handling and patches. The namespace is
  `MoreSailwindSails.Stays.FishermansStay`, with `.Patches` for Harmony patches.
- `src/Sails/FishermansStaysail/` owns the staysail family's rig, reefing adapter,
  controls, prefab builder and patches. `MkA/` contains the original 110° cut;
  `MkB/` keeps its head and has a 90° foot; `MkC/` has a foot rising 40°
  toward the aft leech. Each mark supplies its own
  `FishermansStaysailShape` and save identity.

Add future sail families under their own `src/Sails/<Family>/` directory with
corresponding feature tests. Reuse existing mechanics only when their behavior
fits the new sail; the deferred shared-helper cleanup is not a prerequisite.

The geometry checks link feature sources directly; update their project includes
when moving files. The assembly checks resolve internal types by full name;
update those references when renaming types or namespaces. Runtime object and
mesh labels use each family's or mark's prefix; donor hierarchy names remain
unchanged. Existing prefab IDs **400/401/402**, native save slots and version **0.1.0**
are unchanged; Mk.C adds prefab **403**. Existing-save reload after registration
cleanup still needs in-game verification.

### Test organization

Both `tests/GeometryChecks/` and `tests/AssemblyChecks/` contain
`FishermansFlyingSail/`, `FishermansStay/` and `FishermansStaysail/` directories.
The latter has `MkA/`, `MkB/` and `MkC/` for variant checks. Put each feature's
checks and helpers in its directory, using the namespace
`MoreSailwindSails.Tests.<Suite>.<Feature>`. Flying-sail rig-profile checks belong
with the flying sail; authored stay-profile checks belong with the stay.
Shared staysail geometry behavior lives at the family level, parameterized by
`BehaviorCases` over all three mark factories. Keep cut and prefab identity checks
under their marks. Family edge-fit checks cover the active support bow and
finite failure fallback. Test output labels executed behavior and
structural control-finalizer inspection separately.

Root `Program.cs` files handle setup and run the checks. Shared Harmony signature
validation and IL decoding live in `tests/AssemblyChecks/Shared/`, using the
corresponding `.Shared` namespace. The two project paths and validation commands
remain unchanged.

## Flying Sail implementation notes

- Registration creates an independent sail from the brig jib after Shipyard
  Expansion initializes its components. The plugin GUID and prefab index 400
  remain stable for save compatibility.
- Boat profiles resolve physical mast pairs and active pulley guides. Each sail
  owns its controls; native mast capacity, overlap and save rules remain active.
- Only the four corners are pinned. Shaping moves existing bones without
  replacing the live Cloth mesh. Partial hoists use a procedural renderer;
  fully raised sails use Unity Cloth.
- The nominal cut is an isosceles trapezoid: luff `2 × width`, head rising 20°
  aft and foot falling 20° aft, giving an aft edge about `2.728 × width`.
  Width measures fore-to-aft fabric span. Edge budgets come from the actual
  corners. The upper aft corner retains 85% of the sheet angle, with foot and
  leech fitted together.
- The hinge runs through the offset luff, parallel to the fore mast. Each
  corner sits 0.4572 m beyond the mast capsule's surface, connected by its own
  native-styled straight line and separate leaf endpoints. The ropes keep their
  length through sheeting and scaling; mast ends follow the hoisting corners
  vertically. Ties hide when struck, unsupported, loading or disabled.
- A sail-owned visual route joins each fixed mast tie to its luff corner and
  aft corner. The shared head span continues to the aft guide, then branches to
  the sheet controls; lower branches go directly from the clew to those controls.
  Shared spans draw once. Head/foot extensions ease into the short straight
  mast ties; their aft tangents follow their own fabric-edge chords. External
  spans (head to aft guide, guide to controls, and clew to controls) follow
  direct chords with downward parabolic sag of 0.5–2.5% of span length from
  native sheet slack. Knots and guides permit direction changes between spans.
  The same native rope material/width is used throughout. A marker-scoped
  `RopeEffect.LateUpdate` postfix hides this sail's original line and optional
  `ClothRope` visuals while leaving native tension/input calculations active.
  Routes draw after the sail pose; striking hides extensions/ties and parks
  the upper/lower branches at the aft/fore guides. Loading, missing supports and
  disabling the sail hide the custom routes.
- Each of the four corners has one native-style knot, shared by its meeting
  ropes and visible with either rope setting. The native jib-sheet prefab's
  unreadable mesh is baked from a private inactive renderer copy, then its
  compact connected knot section is isolated from the rope tube. The installed
  asset has 96 knot vertices and 66 tube vertices. Generated geometry belongs
  to the template asset owner and shares the native rope material; temporary
  donor/bake objects are discarded. Missing/incompatible donors produce one
  construction warning and omit knots without disabling the sail. Knot leaves
  live outside fabric scaling, follow corner positions and outgoing rope
  directions, and share the route's hide/strike lifecycle. No donor scripts run
  and no skin bones are rotated to orient knots.
- The transverse rest section is a circular arc with depth `0.20 × width`,
  retained through the head, middle and foot. Twelve shaping intervals smooth
  that arc over the existing 24 × 32 mesh; topology and bind poses are created
  under the inactive template only. Both edges contain spare fabric and only
  the four corners are pinned, so fabric can arch away from the ropes. Interior
  Cloth travel is bounded relative to local camber while retaining edge freedom.
  Bending stiffness is 0.15 with stretching stiffness retained at 0.99;
  native WindCloth damping ranges from 0.08 to 0.45. This softens physical
  movement without changing the profile, movement limits or donor wind forcing.
- The rest mesh includes spare fabric for a mastward luff arc, capped at
  0.2286 m before scaling. Bone targets retain that inward direction on both
  tacks, reduce it with the smallest scale when shrinking, and cap enlargement
  at nine inches. Partial hoists gather the arc using the existing deployment
  curve. Signed full-height camber changes smoothly with the tack.
  Scale-dependent Cloth coefficients reserve at least a quarter of the mast
  gap beyond the luff arch and its travel sphere. Coefficients update during
  fitting/scaling; tack changes move bones without resetting Cloth.
- Shipyard collision uses thin strips inscribed between the rising head and
  falling foot, swept around the offset luff. The first strip is retained
  because the whole neutral panel clears the mast. Bounds remain separate
  from billow. Fit checks include the mast radius and tie gap in the required
  span and compare each head with its own active supporting guide.
- Registration generates the base mesh at one-third of the original width
  (`sourceSail.installHeight / 3`); its luff and height shrink proportionally.
  The clone's installation height comes from the new luff, while collision
  strips and renderer bounds use the new fabric width. Shadow samples and sail
  area are generated from that smaller mesh. The donor is unchanged.
- New Flying Sail selections start at 100% through
  `SailScaler.SetScaleAbs(1f, 1f)` after SE's shipyard initialization. The previous
  33⅓% preset is removed; the mast ties remain 18 inches long at every scale.
- Native palette entry 11 and Shipyard Expansion texture index 0 supply the
  white/plain defaults. Scoped patches limit textures and sheet travel only for
  this sail.

Flying Sail tuning feedback: the user reported the rounded billow generally
looked good, but the external lines looked over-arched and wire-like, the cloth
looked starched, and the sail attachments needed native-style knots. This pass
uses direct external spans, lower cloth bending/damping and four native knots.

Release, GeometryChecks, AssemblyChecks and formatting passed. Geometry checks
cover direct-span attachment, gravity-only sag, local tie easing, knot section
selection/compaction and incompatible donor rejection. Existing circular-profile,
weighted-skin, corner-pin, tack, hoist, luff-clearance and neutral collision
checks remain in place. Assembly checks cover native rope suppression, knot
baking/material wiring, temporary-asset cleanup and live visual lifecycle.
A temporary check against the installed jib-sheet asset also verified that the
production selector retains its 96 knot vertices / 192 triangles and excludes
all 66 rope-tube vertices. Neither suite simulates Unity Cloth or executes Unity
mesh baking/rendering.

Runtime validation is pending on Brig, then Sanbuq, with both rope settings:
confirm more flexible cloth, less arched external ropes and correctly seated
knots at all four corners. Check both tacks, eased/tight sheets, weak/strong
wind, partial/full lowering and redeployment, resizing/recoloring, mixed sails,
previews/cancellation, support removal and save/reload.

See [AGENTS.md](../AGENTS.md) for the code map, installed-assembly inspection
tools and regression lessons, including approaches that failed in game.

## Staysail implementation and verification

Mk.A registers as staysail prefab **401**, Mk.B as **402** and Mk.C as **403**,
after Shipyard Expansion and before All Sails caches its inventory. Registry membership restricts fitting to an
active Fisherman's Stay. The native stay slot owns the saved sail, while the
rig places its hinge and full pinned luff on the forward physical mast.
The saved installation coordinate measures downward displacement from the
stay's forward endpoint, with 15 cm head and aft-mast clearances.
Native collision checks remain active; custom fit checks use forward spar
length and mast separation. The deployed luff must fit its selected section.

Each mark provides its cut and owned-asset prefix through
`FishermansStaysailShape`. Registration attaches that shape to the clone under
an inactive container and uses it for template geometry and asset naming;
callers supply only the selected shape type, prefab identity and current prefab.
The family prefab builder owns brig-jib donor index **110** and the **20°**
template head slope, giving each mark its own cloth and shadow meshes. On first binding,
each creates an owned mesh for the actual stay angle before enabling Cloth.
The mesh and bind poses then remain fixed. Its luff is straight and fully
pinned; the current experiment holds the aft head at 14° while retaining the
coupled foot/leech solver. Uniform scaling preserves the cut. New shipyard
selections use `SailScaler.SetScaleAbs(0.5, 0.5)` after SE's initialization;
existing saves retain their stored dimensions.

Mk.B retains Mk.A's head and fixed 14° upper corner. Its foot has 90° corners
against the luff and leech in the fore-mast frame, so it is deck-parallel on
upright masts and tilts slightly on raked masts. All three marks use the same
halyard, sheet, reefing, appearance and fitting code.

Mk.C retains the same head and width, but its luff is `1.5 × width`. It raises
its aft foot by `width × tan(40°)` from the fore foot. This is 40° above the deck
on upright masts and tilts with fore-mast rake. The template head remains 20°
and the installed head follows the actual stay. The longer luff needs more
forward-mast clearance; existing fit checks measure it from the cut. Mesh,
tension and aerodynamic calculations use the resulting geometry and area.
Collision strips use the lowest head and highest foot across each strip,
keeping a 5 cm margin at each edge; strips too shallow for these margins and
a 1 cm height are disabled, as are clipped strips less than 1 mm wide. This supports rising, level and falling feet.

Mk.C geometry, family behavior and registration checks cover the new cut;
collision containment checks cover all three marks. Unity prefab construction
and Cloth are not executed by these checks. In-game Mk.C validation remains
pending: start on Brig, then Sanbuq, covering both tacks, eased/tight sheets,
weak wind, partial/full furl and redeployment, resizing/recoloring, mixed marks,
previews/cancellation, support removal and save/reload. Check rope attachment,
propulsion, controls and collision clearance.

The user reported that the fixed 20° experiment appeared to work and requested
a further 6° inward adjustment. The user then found the 14° setting pretty good. The current change adds an
aft halyard and proportional reef-angle transition, which await in-game validation.

Each mark supplies a required `FixedUpperHeadAngle`, currently 14° for all three.
The dormant staysail sheet-following policy and optional inward trim were
removed in CLEANUP-3. The fixed target rotates
the neutral head around the fore-mast axis, retaining height and head span.
The angle is multiplied directly by clamped `currentUnroll`: 14° at full
hoist, 7° at half reef, and 0° fully furled. The lower corner blends from its
sheet angle toward this reefed head angle using the existing gathering factor;
it no longer gathers toward 0° while the head is still out. The native bundle
uses the posed head endpoints, maintaining its route through the visibility
threshold and returning to neutral at full furl.

Per-instance `FishermansStaysailFixedHead` state selects the leeward side using
apparent wind projected onto `Cross(mastAxis, neutralAftDirection)`. This frame
is independent of the sheet-rotated body and cloth. A ±0.6 m/s deadband retains
the prior side; initial indeterminate wind defaults to +14°. Subsequent tack
changes use the existing smoothing rate, settling exactly at ±14°. Wind strength
and lower-sheet travel cannot trim the settled upper position.
`FishermansStaysailEdgeFit` retains the load-dependent support bow and calls
the coupled solver, which adjusts the lower clew and leech without moving the
upper corner. Invalid fits retain finite fallback
points and the existing warning.

The functional halyard is now cloned from the supporting aft base's reef winch,
bound to the existing native reef controller, and routed through owned guides
at the authored aft pulley to a leaf beneath the top-aft corner. The guide
chain retains its 5 cm separation along the aft spar. The fore-head halyard
route and both decorative upper-sheet lines are removed. The existing lower
sheet sources and native reef direction remain unchanged.

The authored ancestry tables cover both fore and aft sections of all 97 stay
variants. Resolve active aft ancestors and require a valid aft-base reef winch;
do not fall back to the fore mast. Installed-asset checks confirm matching
rendered/collidable reef winches on all selected aft bases across seven boats.
Control slots are allocated across sails sharing an aft base or lower-sheet
source, and owned controls are repositioned/rebound when their sources change.
Removal protection includes both full mast chains. No save fields or migration
are needed; controls reconstruct from the sail's existing native stay slot.

The user's Sanbuq screenshots showed a smoother starboard tack and pronounced
upper/middle folds on port after the first upper-trim pass. The installed DLL
matched the tested build. Opposite-tack weighted skin and triangle-edge checks
pass with equivalent mirrored rig/wind inputs; they do not reproduce a directional
cut failure. The prior cloth allowances did exceed the middle panel's shallow
camber, permitting it to fold across its target plane. That is a plausible
contributor, not a confirmed simulation-level cause of the screenshot difference.

The revised rest/target profile uses a sampled sine across the span and a
`(1-v)*(1+0.75*v)` vertical taper. It retains the 12%-width head peak but gives
rounder shoulders and more middle-panel depth. Interior movement is capped at
60% of local camber plus small allowances near the foot and leech, retaining
the existing clew taper. Rest vertices, bind poses and Cloth coefficients are
initialized together; tacking only moves the existing bones. The native wind
response, mesh lifecycle and Flying Sail remain unchanged. The fixed-head
experiment preserves these billow settings and existing length constraints.

The installed brig jib (prefab 110, `sharedassets15.assets`) supplies the native
`reef` clip/controller and `furled__sail_cloth_jib` mesh. An inactive, stripped
copy of the original animation hierarchy preserves the clip's binding paths.
`AnimationClip.SampleAnimation` samples the native fold-bone scale and moving
rope attachment at `1 - currentUnroll`. The family adapter maps those channels
to upward gathering: the head retains its span while the foot rises toward it.
The luff stays on the fitted fore-mast section; the reefed foot progressively
returns beneath the neutral head as the leech shortens. The original reef component is disabled so it cannot compete with
custom renderer/material handling. The original Animator remains as SE's
scaling reference. No extracted game assets are distributed.

Below 4% deployment, the native furled bundle is visible along the neutral
sloping head, fitted to its length and centered between its endpoints. Above that threshold, the Mk.A panel displays the sampled pose; fully
deployed cloth uses its initialized solver. Recoloring and the native plain
texture apply to the panel and bundle. Ropes attach to independent leaves and
follow the moving corners rather than rotating skin bones.

Automated validation covers all 97 authored stay frames, the nominal cut and
pin mask, repeated sheeting, edge budgets, reef-channel normalization and
reversals, partial-reef edge budgets, renderer thresholds, new-sail scaling
scope, retained optional-trim span/travel limits, fixed-head lower-sheet sweeps,
sheet-independent stay distance at each reef level, aft ancestry and halyard
binding, both tacks, wind reversals/deadband,
reef fading, constrained-fit fallback, full weighted-skin symmetry,
triangle lengths and interior travel bounds, and all 54 statically
declared Harmony patch signatures. The optional
SailInfo integration has a separate installed-signature check. Installed
asset inspection confirmed the donor clip, two-bone hierarchy, animated scale
and rope channels, and furled mesh. These checks do not run Unity animation or
Cloth, so the actual motion and appearance remain unverified.

SailInfo 1.2.1's instance `WinchInfoSail.SailDegree()` uses the donor's transform
axis. A soft dependency orders optional integration after SailInfo; a narrowly
scoped prefix uses Mk.A's mast-relative sheet angle instead. The reported angle
is not clamped. Final hinge limits still clamp native sway to ±40° and preserve
tighter collision restrictions. SailInfo settings and force readouts are unchanged.

In-game acceptance: start on the Brig with a vanilla staysail for comparison.
Check Mk.A menu restrictions, uniform sizing, movement, support protection,
both tacks and tighter collision limits. On Sanbuq and Brig, repeatedly sweep
the lower sheets from tight to eased while watching the fixed upper corner.
It should stay 14° out while fully deployed and at constant distance from the
stay for a given reef amount. Use the aft halyard to verify 14°/7°/0° at
full/half/zero deployment, with no rope jump at the bundle threshold. Check
that no fore-mast halyard or decorative upper-sheet line remains. Inspect for twisting, new creases, corner jumps, slack collapse
or rope detachment. Change wind strength, cross tacks, and test weak wind to
check side retention and smooth transitions. Verify new sails start at half width
and height and existing saves keep their size. Compare SailInfo's readout to
physical travel on both sheets, including heel and a steep stay. Release the
halyard, pause and reverse at several positions, fully furl upward, inspect
the bundle at the head and its ropes, then winch in again. Check no masthead jump, floating luff, detached corners or
stale full-size cloth; test recoloring, cancellation and save/reload. Repeat on
a steeper fallback stay and an offset topmast, then the other supported boats.
The first in-game pass reported unwanted furling on deck, excessive initial
size and incorrect SailInfo degrees. These revisions address those observations;
the rounded profile and cloth bounds remain with the fixed 14° head policy.
The earlier 2.5% trim was removed in CLEANUP-3. The user found the 14° setting
pretty good and reported favorable initial validation after that cleanup,
without itemizing boats or scenarios. Full reefing, bundle alignment, sizing,
angle reporting and save/reload coverage remain pending. Passing length
constraints and mirrored skin tests does not establish stable Unity Cloth.

## Fisherman's Stay profiles and verification

The stay profiles contain explicit physical mast IDs, mast-local endpoints,
halyard guide references, prerequisites, exclusions and permanent mount IDs.
They use installed assets from `Sailwind_Data/level24`, Shipyard Expansion's
`shipyard_expansion.assets`, `Leopard/leopard` and
`ShatteredSeasExpansion/veil piercer`. SE imports its parts below each boat's
model transform; asset coordinates must be compared in that common frame.
Use the installed assemblies to confirm import parents and part dependencies.

| Boat | Part groups | Stay variants | Forward-masthead fallbacks |
| --- | ---: | ---: | ---: |
| Brig | 2 | 24 | 4 |
| Junk | 2 | 9 | 2 |
| Jong | 5 | 9 | 0 |
| Sanbuq | 2 | 26 | 5 |
| Cog | 1 | 3 | 0 |
| Leopard | 2 | 18 | 4 |
| Shroud | 2 | 8 | 2 |

The nominal angle is 70° between the aft spar's downward axis and the stay
(110° at the forward end for parallel spars). During authoring, intersect that
line with a connected forward spar. If the intersection would be above the
highest forward spar, use its physical masthead instead. Store that endpoint;
do not repeat this calculation at runtime. Exclude higher aft topmasts from
lower-mast variants. The physical mast axes and guide heights were inspected
from the installed assets; local coordinates are rounded to five decimals.

`tests/GeometryChecks/FishermansStay/StayMeasurements.txt` contains only reference measurements
for tests: mast transforms, physical spar extents and guide positions. It
contains no meshes, textures or assemblies. GeometryChecks independently
transforms each profile endpoint into this measured frame, checks physical
attachment and preferred/fallback angles, and verifies fixed mount IDs and
higher-section exclusions. The existing flying-sail mappings are also checked.

Registration appends fixed part groups after SE initializes customization.
Never reorder existing groups or options, or renumber their mount IDs. Slots
128–255 are reserved for these stays; occupied IDs reject registration for that
boat without appending a partial profile. The save constructor and shipyard
buttons expand to 256 without shrinking larger arrays. Missing appended slots
in old saves or cancellation snapshots restore **None**. Original stays and
the flying sail keep their identities and mechanics.

Stay geometry and walking geometry are normalized independently to the same
endpoints because donor mesh bounds can differ from native `mastHeight` and
from each other. Donor meshes/materials remain shared and unmodified; fresh
inactive roots own the new native components and cloned controls. No cloth
mesh, bone, solver or force changes are part of this feature.

Automated checks pass; in-game validation pending. Start on the Brig:

1. Install a lower-mast variant, then fit a native staysail. Check independent
   sheets/halyard, trimming, hoisting, resizing and normal collision restrictions.
2. Test aft topmast fitted with a bare foremast: the steeper stay must meet the
   foremast head. Add the forward topmast and select its matching variant.
3. Check mast/topmast exclusions, occupied stay removal and support removal.
   Cancel valid and invalid orders repeatedly; no frozen UI or orphaned controls.
4. Save/reload both with and without a stay. Confirm native stays and existing
   Fisherman's Flying Sails keep their controls, geometry and save slots.
5. Repeat on the other supported boats, including the Cog's fore/aft direction,
   Leopard's three-section masts and Shroud's short/tall mast options.

Do not describe these runtime scenarios as verified until actually observed.

## Release metadata

Keep `Plugin.PluginVersion`, the project version and both documentation startup
examples consistent. The first release version is **0.1.0**; earlier development
version numbers are not the public release sequence. Version changes must not
change the plugin GUID or prefab index. Distribute only the plugin DLL, without
game assemblies or extracted assets.

After updating the version and merging the release commit to `master`, run
`./scripts/tag-release.sh` from a clean checkout. It switches to `master`, fast-forwards
from `origin/master`, checks that `src/Plugin.cs` and the project agree on the
version, then creates and pushes an annotated `v<version>` tag. It stops if that
tag already exists locally or on GitHub. This script does not build or publish
a release asset.


## Shared winch placement (CLEANUP-1)

Version **0.1.0** uses `src/Controls/FishermanWinchControls.cs` for inactive cloning,
owned rotation handles, outline reset and boat-level reservations. Flying Sails,
Mk.A/Mk.B/Mk.C and stay-owned vanilla controls keep their existing bindings and rope
routes. Only active owners with a bound rope reserve space; registration-only and
empty stay variants do not. Donor changes replace affected clones and release old
reservations without destroying the sail-owned rope controller. Native wheel
rotation is a child of the mounting transform, so placement refreshes cannot
be interpreted as player winch input.

The seven boat classes in `src/BoatRigs/` record 151 donor/role mappings: 37 mast
references and 114 bounded surface mappings measured on 2026-09-24. Their shared record type is
in `src/BoatRigs/Definitions.cs`; candidate positions are calculated in
`src/Controls/WinchPlacementGeometry.cs`. Sources are the installed
`level24`, `shipyard_expansion.assets`, `Leopard/leopard` and
`ShatteredSeasExpansion/veil piercer`. Expansion transforms were converted through
the corresponding boat model frame before comparison. Mast collider axes identify
the spar direction; the native winch datum supplies attachment radius and facing.
Mast fittings can sit below the native sail-space collider's axial range, so that
range is not treated as the physical bottom of the spar. Deck-facing coils near a
mast use measured supporting surfaces. All non-mast controls use finite solid
support strips; a donor's face tangent alone does not establish physical support.

Regular candidate spacing uses the installed interaction-sphere size, with a
0.35 m minimum and 2 cm beyond the padded radii. Mast candidates prefer the native face vertically,
then ±90° and 180° around the authored axis, rotating the face along with its
position. Mast height stays between 0.7 m below and 1.4 m above the native datum;
surface positions stay within 1.401 m of the donor, following measured surface
height and normal. Surface candidates include both ends inset by the interaction
radius, so nearby fittings cannot strand usable space between grid positions.
Nearby native fittings and
all reserved controls exclude candidates. There is no unlimited offset fallback:
an exhausted fitting is hidden, logs once and retries, while its native controller
stays alive. These bounds require in-game accessibility and surface-clearance
checks; numeric support strips and mast cylinders do not model every hull detail.

`tests/GeometryChecks/FishermansStay/WinchMeasurements.txt` contains only numeric
measurements: boat, source mast ID, role, donor position, face normal, support axis
point and interaction radius. Checks cover all supported profile references,
three extra controls per mast donor in isolation, at least two per bounded
surface donor, reservation lifecycle and invariance of mast attachment radius/facing.
Assembly checks verify structural clone and teardown wiring; they do not simulate
Unity Awake/Start, previews, handles or
outlines. The user reported improved placement; full coverage on all seven
boats remains pending. Follow the winch validation scenarios in
[AGENTS.md](../AGENTS.md). No game assets or DLLs are included in the fixture.

The 2026-09-24 organizational follow-up consolidated the boat tables without
changing their authored values or ordering. Before/after canonical snapshots
matched exactly for seven boats, 97 stay variants, 151 winch mappings, 69 mast
section chains and 453 placement cases. The full Release build, formatting,
geometry and assembly checks passed. Profile checks also cover missing entries,
Leopard's three-section chain and rejection of cyclic ancestry. This refactor
adds no new in-game validation; the outstanding winch scenarios above still apply.

### Brig rail correction (2026-09-24)

The user's `screenshot_20260924_214716-region.png` shows an added sheet winch
floating outboard and another over the stair opening. The former horizontal
tangents ran mostly across the boat; tangent alignment did not establish that a
solid surface supported a candidate. Installed aft sheet donors also have
inconsistent heights relative to the rail cap.

All 24 Brig left/right donor mappings now use boat-local centerlines measured
from `level24`'s `medi medium new/structure_container/trim_006`. The three usable
faces on each side span z **-4.2071 to -8.3161**, **-8.3493 to -11.7052**, and
**-12.0830 to -13.0060** metres. Short bevels and bends are omitted, and each
end is inset by the measured interaction radius. Candidates follow rail height,
remain within **1.401 m** of their donor, and are tried nearest first. The
installed winch mesh's base is local z **-0.095856** at scale **0.8**; mounting
origins sit **0.0766848 m** along the cap normal, with the parent mount rotated
to align the donor face to that normal. Native donors are not moved.

`tests/GeometryChecks/FishermansStay/BrigRailMeasurements.txt` records independent
numeric face bounds. Checks verify seating and orientation, rail-end clearance,
stair avoidance, rejection of the former across-boat offsets, neighboring native
fittings, multiple controls and exhaustion. This first correction left other boats
and Brig mast winches on their existing placement paths; all shared consumers of a corrected Brig
sheet mapping use the same rail positions. Reservation ownership, wheel input,
rope binding and the hide/diagnostic/retry behavior are unchanged.

Release build, GeometryChecks, AssemblyChecks and formatting passed for this
correction. In-game confirmation remains pending: inspect both sides on Brig,
including multiple/mixed sails, mouse/VR handles and outlines, boat movement,
preview cancellation and save/reload. Automated geometry checks do not establish
live Unity accessibility or appearance.

### Remaining boat surfaces (2026-09-24)

The subsequent Sanbuq/Junk screenshots (`screenshot_20260924_222025-region.png`
and `screenshot_20260924_222105-region.png`) confirmed unsupported sheet placements
outside Brig. The other 90 non-mast mappings now use the same bounded support
approach. `WinchSurfaceSegment` accepts an explicit normal for cross-sloping rails
and transverse beams; each donor supplies its measured face normal and mesh-base
offset. The native donors remain unchanged, and the mounting parent carries the
position/facing correction. Mast placement and control ownership are unchanged.

Supporting meshes, measured in each boat's local frame:

| Boat | Permanent support surfaces |
| --- | --- |
| Sanbuq | `structure/Cube_013`: forward and raised aft rail caps; lower trim faces excluded |
| Junk | `structure/trim_001` rail caps, `Cube_035` raised handrails, `Cube_032` transverse reef beam |
| Jong | `structure/trim_010` forward, middle and aft rail caps |
| Cog | `structure/trim_001` aft rail caps |
| Leopard | `structure_container/decking trim` upper rails; `mainfife back` and `mizzenfife` interiors excluding raised end posts |
| Shroud | `Clipper_Upper_Trim` upper rails; `Halyard_Points/Cube.004` and `Cube.005` interiors excluding rounded ends |

Only measured numeric bounds appear in
`tests/GeometryChecks/FishermansStay/WinchSurfaceMeasurements.txt`. Regression
checks compare candidate contact points against those face bounds, allowing up
to 2 cm for slight cap-face warp, and check the expected facing, endpoint clearance
and donor-distance limit. Each of these 90 mappings retains an available slot
even with all measured donor variants treated as native obstructions. Reservations
must still exhaust rather than extend beyond the physical support. A dedicated
Leopard case checks the usable strip endpoint missed by a donor-centered grid.
Unmeasured non-mast definitions now produce no candidates rather than tangent
offsets with unknown support.

Both runtime logs also contained a Brig `No free authored winch position` warning.
Installed Brig fore/main sheet fittings include adjacent native stations near
z -5.30, -5.86, -6.40 and -6.92 m; these compete with added controls for bounded
space. The logs do not identify enough preview/ownership state to attribute that
warning to a specific conflict. Keep the diagnostic/retry behavior and verify
mixed-sail capacity in game rather than allowing unsupported overflow positions.

Release, GeometryChecks, AssemblyChecks and formatting passed. In-game validation
is pending: start on Brig, then reproduce the Sanbuq/Junk screenshots, then check
Jong, Cog, Leopard and Shroud with both sides, multiple/mixed sails, mouse/VR
handles and outlines, boat movement, previews/cancellation and save/reload.
