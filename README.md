# Fisherman's Sail

A Sailwind mod, starting with a single startup log message. The first milestone
proves that our compiled plugin loads through BepInEx. A new sail type will come
later.

## Development environment

This repository provides a Nix flake for **x86_64 Linux**. It supplies the .NET 8
SDK, which compiles the plugin for the game's older `netstandard2.0` API target.
You need Nix with `nix-command` and `flakes` enabled, a local Sailwind installation,
and working **BepInEx 5**. No separate Unity editor is needed for this milestone.

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

The project defaults to `~/.local/share/Steam/steamapps/common/Sailwind`.
For another installation, pass the game directory explicitly:

```sh
dotnet build -c Release -p:SailwindDir="/path/to/Sailwind"
```

The build references `BepInEx/core/BepInEx.dll` and the game's
`Sailwind_Data/Managed/UnityEngine.dll` and `UnityEngine.CoreModule.dll`.
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

3. Launch Sailwind normally through Steam and reach the main menu.
4. Open `BepInEx/LogOutput.log` in the game directory. Look for both:

   ```text
   [Info   :   BepInEx] Loading [Fisherman's Sail 0.1.0]
   [Info   :Fisherman's Sail] Fisherman's Sail 0.1.0 loaded!
   ```

   Spacing may vary. The second message should appear once per game launch, with
   no loading errors for Fisherman's Sail. You do not need to load a save.

To find the message from a terminal:

```sh
rg -F "Fisherman's Sail" "$sailwind_dir/BepInEx/LogOutput.log"
```

If there is no fresh log, check that BepInEx itself starts through your usual Steam
launch setup. If other plugins load but ours does not, check the DLL location and
look for dependency or plugin-loading errors. Disk logging must be enabled with
`Info` included in `BepInEx/config/BepInEx.cfg` under `[Logging.Disk]`.

Rebuild and copy the DLL again after each change, then restart the game. To
uninstall, close the game and remove only
`BepInEx/plugins/FishermansSail/FishermansSail.dll`.

## How it works

`Plugin.cs` declares the plugin's identifier, name, and version with
`BepInPlugin`. BepInEx discovers this class and creates it; Unity invokes `Awake`,
which writes our message through BepInEx's logger. The plugin does not yet modify
gameplay or save data.

See the [BepInEx plugin tutorial](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html)
and [logging guide](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/3_logging.html).

To check the development environment:

```sh
nix flake check
nix develop -c dotnet --list-sdks
```

A successful build checks compilation. The fresh in-game log is the proof that
the plugin actually loads.
