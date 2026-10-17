# Poacher opening, pursuit and capture

The first playable poacher feature uses two original procedural placeholder characters with field coats, brimmed caps, packs, a net and a dart tube. Their appearance and gait are intentionally available for refinement. No paid model, animation or sound pack is required.

## Opening

Selecting Roll/Play hides the menu UI and plays a twig snap, followed by quiet approaching nonverbal poacher murmurs. The two poachers walk toward the tree over 1.45 seconds. The panda opens its eyes and reacts before the existing tree fall, forward roll and fade into gameplay. The normal menu and Shop remain calm until Play is selected. The murmurs are synthetic placeholders, not recorded/intelligible dialogue; replace with voice acting later if desired.

## Pursuit

Both poachers run behind the panda with animated arms and legs. They follow the runner through world rebasing and fall back when the panda enters a protected bamboo chute. This is a visual pursuit; they do not introduce a new chase meter, catch the panda on a timer, attack during a successful run or change obstacle difficulty. Their placeholder feet/props do not participate in physics, and their movement does not navigate obstacles yet.

## Ending

An actual unprotected defeat stops normal gameplay as before and starts a 3.6-second capture vignette. The running panda is hidden while a reusable copy of the approved character is framed by the camera. Poachers approach, throw a rope net (0.65–1.4s), then show a small turquoise-feathered dart cue and soft puff (1.85–2.1s). The panda closes its eyes and droops sleepily (2.1–2.8s). The result panel then reads Caught by poachers, preserving the original obstacle/bomb explanation and score.

The ending has no injury detail. The net and dart are cosmetic cues, not new projectile combat mechanics. Shield/chute-protected contacts cannot start the ending. Restart remains available during the sequence to skip/replay. Pause freezes the sequence and pauses its audio. Restart restores the running panda and chase camera; returning to menu cleans up the stage and generated resources. Disabling the pursuit component during capture also restores the panda/camera.

Panda vocals are increased lightly from 0.22 to 0.25 source volume (about 14%). Existing music, steps, gem sounds and other action levels remain unchanged. All new sounds follow Settings master volume. Twig/murmur/net/dart clips are generated once, and the characters, captive, net and dart are pooled/reused across restarts.

## Validation

Unity 6000.6.5f1 Windows/editor runtime, iOS-defined runtime and editor tools compile. New PoacherChecks verify net-before-dart-before-sleep ordering, results gating, pausing and long-frame clamping across 10,000 variable-frame captures. Existing chute, pickup, powerup, gesture and endless-runner checks pass. Unity Play Mode rendering, audio mix, prop alignment, camera framing and device performance still require manual review; they could not be rendered/heard here.

Standalone test: dotnet run --project Tests/PoacherChecks/PoacherChecks.csproj.

## Unity walkthrough

1. StartMenu > Shop > Back: check the original preview and music. Select Roll/Play and listen for twig plus murmurs; watch the approaching poachers, panda wake, fall and roll transition.
2. Run and change lanes. Look for the two poachers behind the panda; check they do not hide the player or cause collisions. Check at least one world rebase and a bamboo chute: pursuers should fall back and then return.
3. Hit an obstacle unprotected. Watch the camera, net throw, dart cue and sleepy panda, then the Caught by poachers panel with the original failure reason.
4. Test a shield/chute-protected contact: no capture sequence should start. Test a bomb defeat outside protection: the usual explosion may precede the capture.
5. Restart immediately during net flight, again during the dart cue, and after the result panel. The normal panda, camera, sounds and controls should return without a remaining net or dart.
6. Pause/resume mid-capture and return to Main menu. Check no duplicate actors, capture audio, hidden panda or stuck cinematic camera on the next run.
7. Compare portrait and landscape framing, and adjust master volume/mute. The captive camera backs away more in portrait to fit the scene.
