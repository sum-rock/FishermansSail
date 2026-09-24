# Development

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
dotnet restore tests/GeometryChecks
dotnet restore tests/AssemblyChecks
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
The output is `bin/Release/netstandard2.0/FishermansSail.dll`.

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
  fallbacks, geometry alignment and older shipyard snapshots.
- **AssemblyChecks** validates Harmony targets against installed assemblies,
  texture load paths, patch ordering, control-list restoration and restrictions
  on the live Cloth lifecycle, plus stay registration ordering and save capacity.

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
./install-local.sh
```

Use `./install-local.sh "/path/to/Sailwind"` for another installation. This
script copies only the built DLL; it does not build it. Builds and checks do
not replace the installed plugin or change saves.

Confirm `Fisherman's Sail 0.1.0 loaded!` in `BepInEx/LogOutput.log`. Registration
should report donor index **110**, sail index **400** and **825** vertices. Read
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

`Plugin.cs` owns the plugin metadata and assembly-wide Harmony registration.
The plugin identity and output remain `FishermansSail`; the existing sail is
named **Fisherman's Flying Sail** in game.

- `Sails/FishermansFlyingSail/` contains the mast-mounted sail's registration,
  geometry, appearance, cloth rig and controls, using the namespace
  `FishermansSail.Sails.FishermansFlyingSail` and `FishermansFlyingSail` type prefix.
- Its `Patches/` subdirectory contains all feature-specific Harmony patches in
  the corresponding `.Patches` namespace, including registration, appearance
  and the order-text freeze guard.
- `BoatRigs/` contains one static class/file per boat in `FishermansSail.BoatRigs`.
  Each exposes a complete `BoatRigDefinition` through `Definition`, with private
  factories for Flying Sail supports, stay variants, mast ancestry and winch mounts.
  `Definitions.cs` holds the shared data types, validation and `BoatRigCatalog`.
  Resolve ancestry and winches through the selected profile (`Sections`, `Base`,
  `WinchMount`); individual winch records inherit boat identity from that profile.
- `Controls/` owns shared winch allocation, cloning and placement calculations.
  `WinchPlacementGeometry.cs` uses the authored mounting data without owning any
  boat tables.
- `Stays/FishermansStay/` owns the new stays, their independent controls, native
  mount registration, save handling and patches. The namespace is
  `FishermansSail.Stays.FishermansStay`, with `.Patches` for Harmony patches.
- `Sails/FishermansStaysail/` owns the staysail family's rig, reefing adapter,
  controls, prefab builder and patches. `MkA/` contains the original 110° cut;
  `MkB/` keeps its head and has a 90° foot. Each mark supplies its own
  `FishermansStaysailShape` and save identity.

The geometry checks link feature sources directly; update their project includes
when moving files. The assembly checks resolve internal types by full name;
update those references when renaming types or namespaces. Runtime object and
mesh labels use `FishermansFlyingSail`; donor hierarchy names remain unchanged.
Prefab index **400**, native mast save slots and version **0.1.0** are unchanged.
The new menu name and loading existing sails still need in-game verification.

### Test organization

Both `tests/GeometryChecks/` and `tests/AssemblyChecks/` contain
`FishermansFlyingSail/`, `FishermansStay/` and `FishermansStaysail/` directories.
The latter has `MkA/` and `MkB/` for variant checks. Put each feature's
checks and helpers in its directory, using the namespace
`FishermansSail.Tests.<Suite>.<Feature>`. Flying-sail rig-profile checks belong
with the flying sail; authored stay-profile checks belong with the stay.

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
- The neutral cut has a horizontal head, forward depth about 1.692 times its
  width and aft depth equal to its width. The upper aft corner follows 85% of
  the sheet angle. The tension solver fits foot and leech together.
- Shipyard collision checks use a thin neutral panel, excluding the supporting
  mast radius plus 2 cm at the luff. Collision bounds remain separate from billow.
- Native palette entry 11 and Shipyard Expansion texture index 0 supply the
  white/plain defaults. Scoped patches limit textures and sheet travel only for
  this sail.

See [AGENTS.md](../AGENTS.md) for the code map, installed-assembly inspection
tools and regression lessons, including approaches that failed in game.

## Mk.A implementation and verification

Mk.A registers as staysail prefab **401** and Mk.B as **402**, after Shipyard
Expansion and before All Sails caches its inventory. Registry membership restricts fitting to an
active Fisherman's Stay. The native stay slot owns the saved sail, while the
rig places its hinge and full pinned luff on the forward physical mast.
The saved installation coordinate measures downward displacement from the
stay's forward endpoint, with 15 cm head and aft-mast clearances.
Native collision checks remain active; custom fit checks use forward spar
length and mast separation. The deployed luff must fit its selected section.

