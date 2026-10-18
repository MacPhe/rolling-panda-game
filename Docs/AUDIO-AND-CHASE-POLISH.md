# Audio controls, chase rhythm and performance polish

Menu Settings now offers Overall, Music, Sound effects and Ambient sounds sliders. Overall retains the existing RollingPanda.Volume preference; new categories default to 100%, preserving the approved quiet source levels. Desktop sliders and a dedicated mobile drag path share safe-area coordinates. Changes preview immediately and save on release, Back or leaving the menu. Music fades use separate envelopes so muting does not destroy the fade state. Shrine, percussion and mist music follow Music; footsteps, panda actions/voice, gems and poachers follow Sound effects. The new forest-air bed and sparse distant bird calls follow Ambient sounds. No paid assets are added.

Forest ambience uses two bounded voices and original clips generated once. Its six-second filtered-noise loop warms the filter over the repeating noise before writing the cycle; bird calls are spaced 11�25 seconds apart. Pause freezes playback; defeat softens the bed and suppresses birds; scene destruction releases clips. Imported music remains local and is not redistributed.

Poachers now alternate between closing and falling behind using overlapping distance waves. Their desired gap ranges from 2�14m, with a 15m retreat through chutes. Travel between gaps is rate-limited and lateral follow remains frame-rate independent. When distant they naturally leave the rear of the chase view, rather than disappearing abruptly in front of the camera. Quiet murmurs occur only at close range with a 12-second minimum interval. This remains cosmetic pursuit: it adds no timed catch or unavoidable damage.

Capture now prefers a front three-quarter camera angle. A non-allocating obstruction check tries wider angles around the fatal obstacle. The net has asymmetric folds, covers the head more fully and settles slightly after landing; it remains a procedural visual rather than simulated cloth. Results move lower so the face remains visible. Failure instructions now name small/large rocks rather than obsolete coloured blocks.

HUD counters reuse GUIContent and only rebuild text when values change. Pickup toasts retain their text instead of rebuilding it on every GUI event. Finished merged forest meshes discard their CPU copies after upload, retaining the existing geometry and pooled transform movement.

## Verification on 18 October 2026

- Unity 6000.6.5f1 editor/runtime, iOS-defined runtime and editor tools compile successfully.
- Gesture, endless-course, chute, magnet, shield, special-pickup and poacher regression suites pass. Pursuit tests additionally sweep 100,000 metres for bounded/continuous pressure and chute retreat.
- Live Unity cold-start test: independent category mute, master gain, actual small-rock defeat at 70.97m, net/sleep/results, capture pause and restart/camera recovery passed with zero new runtime errors.
- A separate rendering soak after restart reached 1,800m with zero new runtime errors, including close pursuit around 210m and distant pursuit around 800m. Obstacle colliders were deliberately disabled for this soak after the genuine collision test; it is not an obstacle-avoidance or complete gameplay certification.
- Independent saved category preferences, master level, clamping and NaN handling passed; original user volume settings were restored.
- Editor profiler observations before/after HUD cleanup: average main-thread sample approximately 6.97ms in both; average allocation 14,130 versus 12,723 bytes. These include editor activity, screenshots and repeated editor observations, so they do not establish a player FPS gain or isolate all allocation sources. Render batch counter was unavailable (zero); sampled peak geometry was approximately 0.94 million triangles. Fur/shader cost and GPU time still warrant device profiling.
- Unity returned to Edit Mode with StartMenu unmodified and original enter-play configuration restored. The temporary test harness was removed.

Local screenshots and full reports are in outputs/polish-playtest in the task workspace. Actual iPhone performance, touch feel, audio balance on speakers/headphones and long thermal behaviour remain unverified. Existing third-party UMotion importer-version warnings still appear during editor reload; no vendor DLL or metadata was altered by this pass.
