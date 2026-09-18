# Fisherman's Sail

A Sailwind mod with an experimental **Fisherman's Sail Prototype**, cloned from
the game's **brig jib** (prefab 110, category `staysail`). The prototype widens
the free cloth edges while keeping the original corners, pinned cloth vertices,
rigging, and controls. This is a deformation experiment, not yet a four-sided
fisherman's staysail.

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
   [Info   :Fisherman's Sail] Fisherman's Sail 0.2.0 loaded!
   ```

4. Load a test save with access to the brig and a shipyard. When the game's prefab
   directory initializes, the log should also contain:

   ```text
   Registered Fisherman's Sail Prototype: source=110, index=400, vertices=289, ...
   ```

   That line reports changed and pinned vertex counts and the old/new sail areas.
   It confirms registration, not that cloth simulation has been verified.

5. At a shipyard, select a stay that accepts the brig jib, open **Staysails**, and
   choose **Fisherman's Sail Prototype**. It is available in each shipyard and
   also integrates with All Sails in All Shipyards if installed. Check subsequent
   menu pages if needed. The original **brig jib** remains available wherever it
   was previously sold.
6. Compare the unfurled outline with the original. The free edges should bow
   outward; the luff and clew should stay attached. Test furling, unfurling,
   sheeting on both sides, and shipyard resizing. Reenter the shipyard and check
   that the prototype appears only once. On the test save, save/reload with this
   mod installed and confirm the sail returns correctly.

Use a test save or a backup for this experimental sail. Sailwind saves its prefab
index, so a save with the prototype fitted needs this mod to recreate it. Remove
the prototype at a shipyard and save before uninstalling. The copied furled model,
wind center, and collision setup still use the brig jib's layout; this milestone
does not establish accurate fisherman-sail performance or perfect cloth animation.

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

## How it works

`Plugin.cs` declares the plugin metadata and Shipyard Expansion dependency, then
installs Harmony patches at startup. `PrototypeSail.cs` clones the brig jib under
an inactive template container after Shipyard Expansion configures its source
components. Registration runs before All Sails in All Shipyards caches its sail
list. Shipyard hooks append the prototype once, without replacing inventory entries.

The cloth mesh is cloned separately from the GameObject so it is not shared with
the original brig jib. `PrototypeGeometry.cs` changes local X coordinates with a
smooth outward bulge of up to 20% of the original width, tapering to zero at the
mesh bounds and leaving pinned vertices untouched. Vertex order, triangles, UVs,
bone weights, bind poses, and cloth constraints are retained. Normals, mesh bounds,
and sail area are recalculated. The existing game code still supplies sail physics
and controls; the increased mesh area affects its calculated area and price.

See the [BepInEx plugin tutorial](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html)
and [logging guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/3_logging.html).

To check the development environment:

```sh
nix flake check
nix develop -c dotnet --list-sdks
nix develop -c dotnet run --project tests/GeometryChecks -c Release
```

The geometry checks verify that the input is unchanged, pinned vertices and
corners remain fixed, the outline changes within its original bounds, and invalid
inputs are rejected. They run managed geometry code, not Unity's cloth simulation.
The same checks were also run locally against the actual brig mesh (289 vertices,
34 pinned, 512 triangles), with no inverted triangles. Game geometry is not
included in this repository.

Compilation and geometry checks pass locally. In-game appearance, rigging,
furling, and save/reload still require the manual checks above.
