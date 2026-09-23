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

Version **0.7.15** excludes disabled mast sections and pulley attachments from the
upper control-line route. Version 0.7.14 was observed routing above the Brig's
visible mast; its lookup could select the absent donor topmast even though the
lower mainmast legally supported the stay. Guides now come from all configured
aft sections, and eligibility is checked when drawing. The highest active guide
is used, with changes logged by mast identity, attachment path and boat-relative
position. Loading and mast-option changes can therefore select the lower-mast
pulley without retaining a disabled upper guide. Hardware, halyards, corner motion,
cloth and propulsion are retained. The correction still needs in-game validation
on the reported Brig configuration, with and without its optional topmast.

Version **0.7.14** routes the upper aft control lines upward through the existing
upper halyard pulley on the aft mast, then down to their existing sheet controls.
The highest valid halyard guide is selected relative to the boat, so heel does
not change the selection. The pulley and its halyard connections are reused
without copying or moving hardware. Sheet slack, corner motion, cloth shaping and
propulsion are retained; the shaping frame still uses the triatic attachment.
Mast changes refresh the guide reference. Missing guides retain the previous
triatic route and produce one warning per stay instance. The upward lead is a
visual routing change, not an added physical pulley constraint. Pulley alignment
and halyard operation still require in-game validation.

Version **0.7.13** moves billow through an initialized grid of shaping bones.
Seven control columns across 33 rows give 231 bones, retaining the original
corner and leech indices. Each vertex blends between two neighboring controls;
the rest curve is sampled on that same grid, removing any one-sided residual
between the mesh and its skin targets. Top/luff camber still peaks at 12%/6% of
width, and only the four corners are pinned.

Apparent flow normal to the posed panel selects the target side outside a
0.6 m/s dead band. The curve moves smoothly through the panel plane using the
existing three-per-second exponential response. Top/luff travel is reduced so
the loaded peaks follow the moving curve while the free leech and clew
reinforcement retain their limits. Furling fades the shaping offsets through the
bones instead of a flattening blend shape. Meshes, bind poses and bone scales
remain fixed during play; tacking does not reset Cloth. Shadow samples sit on the
center plane and collision bounds cover both camber directions using positive
scales. Corner controls, the moving upper corner and propulsion are retained.

Checks cover exact signed skin targets, corner pins, spare cloth, transition
poses, repeated tacks at several frame rates, furling, wind frames and propulsion.
Compiled-code checks reject runtime mesh replacement and cloth lifecycle changes
inside the shaping update. Actual Unity cloth stability and appearance during
repeated tacks still require in-game validation.

Version **0.7.12** restores the 0.7.10 sail runtime after the 0.7.11 experiment
caused the cloth to detach and repeatedly reset in game. Live cloth mesh swaps,
wind-triggered cloth resets and negative shadow scaling are removed. The player
log confirmed a negative-scale warning on the sail's shadow BoxCollider; the
precise native cloth failure was not captured in the log. The moving upper
corner, upper control lines, free leech, clew reinforcement and working propulsion
remain as in 0.7.10. A compiled-code regression check rejects cloth mesh replacement
from installed sail methods. The port-tack billow bias remains unresolved;
0.7.11's pure geometry tests did not validate Unity's live cloth behavior.

Version **0.7.11** (withdrawn after in-game cloth instability) gives the sail mirrored rest-camber meshes so billow can face
the apparent wind's flow on either tack. The filtered wind component normal to
the cloth selects the side; a dead band retains the previous side near
head-to-wind. A side change swaps the full and reefed renderers to the matching
immutable mesh, restores cloth constraints and requests one cloth refresh.
Both meshes have identical topology, weights, bind poses and rest edge lengths,
with separately recalculated normals and matching reef-gathering deltas. Shadow
samples follow the chosen side, and shipyard collision strips cover both curves.
The free leech, sheet routes, corner motion and propulsion are retained. Checks
cover mirrored rest lengths, signed billow, stable tack selection, both camber
variants across trim/furl poses, and propulsion. Cloth reset behavior and the
visible transition between tacks still need in-game validation.

