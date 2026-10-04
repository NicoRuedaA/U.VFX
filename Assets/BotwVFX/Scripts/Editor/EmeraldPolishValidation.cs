using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    public static class EmeraldPolishValidation
    {
        public static void RunBatch()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BotwVFX/Prefabs/Emerald/164-Sustituto.prefab");
                var instance = UnityEngine.Object.Instantiate(prefab);
                var fx = instance.GetComponent<EmeraldMoveVfx>();
                var replacement = instance.GetComponent<EmeraldActorReplacement>();
                if (replacement == null) throw new InvalidDataException("Move 164 lacks actor replacement binding.");
                var actor = new GameObject("Actor");
                var visible = actor.AddComponent<MeshRenderer>();
                var hidden = new GameObject("OriginallyHidden").AddComponent<MeshRenderer>();
                hidden.transform.SetParent(actor.transform); hidden.enabled = false;
                replacement.Bind(actor.transform);
                void Expect(bool shown, string phase)
                {
                    if (visible.enabled != shown || hidden.enabled)
                        throw new InvalidDataException("Actor visibility mismatch: " + phase);
                }
                fx.Preview(.4f); Expect(true, "before spawn");
                fx.Preview(1.5f); Expect(false, "model present");
                fx.Preview(2.9f); Expect(true, "after model");
                fx.Preview(1.5f); Expect(false, "backward seek");
                fx.StopAndClear(); Expect(true, "stop");
                fx.Preview(1.5f); fx.Play(); Expect(true, "replay");
                fx.StopAndClear(); fx.Preview(1.5f);
                // Rebinding releases the previous owner before taking another actor.
                replacement.Bind(null); Expect(true, "unbind/move change");
                replacement.Bind(actor.transform); fx.Preview(1.5f);
                UnityEngine.Object.DestroyImmediate(instance); Expect(true, "destroy");
                UnityEngine.Object.DestroyImmediate(actor);

                int checkedMoves = 0;
                foreach (string path in Directory.GetFiles("Assets/BotwVFX/Prefabs/Emerald", "*.prefab").OrderBy(p => p))
                {
                    var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                    var effect = go.GetComponent<EmeraldMoveVfx>();
                    if (!effect.hasRecipe || Math.Abs(effect.duration - 3.6f) > .001f)
                        throw new InvalidDataException("Missing recipe or bad duration: " + path);
                    effect.Preview(effect.peakTime);
                    effect.Preview(-1f);
                    if (effect.player.enabled || go.GetComponentsInChildren<Renderer>(true).Any(r => !(r is ParticleSystemRenderer) && r.enabled)
                        || go.GetComponentsInChildren<ParticleSystem>(true).Any(p => p.particleCount != 0))
                        throw new InvalidDataException("Effect fails idle cleanup: " + path);
                    UnityEngine.Object.DestroyImmediate(go); checkedMoves++;
                }
                if (checkedMoves != 165) throw new InvalidDataException("Expected 165 moves.");
                Debug.Log("[Polish validation] OK: actor spawn/seek/stop/replay/rebind/destroy restore exact enabled states; 165 recipes, durations and idle cleanup checked.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
    }
}
