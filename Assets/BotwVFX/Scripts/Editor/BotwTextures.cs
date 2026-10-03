using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Genera todas las texturas de los efectos de forma procedural (PNG en
    /// Assets/BotwVFX/Textures). Sustituyen a las que en el artículo se pintan en
    /// Photoshop: formas simples con un "campo de valores" de 1 en el centro a 0
    /// en el borde, para que el corte toon del shader las erosione bien.
    /// </summary>
    public static class BotwTextures
    {
        public const string Folder = "Assets/BotwVFX/Textures";

        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            Save("T_Noise", 256, true, NoiseRGBA);
            Save("T_Caustics", 256, true, Caustics);
            Save("T_SoftCircle", 128, false, SoftCircle);
            Save("T_Ray", 128, false, Ray);
            Save("T_Star", 256, false, Star);
            Save("T_Ring", 256, false, Ring);
            Save("T_Gradient", 64, false, GradientTex);
            Save("T_SmokePuff", 256, false, SmokePuff);
            Save("T_SheikahRing", 512, false, SheikahRing);
        }

        public static Texture2D Load(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{name}.png");

        static void Save(string name, int size, bool repeat, Func<int, Color[]> generator)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            tex.SetPixels(generator(size));
            tex.Apply();
            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------ texturas

        // R = fbm, G = celdas (1 - F1), B = fbm fino, A = bordes de celdas (F2 - F1)
        static Color[] NoiseRGBA(int n)
        {
            var r = new float[n * n];
            var g = new float[n * n];
            var b = new float[n * n];
            var a = new float[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (float)x / n, v = (float)y / n;
                int i = y * n + x;
                r[i] = Fbm(u, v, 4, 5, 11);
                var w = Worley(u, v, 8, 23);
                g[i] = 1f - w.x;
                b[i] = Fbm(u, v, 8, 4, 37);
                var w2 = Worley(u, v, 6, 51);
                a[i] = w2.y - w2.x;
            }
            Normalize(r, 1.15f);
            Normalize(g, 1f);
            Normalize(b, 1.15f);
            Normalize(a, 1f);
            var px = new Color[n * n];
            for (int i = 0; i < px.Length; i++)
                px[i] = new Color(r[i], g[i], b[i], a[i]);
            return px;
        }

        // Líneas brillantes tipo cáusticas (bordes de Voronoi con distorsión).
        static Color[] Caustics(int n)
        {
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (float)x / n, v = (float)y / n;
                float wu = u + 0.06f * (Fbm(u, v, 4, 3, 71) - 0.5f);
                float wv = v + 0.06f * (Fbm(u, v, 4, 3, 83) - 0.5f);
                var w1 = Worley(Frac(wu), Frac(wv), 5, 91);
                var w2 = Worley(Frac(wu * 2f + 0.3f), Frac(wv * 2f + 0.7f), 10, 97);
                float l1 = 1f - Smooth(0f, 0.16f, w1.y - w1.x);
                float l2 = 1f - Smooth(0f, 0.12f, w2.y - w2.x);
                float c = Mathf.Clamp01(Mathf.Max(l1, l2 * 0.55f));
                px[y * n + x] = new Color(c, c, c, c);
            }
            return px;
        }

        static Color[] SoftCircle(int n)
        {
            return Field(n, (u, v) =>
            {
                float r = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f)) * 2f;
                return Mathf.Clamp01(1f - r);
            });
        }

        // Huso vertical: 1 en el centro, afilado en las puntas. Para rayos y chispas.
        static Color[] Ray(int n)
        {
            return Field(n, (u, v) =>
            {
                float x = Mathf.Abs(u * 2f - 1f);
                float s = Mathf.Pow(Mathf.Sin(Mathf.PI * v), 0.8f);
                return Mathf.Clamp01(1f - x / Mathf.Max(s * 0.9f, 1e-3f)) * s;
            });
        }

        // Estrella de 4 puntas + 4 diagonales cortas + núcleo. Para destellos y lens flares.
        static Color[] Star(int n)
        {
            return Field(n, (u, v) =>
            {
                float x = u * 2f - 1f, y = v * 2f - 1f;
                float r = Mathf.Sqrt(x * x + y * y);
                float core = Mathf.Clamp01(1f - r * 2.2f);
                float h = Spike(x, y, 0.035f);
                float vert = Spike(y, x, 0.035f);
                float dx = (x + y) * 0.7071f, dy = (x - y) * 0.7071f;
                float d1 = Spike(dx * 1.8f, dy, 0.03f) * 0.7f;
                float d2 = Spike(dy * 1.8f, dx, 0.03f) * 0.7f;
                return Mathf.Clamp01(Mathf.Max(Mathf.Max(core, h), Mathf.Max(vert, Mathf.Max(d1, d2))));
            });
        }

        static float Spike(float along, float across, float width)
        {
            float len = Mathf.Clamp01(1f - Mathf.Abs(along));
            float w = width * len + 1e-4f;
            return Mathf.Clamp01(1f - Mathf.Abs(across) / w) * len;
        }

        // Banda circular: para ondas expansivas en billboard.
        static Color[] Ring(int n)
        {
            return Field(n, (u, v) =>
            {
                float r = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f)) * 2f;
                return Mathf.Clamp01(1f - Mathf.Abs(r - 0.78f) / 0.2f);
            });
        }

        // 1 abajo -> 0 arriba (V). La bola de pinchos usa UV.y = altura del pincho.
        static Color[] GradientTex(int n)
        {
            return Field(n, (u, v) => 1f - v);
        }

        // Bola de humo "coliflor": varias semiesferas unidas.
        // R = sombreado (luz desde arriba), G = ruido, B = erosión, A = máscara.
        static Color[] SmokePuff(int n)
        {
            var bumps = new Vector3[]
            {
                new Vector3(0.50f, 0.50f, 0.27f),
                new Vector3(0.50f, 0.66f, 0.19f),
                new Vector3(0.33f, 0.58f, 0.16f),
                new Vector3(0.67f, 0.58f, 0.17f),
                new Vector3(0.30f, 0.40f, 0.15f),
                new Vector3(0.70f, 0.40f, 0.15f),
                new Vector3(0.43f, 0.29f, 0.15f),
                new Vector3(0.60f, 0.29f, 0.14f),
            };
            var light = new Vector3(0.35f, 0.75f, 0.55f).normalized;
            var height = new float[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n, v = (y + 0.5f) / n;
                float h = 0f;
                foreach (var bp in bumps)
                {
                    float d2 = (u - bp.x) * (u - bp.x) + (v - bp.y) * (v - bp.y);
                    float rr = bp.z * bp.z;
                    if (d2 < rr)
                        h = Mathf.Max(h, Mathf.Sqrt(rr - d2));
                }
                height[y * n + x] = h;
            }

            var px = new Color[n * n];
            float step = 1f / n;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int i = y * n + x;
                float h = height[i];
                float hx = (height[y * n + Mathf.Min(x + 1, n - 1)] - height[y * n + Mathf.Max(x - 1, 0)]) / (2f * step);
                float hy = (height[Mathf.Min(y + 1, n - 1) * n + x] - height[Mathf.Max(y - 1, 0) * n + x]) / (2f * step);
                var normal = new Vector3(-hx, -hy, 1f).normalized;
                float shade = h > 0f ? Mathf.Clamp01(Vector3.Dot(normal, light) * 0.9f + 0.1f) : 0f;

                float u = (float)x / n, v = (float)y / n;
                float noise = Fbm(u, v, 4, 4, 131);
                var w = Worley(u, v, 5, 151);
                float erosion = Mathf.Clamp01((1f - w.x * 1.2f) * 0.8f + Fbm(u, v, 8, 3, 171) * 0.35f);
                float mask = Mathf.Pow(Mathf.Clamp01(h / 0.24f), 0.65f);
                px[i] = new Color(shade, noise, erosion, mask);
            }
            return px;
        }

        // Marco circular con segmentos y puntos, inspirado en la tecnología Sheikah.
        static Color[] SheikahRing(int n)
        {
            float aa = 2.5f / n;
            return Field(n, (u, v) =>
            {
                float dx = u - 0.5f, dy = v - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float ang = Mathf.Atan2(dy, dx) / (Mathf.PI * 2f) + 0.5f;

                float outer = Band(r, 0.9f, 0.028f, aa);
                float thin = Band(r, 0.7f, 0.011f, aa);
                float segAng = Frac(ang * 12f);
                float segments = Band(r, 0.8f, 0.04f, aa) * Smooth(0.06f, 0.06f + 0.02f, segAng) * (1f - Smooth(0.9f, 0.92f, segAng));
                float ticks = Band(r, 0.955f, 0.02f, aa) * (1f - Smooth(0.08f, 0.12f, Mathf.Abs(Frac(ang * 48f) - 0.5f) * 2f));

                // Puntos cada 15 grados.
                float dotAng = (Mathf.Round(ang * 24f) / 24f - 0.5f) * Mathf.PI * 2f;
                float px = Mathf.Cos(dotAng) * 0.31f, py = Mathf.Sin(dotAng) * 0.31f;
                float dd = Mathf.Sqrt((dx - px) * (dx - px) + (dy - py) * (dy - py));
                float dots = 1f - Smooth(0.009f, 0.009f + aa, dd);

                float value = Mathf.Max(Mathf.Max(outer, thin), Mathf.Max(segments, Mathf.Max(ticks, dots)));
                // Un poco de gradiente interno para que la erosión no sea uniforme.
                return value * (0.75f + 0.25f * (1f - Mathf.Abs(r - 0.8f) * 3f));
            });
        }

        static float Band(float r, float center, float halfWidth, float aa)
        {
            return 1f - Smooth(halfWidth, halfWidth + aa, Mathf.Abs(r - center));
        }

        // ------------------------------------------------------------ utilidades

        static Color[] Field(int n, Func<float, float, float> f)
        {
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float c = f((x + 0.5f) / n, (y + 0.5f) / n);
                px[y * n + x] = new Color(c, c, c, c);
            }
            return px;
        }

        static void Normalize(float[] data, float contrast)
        {
            float min = float.MaxValue, max = float.MinValue;
            foreach (var d in data)
            {
                min = Mathf.Min(min, d);
                max = Mathf.Max(max, d);
            }
            float range = Mathf.Max(max - min, 1e-5f);
            for (int i = 0; i < data.Length; i++)
            {
                float t = (data[i] - min) / range;
                data[i] = Mathf.Clamp01((t - 0.5f) * contrast + 0.5f);
            }
        }

        static float Frac(float x) => x - Mathf.Floor(x);

        static float Smooth(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }

        static int Wrap(int i, int period) => ((i % period) + period) % period;

        // Ruido Perlin periódico (tileable) en [0,1].
        static float Perlin(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            float Dot(int ix, int iy, float dx, float dy)
            {
                float a = Hash(Wrap(ix, period), Wrap(iy, period), seed) * Mathf.PI * 2f;
                return Mathf.Cos(a) * dx + Mathf.Sin(a) * dy;
            }
            float n00 = Dot(x0, y0, fx, fy);
            float n10 = Dot(x0 + 1, y0, fx - 1f, fy);
            float n01 = Dot(x0, y0 + 1, fx, fy - 1f);
            float n11 = Dot(x0 + 1, y0 + 1, fx - 1f, fy - 1f);
            float ux = fx * fx * fx * (fx * (fx * 6f - 15f) + 10f);
            float uy = fy * fy * fy * (fy * (fy * 6f - 15f) + 10f);
            float v = Mathf.Lerp(Mathf.Lerp(n00, n10, ux), Mathf.Lerp(n01, n11, ux), uy);
            return v * 0.7071f + 0.5f;
        }

        static float Fbm(float u, float v, int basePeriod, int octaves, int seed)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            int p = basePeriod;
            for (int o = 0; o < octaves; o++)
            {
                sum += Perlin(u * p, v * p, p, seed + o * 17) * amp;
                norm += amp;
                amp *= 0.5f;
                p *= 2;
            }
            return sum / norm;
        }

        // Voronoi periódico: devuelve (F1, F2) en unidades de celda.
        static Vector2 Worley(float u, float v, int cells, int seed)
        {
            float x = u * cells, y = v * cells;
            int cx = Mathf.FloorToInt(x), cy = Mathf.FloorToInt(y);
            float f1 = 10f, f2 = 10f;
            for (int oy = -1; oy <= 1; oy++)
            for (int ox = -1; ox <= 1; ox++)
            {
                int gx = cx + ox, gy = cy + oy;
                int wx = Wrap(gx, cells), wy = Wrap(gy, cells);
                float px = gx + Hash(wx, wy, seed);
                float py = gy + Hash(wx, wy, seed + 1);
                float d = Mathf.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
                if (d < f1)
                {
                    f2 = f1;
                    f1 = d;
                }
                else if (d < f2)
                {
                    f2 = d;
                }
            }
            return new Vector2(f1, f2);
        }
    }
}