Version **0.7.10** removes the visible rope along the leech and frees all 31
intermediate aft-edge vertices. Only the four corners remain pinned. The leech
can flex around its existing tension-fitted skin targets, with movement peaking
at 6% of sail width midway along the edge and tapering tightly near the clew.
The moving upper corner, upper sheet routes, sail cut, top/luff camber, cloth
stiffness/damping and propulsion are retained. No extra inward bow or fabric is
added. Checks cover the four-corner pin mask, bounded edge travel, clew
reinforcement, trim/furl sweeps and propulsion; visible flex still needs an
in-game comparison.

Version **0.7.9** releases the upper aft corner from the triatic stay. It follows
85% of the native sail angle around the forward mast, preserving the top span
while letting the upper corner move outward with some twist relative to the clew.
Upper sheet branches run from that corner through the actual aft mast/triatic
attachment and down to the existing port and starboard sheet controls. Their sag
follows the corresponding native sheet slack; they add no winches or physical
rope constraints. The supported leech, clew reinforcement, top/luff camber and
propulsion correction remain in place. Upper-corner rotation fades with deployment
during furling, and the gathered bundle follows the resulting upper corners.
Automated checks cover trim and furl sweeps, rope routes, rake/scaling, posed
mesh geometry and propulsion. Wind-driven appearance still needs an in-game check.

Version **0.7.8** attaches the entire leech to the clew-to-stay control line.
The rope now samples the same bones as the pinned aft edge, removing the
separate inward-bowing leech target. The coupled foot/leech tension solver
retains a modest wind/gravity curve. Cloth movement tapers smoothly toward zero
within 20% of the panel dimensions around the clew to reduce adjacent folding
when easing the sheets. The top and forward edges retain their existing camber
and freedom to billow between their corners; sail cut, propulsion and furling
remain unchanged. Automated checks cover the supported edge, clew taper,
continuous trim sweeps on both tacks, geometry, propulsion and game assembly
compatibility. The resulting cloth motion still needs an in-game comparison.

Version **0.7.7** fits the clew against both the foot and the curved leech.
The independent 10–16%-of-width inward shortening is removed. The solver chooses
the nearest sheet-requested position that maintains foot tension and the available
leech arc length, with a 1% reserve when fully set. The reserve and edge lengths
follow furling. Impossible anchor spans retain a finite fallback and log one
warning when deployed rather than generating invalid positions.

The forward edge now has actual spare cloth: a rest curve peaking at 6% of width
adds about 0.3% to its edge length. Its movement allowance peaks at 13% of width
so that curve can billow on either tack; both taper into the panel and fixed
corners. Existing camber gathering handles furling, and collision strips include
the revised camber. The four-corner attachment, separate control line, cloth
stiffness/damping and corrected propulsion remain in place. Automated checks cover
both edge lengths, symmetry, scaling, furling and propulsion. The reduction in
folds and visible luff billow still require an in-game comparison.

Version **0.7.6** attaches the cloth at only its four corners. Intermediate
forward-edge vertices now have travel peaking at 4% of sail width midway along
the luff, tapering toward the fixed corners and fading into the panel. Both the
pin mask and zero-travel luff profile are updated. Corner poses, sail cut, cloth
stiffness/damping, independent control line and the working 0.7.5 propulsion
correction are retained. This is a focused change to compare in game before
adjusting the mesh's slack further. Check luff movement, overall wrinkles, both
tacks, furling and save/reload.

Version **0.7.5** aligns the wind sensor and force direction with the posed
fisherman sail. The inherited brig-jib sensor rotation was incompatible with the
procedural mast frame: native wind capture could reject broadside wind as luffing.
The sensor now uses the mast axis, effective fore-to-aft chord, and sail normal;
the force application point follows the posed corner-area centroid. Only this
prototype's frame and force direction are corrected. Native capture, furling,
shadow, damage and propulsion calculations still run, so SailInfo reads their
actual force output without a display override.

Cloth travel is reduced, especially along the free leech, with stronger bending
resistance and damping. The donor's serialized cloth wind response is retained
(the inspected brig jib uses 5), replacing the weak 0.6 override that let gravity
dominate. The fixed upper attachments, hollow free leech, and separate control
line remain. Geometry, aerodynamic regression and game-assembly checks pass;
cloth smoothness and actual propulsion still need an in-game test on both tacks.

