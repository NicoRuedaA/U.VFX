using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Renderiza "hojas de contacto" (6 instantes por efecto) en PNG.
    /// Útil para revisar los efectos fotograma a fotograma o para un portfolio.
    /// Menú: Tools/BotW VFX/Capture Contact Sheets (guarda en la carpeta Captures del proyecto).
    /// Línea de comandos: -executeMethod BotwVfx.EditorTools.BotwCapture.CaptureBatch -captureDir RUTA
    /// </summary>
    public static class BotwCapture
    {
        const int Width = 640;
        const int Height = 360;

        static readonly float[][] Times =
        {
            new[] { 0.02f, 0.06f, 0.12f, 0.25f, 0.45f, 0.7f },     // Bomba remota
            new[] { 0.03f, 0.1f, 0.2f, 0.4f, 0.9f, 2f },          // Explosión
            new[] { 1f, 2.2f, 2.72f, 2.78f, 2.9f, 3.4f },         // Rayo Guardián
            new[] { 0.12f, 0.3f, 0.5f, 0.9f, 1.3f, 1.45f },       // Flecha ancestral
        };

        [MenuItem("Tools/BotW VFX/Capture Contact Sheets", priority = 40)]
        public static void CaptureFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures"));
            CaptureAll(dir);
            EditorUtility.RevealInFinder(dir);
        }

        public static void CaptureBatch()
        {
            try
            {
                string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures"));
                CaptureAll(dir);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        static void CaptureAll(string dir)
        {
            Directory.CreateDirectory(dir);
            EditorSceneManager.OpenScene(BotwScene.ScenePath);
            var demo = Object.FindAnyObjectByType<DemoController>();
            var cam = demo.targetCamera;
            var timelines = Object.FindObjectsByType<VfxTimeline>();
            var facers = Object.FindObjectsByType<FaceCamera>();
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var frame = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            try
            {
                // Vista general en reposo (2x2).
                var overview = new Texture2D(Width * 2, Height * 2, TextureFormat.RGB24, false);
                foreach (var t in timelines)
                    t.Preview(-1f);
                for (int i = 0; i < demo.stations.Length && i < 4; i++)
                {
                    DemoController.PoseCamera(cam, demo.stations[i]);
                    Render(cam, rt, frame);
                    overview.SetPixels(i % 2 * Width, (1 - i / 2) * Height, Width, Height, frame.GetPixels());
                }
                Save(overview, Path.Combine(dir, "overview.png"));

                for (int i = 0; i < demo.stations.Length && i < Times.Length; i++)
                {
                    var station = demo.stations[i];
                    var sheet = new Texture2D(Width * 3, Height * 2, TextureFormat.RGB24, false);
                    foreach (var t in timelines)
                        t.Preview(-1f);
                    DemoController.PoseCamera(cam, station);
                    for (int k = 0; k < 6; k++)
                    {
                        station.effect.Preview(Times[i][k]);
                        foreach (var f in facers)
                            f.Face(cam.transform.position);
                        Render(cam, rt, frame);
                        sheet.SetPixels(k % 3 * Width, (1 - k / 3) * Height, Width, Height, frame.GetPixels());
                    }
                    station.effect.Preview(-1f);
                    string file = Path.Combine(dir, $"{i + 1}_{Sanitize(station.name)}.png");
                    Save(sheet, file);
                    Debug.Log($"[BotW VFX] Captura: {file}");
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(frame);
            }
        }

        // Depuración: una estación y un instante, renderizados con cada modo de _BotwDebugMode.
        public static void CaptureDebugBatch()
        {
            try
            {
                string dir = GetArg("-captureDir") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures"));
                int station = int.Parse(GetArg("-station") ?? "1");
                float time = float.Parse(GetArg("-time") ?? "0.1", System.Globalization.CultureInfo.InvariantCulture);
                Directory.CreateDirectory(dir);
                EditorSceneManager.OpenScene(BotwScene.ScenePath);
                var demo = Object.FindAnyObjectByType<DemoController>();
                var cam = demo.targetCamera;
                foreach (var t in Object.FindObjectsByType<VfxTimeline>())
                    t.Preview(-1f);
                DemoController.PoseCamera(cam, demo.stations[station]);
                demo.stations[station].effect.Preview(time);
                foreach (var f in Object.FindObjectsByType<FaceCamera>())
                    f.Face(cam.transform.position);
                var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                var frame = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                var sheet = new Texture2D(Width * 3, Height * 2, TextureFormat.RGB24, false);
                for (int k = 0; k < 6; k++)
                {
                    Shader.SetGlobalFloat("_BotwDebugMode", k);
                    Render(cam, rt, frame);
                    sheet.SetPixels(k % 3 * Width, (1 - k / 3) * Height, Width, Height, frame.GetPixels());
                }
                Shader.SetGlobalFloat("_BotwDebugMode", 0f);
                Save(sheet, Path.Combine(dir, $"debug_{station}_{time:0.00}.png"));
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        static void Render(Camera cam, RenderTexture rt, Texture2D into)
        {
            cam.targetTexture = rt;
            cam.Render();
            cam.Render();
            RenderTexture.active = rt;
            into.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            into.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
        }

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
            return s.Replace(' ', '_');
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
