# Fisherman's Sail

A Sailwind mod with an experimental **Fisherman's Sail Prototype**, based on
an independent clone of the game's **brig jib** (prefab 110, category `staysail`).
Version **0.4.0** replaces the triangular prototype with a four-corner trapezoid:
a horizontal top, two 90° top corners, a 40° lower forward corner, and a 140°
lower aft corner. The aft depth is half the top width; the forward depth is
approximately 1.692 times the top width. These are the neutral, fully set angles;
wind, sheeting, and mast rake can deform the working sail.

Version **0.4.1** corrects skin influences to descending weight order, recreates
Cloth for the new topology instead of retaining the donor's simulation data, and
recalculates tangents. The geometry checks now enforce Unity's weight ordering
as well as matching each weight to the correct corner. The visual artifact fix
still needs an in-game check after restarting with the rebuilt DLL.

Version **0.6.0** replaces heuristic stay discovery with explicit boat rig profiles
in `BoatRigs/`. Brig, Junk, Jong, Sanbuq, Cog, Leopard, and Shroud each have a
separate definition listing donor stays, mast pairs, attachment sections, height
references, and furl-control masts. Positions still come from the live transforms.
The existing mount IDs and appended group/option order are retained. Unknown boats
or profiles that do not match the installed rig are logged and skipped. These
profiles target the installed Shipyard Expansion rig layouts; additional rig mods
may require profile updates. Sail mesh and furling behavior are unchanged.

Version **0.5.3** resolves stay attachments against connected mast sections.
The Brig's topmast variants can attach below the topmast collider when the required
lower mainmast supplies the physical spar at that height. The lookup follows
explicit mast dependencies and requires overlapping sections within one metre
horizontally; it never treats a gap or a separate neighboring mast as solid spar.
Rendering, availability checks, and halyard guides use the same resolved section.
Existing stay names, indices, and part ordering are unchanged.

Version **0.5.2** draws a separate gathered bundle when fully furled. Partly
furled sails use a skinned renderer without cloth simulation; only fully deployed
sails use the Cloth renderer. This avoids rendering stale solver triangles during
furling. The three visuals are selected after WindCloth's visibility updates.

The Formast stay's furl winch is copied from the foremast, and the Mizzenmast
stay's from the mizzenmast. Halyard guides sit on those physical mast axes at the
triatic height, with an explicit winch-to-guides-to-upper-sail-corner rope route.
Rope endpoints are separate from skin bones because the game rotates endpoints
while rendering ropes. Sheet controls remain alongside the angled donor stay's
sheet winches. These visual and attachment corrections still need in-game checks.

Version **0.5.1** routes shipyard `RefreshCloth()` requests to the procedural
rig. This avoids the logged null-reference error when Shipyard Expansion tries
to unfurl the sail through its disabled donor animator. Ordinary sails retain
the native refresh behavior. The reported hard freeze still needs an in-game
retest; fixing the logged error alone does not prove its cause.

Version 0.5.1 also discovered main-to-mizzen donors named simply "middle stay"
when their group has no upper/top donor, including the junk-medium boat's three
mast configurations. These new groups are appended after existing triatic groups
to preserve saved part positions. Explicit lower/bottom donors are excluded.

Version **0.5.0** names the forward entry **Formast Triatic Stay** and the rear
entry **Mizzenmast Triatic Stay**. Both can be selected independently on supported
three-masted rigs. The rear stay connects mainmast to mizzenmast at the mizzen's
upper sail-mount height. Existing mount IDs and part ordering are preserved;
previous rear entries receive the new name rather than creating duplicate stays.
The 0.6.0 profiles explicitly identify these rear stays, including layouts whose
aft mast is named "main 2".

The **Formast Triatic Stay** connects the forward mast pair.
It runs horizontally at the foremast’s upper sail-mount height, meeting both
mast axes at the same boat-relative height. It has its own sail mount and winches, so it can
coexist with the original angled stay. The rope is visible in this version.

