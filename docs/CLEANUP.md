# MoreSailwindSails — deferred architecture cleanup

CLEANUP-1, CLEANUP-2, CLEANUP-3 and CLEANUP-5 are implemented and passed their
applicable automated checks. Their detailed history is in Git; current behavior
and validation limits are summarized in [AGENTS.md](../AGENTS.md) and the
[development guide](DEVELOPMENT.md). Implementation completion does not imply
full in-game validation.

No cleanup work is currently active. The remaining item is saved for a later
request; this document does not authorize resuming it.

## CLEANUP-4 — Extract identical shared calculations

- [ ] Deferred; implementation not retained.

**Reason:** The user reverted the extraction to keep Flying Sail development
independent of the staysail family. Keep their helpers separate for now.

**Future scope:** Reassess which calculations still match once Flying Sail work
settles. Candidates are the tension solver and aerodynamic frame construction,
with appearance setup and text wrapping reviewed for exact duplication. Extract
only confirmed common logic into small helpers; retain family-specific tuning,
Harmony filters, ordering/input protections, mounting, reefing and lifecycle
behavior. Do not introduce a generic rig hierarchy. Existing shared winch
controls are outside this deferral.

**Acceptance when resumed:** Preserve geometry/wind-frame outputs, finite tension
fallbacks, appearance defaults and text-guard scope. Run formatting checks,
Release build, GeometryChecks, AssemblyChecks and `git diff --check`. Separately
verify both families' propulsion, appearance and affected shipyard interactions
in-game. Preserve plugin identity, native save layout and prefab IDs 400/401/402.
Record actual checks and remaining in-game uncertainty at handoff.
