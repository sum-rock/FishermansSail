# Fisherman's Sail

A Sailwind mod providing a four-corner **Fisherman's Sail Prototype**, registered
at sail prefab index **400** and based on the brig jib's native cloth and controls.

Version **0.8.1** installs directly on a physical mast under **Other**. A supported
active mast aft of the selected mast is required, with existing halyard fittings
on both masts. No triatic stay is created or required. The normal mast capacity,
vertical-space and collision rules apply. Resize the sail to fit between the
masts, below their pulleys and above other sails.

Version **0.8.1** corrects false shroud obstruction reports from 0.8.0. The
shipyard checker now measures a thin neutral panel instead of full-height boxes
filled to maximum billow on both sides. It excludes the supporting mast's radius
plus 2 cm at the luff, where attachment contact is intentional. Other sail and
rigging collisions still apply, including the native angular sweep. The sweep
now also returns to the aligned neutral rotation instead of the donor's axes.
An offline triangle/box check against the installed Brig assets reproduced the
old shroud contacts; live Unity collision validation remains pending.

The sail owns independent port/starboard sheets and a hoist winch. It rises from
a small gathered pose near the forward mast's deck base and opens as it rises.
The deck datum is estimated from that mast's hoist winch height. Fully lowered
cloth is invisible; upper control-line ends park at the aft pulley and lower
sheet ends at the forward pulley, retaining the deck-to-mast runs. The rope ends
move to their parked positions when the cloth disappears. Fully raised cloth,
corner twist, tension fitting and propulsion retain the previous behavior.

Before upgrading from 0.7.x, remove the prototype sails and triatic stays and
save using the previous DLL. Old triatic installations are not migrated. New
mast installations use the game's native save format and need this mod to reload.
Remove them and save before uninstalling. The user confirmed the 0.7.15 pulley
routing looked correct in game; direct mast mounting and deck-up hoisting still
need in-game validation in 0.8.1. The 0.8.0 test reached the shipyard preview
but installation was blocked by the reported shroud collisions.

## Earlier prototypes

The following notes describe older releases and their historical stay-based setup.

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

1. Remove old prototype sails and triatic stays before upgrading from 0.7.x.
   Close Sailwind before replacing the plugin.
2. Build and run the installer from this repository:

   ```sh
   nix develop -c dotnet build -c Release
   ./install-local.sh
   ```

   Use `./install-local.sh "/path/to/Sailwind"` for another installation. The
   script copies only `bin/Release/netstandard2.0/FishermansSail.dll`; it does not
   build the mod.
3. Launch the game and confirm `Fisherman's Sail 0.8.1 loaded!` in
   `BepInEx/LogOutput.log`. Prefab registration should report source **110**, index
   **400**, and **825** vertices.
4. On the Brig, select the physical **foremast**, open **Other**, and select
   **Fisherman's Sail Prototype**. The active mainmast supplies the aft support;
   it need not carry another sail. On supported three-masted boats, a mainmast
   can similarly use a mizzenmast aft. Unsupported profiles and masts without a
   configured active aft support cannot accept the sail.
5. Resize and move the preview using normal shipyard controls. The long forward
   edge must fit the selected mast, both upper corners must stay below their
   support pulleys, and the top width must leave clearance from the aft mast.
   Native vertical overlap and actual collision errors must clear before fitting.
   The base top width is 13.8 m and the forward depth is about 1.692 widths, so
   reducing the default size will often be necessary.
6. Confirm independent port/starboard sheets and a hoist winch. They are copies
   offset 0.35 m from the donor controls (further offsets for additional fisherman
   sails). Check access and clearance. A gaff or square sail on the same mast must
   keep its own controls; adding/removing either sail must not steal winches.
7. Hoist from fully lowered: cloth must rise from near deck level, open smoothly,
   and reach the normal four-corner cut. Pause and reverse the hoist repeatedly.
   At full strike no cloth or bundle should remain visible; the upper line ends
   park at the aft pulley, the lower sheet ends at the forward pulley. Check that
   the existing halyards still work and that the next hoist reconnects every line.
8. Test port/starboard tacks, eased/tight sheets and weak wind. Keep useful forward
   force, attached corners, free leech flex and smooth camber reversal. Repeat
   with another sail on the mast, on another supported boat, after recoloring,
   and after save/reload. Check for extra winches after canceling shipyard orders.
9. Preview removing the aft support or either occupied mast: the order must reject
   removal while the fisherman depends on it. Remove the sail first; both masts
   should then be removable normally. Test optional topmasts both present and
   absent. The `Fisherman mast rig` log identifies the active support and pulley.

Automated checks do not simulate Unity Cloth or the live shipyard. In-game
validation is required for the new installation, controls and hoisting behavior.
The installed plugin is separate from the build output; install only after
closing Sailwind. No build or test changes game saves or installed assemblies.

## How it works

`PrototypeSail.cs` clones the brig jib under an inactive template, changes its
category to `other`, and preserves prefab index 400. Registration runs after
Shipyard Expansion and before All Sails in All Shipyards caches its list.
The physical mass and angular damping explicitly retain the former staysail
values despite the category change.

`FishermanRigging.cs` resolves the selected mast and an active aft support from
`BoatRigs/`. Profiles still name the existing donor stays as sources for sheet
winch assets; those stays need not be installed. Connected mast sections share
one support group, and only active mast/pulley references can be selected.
Existing fittings are reused without moving or cloning the pulley hardware.
Each fitted sail owns its cloned controls and cleans them up when removed.

`MastInstallationPatches.cs` adds support/fit checks to the shipyard and protects
supporting masts during removal previews. The native mast control-binding pass
sees only ordinary sails; a Harmony finalizer restores the full sail list even
if native binding throws, then attaches the fisherman's independent controls.
The complete list remains the source for native mast capacity, overlap and saves.
No synthetic mast IDs, new rigging parts or save-array expansion are registered.

`FishermanSailRig.cs` aligns the model with the physical forward mast and aft
support, retaining the mast hinge and aerodynamic frame. Corner bones move from
the deck gathering point to their fully set positions during hoisting. Partial
hoists use the procedural skinned renderer; only fully raised sails use Cloth.
The invisible bundle renderer is retained solely for native color/animation
references. Rope endpoints remain separate leaves so native rope rotation cannot
rotate skin bones.

`PrototypeGeometry.cs`, `FishermanBillow.cs` and `FishermanTension.cs` retain the
fixed cloth mesh, four pinned corners, free leech, bone-driven camber and coupled
edge constraints. Meshes and bind poses never change on tacks. The order-text
wrapping guard still protects long fisherman messages from NANDFixes recursion.

Run the regression checks:

```sh
nix develop -c dotnet run --project tests/GeometryChecks -c Release
nix develop -c dotnet run --project tests/AssemblyChecks -c Release
git diff --check
```

Checks cover hoisting/fitting geometry, active guides, tension, skinning,
aerodynamics, boat profiles, order text, installed Harmony signatures and cloth
lifecycle restrictions. Historical stay-geometry fixtures remain available through
`-- --stay-fixture /path/to/stays.json`; they validate shared mast geometry helpers,
not current shipyard placement. Native mast winch interactions, deck clearance and
Unity cloth motion still need the manual checks above.
