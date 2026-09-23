# Working on Fisherman's Sail

Follow current user instructions over historical design choices

## Start here

- Read `README.md`, `Plugin.cs`, and the relevant implementation before changing it.
  Check `git status --short` and preserve existing user changes.
- This is a C# Sailwind mod using BepInEx 5, HarmonyX, and Shipyard Expansion.
  The plugin targets `netstandard2.0`; the executable checks use .NET 8.
- Work from the installed game's actual assemblies and assets. Upstream source
  helps explain behavior but may differ from the installed versions.
- Make focused, reviewable changes. Preserve successful rigging and propulsion
  while tuning cloth. Update release metadata and describe exactly what was tested.
- Implementation requests authorize doing the work, building, and checking it.
  Do not repeatedly request confirmation for routine work already authorized.
- Do not commit, push, change saves, or replace installed game files merely as part
  of a build. Do not include proprietary assemblies or extracted game assets in Git.
- The sail installs on physical masts under Other and requires an active aft mast.
  This installation model has received positive in-game feedback. Preserve
  normal vertical mast-space rules and native mast save slots.
- The first release version is **0.1.0**. Earlier development version numbers
  are not the public release sequence. Keep the plugin GUID and prefab index 400
  stable when changing release metadata.

## Code map

| Area                                                           | Main files                                                      |
| -------------------------------------------------------------- | --------------------------------------------------------------- |
| Plugin metadata, registration and independent assets           | `Plugin.cs`, `FishermanSail.cs`                                 |
| Native appearance defaults and texture options                 | `FishermanAppearance.cs`                                       |
| Mesh, skin weights, pins and bone indexing             | `FishermanGeometry.cs`                                          |
| Live rig, corners, shaping bones, furling and render selection | `FishermanSailRig.cs`                                           |
| Camber response, movement limits and edge curves               | `FishermanBillow.cs`                                            |
| Coupled foot/leech length constraints                          | `FishermanTension.cs`                                           |
| Mast rotation, upper-corner motion and upper rope routes       | `FlyingSailGeometry.cs`, `FishermanSupportLine.cs`              |
| Sheet travel and post-sway hinge limits                       | `FishermanTravel.cs`, `FishermanTravelPatch.cs`                |
| Aerodynamic frame and scoped native force patches              | `FishermanAerodynamics.cs`, `AerodynamicPatches.cs`             |
| Boat-specific mast pairs, active guides and independent controls | `BoatRigs/`, `FishermanRigging.cs`                              |
| Mast installation, support protection and deck-up hoisting     | `MastInstallationPatches.cs`, `MastInstallationGeometry.cs`, `FlyingSailPatches.cs` |
| Shipyard order-text freeze protection                          | `FishermanOrderText.cs`, `FishermanOrderTextPatch.cs`                     |

## Build and checks

The environment is defined by `flake.nix` and `flake.lock`. CSharpier is pinned in
`.config/dotnet-tools.json`; `.pre-commit-config.yaml` defines the formatting hook.
See [the development guide](docs/DEVELOPMENT.md) for setup, local installation
and the release verification checklist.

For a fresh checkout:

```sh
nix develop -c dotnet tool restore
nix develop -c dotnet restore tests/GeometryChecks
nix develop -c dotnet restore tests/AssemblyChecks
nix develop -c dotnet build -c Release
```

After dependencies have been restored, the development validation command is:

```sh
nix develop -c bash -c 'dotnet csharpier format . && dotnet build -c Release --no-restore && dotnet run --project tests/GeometryChecks -c Release --no-restore && dotnet run --project tests/AssemblyChecks -c Release --no-restore'
git diff --check
```

- Use `dotnet csharpier check .` when only checking formatting. Formatting mutates
  tracked files; do not run the formatter during a read-only planning task.
- First-time Nix/NuGet operations may need network access. Use the environment's
  normal escalation mechanism if required rather than changing project dependencies.
- The game path can be overridden with `-p:SailwindDir="/path/to/Sailwind"`.
- Output: `bin/Release/netstandard2.0/FishermansSail.dll`. Only the plugin DLL is
  needed for deployment. Keep `PluginVersion`, the project version and README
  startup examples in README and the development guide consistent for releases.
- Run checks appropriate to a code change. Documentation-only changes normally
  need a diff/link/path review, not another full build.

`tests/GeometryChecks` exercises the pure geometry, skinning, tension, shaping,
wind-frame, boat-profile and text-wrapping logic. `tests/AssemblyChecks` checks
actual installed method signatures, Harmony injections, control-list restoration and
cloth lifecycle restrictions without starting Unity. Direct IL decoding is used
for lifecycle checks; asking Harmony to create native patch stubs failed in this
standalone test environment.

**Neither suite simulates Unity Cloth.** A passing build and mathematically valid
mesh do not establish stable cloth motion. Say "automated checks pass; in-game
validation pending" until the actual behavior has been observed.

## Inspection tools and local references

Prefer `rg`/`rg --files` for searches. Inspect relevant code, screenshots and logs
before guessing at a cause. Batch independent reads; keep dependent edits and
validation sequential. Use the available local image viewer for screenshot paths
or `file:///...` references; raster image generation is not needed for this work.

