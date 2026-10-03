using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Capturas de los movimientos Emerald en la escena de galería.
    /// - CaptureBatch: sin Play, con Preview(t) (instantes exactos: anticipación, impacto, +0,15 s, +0,6 s).
    ///   -captureDir RUTA  -moves 1,53,56  [-shader "Nombre/Del Shader"] (sustituye el material horneado, para comparar)
    /// - PlayBatch: en Play, como un usuario (galería real, sacudida, cámara lenta); falla con cualquier error.
    ///   -captureDir RUTA  -moves 1,94
    /// </summary>
    [InitializeOnLoad]
    public static class EmeraldCapture
    {
        const int Width = 640;
        const int Height = 360;
        static readonly int[] DefaultMoves = { 1, 53, 56, 58, 75, 85, 94, 153 };
        const float ImpactShotDelay = 2f / 60f;

        [MenuItem("Tools/BotW VFX/Capture Emerald Moves", priority = 41)]
        public static void CaptureFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "Emerald"));
            CaptureAll(dir, DefaultMoves, null);
            EditorUtility.RevealInFinder(dir);
        }

        public static void CaptureBatch()
        {
            try
            {
                string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "Emerald"));
                CaptureAll(dir, ParseMoves(), GetArg("-shader"));
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        static void CaptureAll(string dir, int[] ids, string shaderOverride)
        {
            Directory.CreateDirectory(dir);
            EditorSceneManager.OpenScene(EmeraldMoves.ScenePath);
            var gallery = Object.FindAnyObjectByType<EmeraldGallery>();
            var cam = gallery.targetCamera;

            Material overrideMat = null;
            string suffix = "";
            if (!string.IsNullOrEmpty(shaderOverride))
            {
                var shader = Shader.Find(shaderOverride);
                if (shader == null)
                    throw new InvalidOperationException($"No se encuentra el shader '{shaderOverride}'.");
                overrideMat = new Material(shader);
                suffix = "_orig";
            }

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var frame = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                foreach (int id in ids)
                {
                    var prefab = gallery.moves.FirstOrDefault(m => m != null && m.moveId == id);
                    if (prefab == null)
                        throw new InvalidDataException($"No hay prefab para el movimiento {id}.");
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, gallery.stage);
                    var fx = go.GetComponent<EmeraldMoveVfx>();
                    if (overrideMat != null)
                        fx.player.effectMaterial = overrideMat;
                    string baseName = $"{id:000}_{Sanitize(fx.moveName)}{suffix}";
                    gallery.PoseFor(fx, cam);

                    // Impacto: 2 fotogramas (a 60 Hz) después del instante calculado, para que las
                    // ráfagas del impacto ya hayan salido (en t exacto la simulación lleva 0 s).
                    float ti = fx.impactTime;
                    float shot = ti + ImpactShotDelay;
                    RenderAt(fx, shot, cam, rt, frame, out int draws);
                    SaveCopy(frame, Path.Combine(dir, baseName + "_impact.png"));

                    var times = new[] { ti * 0.5f, shot, Mathf.Min(ti + 0.15f, fx.duration), Mathf.Min(ti + 0.6f, fx.duration) };
                    var sheet = new Texture2D(Width * 2, Height * 2, TextureFormat.RGB24, false);
                    for (int k = 0; k < times.Length; k++)
                    {
                        RenderAt(fx, times[k], cam, rt, frame, out _);
                        sheet.SetPixels(k % 2 * Width, (1 - k / 2) * Height, Width, Height, frame.GetPixels());
                    }
                    Save(sheet, Path.Combine(dir, baseName + "_sheet.png"));

                    // En reposo no debe quedar nada de la animación horneada.
                    fx.Preview(-1f);
                    bool restOk = !fx.player.enabled;
                    Debug.Log(string.Format(CultureInfo.InvariantCulture,
                        "[BotW VFX] Emerald captura {0}: impacto t={1:0.000}s (captura +0.033s) fuerza={2:0.00} dibujos={3} reposo={4} ({5})",
                        baseName, ti, fx.impactStrength, draws, restOk ? "OK" : "ERROR", string.Join(", ", times.Select(t => t.ToString("0.00", CultureInfo.InvariantCulture)))));
                    if (!restOk)
                        throw new InvalidOperationException($"{baseName}: el reproductor sigue activo en reposo.");
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(frame);
                if (overrideMat != null)
                    Object.DestroyImmediate(overrideMat);
            }
        }

        static void RenderAt(EmeraldMoveVfx fx, float t, Camera cam, RenderTexture rt, Texture2D into, out int draws)
        {
            fx.Preview(t);
            cam.targetTexture = rt;
            cam.Render(); // calentamiento, como en BotwCapture
            // Sin bucle de juego entre medias: encolamos los dibujos horneados para esta cámara.
            fx.DrawBakedNow(cam);
            draws = fx.player != null && fx.player.enabled ? fx.player.VisibleMeshCount : 0;
            cam.Render();
            RenderTexture.active = rt;
            into.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            into.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
        }

        // ---------------------------------------------------------------- Play

        const string PlayDirKey = "EmeraldPlayTest.Dir";
        const string PlayMovesKey = "EmeraldPlayTest.Moves";

        static readonly List<string> problems = new List<string>();
        static int[] playMoves;
        static int playIndex;
        static int phase;
        static double phaseStart;
        static float minTimeScale = 1f;
        static float maxShake;
        static int maxDraws;

        static EmeraldCapture()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(PlayDirKey, "")))
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void PlayBatch()
        {
            string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "Emerald"));
            Directory.CreateDirectory(dir);
            SessionState.SetString(PlayDirKey, dir);
            SessionState.SetString(PlayMovesKey, string.Join(",", ParseMoves()));
            EditorSceneManager.OpenScene(EmeraldMoves.ScenePath);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.EnterPlaymode();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Application.logMessageReceived += OnLog;
            playMoves = SessionState.GetString(PlayMovesKey, "1").Split(',').Select(int.Parse).ToArray();
            playIndex = 0;
            phase = 0;
            phaseStart = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }

        static void OnLog(string message, string stackTrace, LogType type)
        {
            if (stackTrace != null && stackTrace.Contains("UnityEditor.Search."))
                return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                problems.Add($"{type}: {message}\n{stackTrace}");
        }

        static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            var gallery = Object.FindAnyObjectByType<EmeraldGallery>();
            if (gallery == null)
            {
                FinishPlay("No hay EmeraldGallery en la escena.");
                return;
            }
            gallery.autoAdvance = false;
            minTimeScale = Mathf.Min(minTimeScale, Time.timeScale);
            if (VfxDirector.Instance != null)
                maxShake = Mathf.Max(maxShake, VfxDirector.Instance.ShakeOffset.magnitude);
            if (now - phaseStart > 30)
            {
                FinishPlay($"Tiempo agotado en la fase {phase} (movimiento {playIndex}).");
                return;
            }

            var fx = gallery.Current;
            switch (phase)
            {
                case 0: // mostrar el movimiento
                    if (playIndex >= playMoves.Length)
                    {
                        phase = 2;
                        phaseStart = now;
                        break;
                    }
                    int index = Array.FindIndex(gallery.moves, m => m != null && m.moveId == playMoves[playIndex]);
                    if (index < 0)
                    {
                        FinishPlay($"No hay prefab para el movimiento {playMoves[playIndex]}.");
                        return;
                    }
                    gallery.Show(index);
                    phase = 1;
                    phaseStart = now;
                    break;
                case 1: // esperar al impacto y capturar
                    if (fx != null && fx.IsPlaying && fx.CurrentTime >= fx.impactTime)
                    {
                        maxDraws = Mathf.Max(maxDraws, fx.player.VisibleMeshCount);
                        if (!fx.player.enabled || fx.player.VisibleMeshCount == 0)
                            problems.Add($"{fx.Title}: la animación horneada no dibuja en el impacto.");
                        Capture(gallery.targetCamera, $"play_{fx.moveId:000}_{Sanitize(fx.moveName)}");
                        playIndex++;
                        phase = 0;
                    }
                    break;
                case 2: // el último debe terminar y quedar en reposo
                    if (fx != null && !fx.IsPlaying)
                    {
                        if (fx.player.enabled)
                            problems.Add($"{fx.Title}: el reproductor sigue activo en reposo.");
                        FinishPlay(null);
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
            File.WriteAllBytes(Path.Combine(SessionState.GetString(PlayDirKey, "."), name + ".png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void FinishPlay(string fatal)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            if (fatal != null)
                problems.Add(fatal);
            if (maxShake <= 0.001f)
                problems.Add("La sacudida de cámara no se activó.");
            string dir = SessionState.GetString(PlayDirKey, ".");
            string report = problems.Count == 0
                ? string.Format(CultureInfo.InvariantCulture, "OK. {0} movimientos, timeScale mínimo = {1:0.00}, sacudida máxima = {2:0.000}, dibujos máximos = {3}",
                    playMoves.Length, minTimeScale, maxShake, maxDraws)
                : string.Join("\n\n", problems);
            File.WriteAllText(Path.Combine(dir, "emerald_playtest.txt"), report);
            Debug.Log("[BotW VFX] Emerald PlayTest: " + (problems.Count == 0 ? report : $"{problems.Count} problema(s)\n{report}"));
            SessionState.EraseString(PlayDirKey);
            SessionState.EraseString(PlayMovesKey);
            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        // ---------------------------------------------------------------- utilidades

        static int[] ParseMoves()
        {
            string arg = GetArg("-moves");
            return string.IsNullOrEmpty(arg) ? DefaultMoves : arg.Split(',').Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
        }

        static void SaveCopy(Texture2D tex, string path) => File.WriteAllBytes(path, tex.EncodeToPNG());

        static void Save(Texture2D tex, string path)
        {
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s.Replace(' ', '_').Replace('/', '_');
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
