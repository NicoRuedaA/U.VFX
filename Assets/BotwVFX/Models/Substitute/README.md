# Substitute model

This work is based on "Substitute" (https://sketchfab.com/3d-models/substitute-a21a74a4d5cd42479beb0dab4944ee95) by LunaEagle (https://sketchfab.com/LunaEagle) licensed under CC-BY-4.0 (http://creativecommons.org/licenses/by/4.0/)

The original supplied glTF, binary buffer, texture and license are preserved in `Source/`. Retain this credit when redistributing the model or sharing work based on it.

## Project adaptations

- Converted the two static glTF meshes to one FBX mesh using Blender 5.2.1, preserving UVs and normals.
- Normalized to 1.65 metres high with a ground-level pivot; added white vertex colours for the existing textured Toon Lit shader.
- Assigned the original base-colour texture to a dedicated material using the existing BotW environment toon shader. No global shader changes.
- The model follows move 164's existing spawn/grow/hold/shrink timeline and renderer visibility lifecycle.

## Rebuild

From the repository root, run `blender --background --python Tools/convert_substitute.py` to regenerate `Substitute.fbx` solely from the bundled source. No Downloads-folder dependency or new Unity package is needed.

Unity's existing Emerald move builder (`BotwVfx.EditorTools.EmeraldMoves.BuildBatch`) creates/updates `Substitute_Toon.mat` and embeds the model into move 164. Move 164 now explicitly binds the gallery attacker: only its original renderers are hidden while the model is visible, then their prior enabled states are restored on stop, seek, replay, destruction or move change. A dedicated white-smoke material leaves other effects unchanged.