The current machine's game directory is:

```text
/home/august/.local/share/Steam/steamapps/common/Sailwind
```

Important paths relative to it:

- `Sailwind_Data/Managed/Assembly-CSharp.dll` and the Unity managed assemblies.
- `BepInEx/core/`: BepInEx and bundled HarmonyX (`0Harmony.dll`).
- `BepInEx/plugins/ShipyardExpansion/ShipyardExpansion.dll` and `SE_Bridge.dll`.
- `BepInEx/plugins/NANDFixes/NANDFixes.dll`.
- `BepInEx/plugins/AllSailsAllShipyards.dll` and `SailInfo.dll`.
- `BepInEx/plugins/FishermansSail/FishermansSail.dll`: installed mod, distinct from
  the newly built output. Confirm the startup version when diagnosing a report.

Temporary investigation tools exist outside the repo. They are useful starting
points, **not guaranteed dependencies**; inspect them before running and recreate
them if `/tmp` has been cleared:

- `/tmp/fisherman-inspect/`: a small `ICSharpCode.Decompiler` console helper,
  decompiled classes and asset-inspection scripts.
- `/tmp/fisherman-assets-env/bin/python`: a Python environment with UnityPy for
  reading serialized scene/prefab assets and tracing object/path IDs.
- `/tmp/fisherman-freeze-20260921-200022/`: captured player/BepInEx logs and thread
  stacks from the shipyard freeze investigation.
- Mono's `monodis` has also been used for assembly inspection when useful.

Example, using the existing decompiler build:

```sh
nix develop -c dotnet /tmp/fisherman-inspect/bin/Debug/net8.0/Inspect.dll \
  '/home/august/.local/share/Steam/steamapps/common/Sailwind/Sailwind_Data/Managed/Assembly-CSharp.dll' \
  WindCloth RopeEffect Sail
```

The helper takes an assembly path followed by type names. Its project currently
references an ILSpy decompiler DLL supplied by the local C# editor tooling;
inspect `Inspect.csproj` before rebuilding instead of assuming that dependency's
versioned path still exists. Prefer reproducible, isolated helpers over adding
debugging dependencies to the shipped plugin.

Useful cached investigations include `game.cs`, `sailrig.cs`, `ropeeffect.cs`,
`colchecker.cs`, `scaler.cs`, `se.cs`, `ordertext.cs` and `textwrap.cs` under that
temporary directory. They can become stale after a game or mod update. UnityPy
scripts there inspect meshes, mast sections, hierarchy and rigging relationships;
keep these read-only against the installed assets.

## Reference repositories

Installed binaries were the main behavioral reference; these upstream repositories
are useful source references, not automatically matching local builds:

