# Touch controls checkpoint

Continue using `GameLoopPrototype.unity`. The keyboard controls still work. In Play mode, click the Game view and drag the mouse across the open track area to simulate a swipe:

- Left/right: change one lane.
- Up: jump.
- Down: begin the same timed slide used by the keyboard. Holding the mouse after a down-swipe does not prolong it; standing still waits until overhead clearance is safe.
- Bottom buttons: Restart and Pause/Resume.

Each gesture triggers once after passing a threshold of 6% of the shorter screen dimension (minimum 20 pixels). Small taps and ambiguous diagonals are ignored; gestures taking more than 0.8 seconds to reach the threshold are discarded. A gesture beginning on the HUD, buttons, or outside the safe area cannot control the player. One primary finger is tracked; cancellation or a changed finger ID resets the gesture. Desktop mouse emulation is excluded from mobile builds. iOS button taps use the Input System touch stream explicitly, rather than relying on desktop GUI mouse events.

The HUD and buttons are placed within `Screen.safeArea` and recompute their layout when screen dimensions change. F8 on desktop toggles an approximate top/bottom inset preview. This is a layout aid, not an iPhone 16 Pro Max hardware simulation. Use a portrait Game view (for example 9:19.5) for a phone-shaped preview. The original movement-only scene retains its keyboard help overlay; the phone HUD belongs to the game-loop scene.

Losing app focus or backgrounding pauses the runner and cancels unfinished gestures. It remains paused until Resume/P is used. This can also happen when switching from Unity to another Windows app.

## Manual checks in Unity

1. Drag left/right several times: one lane change per drag, with no movement beyond the outer lanes.
2. Drag up/down: jump and slide work through the existing obstacles; keyboard controls still work.
3. Click or drag from the HUD/buttons: no lane change or jump. Restart and Pause/Resume still work.
4. Press F8 in portrait Game view: all HUD text and both buttons stay within the inset region. Toggle off again.
5. Pause and restart after collecting gems and after defeat: counters and gems reset correctly.
6. Switch to another application while running, return, and Resume: movement was paused and the old drag cannot trigger an action.
7. Check the Console after stopping and re-entering Play.

## Verification limits

All 15 pure gesture regression checks passed: four directions, one action per gesture, jitter, diagonals, blocked UI starts, cancellation, timeout, and reset. The gameplay scripts compile without warnings against the installed Unity and Input System references for both editor and iOS conditional code. This compilation is not an iOS build. Interactive mouse, layout, and button checks still require Unity Play mode. Actual iPhone touch events, safe-area values, background behaviour, rendering performance, signing, and installation remain unverified and are deferred until a device build is available.

The bundled SDK's normal test-project restore was blocked by access to the user-wide NuGet configuration. The same test source was compiled directly with the bundled Roslyn compiler and .NET 8 reference assemblies, then executed successfully with the bundled runtime; no dependency downloads were needed.

The standalone regression checks are in `Tests/SwipeGestureChecks`. With a .NET 8 SDK, first run `dotnet restore Tests/SwipeGestureChecks --configfile Tests/SwipeGestureChecks/NuGet.Config`, then `dotnet run --project Tests/SwipeGestureChecks --no-restore`. They do not require packages or a phone.
