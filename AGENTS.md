# Working on MoreSailwindSails

Current user instructions take precedence over historical design choices.
Read [README.md](README.md), [src/Plugin.cs](src/Plugin.cs) and the relevant code before
editing. Check `git status --short` and preserve existing user changes.

## Scope and identity

- MoreSailwindSails is an expandable collection of Sailwind sail types. The
  current families are Fisherman's Staysails (Mk.A/Mk.B) and Fisherman's Flying
  Sails, with Fisherman's Stays as supporting rigging. Future sail families
  belong to this mod without being forced into either existing family's design.
- The repository, `src/MoreSailwindSails.csproj` and output `MoreSailwindSails.dll`
  use the project name, as does the BepInEx display name `MoreSailwindSails`.
  The C# namespace remains `FishermansSail`.
- This is a C# Sailwind mod using BepInEx 5, HarmonyX and Shipyard Expansion.
  The plugin targets `netstandard2.0`; executable checks use .NET 8.
- The mod is unreleased; **0.1.0** is the planned first release. Keep GUID
  `com.august.moresailwindsails`, DLL name `MoreSailwindSails.dll` and prefab IDs
  **400** (Flying Sail), **401** (Staysail Mk.A), **402** (Staysail Mk.B) stable.
  For releases, keep plugin/project versions and documented startup examples consistent.
- Keep Flying Sail and staysail mechanics independently editable. Shared-helper
  extraction is deferred in [CLEANUP.md](docs/CLEANUP.md); do not resume it
  without a new request. Existing shared winch controls remain in use.
- Implementation requests authorize editing, building and checking. Do not
  repeatedly ask permission for routine authorized work. Do not commit, push,
  alter saves or replace installed game files merely as part of a build.
- Use installed assemblies/assets as the behavioral reference; upstream source
  may differ. Never commit proprietary assemblies or extracted game assets.

## Code map

| Area | Location and responsibilities |
| --- | --- |
| Plugin | [src/Plugin.cs](src/Plugin.cs): identity, dependencies and Harmony discovery |
| Flying Sail | [src/Sails/FishermansFlyingSail/](src/Sails/FishermansFlyingSail/): registration, rig, geometry, billow, tension, aerodynamics and mast installation |
| Staysails | [src/Sails/FishermansStaysail/](src/Sails/FishermansStaysail/): family rig, prefab builder, reefing, fixed head and edge fitting; `MkA/` and `MkB/` hold mark definitions |
| Fisherman's Stays | [src/Stays/FishermansStay/](src/Stays/FishermansStay/): independent mounts, registration, previews and save compatibility |
| Boat profiles | [src/BoatRigs/](src/BoatRigs/): one class per boat owns supports, stays, mast ancestry and winch mounts; `Definitions.cs` owns shared types/catalog |
| Winch controls | [src/Controls/](src/Controls/): boat-owned cloning/reservations and placement calculations |
| Checks | [tests/GeometryChecks/](tests/GeometryChecks/), [tests/AssemblyChecks/](tests/AssemblyChecks/) |

Feature namespaces follow their directories under `FishermansSail`; Harmony
patches live in each feature's `Patches/` directory. Boat definitions use
`FishermansSail.BoatRigs`. See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed
implementation notes, asset provenance and verification procedures.

## Runtime safeguards

- **Keep live Cloth topology fixed.** Do not swap meshes, rebuild Cloth, change
  bind poses or reset the solver on tacks. Shape through existing bone positions.
  The mirrored-mesh experiment passed geometry tests but detached/reset in game.
  Initialization and existing furl/render refreshes have separate purposes.
- Construct templates under inactive parents. Create fresh Cloth for new topology,
  retain the donor Animator as SE's scaling reference and preserve the hierarchy
  expected by `SailShadowCol`. Template-owned meshes and instance-owned meshes
  have distinct lifetimes. Do not mirror collider transforms with negative scale.
- Rope endpoints must be separate leaf transforms, never skin bones:
  native `RopeEffect` rotates endpoints with `LookAt`.
- Preserve coupled foot/leech fitting and finite failure handling. Keep corner
  control, cloth appearance and propulsion distinct. Use posed aerodynamic frames
  and scoped force patches; do not fake SailInfo values or alter vanilla forces.
  Retain donor wind-cloth response unless evidence warrants a change.
