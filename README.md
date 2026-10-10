# Rolling Panda

A planned three-lane mobile endless runner in semi-realistic stylised 3D: an expressive red-headband panda, bamboo and temple scenery, jumping, sliding, obstacles, and blue gems.

## Phase 1 status

This branch establishes the Unity foundation only. It contains the bundled Universal 3D template's camera, lighting, environment, URP settings, and Input System actions. Gameplay and the panda character will be added in later phases.

- Editor: **Unity 6000.6.5f1** (revision `3ff58d469c8a`), matching the installed editor used to extract the template.
- Universal Render Pipeline: **17.6.0**.
- Input System: **1.19.0**. The included template actions are a starting point; runner actions and swipe recognition are not implemented yet.
- Main scene: `Assets/_RollingPanda/Scenes/Main.unity`, enabled in Build Settings.
- Portrait orientation is configured for the eventual mobile game.

## First-time setup

1. In GitHub, select the Phase 1 branch or pull request. Download its ZIP and extract it into a short local path, or clone and check out that branch with GitHub Desktop.
2. Open Unity Hub. Under Installs, install **6000.6.5f1** if it is missing. Sign in and make sure a valid Unity licence is active. Do not accept an editor upgrade for this checkpoint.
3. In Hub, choose Projects > Add > Add project from disk. Select the folder containing `Assets`, `Packages`, and `ProjectSettings`.
4. Open the project and allow the first package download, import, and shader compilation to finish. Internet access may be needed. The bundled template includes a package lock file; review any changes after successful resolution.
5. In the Project window, open `Assets/_RollingPanda/Scenes/Main.unity`.
6. Open Window > General > Console. Resolve red errors before continuing. Press Play: the template environment should display, with no controllable panda yet. Press Play again to stop.
7. Close and reopen the project and repeat the check before beginning Phase 2.

If the scene is pink, first check that URP resolved in Window > Package Management > Package Manager and that the pipeline references remain assigned in Project Settings > Graphics and Quality. Do not replace shaders at random.

If import stalls on licensing, open Hub, sign in, and activate your eligible licence through Hub. No account credentials belong in this repository.

## Where work belongs

| Folder | Purpose |
|---|---|
| `Assets/_RollingPanda/Scenes` | Game scenes; start with Main |
| `Assets/_RollingPanda/Scripts` | Small C# gameplay components |
| `Assets/_RollingPanda/Prefabs` | Reusable player, obstacles, gems, track sections |
| `Assets/_RollingPanda/Art` | Panda, headband, bamboo, temple, materials, animations |
| `Assets/_RollingPanda/Audio` | Music and sound effects |
| `Assets/_RollingPanda/UI` | Menus, score, gem count, restart |
| `Assets/Settings` | Template URP assets and volume profiles |

Move and rename assets inside Unity so their `.meta` files move with them. Empty folders are represented by their folder `.meta` files and can be recreated by Unity. Commit `Assets` including all `.meta` files, `Packages`, and `ProjectSettings`. Generated caches, personal settings, builds, and signing keys are ignored. Text serialization and visible metadata are configured. Git LFS is not required by this foundation; agree a policy before adding large binary art assets.

## Mobile setup, later

The first checkpoint targets Editor verification. For Android, add Android Build Support and its SDK/NDK and OpenJDK through the matching editor's Hub modules. Then use File > Build Profiles to create an Android profile and test on a real phone. Android compilation, signing, device deployment, and store submission have not been performed. iOS requires the appropriate Apple build environment. Application identifiers and signing settings must be reviewed before distribution.

## Safe next checkpoints

1. Capsule player: auto-run, three lanes, jump, slide, follow camera; keyboard first.
2. One fixed course: obstacles, blue gems, distance score, defeat, restart.
3. Real-phone build: swipes, UI touch handling, safe areas, pause/resume.
4. Endless track: recycled sections, validated obstacle patterns, bounded speed increase.
5. Art pass: expressive animated panda with red headband, bamboo and temple modules; preserve obstacle readability.
6. Polish: audio, best score, tutorial, profiling and repeated-run stability.

Make one small checkpoint at a time. Keep the panda's visual model separate from its collision shape so animation does not change game rules.

## Validation and audit

See [Phase 1 validation](docs/PHASE1-VALIDATION.md). The editor was launched in batch mode, but logged licence initialization failure and repeated reconnect attempts. A successful import, compilation, Play mode run, and build are **not yet verified**. The existing `blank.yml` workflow only prints sample messages and is not a Unity test or build.

The read-only audit on 10 October 2026 found only a 39-byte README and the placeholder workflow on `main`, with two commits from 13 July 2025. No earlier prototype, Unity source, or art was present. Recover external source and check asset licences before reuse. No project licence or third-party rights are asserted by this scaffold.
