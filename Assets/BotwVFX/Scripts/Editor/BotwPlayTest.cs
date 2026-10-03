using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Prueba automática en Play Mode (pensada para línea de comandos):
    /// entra en Play, lanza los 4 efectos como lo haría la demo, captura
    /// algunos fotogramas y falla si aparece cualquier error o excepción.
    /// -executeMethod BotwVfx.EditorTools.BotwPlayTest.RunBatch -captureDir RUTA
    /// </summary>
    [InitializeOnLoad]
    public static class BotwPlayTest
    {
        const string DirKey = "BotwPlayTest.Dir";

        struct Shot
        {
            public int station;
            public float effectTime;
            public string name;
        }

        static readonly Shot[] Shots =
        {
            new Shot { station = 0, effectTime = 0.25f, name = "play_bomb" },
            new Shot { station = 1, effectTime = 0.45f, name = "play_explosion" },
            new Shot { station = 2, effectTime = 2.2f, name = "play_guardian_charge" },
            new Shot { station = 2, effectTime = 2.9f, name = "play_guardian_impact" },
            new Shot { station = 3, effectTime = 0.9f, name = "play_arrow" },
        };

        static readonly List<string> problems = new List<string>();
        static int shotIndex;
        static int phase;
        static double phaseStart;
        static float minTimeScale = 1f;
        static float maxShake;

        static BotwPlayTest()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(DirKey, "")))
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void RunBatch()
        {
            string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures"));
            Directory.CreateDirectory(dir);
            SessionState.SetString(DirKey, dir);
            EditorSceneManager.OpenScene(BotwScene.ScenePath);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.EnterPlaymode();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;
            Application.logMessageReceived += OnLog;
            shotIndex = 0;
            phase = 0;
            phaseStart = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }

        static void OnLog(string message, string stackTrace, LogType type)
        {
            // El indexador de búsqueda del editor a veces lanza excepciones propias en batchmode.
            if (stackTrace != null && stackTrace.Contains("UnityEditor.Search."))
                return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                problems.Add($"{type}: {message}\n{stackTrace}");
        }

        static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            var demo = Object.FindAnyObjectByType<DemoController>();
            if (demo == null)
            {
                Finish("No hay DemoController en la escena.");
                return;
            }

            minTimeScale = Mathf.Min(minTimeScale, Time.timeScale);
            if (VfxDirector.Instance != null)
                maxShake = Mathf.Max(maxShake, VfxDirector.Instance.ShakeOffset.magnitude);

            if (shotIndex >= Shots.Length)
            {
                Finish(null);
                return;
            }

            var shot = Shots[shotIndex];
            var fx = demo.stations[shot.station].effect;
            switch (phase)
            {
                case 0: // seleccionar estación y dejar que la cámara llegue
                    if (shotIndex == 0 || Shots[shotIndex - 1].station != shot.station)
                    {
                        demo.Play(shot.station);
                        phase = 1;
                        phaseStart = now;
                    }
                    else
                    {
                        phase = 3; // mismo efecto ya en marcha: esperar al siguiente instante
                    }
                    break;
                case 1:
                    if (now - phaseStart > 1.6 && !fx.IsPlaying || now - phaseStart > 12)
                    {
                        demo.Play(shot.station); // repetir con la cámara ya colocada
                        phase = 3;
                    }
                    break;
                case 3:
                    if (fx.IsPlaying && fx.CurrentTime >= shot.effectTime)
                    {
                        Capture(demo.targetCamera, shot.name);
                        shotIndex++;
                        phase = 0;
                    }
                    else if (!fx.IsPlaying && now - phaseStart > 20)
                    {
                        problems.Add($"El efecto {fx.name} terminó sin llegar a t={shot.effectTime}");
                        shotIndex++;
                        phase = 0;
                    }
                    break;
            }
        }

        static void Capture(Camera cam, string name)
        {
            const int w = 960, h = 540;
            var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            var previous = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = previous;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(Path.Combine(SessionState.GetString(DirKey, "."), name + ".png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void Finish(string fatal)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            if (fatal != null)
                problems.Add(fatal);
            if (minTimeScale > 0.5f)
                problems.Add($"La cámara lenta del impacto no se activó (timeScale mínimo = {minTimeScale:0.00}).");
            if (maxShake <= 0.001f)
                problems.Add("La sacudida de cámara no se activó.");

            string dir = SessionState.GetString(DirKey, ".");
            string report = problems.Count == 0
                ? $"OK. timeScale mínimo = {minTimeScale:0.00}, sacudida máxima = {maxShake:0.000}"
                : string.Join("\n\n", problems);
            File.WriteAllText(Path.Combine(dir, "playtest.txt"), report);
            Debug.Log("[BotW VFX] PlayTest: " + (problems.Count == 0 ? report : $"{problems.Count} problema(s)"));
            SessionState.EraseString(DirKey);
            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return null;
        }
    }
}