- Keep controls independent of native mast sail order. Filter custom sails only
  during native control binding, then restore the full list in a finalizer.
  Capacity, collision, overlap and saving must see all sails. Retain the hidden
  color-reference renderer required by native recoloring.
- Resolve ropes against authored, connected **active mast sections and guides**;
  registration/preview refresh may precede activation. Protect occupied supports.
- Enforce **±40°** travel after native `JibAngleMaster.Update` adds sway, retaining
  tighter collision limits. Preserve prefab, sweep and restored limits without
  resetting Cloth or snapping transforms.
- Preserve each feature's iterative order-text guard before NANDFixes. HarmonyX
  runs later prefixes even after `false`; append wrapped lines to the native list
  and consume the input so later prefixes cannot recurse on it.
- New sails use native white palette **11** and SE plain texture **0**. Preserve
  recoloring/saved colors, restrict textures to plain, and scope selector/material
  guards to custom sails. Never modify donor/shared assets to set appearance.

## Family-specific behavior

### Flying Sail

- Installs on a physical mast under **Other**, requires an active aft support,
  and uses native mast save slots and normal vertical-space rules.
- Retain the **85%** upper-corner sheeting response. The attempted 60% response
  caused creases in game and was reverted.
- Hoists from the deck; fully striking hides the cloth and parks its ropes.
- Collision uses a thin neutral panel clipped by mast radius plus **2 cm**,
  separate from billow bounds. Keep native overlap/obstruction rules and restore
  the aligned neutral rotation after the collision sweep.

### Fisherman's Stays and staysails

- Stay endpoints are authored per configuration: prefer **70°** from the aft
  spar, using the forward physical masthead and a steeper stay when necessary.
  Preserve physical fore/aft ordering; do not infer endpoints at runtime.
- Append part groups without reordering save slots. Preserve mount IDs **128–255**,
  grow capacity without shrinking other mods' arrays, and default missing older
  snapshot entries to None. Protect occupied stays/supports during invalid
  previews, restore preview state in a finalizer, validate profiles first, and
  roll back a boat's new stays if construction fails.
- Both marks fit registered Fisherman's Stays. Mk.A has a downward-sloping foot;
  Mk.B has a 90° foot cut. Each mark's shape component supplies geometry and asset
  prefixes for both template and instance creation. The family prefab builder
  owns donor **110** and template slope **20°**; installed cuts use actual stay
  slopes. Do not reintroduce separate geometry/prefix registration arguments.
- Both marks hold the upper aft corner at **14° × clamped currentUnroll** on the
  leeward side: 14° deployed, 7° half reefed, 0° furled. Select tacks in the neutral
  mast/stay frame with a **0.6 m/s** deadband, retaining the prior side in weak
  wind and defaulting positive on indeterminate initialization. Smooth changes;
  preserve head height/span and independence from lower sheets and wind strength.
- The old staysail 85% response and inward trim are removed. Preserve support bow
  and coupled tension in `FishermansStaysailEdgeFit`; it must not move the fixed
  head. Retain rounded billow and interior travel bounded near local camber,
  with foot/leech freedom. Sanbuq's reported tack asymmetry was not reproduced
  by mirrored geometry checks; excess travel was a suspected contributor.
- Keep the deployed luff on the fore mast and neutral head aligned with the stay.
  Reef upward using the brig jib's native animation; show the recolorable native
  bundle below **4%** deployment. Sample animation under an inactive hierarchy
  without live donor scripts/colliders. Do not substitute Flying Sail hoisting.
- The aft-base reef winch drives the existing reef controller. Route the halyard
  through active aft guides and a leaf beneath the top-aft bone. Keep independent
  lower sheets; the former fore halyard/decorative upper sheets stay removed.
- New selections start at **50% width/height** through SE scaling; preserve saved
  sizes. Optional SailInfo integration reports actual mast-relative sheet angle
  without clamping its label.

### Boat profiles and winches

- Seven profiles contain **97 stay variants** and **151 donor/role winch mappings**.
  Keep authored values in the corresponding boat class, shared types in
  `src/BoatRigs/Definitions.cs` and placement math in `src/Controls/WinchPlacementGeometry.cs`.
- All three control paths use boat-owned reservations keyed by actual donor
  identity. Release unused reservations and refresh/rebind when donors change;
  preserve sail-owned controllers. Unbound stay variants consume no slots.
- Initialize clones inactive with owned external handles and fresh outlines.
  Reposition the parent mount, not the wheel whose local rotation drives input.
