# Movement prototype

## Open and play

1. Return to Unity and let it import the new scripts. If needed, use Assets > Refresh.
2. Open `Assets/_RollingPanda/Scenes/MovementPrototype.unity` from the Project window.
3. Press Play, then click the Game view so keyboard input reaches the game.
4. The track and capsule appear when Play starts. They are generated for this disposable movement test, so the scene looks empty before Play. Main remains the Phase 1 scene.

| Action | Keyboard |
|---|---|
| Change lane | A / D or Left / Right arrows |
| Jump | Space, W, or Up arrow |
| Slide | S or Down arrow |
| Pause / resume | P or Escape |
| Restart the test | R |

The player runs at 10 metres per second. Lanes are 3 metres apart, jump height is approximately 2.2 metres, and a tapped slide lasts one second. Hold S/Down to stay low longer. Repeated jump presses in the air do not add extra jumps. Sliding starts only on the ground; jumping cancels a slide when there is room to stand. Lane changes are available during jumps and slides. The fixed test course ends at 990 metres (about 99 seconds). Press R to reset it. This is not an endless-track implementation yet.

## Check before the next phase

- No red Console errors after opening the scene, entering Play, and stopping.
- A capsule with a red headband cue runs along the marked track and the camera follows.
- Repeated left/right taps stop at the outer lanes; simultaneous opposite keys cancel each other.
- Jump lands on the track. Repeated jump taps while airborne do not cause flight.
- Slide visibly shortens the player and returns it to full height automatically, with its feet on the track.
- Lane changes work during jumping and sliding.
- Pause stops movement; resume continues it. R resets the position, lane, jump, slide, pause, and camera.
- Stop and re-enter Play: only one player and track are created.
- Let the course finish, then press R to restart.

The initial Phase 1 open/Play/Console checks were confirmed by the user on 10 October 2026. For this movement checkpoint the four scripts were compiled successfully with the installed Unity 6000.6.5f1 compiler and Unity/Input System references. Scene wiring and metadata were checked statically. Interactive movement, rendered appearance, and device behaviour still require the above checks in Unity. No mobile controls, collision obstacles, scoring, gems, art assets, or build configuration changes are included.

The placeholder is deliberately separate from its CharacterController collision shape. Later art can replace the visual without rewriting movement. The runtime-created layout should become authored prefabs and track sections in the endless-track phase. Do not use this keyboard overlay as the final mobile UI.
