# Gameplay percussion and music auditions

Gameplay uses the user-approved Loop 5 - Complexity-4 from the imported Japanese Traditional Percussion pack. Its AudioSource volume is 0.15, with a 0.6-second fade-in. It is a single looping source at its original tempo and pitch. It starts after the entry hold, pauses/resumes with the run, stops on defeat/finish/entry hold, and restarts from the beginning after a run reset. A shield stopping forward movement does not stop the music. The existing master volume controls it along with effects.

MovementAudioBuilder automatically adds the imported percussion reference to Resources/Presentation/MovementAudioArt. If it is missing, choose Rolling Panda > Refresh movement audio outside Play Mode and confirm the percussion pack is imported. The clip path is Assets/Japanese Traditional Percussion - One Adaptive Music Loop/Loop 5 - 127 BPM - 4_4/Loop 5 - Complexity-4.wav. Vendor audio is not uploaded by this change.

Menu music and a secondary gameplay layer are not selected yet. Local audition files compare the existing Shrine track with a quieter two-stage 1800 Hz low-pass, Potion Store at a quieter level and 2200 Hz low-pass, and Jungle as a very muted 1600 Hz gameplay alternative. These processing choices soften the sound but do not change the underlying composition. They are auditions only, not claims that the tracks fit the requested atmosphere. The percussion audition uses the new 0.15 gain.

## Verification

Unity 6000.6.5f1 Windows/editor, iOS and editor-tool C# compilation passes. The source clip exists locally and is an approximately 7.56-second stereo WAV. Audio files and audition processing were inspected, but Unity playback and the loop seam still need listening in-game.

## Check in Unity

1. StartMenu should remain without music while we choose a menu track.
2. Roll/Play: percussion should fade in once control passes to the runner. Listen across several loop boundaries.
3. Pause/resume: the beat should resume from its paused position without a duplicate source.
4. Lose, restart and return to Main menu: confirm music stops/restarts correctly.
5. Check Settings volume including mute. Compare the balance with footsteps and jump/slide effects.

To tune the percussion, select Resources/Presentation/MovementAudioArt and adjust Percussion Volume; 0.15 is the starting value. Adventure has not been mixed in.
