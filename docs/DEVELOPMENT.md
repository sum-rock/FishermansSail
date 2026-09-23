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
  wrapping.
- **AssemblyChecks** validates Harmony targets against installed assemblies,
  texture load paths, patch ordering, control-list restoration and restrictions
  on the live Cloth lifecycle.

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
- `BoatRigs/` contains boat definitions in `FishermansSail.BoatRigs`.
- Future stays belong in `Stays/`, and other sail types in sibling directories
  under `Sails/`. Add these when implementing them; extract shared behavior only
  when the implementations establish what they need.

The geometry checks link feature sources directly; update their project includes
when moving files. The assembly checks resolve internal types by full name;
update those references when renaming types or namespaces. Runtime object and
mesh labels use `FishermansFlyingSail`; donor hierarchy names remain unchanged.
Prefab index **400**, native mast save slots and version **0.1.0** are unchanged.
The new menu name and loading existing sails still need in-game verification.

## Implementation notes

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

## Release metadata

Keep `Plugin.PluginVersion`, the project version and both documentation startup
examples consistent. The first release version is **0.1.0**; earlier development
version numbers are not the public release sequence. Version changes must not
change the plugin GUID or prefab index. Distribute only the plugin DLL, without
game assemblies or extracted assets.
