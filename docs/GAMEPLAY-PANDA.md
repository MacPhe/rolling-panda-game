# Approved panda in gameplay

The runner now uses the approved look-study head: continuous rounded cheeks and muzzle, amber eyes, a smile, short ivory/charcoal fur and the removable red headband. Matching short fur is applied to the existing body and limbs. The shop preview clones this same character.

The existing body proportions, animation pivots, run/front-flip/side-kick slide, collision controller and movement rules are retained. Cosmetic attachment points are provided for head, back, waist and wrists. The isolated look-study scene remains available for comparison. Its studio lighting and buttons are not applied to gameplay.

This step replaces the gameplay and shop character. The sleeping menu panda remains the earlier separate model; its handoff still uses the existing fade. Full-body sculpting and a matching sleeping character remain later visual work.

## Validation

Windows/editor runtime, iOS runtime and editor tools compile against Unity 6000.6.5f1 assemblies. Existing movement, obstacle, endless scheduling and powerup regression checks pass. This environment cannot run Unity Play Mode, compile the graphics shader on the target GPU or measure iPhone performance.

## Unity check

1. Open StartMenu and enter Play Mode. In Shop, rotate the panda and check the face, band and fur while it sways.
2. Roll/Play and check the fade into the new runner.
3. Run, change lanes, jump/front-flip and slide/side-kick. Check that the head follows the poses and no old face remains visible.
4. Restart and repeat. Check daytime and nighttime readability and the Console.
5. Keep the isolated look-study scene for comparison. Mobile frame rate and fur cost still need device testing.