Version **0.7.4** frees the intermediate leech vertices from the control line.
Only the upper aft corner and clew remain attached; the upper corner still meets
the triatic stay. Under load the leech's skin targets bow into the sail, and the
cloth solver has additional travel to billow around them. The visible line is now
an independent, nearly taut clew-to-stay span with slight gravity sag. It no longer
traces the fabric edge. The free-edge bow fades during furling; the line hides when
fully struck. Cut, area, saved IDs and native sheet/windward slack controls are
unchanged. Check the resulting rope/cloth separation against `model2.png` on both
tacks in game, including furling and save/reload.

Version **0.7.3** fixes the upper aft corner directly to the triatic stay.
The revised cut has an aft depth equal to the sail width; the forward depth stays
approximately 1.692 widths. This supersedes the original 40°/140° lower angles.
The mesh includes top-edge camber of 12% of width, tapering to the side edges and
foot, so billowing uses actual spare fabric. Top cloth remains free on both tacks.
The entire leech and its visible control line share one curve ending at the fixed
upper attachment. Curves exceeding 98% of available leech length bring the clew
closer to that attachment instead of pulling the head off the stay. Existing sheet
controls and windward slack remain native. Camber gathers away during furling,
and the bundle stays between the fixed upper attachments.

Area, centroid, bounds, shadow samples and shipyard collision strips follow the
revised cut. Shadow sampling uses nine points and retains the native component's
two-parent lookup of its Sail despite the added pivot frame. Existing sails load
the new cut at their saved scale and may need clearance checked in the shipyard.
Geometry and compatibility checks cover the new shape; in-game comparison against
the annotated outline, tacking, furling, and save/reload remain necessary.

Version **0.7.2** makes the loaded clew-to-stay line define the sail's aft edge.
The upper aft corner lies partway along that curve, closer to the centerline than
the clew. Each leech mesh row has its own moving attachment on the same curve;
the rendered line uses those exact points before continuing to the fixed stay.
The supported leech keeps within its original length. Greater chord slack and
interior cloth travel allow a deeper belly. Existing port/starboard sheet controls
and their native slack behavior are retained. The shape blends back into the
reefed outline as it is furled. In-game cloth fullness, both tacks, and save/reload
still need visual verification.

Version **0.7.1** adds a visible running line from the clew through the upper aft
corner to a fixed point on the triatic stay. The existing sheet winches still
control the sail. The aft corners move slightly toward the mast to leave spare
cloth, while the upper aft corner also twists with apparent wind relative to the
clew. Softer bending, increased cloth travel through the belly, and stronger
cloth wind response allow a fuller shape. Corner slack fades out during furling;
the added line hides when fully struck. These settings require in-game comparison
on both tacks, including furling and save/reload.

Version **0.7.0** gives the fisherman sail a pivot along the forward mast when
installed on a triatic stay. Its whole forward edge stays at that mast, including
at smaller sail scales, while the upper aft corner and clew swing outward together
under the game's existing apparent-wind forces and sheet limits. The top-edge
fabric between its corners can billow; it is no longer pinned along the stay.
The model, wind center, shadow, halyard endpoints, and furled visuals move together.
The shipyard collision sweep uses the same mast axis. Sail and stay IDs and native
saved installation coordinates are retained. This motion needs in-game validation
on both tacks, at different sheet settings, and through furling and save/reload.

Version **0.6.2** prevents recursion in NANDFixes 1.4.3's order-text wrapping.
Removing or replacing a triatic stay can put more than 45 characters before the
order arrow. NANDFixes then recursively passes that same prefix to `AddLine`
without shortening it. Triatic order lines now wrap iteratively into the native
scrollable line list, before NANDFixes runs. Prices, arrows and error text are
retained; short and unrelated lines retain their normal handling. No external
mod settings, stay identities, or save data are changed.

Version **0.6.1** makes installation prerequisites follow the resolved attachment
sections. The Brig's forward topmast-stay variants no longer require an unused
main topmast when the horizontal stay touches the lower mainmast. Height-reference
and furl-control masts remain required, as do unrelated donor prerequisites and
exclusions. Native checks still reject missing supporting masts.

