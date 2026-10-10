# Evolving forest sky

The flat fog-coloured background is replaced by a procedural URP skybox: blue gradient, soft drifting clouds and sun during the day; warm peach/violet dusk; deep blue night with stars, gentle twinkle, moon and green/violet aurora ribbons. Norway's northern-light spectacle is the visual inspiration; the forest world remains our stylised Chinese setting.

The sky follows the existing repeating 6,000 m distance cycle: day first, mist around 800–2300 m, dusk around 1600–3400 m, deepest night around 3400–4800 m, and dawn returning to day by 6000 m. Autumn forest continues around 2400–4800 m. Sky animation freezes with gameplay pause/end, while clouds drift in the held menu. Restart returns the sky to daytime through distance reset.

Mist now starts farther from the panda (80 m normally, 60 m in the mist phase) and ends at 165/135 m. The sky shader does not apply scene fog, so stars/clouds retain contrast; its own thin horizon haze blends with the distance. Sun/moon positions are artist-directed for the low chase-camera view. Existing scene sunlight, ambient lighting and warm shrine/lantern lights remain controlled by RunAtmosphere.

The shader lives in Resources/Presentation so the player build retains it. One skybox material is created and cleaned up with the run; no cloud/star objects, textures or extra lights are spawned. The sky stays camera-centred through section recycling/origin resets. The procedural noise, trigonometry and transparency-free sky draw still require mobile GPU profiling.

Validation: desktop runtime, iOS conditional and editor-tool C# compilation pass, and existing gameplay/powerup rules checks pass. This does not compile the HLSL shader or verify its rendered appearance: the running Unity editor must import it and compile the platform variants.

Unity check:
1. Stop Play, update files and allow shader import. Confirm the Console has no shader errors or pink sky.
2. Start from StartMenu or VisualPrototype. Check cloud/sun visibility through the tree canopy, then dusk, stars/aurora after 3400 m and dawn after 4800 m.
3. Pause/resume and restart; clouds/twinkle should hold on pause and distance-driven colours return to day on restart.
4. Check both orientations, road/obstacle readability in mist/night and the menu-to-run fade. The dense canopy can naturally obscure parts of the sky; check open views too.
5. Review aurora brightness and star size on the iPhone when physical-device builds become available.
