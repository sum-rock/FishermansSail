# Fisherman's Sail

A Sailwind mod providing a four-corner **Fisherman's Sail Prototype**, installed
on a physical mast under **Other**. Version **0.8.3** targets BepInEx 5 and
Shipyard Expansion, using the brig jib's native cloth and control assets.

The selected mast needs a supported active mast aft of it, with halyard fittings
on both masts. The aft mast need not carry another sail. Profiles cover the Brig,
Junk, Jong, Sanbuq, Cog, Leopard and Shroud. Normal mast capacity, vertical-space
and collision rules apply. Resize the sail to fit between the masts, below their
pulleys and above other sails.

The sail has its own port/starboard sheet controls and a hoist winch. Cloth opens
as it rises from a small gathered pose near the forward mast's deck base. The
deck height is estimated from that mast's hoist winch. Fully lowered cloth is
invisible; upper control-line ends park at the aft pulley and lower sheet ends
at the forward pulley, retaining the deck-to-mast runs. Existing pulley hardware
is reused. Only the four corners are pinned, leaving the edges free to billow.

Outward rotation is limited to **40 degrees on each side** of the neutral
fore-and-aft alignment along the hull. Obstructions can restrict travel further.
The limit applies to both sheet controls, native sway, the shipyard preview and
previously saved sails, without resetting the cloth or snapping the sail's pose.

The neutral cut has a horizontal head, a forward depth of approximately 1.692
times its width, and an aft depth equal to its width. The default width is
13.8 m, so resizing is often necessary. The upper aft corner follows 85% of the
sheet angle, allowing twist while the coupled tension solver fits foot and leech.
Wind-driven shaping moves existing bones; the live cloth mesh remains fixed.

The shipyard collision checker uses a thin neutral panel and excludes the
supporting mast radius plus 2 cm at the luff, where attachment contact is
intentional. Other rigging and sail collisions still apply. Its angular sweep
returns to the panel's aligned neutral position and rotation.

The user confirmed the 0.8.1 result looked good in game. Version 0.8.3 adds the
40-degree travel limit on top of the 0.8.2 support-profile cleanup. In-game
validation of both changes remains pending; automated checks do not simulate
Unity Cloth, hinge physics or the live shipyard.

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

1. Close Sailwind, then build and install from this repository:

   ```sh
   nix develop -c dotnet build -c Release
   ./install-local.sh
   ```

   Use `./install-local.sh "/path/to/Sailwind"` for another installation. The
   script copies only `bin/Release/netstandard2.0/FishermansSail.dll`; it does not
   build the mod. Builds and tests do not replace the installed DLL or change saves.
2. Launch the game and confirm `Fisherman's Sail 0.8.3 loaded!` in
   `BepInEx/LogOutput.log`. Registration reports source **110**, sail prefab index
   **400**, and **825** vertices.
3. On the Brig, select the physical **foremast**, open **Other**, and choose
   **Fisherman's Sail Prototype**. The active mainmast supplies aft support.
   On supported three-masted boats, the mainmast can use a mizzenmast aft.
4. Resize and move the preview using normal shipyard controls. The forward edge
   must fit the selected mast, both upper corners must sit below their pulleys,
   and the width must leave clearance from the aft mast. Check shroud clearance
   and that actual panel obstructions and overlapping sails still block fitting.
5. Confirm independent sheet controls and a hoist winch. They are copied and
   offset 0.35 m from the native controls, with additional offsets for multiple
   fisherman sails. Check access and that gaff or square sails retain their controls.
6. Hoist from fully lowered, pause, reverse, and redeploy. Cloth should rise and
   open smoothly, disappear at full strike, and reconnect to the parked rope ends.
   Check that the existing halyards still work.
7. Test repeated port/starboard tacks, eased/tight sheets, weak wind, another sail
   on the same mast, multiple fisherman sails, resizing, recoloring and save/reload.
   Check useful forward force, attached corners, free leech movement and smooth
   camber reversal. With both sheets fully eased, confirm the sail stops near
   40 degrees to port and starboard (or sooner where obstructed), including after
   loading an older save. Tightening should remain smooth. Check that shipyard
   rotation angles are at most 40 degrees per side. Repeat on another supported boat.
8. Preview removing either supporting mast or an occupied topmast: removal must
   be rejected until the sail is removed. Test optional topmasts present and absent,
   and check for extra winches after canceling orders. The `Fisherman mast rig`
   log identifies the active mast pair and pulley.

Installed sails use the game's native mast save slots and prefab index 400.
Keep this mod installed to load them; remove its sails and save before uninstalling.

## Implementation

- `PrototypeSail.cs` registers an independent prefab after Shipyard Expansion and
  before All Sails in All Shipyards caches its list. It preserves the donor's
  mass, angular damping and wind-cloth response.
- `BoatRigs/` defines physical mast sections and native sheet-control sources.
  Control sources supply assets and need not be installed. `FishermanRigging.cs`
  selects active masthead guides, owns the independent controls and cleans them
  up when the sail is removed.
- `MastInstallationPatches.cs` enforces support/fit rules and protects supporting
  masts during removal previews. Native control binding temporarily processes
  ordinary sails; a Harmony finalizer always restores the complete list before
  attaching the fisherman's controls. Mast capacity, overlap and saves use that list.
- `MastInstallationGeometry.cs` supplies mast-axis interpolation, deck-up hoisting
  and the neutral collision envelope. `FlyingSailPatches.cs` aligns the native
  collision sweep with the sail's mast axis.
- `FishermanSailRig.cs` poses the corners and shaping bones. Partial hoists use a
  procedural skinned renderer; fully raised sails use Unity Cloth. A hidden,
  meshless renderer supplies the native furled-color reference. Rope endpoints
  are separate leaves, so native rope rotation cannot rotate skin bones.
- `FishermanTravel.cs` and its patch bound native sheet limits and the final
  hinge limits after sway, preserving narrower collision limits on each side.
- `PrototypeGeometry.cs`, `FishermanBillow.cs` and `FishermanTension.cs` define the
  fixed mesh, camber response and coupled edge constraints. `FishermanAerodynamics.cs`
  and `AerodynamicPatches.cs` align native forces with the posed sail.
- `FishermanOrderText.cs` and its patch wrap long sail order/error text safely
  before NANDFixes, avoiding recursive wrapping.

Run all development checks after restoring dependencies:

```sh
nix develop -c bash -c 'dotnet csharpier format . && dotnet build -c Release --no-restore && dotnet run --project tests/GeometryChecks -c Release --no-restore && dotnet run --project tests/AssemblyChecks -c Release --no-restore'
git diff --check
```

Checks cover mast support/control mappings, fitting, collision bounds, active
pulley selection, hoisting, travel limits, skinning, tension, shaping, aerodynamics, safe order
text, installed Harmony signatures and cloth lifecycle restrictions.
