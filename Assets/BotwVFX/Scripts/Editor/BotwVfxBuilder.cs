using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Menú "Tools/BotW VFX". Regenera texturas, mallas, materiales, prefabs y la escena.
    /// Ojo: "Rebuild Everything" sobrescribe los prefabs y la escena de demo; si
    /// retocas un efecto a mano, duplícalo antes.
    /// </summary>
    public static class BotwVfxBuilder
    {
        [MenuItem("Tools/BotW VFX/Rebuild Everything", priority = 0)]
        public static void BuildAll()
        {
            EnsureUrpSettings();
            BotwTextures.GenerateAll();
            BotwMeshes.GenerateAll();
            AssetDatabase.SaveAssets();
            BotwMaterials.CreateAll();
            BotwEffects.BuildAll();
            BotwScene.Build();
            AssetDatabase.SaveAssets();
            Debug.Log("[BotW VFX] Todo regenerado. Abre Assets/BotwVFX/Scenes/BotwVFX_Demo.unity y dale a Play.");
        }

        [MenuItem("Tools/BotW VFX/Open Demo Scene", priority = 1)]
        public static void OpenDemo()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(BotwScene.ScenePath);
        }

        [MenuItem("Tools/BotW VFX/Rebuild Prefabs Only", priority = 20)]
        public static void BuildPrefabsOnly()
        {
            BotwMaterials.CreateAll();
            BotwEffects.BuildAll();
            AssetDatabase.SaveAssets();
        }

        // El brillo de intersección necesita la Depth Texture, y el bloom necesita HDR.
        static void EnsureUrpSettings()
        {
            for (int i = 0; i < QualitySettings.count; i++)
            {
                if (QualitySettings.GetRenderPipelineAssetAt(i) is UniversalRenderPipelineAsset urp)
                {
                    urp.supportsCameraDepthTexture = true;
                    urp.supportsHDR = true;
                    EditorUtility.SetDirty(urp);
                }
            }
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset main)
            {
                main.supportsCameraDepthTexture = true;
                main.supportsHDR = true;
                EditorUtility.SetDirty(main);
            }
        }

        // Para línea de comandos: Unity.exe -batchmode -executeMethod BotwVfx.EditorTools.BotwVfxBuilder.BuildAllBatch -quit
        public static void BuildAllBatch()
        {
            try
            {
                BuildAll();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
    }
}