Each mark provides a cut through `FishermansStaysailShape`. The shared prefab
builder gives each mark its own cloth and shadow meshes. On first binding,
each creates an owned mesh for the actual stay angle before enabling Cloth.
The mesh and bind poses then remain fixed. Its luff is straight and fully
pinned; the current experiment holds the aft head at 14° while retaining the
coupled foot/leech solver. Uniform scaling preserves the cut. New shipyard
selections use `SailScaler.SetScaleAbs(0.5, 0.5)` after SE's initialization;
existing saves retain their stored dimensions.

Mk.B retains Mk.A's head and fixed 14° upper corner. Its foot has 90° corners
against the luff and leech in the fore-mast frame, so it is deck-parallel on
upright masts and tilts slightly on raked masts. Both marks use the same
halyard, sheet, reefing, appearance and fitting code.

The user reported that the fixed 20° experiment appeared to work and requested
a further 6° inward adjustment. The user then found the 14° setting pretty good. The current change adds an
aft halyard and proportional reef-angle transition, which await in-game validation.

Mk.A selects a nullable mark-level `FixedUpperHeadAngle` of 14°. The family
default is null, retaining its existing 85% head policy and optional trim for
other marks. Mk.A bypasses both of those responses. The fixed target rotates
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
and lower-sheet travel cannot trim the settled upper position. The fixed mode
passes zero additional trim to the coupled solver, which adjusts the lower clew
and leech without moving the upper corner. Invalid fits retain finite fallback
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
upward reefing, bundle alignment, default sizing and angle reporting await the
next in-game pass. The user found the initial Mk.A upper trim generally good, with the port/starboard
appearance difference described above. The rounded profile and cloth bounds remain in the fixed-14° experiment;
the 2.5% trim is bypassed. The user found the 14° setting pretty good; the aft
halyard and proportional reefing revision await in-game validation. Passing
length constraints and mirrored skin tests does not establish stable Unity Cloth.

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


## Shared winch placement (CLEANUP-1)

Version **0.1.0** uses `Controls/FishermanWinchControls.cs` for inactive cloning,
owned rotation handles, outline reset and boat-level reservations. Flying Sails,
Mk.A/Mk.B and stay-owned vanilla controls keep their existing bindings and rope
routes. Only active owners with a bound rope reserve space; registration-only and
empty stay variants do not. Donor changes replace affected clones and release old
reservations without destroying the sail-owned rope controller. Native wheel
rotation is a child of the mounting transform, so placement refreshes cannot
be interpreted as player winch input.

The seven boat classes in `BoatRigs/` record 151 donor/role mounting directions
and physical mast references measured on 2026-09-24. Their shared record type is
in `BoatRigs/Definitions.cs`; candidate positions are calculated in
`Controls/WinchPlacementGeometry.cs`. Sources are the installed
`level24`, `shipyard_expansion.assets`, `Leopard/leopard` and
`ShatteredSeasExpansion/veil piercer`. Expansion transforms were converted through
the corresponding boat model frame before comparison. Mast collider axes identify
the spar direction; the native winch datum supplies attachment radius and facing.
Mast fittings can sit below the native sail-space collider's axial range, so that
range is not treated as the physical bottom of the spar. Deck-facing coils near a
mast remain deck fittings. Other controls use the tangent to their native face.

Spacing uses the installed interaction-sphere size, with a 0.35 m minimum and
2 cm between reserved radii. Mast candidates prefer the native face vertically,
then ±90° and 180° around the authored axis, rotating the face along with its
position. Mast height stays between 0.7 m below and 1.4 m above the native datum;
rail/deck offsets stay within 1.4 m along the tangent. Nearby native fittings and
all reserved controls exclude candidates. There is no unlimited offset fallback:
an exhausted fitting is hidden, logs once and retries, while its native controller
stays alive. These bounds require in-game accessibility and surface-clearance
checks; a tangent or cylindrical approximation does not model every hull detail.

`tests/GeometryChecks/FishermansStay/WinchMeasurements.txt` contains only numeric
measurements: boat, source mast ID, role, donor position, face normal, support axis
point and interaction radius. Checks cover all supported profile references,
three extra controls per donor in isolation, reservation lifecycle and invariance
of mast attachment radius/facing. Assembly checks verify structural clone and
teardown wiring; they do not simulate Unity Awake/Start, previews, handles or
outlines. All seven boats still require the CLEANUP-1 in-game matrix. No game
assets or DLLs are included in the fixture.


The 2026-09-24 organizational follow-up consolidated the boat tables without
changing their authored values or ordering. Before/after canonical snapshots
matched exactly for seven boats, 97 stay variants, 151 winch mappings, 69 mast
section chains and 453 placement cases. The full Release build, formatting,
geometry and assembly checks passed. Profile checks also cover missing entries,
Leopard's three-section chain and rejection of cyclic ancestry. This refactor
adds no new in-game validation; the outstanding CLEANUP-1 matrix still applies.
