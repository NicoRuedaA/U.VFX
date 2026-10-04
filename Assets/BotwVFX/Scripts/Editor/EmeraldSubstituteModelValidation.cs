using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>Focused, non-rendering regression check for move 164's imported model and lifetime.</summary>
    public static class EmeraldSubstituteModelValidation
    {
        public static void RunBatch()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BotwVFX/Prefabs/Emerald/164-Sustituto.prefab");
                if (source == null) throw new InvalidDataException("Move 164 prefab is missing.");
                var instance = UnityEngine.Object.Instantiate(source);
                var fx = instance.GetComponent<EmeraldMoveVfx>();
                var model = instance.transform.Find("SubstituteDoll/LunaEagle_Substitute");
                if (model == null) throw new InvalidDataException("Imported Substitute model is missing.");
                var renderers = model.GetComponentsInChildren<MeshRenderer>(true);
                var filters = model.GetComponentsInChildren<MeshFilter>(true);
                if (renderers.Length == 0 || filters.Length == 0) throw new InvalidDataException("Model geometry is missing.");
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null || mesh.vertexCount == 0 || mesh.uv.Length != mesh.vertexCount || mesh.colors32.Length != mesh.vertexCount)
                        throw new InvalidDataException("Model must retain geometry, UVs and white vertex colours.");
                }
                fx.Preview(1.5f);
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    if (!renderer.enabled || renderer.sharedMaterial.shader.name != "BotwVFX/Environment/Toon Lit"
                        || renderer.sharedMaterial.GetTexture("_BaseMap") == null)
                        throw new InvalidDataException("Model is not visible with the textured toon material at peak.");
                    bounds.Encapsulate(renderer.bounds);
                }
                if (Mathf.Abs(bounds.size.y - 1.65f) > .02f || Mathf.Abs(bounds.min.y) > .02f)
                    throw new InvalidDataException($"Unexpected model placement: height={bounds.size.y}, floor={bounds.min.y}.");
                foreach (float time in new[] { -.1f, .4f, 2.9f, 3.6f })
                {
                    fx.Preview(time);
                    foreach (var renderer in renderers)
                        if (renderer.enabled) throw new InvalidDataException($"Model remained visible at t={time}.");
                }
                fx.Preview(1.5f);
                var replayBounds = renderers[0].bounds;
                foreach (var renderer in renderers) replayBounds.Encapsulate(renderer.bounds);
                if ((replayBounds.center - bounds.center).sqrMagnitude > .000001f || (replayBounds.size - bounds.size).sqrMagnitude > .000001f)
                    throw new InvalidDataException("Model placement is not deterministic after seeking.");
                UnityEngine.Object.DestroyImmediate(instance);
                Debug.Log("[Substitute validation] OK: textured imported mesh, height 1.65m, floor 0m, peak visible, inactive outside lifetime, deterministic seek.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