When previewing removal or replacement of an installed triatic stay carrying a
sail, the occupied mount and its walking collision root remain active. The native
"current mast still has sails attached" error still blocks confirmation, while
the sail's collision checker remains able to finish. Restore the original option
or remove the sail before changing the stay. This addresses a disabled-checker
path; the reported full game freeze still requires an in-game retest.

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
2. Run the local installer from this repository to replace the installed DLL
   with the current Release build:

   ```sh
   ./install-local.sh
   ```

   For another game directory, use `./install-local.sh "/path/to/Sailwind"`.
   The script copies only `bin/Release/netstandard2.0/FishermansSail.dll`;
   it does not build the mod. Build first if needed with
   `nix develop -c dotnet build -c Release`.

3. Launch Sailwind normally through Steam. Open `BepInEx/LogOutput.log` in the
   game directory and look for the startup message:

   ```text
   [Info   :Fisherman's Sail] Fisherman's Sail 0.7.15 loaded!
   ```

4. Load a test save with access to the brig and a shipyard. When the game's prefab
   directory initializes, the log should also contain:

   ```text
   Registered Fisherman's Sail Prototype: source=110, index=400, vertices=825, ...
   ```

   That line reports vertex and corner counts, aft depth, head camber, and sail areas.
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
7. Sheet on both sides: the upper aft corner must swing away from the stay at
   slightly less than the clew's angle. The upper sheets must rise to the aft
   mast's existing upper halyard pulley and descend to the existing sheet controls,
   with more sag on the slack side. Check that the original halyard still works
   and repeat after adding/removing the optional topmast: the turn must remain at
   an existing active pulley, never floating above an absent mast. The log's
   `Fisherman upper sheet guide changed` entry identifies the chosen attachment.
   The top and forward edges must billow between
   their corners. The aft edge must show raw cloth without a rope joining its
   corners, and should flex in the wind between its two controlled endpoints.
   Tack repeatedly through the wind: the top/luff curve should pass smoothly to
   the leeward side while every corner stays attached. Check for detachment,
   repeated resets or flickering in light wind. Repeat with eased sheets and
   after save/reload, and check separate fisherman sails on opposite tacks.
   Ease the sheets and check that cloth beside the clew stays smooth instead
   of folding over, and confirm the sail still produces forward force. Both forward
   corners stay on the physical foremast axis. Furl halfway, strike fully,
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
1,536 triangles), UVs, sorted bone weights, and cloth constraints. Only the four
corners are pinned. The forward corners stay at the mast, while the aft corners
follow the sheet-driven pose. The free leech has bounded travel around its
tension-fitted skin curve. The top and forward edges can billow between their
corners, and movement tapers through the cloth near the clew. The source brig
mesh and prefab remain unchanged.

The prototype owns one shared cloth mesh. Installed sails retain that mesh and
their bind poses throughout trimming and tacking. A grid of shaping bones carries
the signed camber; the cloth can flex around those targets. Both the cloth and
reefed renderers use these bones, with shaping offsets fading during furling.
Wind changes do not replace the mesh or trigger a cloth reset.

`FishermanSailRig.cs` replaces the cloned triangle animation with four procedural
corner bones, intermediate leech bones driven by the tension solver, and shaping
bones driven by apparent wind and the native reef control. It attaches the native sheets to the lower aft
corner. A frame around the physical forward mast keeps the forward corners in
place while the clew swings under wind and sheet control and the upper aft
corner follows 85% of the native sheet angle, fading back to its neutral position
with furling. Separate upper sheet visuals rise through the aft mast's existing
upper halyard pulley to the same controls. This guide is separate from the triatic
attachment used by the shaping frame. The tension solver
fits the clew using both the foot and leech lengths, including during furling.
The triatic mount remains the game's save and installation reference. The disabled donor Animator remains as
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

The geometry checks verify the revised cut, top camber, triangle winding, area,
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
required continuations and physically adjoin. Donor endpoint prerequisites are
replaced with the resolved attachment sections; height/control masts and other
prerequisites and exclusions are retained (excluding angled stays).
Do not reorder existing groups or variants: saves address these by position.
The resolver validates the complete profile before construction; construction
failure rolls back all new groups on that boat to avoid shifting saved slots.
Add new boat keys using the exact prefab name, without `(Clone)`, and extend the
profile compatibility checks when adding support.