Each configured shipyard stay group gains a separate fisherman entry with
**None** and variants for its mast configurations. The boat profile selects
the mast pair and attachment sections. The forward mast sets the height even
when the aft mast is taller.
For mizzen pairs, the aft mast sets the height instead.
Both physical masts must reach the selected height for the variant to be installed. A source stay must provide two
physical mast dependencies, spar capsule colliders, static stay geometry, and
usable sail controls. Unsupported layouts are logged instead of guessed.

The prototype is a separate sail at stable prefab index **400**. The ordinary
brig jib is unchanged. If another mod occupies index 400, registration stops
with an error instead of replacing that sail.

## Development environment

This repository provides a Nix flake for **x86_64 Linux**. It supplies the .NET 8
SDK, which compiles the plugin for the game's older `netstandard2.0` API target.
You need Nix with `nix-command` and `flakes` enabled, a local Sailwind installation,
and working **BepInEx 5** and **Shipyard Expansion** (developed against 0.11.1).
No separate Unity editor or asset bundle is needed for this prototype.

From this directory, enter the shell:

```sh
nix develop
dotnet --list-sdks
dotnet build -c Release
```

Or build without entering an interactive shell:

```sh
nix develop -c dotnet build -c Release
```

The first run downloads the pinned Nix dependencies and restores the .NET Standard
reference package from NuGet, so it needs internet access. `flake.lock` pins the
Nixpkgs revision; keep it in version control alongside `flake.nix`.

## Formatting and pre-commit hooks

