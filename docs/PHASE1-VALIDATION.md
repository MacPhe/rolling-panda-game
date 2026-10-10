# Phase 1 validation — 10 October 2026

## Prepared

- Source baseline: `d741acca4e9a4ce81ded060d40910088a9482eb0` on `MacPhe/rolling-panda-game`.
- Generated from the installed editor's bundled `com.unity.template.urp-blank-17.2.1.tgz`. That template declares URP 17.6.0 and Input System 1.19.0 in its manifest; the archive filename is not the URP dependency version.
- Editor version and revision were read from the editor startup log.
- Template sample scene was moved to `Assets/_RollingPanda/Scenes/Main.unity`, preserving its GUID and updating the enabled build-scene path.
- Removed template welcome/tutorial assets from the deliverable; retained template rendering and input assets.
- Added project folders with unique folder metadata, repository text/binary attributes, generated-file exclusions, and beginner instructions.
- Preserved the original placeholder workflow. No CI build claim is made.

## Barrier

The batch editor connected to Package Manager and loaded assemblies, but logged `Licensing initialization failed` and repeated licensing-client reconnect attempts. The attempted process was stopped. The template-supplied package lock is included; successful complete import, project compilation, Play mode, visual rendering, and a player build have not been confirmed. This branch is a reviewable scaffold pending the checks below.

## Before treating Phase 1 as complete

- Activate the Unity licence in Hub and open using 6000.6.5f1.
- Let packages resolve and inspect the Console for import and compile errors.
- Open Main and confirm its template environment renders without pink materials.
- Enter and exit Play mode, then reopen the project and repeat.
- Review any changes to the template-supplied `Packages/packages-lock.json` after resolution succeeds.
- Optionally build a desktop player as a further smoke check. Mobile build verification belongs to the mobile checkpoint.

Do not merge solely because the existing example workflow passes; it does not exercise Unity.
