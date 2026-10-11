# Shop preview and movement sound

## What to try

1. Update phase-7-visual-prototype, wait for Unity to finish importing/compiling, then open StartMenu.
2. Open Shop: the sleeping panda is replaced by a standing display of the gameplay character. It gently breathes/sways and periodically lifts bamboo toward its mouth. This is a first procedural gesture, not a facial chew animation or imported UMotion clip.
3. Use Turn left/right (45 degrees each), switch between bamboo and idle poses, then Back. The original menu camera and sleeping panda return. No purchases or currency spending are implemented.
4. Roll/Play. Listen for eight alternating rocky-ground step clips, a jump sound on an accepted jump, and a short ground slide on an accepted slide. Check that airborne movement, stationary shield blocking, pause, game over, restart and return to menu do not leave footsteps playing.
5. Use Settings master volume to adjust or mute the sounds. Try portrait and landscape UI previews. Actual iPhone performance and sound still need device testing.

## Imported clips

The editor automatically creates Resources/Presentation/MovementAudioArt from local imports:

- Assets/Classic Footstep SFX/Rocky_ground/Rocky_ground_step0.wav through step7.wav
- Assets/Classic Footstep SFX/Rocky_ground/Rocky_ground_jump0.wav
- Assets/Classic Footstep SFX/Sliding_on_ground0.wav

If sounds are missing, confirm that pack exists at those paths and choose Rolling Panda > Refresh movement audio outside Play Mode. Missing clips are skipped without blocking gameplay. The generated resource holds references; vendor audio files are not included in this code update. Music is deliberately deferred for a later audition.

Footsteps follow the gameplay visual gait and only fire when grounded, moving forward and not sliding. Playback has modest volume and slight pitch variation. Jump/slide sounds come from actual accepted motor actions rather than button presses. Two reusable 2D AudioSources follow the existing master volume setting; all playback stops when the run is held or ended.

## Character construction

The shop display copies the gameplay panda's render hierarchy and shares its meshes/materials while keeping an independent pose. The held runner stays intact. Preview bamboo has no active collider. Custom transforms animate the display; UMotion's humanoid examples are not retargeted onto this prototype rig. Cosmetic sockets remain on the gameplay model for later equipment integration.

## Validation

Runtime/editor and iOS-defined C# compilation passed. Existing pickup, shield, magnet, gesture and endless-runner regression checks passed. Every selected audio file exists locally. Unity Play Mode appearance, gesture alignment, audio balance and loop-free state transitions require the manual review above; they could not be heard/rendered in this environment.