- Mast positions prefer vertical stacks, then other faces with position and
  facing rotated together. Deck/rail controls follow native surface tangents.
  Use measured spacing and bounded travel. Exhaustion hides the control with a
  diagnostic/retry while retaining its controller; do not add unlimited offsets.

## Build and validation

Use the pinned Nix environment and CSharpier tool configuration. For a fresh
checkout, run the restore commands in [DEVELOPMENT.md](docs/DEVELOPMENT.md).
After dependencies are restored:

```sh
nix develop -c bash -c 'dotnet csharpier format . && dotnet build -c Release --no-restore && dotnet run --project tests/GeometryChecks -c Release --no-restore && dotnet run --project tests/AssemblyChecks -c Release --no-restore'
git diff --check
```

Use `dotnet csharpier check .` instead of formatting during read-only work.
First-time Nix/NuGet operations may need normal network escalation; do not change
project dependencies to bypass it. Override the game path with
`-p:SailwindDir="/path/to/Sailwind"`; see the guide for the assembly-check runtime
argument. Output: `src/bin/Release/netstandard2.0/MoreSailwindSails.dll`.
Documentation-only changes normally require diff/link/path review, not a build.

Tests mirror feature directories and namespaces. Staysail behavior checks cover
both cuts at family level; mark-specific cut/identity checks stay under `MkA/`
and `MkB/`. Root `Program.cs` files only arrange execution. Shared Harmony and
IL helpers live in `tests/AssemblyChecks/Shared/`; measurement fixtures stay
with their feature checks. Geometry checks execute pure calculations. Assembly
checks inspect installed signatures, Harmony wiring and lifecycle structure;
control exception recovery and Unity prefab construction are not executed.
Use direct IL decoding; Harmony native patch stubs failed in this test environment.

**Neither suite simulates Unity Cloth.** Report automated results separately
from observed in-game behavior. Start runtime validation on Brig, then affected
boats (especially Sanbuq for tack/cloth changes). Cover both tacks, eased/tight
sheets, weak wind, partial/full furl and redeployment, mixed/multiple sails,
resizing/recoloring, support removal, previews/cancellation and save/reload.
Check rope attachment, useful propulsion and accessible controls; include
mouse/VR handles, outlines and clearance after boat movement for winch changes.

Known status: the user approved Flying Sail mast/guide placement, collision
clearance and travel limits, reported improved winch placement, and gave favorable
initial feedback after the staysail fixed-head cleanup. This is not exhaustive
validation. Both-mark fitting/save reload after registration cleanup, all-boat
winch coverage, appearance and full cloth/reefing scenarios remain pending.
The last code handoff (CLEANUP-5) passed Release, both suites and formatting checks.

## Local investigation and handoff

Game directory: `/home/august/.local/share/Steam/steamapps/common/Sailwind`.
Inspect `Sailwind_Data/Managed/Assembly-CSharp.dll`, Unity assemblies,
`BepInEx/core/` (including HarmonyX), and installed dependencies under
`BepInEx/plugins/`: ShipyardExpansion/SE_Bridge, NANDFixes, AllSailsAllShipyards
and SailInfo. The installed `BepInEx/plugins/MoreSailwindSails/MoreSailwindSails.dll`
is distinct from build output; confirm its startup version when diagnosing.

Read both logs; Unity warnings may be absent from BepInEx's log:

```text
/home/august/.local/share/Steam/steamapps/common/Sailwind/BepInEx/LogOutput.log
/home/august/.local/share/Steam/steamapps/compatdata/1764530/pfx/drive_c/users/steamuser/AppData/LocalLow/Raw Lion Workshop/Sailwind/Player.log
```

Capture logs before restarting a freeze. Distinguish other mods' exceptions and
suspected causes from confirmed evidence. Prefer `rg`/`rg --files`; inspect
screenshots with the local image viewer. The README links current screenshots.
Temporary tools may exist at `/tmp/fisherman-inspect/` (ILSpy helper/cached
inspection) and `/tmp/fisherman-assets-env/bin/python` (UnityPy). Inspect their
projects, dependency paths and cached results before use; recreate if absent.
Keep asset inspection read-only and debugging dependencies out of the plugin.

For handoff, report the version, change, checks actually run, remaining in-game
uncertainty and built DLL path when applicable. Update validation notes when
user observations confirm or contradict the current approach.
