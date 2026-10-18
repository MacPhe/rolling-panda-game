# Pursuit quality and smoothness pass

This pass replaces the rough poacher animation and surfaces while retaining the established gameplay rules and approved panda roll size. The prior implementation remains in branch history at f6bdd21ceca46c2703d2bcef79121c512728d0c3.

## Findings and changes

| Finding from the code audit | Change |
| --- | --- |
| One-piece legs and arms could only swing rigidly | Articulated shoulders/elbows/hands and hips/knees/boots; procedural two-bone leg posing with stance and swing targets |
| Poachers moved sideways instantly with the runner | Frame-rate-independent lateral follow, turning/lean and smoothed chute retreat/recovery |
| Fixed-speed gait did not respond to run pace | Bounded cadence follows the runner's speed; torso breathing, twist and head counter-motion add secondary movement |
| Capture teleported the pursuers and replaced the panda's pose | Capture starts from current pursuer positions and the panda's actual world pose, then eases into the catch |
| Net and dart flew from hard-coded points | Net releases from the carried hoop; dart tube aims at the panda and its tip provides the launch point |
| Bare solid-colour capsules read as placeholders | Shared original woven-canvas and worn-leather textures, lower gloss, differentiated olive/ochre outfits, pockets, collars, cuffs, belts, pack straps, soles, hats and facial features |
| Camera aim used the final camera location while it was still moving | Eased camera travel with aim computed from its current position |
| Eye toggling scanned the whole captive hierarchy each capture frame | Eye transforms are cached once and only toggled when sleep state changes |
| Extra clothing geometry could inflate rendering and memory cost | Static detail is merged by bone/material, only torso batches cast shadows, generated textures have mipmaps and discarded CPU copies, immutable merged/net mesh CPU copies are released |

The stance/swing paths and joint solving improve the procedural gait, but this is not a professionally authored motion-capture rig or full world-space foot-locking system. Pursuers remain cosmetic and do not navigate obstacles. Their new materials match the game's softer stylised direction; they do not apply panda fur to human clothing. Two shared 256px textures are created once. No paid assets or imported shader dependencies are added.

## Broader gameplay review

The existing pool/rebase scheme, non-allocating obstacle overlaps, staged difficulty, bounded audio voices, chute protection and restart/pause rules remain the stable foundation. This pass targets visible motion discontinuities and a confirmed recurring allocation rather than altering those rules. More geometry does not automatically mean better quality: real frame-time, GPU overdraw, shader cost and device memory still need Unity profiling before further detail is added. The furred panda and forest are likely useful profiling targets, but no bottleneck or FPS improvement is asserted without measurements.

## Verification

Unity 6000.6.5f1 Windows/editor, iOS-defined runtime and editor-tool C# compilation pass. PoacherChecks now cover 100,000 reachable foot samples, fixed upper/lower leg lengths, step-loop/toe-off continuity and 30/60/120fps follow equivalence, plus the existing 10,000 variable-frame capture timeline checks. Chute, gesture, pickup, shield, magnet, special-spawn and endless-runner checks pass.

The initial pass used source inspection. A later connected Unity playtest now verifies the capture, pause/restart and a rendering soak; see AUDIO-AND-CHASE-POLISH.md for evidence and limits. Actual device performance remains unverified.

## Unity playtest

1. Play the opening in landscape and portrait. Inspect the clothing/face/props as the poachers approach. Check knees/elbows and the wake/fall transition.
2. Run at low and high speed; switch lanes rapidly and reverse direction. Pursuers should trail the lane change smoothly, with bounded stride and body motion.
3. Use a chute and cross a world rebase. Check pursuers retreat/recover without snapping and tunnel controls/protection remain intact.
4. Trigger defeat while running, jumping and rolling. The panda should settle from its existing pose instead of popping upright. Check the net starts at the carried hoop and the dart at the tube tip.
5. Pause/resume, restart during the throw, restart during the sleepy reaction, and return to the menu. Check the original panda/camera, prop reset and audio cleanup.
6. Profile a representative run and several captures in Unity before judging frame-rate gains. Check render batches, main-thread frame time, GC allocation and memory over repeated restarts; test the actual phone when available.

Standalone checks: dotnet run --project Tests/PoacherChecks/PoacherChecks.csproj.
