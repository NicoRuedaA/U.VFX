# Emerald Moves + BotW VFX

165 Pokémon move effects and four standalone BotW-inspired effects for **Unity 6000.6.4f1 / URP 17.6.0**.

![Moves 161–165](Assets/BotwVFX/Documentation/Previews/impact-161-165.jpg)

## Try the project
Open `Assets/BotwVFX/Scenes/EmeraldMoves_Demo.unity` and enter Play for the move gallery. The original four-effect demo is `Assets/BotwVFX/Scenes/BotwVFX_Demo.unity`.

## Use the portable package
Import `EmeraldMoves-165-Organized.unitypackage` into a URP project. Everything is grouped under **Assets/EmeraldMoves**. Drag a prefab from **Prefabs**, enable **Play On Start** (and optionally **Loop**) on **Emerald Move Vfx**, then enter Play. Stopped effects are invisible; the runtime package has no editor preview UI.

URP is installed separately; the package does not change project settings. Impact cues use a director that writes global `Time.timeScale`: clear `slowMotion` and `shakes` cues before playing when your game owns those systems.

## Real previews
[001–020](Assets/BotwVFX/Documentation/Previews/impact-001-020.jpg) · [021–040](Assets/BotwVFX/Documentation/Previews/impact-021-040.jpg) · [041–060](Assets/BotwVFX/Documentation/Previews/impact-041-060.jpg) · [061–080](Assets/BotwVFX/Documentation/Previews/impact-061-080.jpg) · [081–100](Assets/BotwVFX/Documentation/Previews/impact-081-100.jpg) · [101–120](Assets/BotwVFX/Documentation/Previews/impact-101-120.jpg) · [121–140](Assets/BotwVFX/Documentation/Previews/impact-121-140.jpg) · [141–160](Assets/BotwVFX/Documentation/Previews/impact-141-160.jpg)

These are actual near-impact renders. All 165 passed source-project playback; final colour/size polish and some four-phase comparisons remain incomplete.

## Export again
With the project closed, set `EMERALD_PACKAGE_OUTPUT` to a new output path and run Unity `-batchmode -nographics -projectPath <project> -executeMethod BotwVfx.EditorTools.EmeraldPackageExport.RunOrganizedBatch -quit`. Requires Python 3 for archive path normalization. The exporter preserves source assets and GUIDs and refuses overwrites.

## Credits
Original visual references: [Daniel Ilett's Remote Bomb](https://www.youtube.com/watch?v=bjZr7gzyvt8) and [Making Zelda-like VFX with Unity](https://80.lv/articles/making-zelda-like-vfx-with-unity/).

Retain the [Emerald upstream license](Assets/EmeraldMoveVFX/UPSTREAM-LICENSE.txt) and [Substitute model credit](Assets/BotwVFX/Models/Substitute/LICENSE.txt). Substitute is by LunaEagle, CC-BY-4.0. These notices do not grant blanket rights to Pokémon material.
