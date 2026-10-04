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
    /// - CompareBatch: vista de combate en tres cuartos (atacante delante a la izquierda, objetivo al fondo),
    ///   los 4 instantes clave de cada movimiento (anticipación, impacto, pico, disipación) en una tira
    ///   NNN_ours.png, para montarla junto a las referencias.  -captureDir RUTA  -moves 1-20
    /// -moves admite listas y rangos: 1,53,56  ·  1-20  ·  1-5,94
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
            // Sin bucle de juego: orientamos a mano los emisores que miran a cámara.
            foreach (var face in fx.GetComponentsInChildren<FaceCamera>(true))
                face.Face(cam.transform.position);
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

        // ---------------------------------------------------------------- comparativa (tres cuartos)

        const int CmpWidth = 480;
        const int CmpHeight = 270;
        // Vista de combate en tres cuartos: detrás y a la izquierda del atacante (-3,0,0), mirando al objetivo (+3,0,0).
        public static readonly Vector3 BattleCameraPosition = new Vector3(-6.4f, 2.3f, -4.6f);
        public static readonly Vector3 BattleCameraLookAt = new Vector3(0.2f, 1.2f, 0.6f);

        public static void CompareBatch()
        {
            try
            {
                string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "EmeraldCompare"));
                CompareAll(dir, ParseMoves());
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        static void CompareAll(string dir, int[] ids)
        {
            Directory.CreateDirectory(dir);
            EditorSceneManager.OpenScene(EmeraldMoves.ScenePath);
            var gallery = Object.FindAnyObjectByType<EmeraldGallery>();
            var cam = gallery.targetCamera;
            var stage = gallery.stage != null ? gallery.stage.position : Vector3.zero;
            cam.transform.position = stage + BattleCameraPosition;
            cam.transform.LookAt(stage + BattleCameraLookAt);

            var rt = new RenderTexture(CmpWidth, CmpHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var frame = new Texture2D(CmpWidth, CmpHeight, TextureFormat.RGB24, false);
            var log = new List<string>();
            try
            {
                foreach (int id in ids)
                {
                    var prefab = gallery.moves.FirstOrDefault(m => m != null && m.moveId == id);
                    if (prefab == null)
                        throw new InvalidDataException($"No hay prefab para el movimiento {id}.");
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, gallery.stage);
                    var fx = go.GetComponent<EmeraldMoveVfx>();
                    var times = new[]
                    {
                        fx.anticipationTime, Mathf.Min(fx.impactTime + ImpactShotDelay, fx.duration),
                        Mathf.Min(fx.peakTime, fx.duration), Mathf.Min(fx.dissipationTime, fx.duration),
                    };
                    var strip = new Texture2D(CmpWidth * times.Length, CmpHeight, TextureFormat.RGB24, false);
                    int peakDraws = 0, peakBatches = 0, peakBaked = 0;
                    for (int k = 0; k < times.Length; k++)
                    {
                        RenderAt(fx, times[k], cam, rt, frame, out int draws);
                        if (k == 2)
                        {
                            peakBaked = draws;
                            // UnityStats no se actualiza con cam.Render() en batchmode: estimamos los draw calls
                            // del efecto (sistemas con partículas vivas + estelas + mallas/líneas visibles + horneados).
                            peakDraws = draws;
                            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
                            {
                                if (ps.particleCount == 0)
                                    continue;
                                peakBatches++;
                                peakDraws += ps.trails.enabled ? 2 : 1;
                            }
                            peakDraws += go.GetComponentsInChildren<Renderer>(true).Count(r => !(r is ParticleSystemRenderer) && r.enabled);
                        }
                        strip.SetPixels(k * CmpWidth, 0, CmpWidth, CmpHeight, frame.GetPixels());
                    }
                    Save(strip, Path.Combine(dir, $"{id:000}_ours.png"));

                    int systems = go.GetComponentsInChildren<ParticleSystem>(true).Length;
                    int renderers = go.GetComponentsInChildren<Renderer>(true).Count(r => !(r is ParticleSystemRenderer));
                    string path = AssetDatabase.GetAssetPath(prefab.gameObject);
                    long bytes = string.IsNullOrEmpty(path) ? 0 : new FileInfo(path).Length;
                    fx.Preview(-1f);
                    bool restOk = !fx.player.enabled && go.GetComponentsInChildren<Renderer>(true).All(r => r is ParticleSystemRenderer || !r.enabled);
                    string line = string.Format(CultureInfo.InvariantCulture,
                        "{0:000} {1}: receta={2} t=[{3}] sistemas={4} mallas/líneas={5} horneados@pico={6} drawCallsEstimados@pico={7} sistemasVivos@pico={8} prefab={9:0.0}KB reposo={10}",
                        id, fx.moveName, fx.hasRecipe ? "sí" : "no", string.Join(", ", times.Select(t => t.ToString("0.00", CultureInfo.InvariantCulture))),
                        systems, renderers, peakBaked, peakDraws, peakBatches, bytes / 1024f, restOk ? "OK" : "ERROR");
                    log.Add(line);
                    Debug.Log("[BotW VFX] Emerald comparativa " + line);
                    if (!restOk)
                        throw new InvalidOperationException($"{id:000}: quedan partes visibles en reposo.");
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(frame);
                File.WriteAllLines(Path.Combine(dir, "compare_log.txt"), log);
            }
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
                        // Solo si la receta deja el clip horneado activo en ese instante.
                        if (fx.BakedActiveAt(fx.CurrentTime) && (!fx.player.enabled || fx.player.VisibleMeshCount == 0))
                            problems.Add($"{fx.Title}: la animación horneada no dibuja en el impacto.");
                        if (fx.hasRecipe && Mathf.Abs(fx.duration - 3.6f) > 0.001f)
                            problems.Add($"{fx.Title}: la receta no dura 3,6 s ({fx.duration:0.00}).");
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
            if (string.IsNullOrEmpty(arg))
                return DefaultMoves;
            var ids = new List<int>();
            foreach (var part in arg.Split(','))
            {
                var range = part.Trim().Split('-');
                int a = int.Parse(range[0].Trim(), CultureInfo.InvariantCulture);
                int b = range.Length > 1 ? int.Parse(range[1].Trim(), CultureInfo.InvariantCulture) : a;
                for (int i = Mathf.Min(a, b); i <= Mathf.Max(a, b); i++)
                    ids.Add(i);
            }
            return ids.ToArray();
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
