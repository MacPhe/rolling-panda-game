# Menu music, mist music and HUD

The menu and Shop loop the full BGM15shrine clip at 0.28 source volume with an 1800 Hz low-pass filter to soften its bright frequencies. This uses the approved softened audition direction, with Unity's runtime filter rather than uploading a processed vendor file. It fades in on menu entry and out when Roll/Play is selected. Disabling the menu stops its source before gameplay continues. The full source repeats; the short audition's artificial ending fade is not included.

Gameplay percussion remains at 0.15 source volume. BGM16jungle loops at up to 0.12 volume through a 1600 Hz low-pass filter. Its level follows the same shared MistAt calculation as the forest visuals: fades in over 800–1500m, stays present through 2300m, fades away by 3000m, and repeats in each 6000m atmosphere cycle. It stops outside mist once its volume reaches zero. Pause resumes both music sources at their paused positions; defeat, restart and return to menu stop/reset them. Shield blocking does not interrupt music. Existing Settings master volume controls all sources.

Footsteps are reduced exactly 30%, from 0.09 to 0.063 source volume. Jump and slide levels are unchanged. Gem collection sounds remain the next upgrade.

HUD: gem counters remain top left, with Score directly underneath in the same panel. Distance alone remains top right. Active powerup timers begin below the larger gem/score panel, and the top touch exclusion is extended to cover it. Existing gem +1 flights still target the same gem icons. The layout uses the existing portrait/landscape safe area scaling.

The editor's MovementAudioBuilder populates Shrine, Jungle and Percussion references from local imported packs. If music is absent, use Rolling Panda > Refresh movement audio outside Play Mode. No vendor audio is uploaded by this change.

## Validation and Unity check

Runtime/editor, iOS-defined runtime and editor-tool C# compilation pass against Unity 6000.6.5f1. Existing pickup, powerup, gesture and endless runner regression checks pass. The full source clips exist locally. Unity visual/audio playback is not available in this environment.

1. Wait for compilation, open StartMenu and listen across the Shrine loop boundary (about 42 seconds). Enter and leave Shop: music should continue.
2. Roll/Play: Shrine fades out and quiet percussion begins with gameplay. Confirm softer footsteps and normal jump/slide levels.
3. Run through 800–1500m: Jungle should gradually join as mist increases. Continue beyond 3000m to hear it leave; it returns in the next mist cycle.
4. Pause/resume, lose, restart and return to the menu; check no duplicated music or stale Jungle playback.
5. Check Settings mute and portrait/landscape HUD. Score should sit under the gems, distance at top right, powerup timers below Score.
