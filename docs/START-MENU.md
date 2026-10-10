# Sleeping panda start menu

Open `Assets/_RollingPanda/Scenes/StartMenu.unity`, then press Play. The tree, sleeping red-headband panda, bamboo, drifting petals and overflowing gem pouch appear at runtime. This is the first 3D composition pass; the character and tree remain procedural prototype art for later refinement. Alagard and the generated Diamondo mesh are reused when installed locally, with built-in font/geometry fallbacks.

- Roll / Play opens VisualPrototype with a fresh run.
- Settings changes and saves master volume; this affects audio once sounds are present.
- Leaderboards shows a saved personal best and labels online rankings as a future feature.
- Shop is a labelled preview with no purchases or currency spending.
- Pause or end a run and choose Main menu to return. Restart and Resume retain their existing behavior.

StartMenu is the first enabled build scene and VisualPrototype is second. If an existing custom Unity Build Profile overrides the shared scene list, add these two scenes in that order to its Scene List. Existing comparison scenes remain in the project and can still be opened directly.

Check in Unity:
1. Open StartMenu and check the sleeping pose, tree framing and gem pouch.
2. Test all four buttons; change volume, go Back and confirm the value survives a return to the menu.
3. Play, collect gems, pause and return to Main menu, then start another fresh run.
4. End a run and confirm its personal best appears in Leaderboards.
5. In Game view, compare portrait and landscape. Keep all buttons inside the safe area.

Validation performed: runtime desktop, iOS conditional code and editor tools compile against the installed Unity 6000.6.5f1 assemblies. Existing gesture/endless regression checks pass. This does not verify Play Mode visuals, an iOS build or physical iPhone touch/performance; those still require Unity/device testing.
