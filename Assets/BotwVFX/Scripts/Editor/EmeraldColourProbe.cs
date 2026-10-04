using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    public static class EmeraldColourProbe
    {
        public static void RunBatch()
        {
            try
            {
                var material = new Material(Shader.Find("BotwVFX/Toon Particle"));
                var rt = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                var pixels = new Texture2D(16, 16, TextureFormat.RGBAFloat, false, true);
                material.SetTexture("_MainTex", Texture2D.whiteTexture);
                material.SetFloat("_NoiseStrength", 0f);
                material.SetFloat("_Erosion", 0f);
                material.SetFloat("_ShadeAmount", 0f);
                material.SetFloat("_AlphaErosion", 0f);
                var input = new Color(.8f, .07f, .015f, 1f);
                string report = $"Project colour space: {QualitySettings.activeColorSpace}\nInput linear: {input}\n";
                for (int mode = 0; mode < 2; mode++)
                {
                    if (mode == 0) material.SetColor("_Color", input);
                    else BotwMaterials.Hdr(material, "_Color", input);
                    material.SetColor("_EdgeColor", material.GetColor("_Color"));
                    Graphics.Blit(Texture2D.whiteTexture, rt, material);
                    RenderTexture.active = rt;
                    pixels.ReadPixels(new Rect(0, 0, 16, 16), 0, 0); pixels.Apply();
                    report += $"{(mode == 0 ? "direct SetColor" : "legacy Hdr gamma")}: stored={material.GetColor("_Color")} linear GPU={pixels.GetPixel(8, 8)}\n";
                }
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache/vfx-emerald-work/final-polish/colour-probe.txt");
                File.WriteAllText(path, report); Debug.Log(report);
                RenderTexture.active = null;
                UnityEngine.Object.DestroyImmediate(pixels); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(material);
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
    }
}