[CSharpier](https://csharpier.com/docs/Installation) is pinned in
`.config/dotnet-tools.json`. The Nix shell also includes `pre-commit`.
After cloning the repository, set up the local tool and Git hook once:

```sh
nix develop
dotnet tool restore
pre-commit install
```

The pre-commit hook restores the pinned tool and formats staged C# and XML project
files. If it changes anything, the commit stops: review the formatting, stage the
updated files, and commit again. Hook commands invoke `nix develop` themselves,
so Git commits also work from a terminal or editor outside the development shell.
The first tool restore needs network access; later runs reuse the cached package.

Run formatting or checks manually from the development shell:

```sh
dotnet csharpier format .
dotnet csharpier check .
pre-commit run --all-files
```

Generated `bin/` and `obj/` directories are excluded. The hook follows
[CSharpier's pre-commit integration](https://csharpier.com/docs/Pre-commit) using
the version in this repository's tool manifest.

## Game references

The project defaults to `~/.local/share/Steam/steamapps/common/Sailwind`.
For another installation, pass the game directory explicitly:

```sh
dotnet build -c Release -p:SailwindDir="/path/to/Sailwind"
```

The build references BepInEx, its bundled Harmony library, the game's
`Assembly-CSharp.dll`, and Unity's core and cloth assemblies.
These existing assemblies are not copied into the plugin output or committed here.

## Install and verify

1. Close Sailwind before replacing the plugin.
2. Copy the compiled DLL into its own directory under `BepInEx/plugins`:

   ```sh
   sailwind_dir="$HOME/.local/share/Steam/steamapps/common/Sailwind"
   install -Dm644 bin/Release/netstandard2.0/FishermansSail.dll \
     "$sailwind_dir/BepInEx/plugins/FishermansSail/FishermansSail.dll"
   ```

   Change `sailwind_dir` if your game is elsewhere. Only the plugin DLL is needed.

3. Launch Sailwind normally through Steam. Open `BepInEx/LogOutput.log` in the
   game directory and look for the startup message:

   ```text
   [Info   :Fisherman's Sail] Fisherman's Sail 0.5.3 loaded!
   ```

4. Load a test save with access to the brig and a shipyard. When the game's prefab
   directory initializes, the log should also contain:

   ```text
   Registered Fisherman's Sail Prototype: source=110, index=400, vertices=825, ...
   ```

   That line reports vertex and corner counts, the forward angle, and sail areas.
   It confirms registration, not that cloth simulation has been verified.

5. At a shipyard, select the **Formast Triatic Stay**, open **Staysails**, and
   choose **Fisherman's Sail Prototype**. It is available in each shipyard and
   also integrates with All Sails in All Shipyards if installed. Check subsequent
   menu pages if needed. The original **brig jib** remains available wherever it
   was previously sold.
6. Unfurl the sail and check the trapezoid orientation: the long edge and lowest
   corner must be forward. Scale it uniformly to fit the available stay and clear
   the deck and lower sails. The base top width remains the donor's install height
   (13.8 m in the inspected game assets); smaller rigs will need scaling down.
   Keep the default flip setting and align the forward top corner with the foremast.
7. Sheet on both sides: the lower aft corner follows the sheets, while the lower
   forward corner remains on the physical foremast axis. Furl halfway, strike fully,
   and unfurl again. The lower corners should rise toward the top, leaving a narrow
   gathered bundle when struck. Check for cloth explosions, detached ropes, or an
   unexpected triangular remnant. Verify that only one sail visual is displayed
   at each stage, including after changing color and after save/reload.
8. Reenter the shipyard and confirm one prototype entry. Resize and recolor it;
   save/reload and confirm its shape, scale, controls, and attachments return.
   Confirm the original brig jib and angled stay still work independently.

The prefab index remains **400**, so existing prototype sails load the new shape
with their saved scale. Their larger outline may need refitting. Foremast anchoring
is provided on fisherman stays; ordinary stays still accept the sail but cannot
supply that dedicated mast attachment. Exact proportions assume uniform scaling.
The native sail physics use the new area, with the force point moved to its centroid;
aerodynamic tuning and Unity cloth behavior still need in-game validation.

Sailwind saves the prefab index, so fitted prototypes require this mod on reload.
Remove them at a shipyard and save before uninstalling.

To find the message from a terminal:

```sh
rg -F "Fisherman's Sail" "$sailwind_dir/BepInEx/LogOutput.log"
```

If there is no fresh log, check that BepInEx itself starts through your usual Steam
launch setup. If other plugins load but ours does not, check the DLL location and
look for dependency or plugin-loading errors. Disk logging must be enabled with
`Info` included in `BepInEx/config/BepInEx.cfg` under `[Logging.Disk]`.

Rebuild and copy the DLL again after each change, then restart the game. Keep one
installed copy of `FishermansSail.dll` to avoid duplicate-plugin warnings. After
removing any fitted prototypes and saving, close the game and uninstall by removing
`BepInEx/plugins/FishermansSail/FishermansSail.dll`.

### Fit and verify the horizontal stay

1. In shipyard rigging customization, find **(no Formast Triatic Stay)** and,
   on a supported rear mast pair, **(no Mizzenmast Triatic Stay)**. Select the
   variants matching the installed masts; each includes the original stay variant
   for identification. Both groups can be installed together.
   Saves without these parts start at None; existing selections retain their IDs. Prices and installation
   costs match the source stay.
2. Select the new horizontal mount in the sail menu and fit a staysail, including
   **Fisherman's Sail Prototype**. Each fisherman stay accepts one sail. Resize
   the sail to fit the available span using the normal shipyard controls.
3. Install an angled stay and sail at the same time. Check both independently:
   furl/unfurl and sheet to port/starboard. Sheet controls are beside the source stay's
   winches; the furl control is beside the appropriate physical mast's furl winch.
   Copies are offset 0.35 m toward the forward mast. Follow each halyard from its
   control to that mast's guides and then the upper corner of the sail. Verify they are accessible and
   clear of surrounding fittings on the vessel being tested.
4. On a three-masted rig, install both triatic stays. Check that the Formast
   entry spans foremast–mainmast and the Mizzenmast entry spans mainmast–mizzenmast.
   Each must meet its shorter end mast at the selected upper mount height.
   Check that both endpoints of each stay are at the same height relative to the boat,
   and remain so while the boat heels. Inspect cloth, rope hardware, and collision
   clearance with both sails deployed.
5. Reopen the shipyard, cancel an order, and save/reload with both stays fitted.
   Confirm the selection, sail size, and installation position return and that
   no duplicate entries or winches appear. Remove the sail before removing its
   stay or required mast. Repeat on a second vessel layout.

The log reports `Registered Formast Triatic Stay` with the source index,
new mount index, span, and geometric availability. This proves registration,
not successful cloth simulation. Fitted fisherman stays and their sails require
this mod when loading the save. Remove the sails, set the fisherman entries to
None, and save before uninstalling.

## How it works

`Plugin.cs` declares the plugin metadata and Shipyard Expansion dependency, then
installs Harmony patches at startup. `PrototypeSail.cs` clones the brig jib under
an inactive template container after Shipyard Expansion configures its source
components. Registration runs before All Sails in All Shipyards caches its sail
list. Shipyard hooks append the prototype once, without replacing inventory entries.

`PrototypeGeometry.cs` generates a separate 24-by-32 quad grid (825 vertices,
1,536 triangles), UVs, four-corner bone weights, and cloth constraints. The top
edge and both lower corners are pinned; other vertices can billow. The source
brig mesh and prefab remain unchanged.

`FishermanSailRig.cs` replaces the cloned triangle animation with four procedural
bones driven by the native reef control. It attaches the sheets to the lower aft
corner and constrains the lower forward corner to the foremast on fisherman stays,
including during sheeting and furling. The disabled donor Animator remains as
Shipyard Expansion's scaling reference. The shipyard collider uses narrow strips
inside the trapezoid; the wind-shadow box covers its bounds. Normals, bounds,
and sail area are recalculated from the new mesh.

See the [BepInEx plugin tutorial](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html)
and [logging guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/3_logging.html).

To check the development environment:

```sh
nix flake check
nix develop -c dotnet --list-sdks
nix develop -c dotnet run --project tests/GeometryChecks -c Release
```

The geometry checks verify all four angles, proportions, triangle winding, area,
UVs, skin weights, attachment constraints, furl positions, independent arrays,
and invalid inputs over multiple sizes. They run managed geometry code, not
Unity's cloth simulation. Game geometry is not included in this repository.

`FishermanStay.cs` creates the separate rigging groups after Shipyard Expansion's
boat initialization. Static meshes/materials are shared without modification;
mounts, controls, rope targets, and walking-collision geometry are independent.
The mount retains the source stay's roll as its axis becomes horizontal,
preserving the sail's existing orientation. Geometry is refreshed during ordinary
and preview part changes.

Mount indices use `128 + source mount index` (128–255), independently of sail
prefab indices. Occupied indices stop that boat's registration. The mod extends
the native mount buttons and save-array capacity, appends customization parts,
and registers mounts before saved sails load. Existing entries are not reordered.

Run the stay geometry checks together with the existing cloth checks, and check
the Harmony targets and save-array capacity against the installed assemblies:

```sh
nix develop -c dotnet run --project tests/GeometryChecks -c Release
nix develop -c dotnet run --project tests/AssemblyChecks -c Release
```

For another game installation, use `-p:SailwindDir=/path/to/Sailwind` on both
projects and pass that directory after `--` to AssemblyChecks as well. An optional
local stay fixture can be supplied to GeometryChecks with
`-- --stay-fixture /path/to/stays.json`. It is a JSON array containing `foreMount`,
`foreBottom`, `foreTop`, `aftBottom`, and `aftTop` three-coordinate arrays in a
common upright boat frame. Proprietary geometry is not committed. The local
stock-asset fixture covers 15 upper-stay samples across boat instances.

Compilation and geometry checks pass locally. In-game appearance, rigging,
furling, and save/reload still require the manual checks above.

Boat profile maintenance: edit the corresponding `BoatRigs/<Boat>.cs` file.
Each group names the original customization part index; each variant names its
source mount, fore/aft physical masts, stay kind, height reference, furl-control
mast, and ordered fore/aft spar sections. Additional sections must be explicit
required continuations and physically adjoin. Native donor prerequisites and
exclusions are retained (excluding angled stays), so both stays can coexist.
Do not reorder existing groups or variants: saves address these by position.
The resolver validates the complete profile before construction; construction
failure rolls back all new groups on that boat to avoid shifting saved slots.
Add new boat keys using the exact prefab name, without `(Clone)`, and extend the
profile compatibility checks when adding support.