| Repository                                                                        | Use                                                             |
| --------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| [sum-rock/FishermansSail](https://github.com/sum-rock/FishermansSail)             | This project's configured remote                                |
| [NANDbrew/ShipyardExpansion](https://github.com/NANDbrew/ShipyardExpansion)       | Parts, mast options, scaling and integration behavior           |
| [NANDbrew/NANDFixes](https://github.com/NANDbrew/NANDFixes)                       | Compatibility, particularly the order-text wrapping interaction |
| [NANDbrew/AllSailsAllShipyards](https://github.com/NANDbrew/AllSailsAllShipyards) | Sail inventory and registration ordering                        |
| [BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)                             | Plugin framework; this project uses BepInEx 5                   |
| [BepInEx/HarmonyX](https://github.com/BepInEx/HarmonyX)                           | Runtime patch behavior used by the installed loader             |
| [icsharpcode/ILSpy](https://github.com/icsharpcode/ILSpy)                         | Managed decompilation tooling                                   |
| [K0lb3/UnityPy](https://github.com/K0lb3/UnityPy)                                 | Serialized Unity asset inspection                               |

See also the [HarmonyX method-patch documentation](https://github.com/BepInEx/HarmonyX/wiki/Method-patches).
HarmonyX runs later prefixes even when a prefix returns `false`; this matters for
the shipyard freeze guard. Use official documentation and actual installed code
for technical decisions, and avoid assuming newer Unity documentation matches
the game's runtime.

## Logs, screenshots and in-game verification

Read both log sources; Unity warnings may not appear in BepInEx's log:

```text
/home/august/.local/share/Steam/steamapps/common/Sailwind/BepInEx/LogOutput.log
/home/august/.local/share/Steam/steamapps/compatdata/1764530/pfx/drive_c/users/steamuser/AppData/LocalLow/Raw Lion Workshop/Sailwind/Player.log
```

The second path is this machine's Proton player log; a native Linux Unity path
was not the Sailwind log. Capture relevant logs before a restart when investigating
a freeze. Separate exceptions from other installed mods from evidence involving
this sail, and distinguish a suspected cause from a confirmed one.

The real-sail reference is
`./references/IssumaFisherman4843.jpg`
`./references/OrbitWithSailNames.jpg`

For a runtime sail change, verify on the Brig first when following the current
test setup, then relevant additional boat profiles. Check repeated port/starboard
tacks, eased/tight sheets, weak wind, partial furling, full strike, redeployment,
multiple sails, resizing/recoloring and save/reload. Keep the four corners and
ropes attached, the free leech flexible, and useful forward force intact. Check
independent controls alongside other mast sails, ordinary vertical overlap,
support-mast removal, deck-up hoisting and parked ropes with invisible struck cloth.

## Lessons that must survive future changes

1. **Keep the live Cloth mesh fixed.** A mirrored-mesh experiment passed
   geometry tests but caused repeated sail detachment/reset in game. Do not swap
   meshes, rebuild Cloth, change bind poses, or reset the solver on tack changes.
   Current shaping changes only bone positions. Existing initialization and
   furl/render-state refreshes have a separate purpose.
2. **Do not mirror collider-bearing transforms with negative scale.** The
   mirrored-mesh experiment produced player log explicitly warned about the shadow
   BoxCollider. Cover both sides with positive-sized bounds and keep samples neutral
   where appropriate.
3. **Rope endpoints must be leaves, not skin bones.** Native `RopeEffect` rotates
   both endpoints with `LookAt`. Giving it a bone corrupts the sail skin. Keep
   separate attachment transforms beneath the corner bones.
4. **Keep corner control, cloth appearance and propulsion distinct.** The donor's
   wind-center axes caused the zero-efficiency bug. Use the posed corner frame
   and scoped aerodynamic patches; do not fake SailInfo values or globally modify
   vanilla sail forces. Retain the donor wind-cloth response unless evidence
   justifies a change; the weak earlier wind override made gravity dominate.
5. **Fit both foot and leech.** Independent inward clew shortening created excess
   slack and folding. Preserve the coupled solver and its finite failure handling.
6. **Protect native lifecycle assumptions.** Construct under an inactive template,
   create fresh Cloth for the new topology, retain the donor Animator as Shipyard
   Expansion's scaling reference, and preserve `SailShadowCol`'s expected parent
   hierarchy. Shared meshes belong to the template owner, not installed instances.
7. **Do not regress shipyard/save compatibility.** Keep sail prefab index 400;
   use native mast save slots.
   The shipyard removal freeze was traced to NANDFixes' recursive
   wrapping of long order text, not simply the presence of a sail. Keep the
   iterative wrapping guard and its Harmony ordering/input protections. Retain
   native rejection of removing occupied masts. Protect the aft support too.
8. **Resolve control lines against active mast sections.** An earlier in-game
   report showed an upper line turning above the Brig's visible mast. Its lookup
   used the donor topmast even when installation required only the lower mainmast.
   Search the profile's connected aft sections and check both mast and guide
   activity when drawing; registration/part refresh can precede activation.
   The user confirmed the pulley correction and mast-mounted result looked good
   in game. Keep active-section selection when changing profiles.
9. **Keep fisherman controls independent of mast sail order.** Native binding
   indexes winch arrays by mastOrder and can exceed dual-sheet array capacity.
   Exclude fisherman sails only during that binding call and always restore the
   full list in the finalizer. Capacity, collision, overlap and saving still use
   the full list. Each sail owns and destroys its extra controls. A hidden, meshless
   renderer remains because native recoloring expects a furled renderer.

10. **Keep shipyard collision checks separate from billow bounds.** Earlier
    full-height strips filled to maximum camber falsely contacted Brig shrouds.
    Offline installed-mesh checks reproduced this; spreader roots also contacted
    the intentional mast attachment area. Use a thin neutral panel
    clipped by the supporting mast radius plus 2 cm, retaining other collision
    and overlap rules. Preserve the aligned neutral rotation when the native
    sweep completes. The user approved this approach in game.

11. **Keep outward travel within 40 degrees per side.** Cap the
    fisherman prefab, collision sweep, restored limits and final native hinge
    limits. `JibAngleMaster.Update` adds sway after combining sheets, so reducing
    only `Sail.minAngle/maxAngle` is insufficient. Preserve tighter collision
    restrictions and apply the final cap after sway without resetting Cloth or
    snapping transforms. The user reported that this limit looked great in game.

12. **Do not repeat the tighter upper-corner experiment.** Reducing the upper
    corner's angle ratio from 85% to 60% passed automated checks but produced
    creases in game. The user reverted it. Retain the 85% ratio and existing
    geometry; mathematical feasibility did not establish stable Cloth behavior.

13. **Use the existing white/plain appearance options.** Default new sails to
    native palette entry 11 and SE texture index 0 (the unpainted
    stock square sail's texture). Keep recoloring and saved colors, restrict
    texture choices to plain, and hide SE's selector only for fisherman sails.
    Guard SE's material update so saved patterns cannot return. Do not invent
    RGB colors or textures, or change donor/shared assets. Appearance validation
    in game remains pending.

For handoff, report the version, behavioral change, checks actually run, remaining
in-game uncertainty, and the built DLL path. Update these notes when a later
in-game result confirms or disproves the current approach.
