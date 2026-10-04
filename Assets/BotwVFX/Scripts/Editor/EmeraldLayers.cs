using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static BotwVfx.EditorTools.Fx;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace BotwVfx.EditorTools
{
    /// <summary>Paleta HDR lineal (núcleo casi blanco, borde saturado), como el resto de efectos BotW.</summary>
    public readonly struct Pal
    {
        public readonly string key;
        public readonly Color core;
        public readonly Color edge;

        public Pal(string key, Color core, Color edge)
        {
            this.key = key;
            this.core = core;
            this.edge = edge;
        }
    }

    /// <summary>
    /// Biblioteca de bloques BotW reutilizables para las recetas Emerald:
    /// destellos, estrellas de impacto, ondas, polvo, humo, escombros, chispas, cargas,
    /// auras y cascos de contorno sobre el atrezo, tajos en media luna, líneas de corte,
    /// líneas de velocidad, fuego, hielo, rayos quebrados, proyectiles con estela,
    /// cuchillas de viento, tornados, sombras, destellos flotantes...
    /// Todos reciben el builder, la posición local, la paleta y los tiempos absolutos.
    /// Los materiales (EMP_&lt;paleta&gt;_&lt;tipo&gt;) y mallas (M_EM_*) se crean bajo demanda.
    /// </summary>
    public static class EmeraldLayers
    {
        // ------------------------------------------------------------ paletas

        public static readonly Pal Cream = new Pal("Cream", new Color(2.6f, 2.35f, 1.8f), new Color(2.2f, 0.95f, 0.2f));
        public static readonly Pal White = new Pal("White", new Color(2.6f, 2.6f, 2.6f), new Color(1.1f, 1.5f, 2.1f));
        public static readonly Pal Orange = new Pal("Orange", new Color(2.9f, 2.2f, 0.9f), new Color(2.5f, 0.55f, 0.04f));
        public static readonly Pal Gold = new Pal("Gold", new Color(3.0f, 2.6f, 1.1f), new Color(2.3f, 1.15f, 0.02f));
        public static readonly Pal Fire = new Pal("Fire", new Color(3.0f, 2.4f, 0.8f), new Color(2.6f, 0.6f, 0.03f));
        public static readonly Pal Ice = new Pal("Ice", new Color(1.9f, 2.7f, 3.0f), new Color(0.25f, 1.1f, 2.6f));
        public static readonly Pal Electric = new Pal("Electric", new Color(3.0f, 2.9f, 1.5f), new Color(2.4f, 1.9f, 0.05f));
        public static readonly Pal Wind = new Pal("Wind", new Color(2.3f, 2.6f, 2.7f), new Color(0.6f, 1.5f, 2.0f));
        public static readonly Pal Mint = new Pal("Mint", new Color(2.2f, 2.8f, 2.4f), new Color(0.3f, 1.7f, 0.9f));
        public static readonly Pal Steel = new Pal("Steel", new Color(2.1f, 2.2f, 2.5f), new Color(0.5f, 0.65f, 1.1f));
        public static readonly Pal Cyan = new Pal("Cyan", new Color(1.7f, 2.7f, 3.0f), new Color(0.05f, 1.3f, 2.7f));
        public static readonly Pal Pink = new Pal("Pink", new Color(2.7f, 1.7f, 2.3f), new Color(2.3f, 0.35f, 1.2f));
        public static readonly Pal Red = new Pal("Red", new Color(2.9f, 1.5f, 0.7f), new Color(2.3f, 0.15f, 0.05f));
        public static readonly Pal YellowBlue = new Pal("YellowBlue", new Color(3.0f, 2.8f, 1.3f), new Color(0.25f, 0.9f, 2.6f));
        public static readonly Pal Ink = new Pal("Ink", new Color(0.05f, 0.05f, 0.09f), new Color(0.3f, 0.27f, 0.42f));
        // Lote 2.
        public static readonly Pal Leaf = new Pal("Leaf", new Color(2.4f, 2.6f, 0.4f), new Color(0.1f, 1.6f, 0.05f));
        // Liana: interior verde oscuro y bordes claros (descripción del movimiento).
        public static readonly Pal VineGreen = new Pal("Vine", new Color(0.2f, 0.62f, 0.08f), new Color(0.75f, 1.7f, 0.3f));
        public static readonly Pal Lilac = new Pal("Lilac", new Color(1.9f, 1.25f, 2.1f), new Color(0.85f, 0.45f, 1.05f));
        public static readonly Pal Violet = new Pal("Violet", new Color(1.8f, 0.7f, 1.9f), new Color(0.75f, 0.06f, 1.1f));
        public static readonly Pal Sand = new Pal("Sand", new Color(1.5f, 1.15f, 0.7f), new Color(0.55f, 0.36f, 0.16f));
        // Sólidos/planos del lote 2 (colores lineales: 0,45 ya se ve como 0,7 en pantalla; valores altos se lavan hacia el blanco).
        public static readonly Pal FootRed = new Pal("FootRed", new Color(1.3f, 0.16f, 0.03f), new Color(0.75f, 0.05f, 0.01f));
        public static readonly Pal LilacSolid = new Pal("LilacSolid", new Color(0.42f, 0.17f, 0.52f), new Color(0.2f, 0.07f, 0.28f));
        public static readonly Pal DrillGrey = new Pal("DrillGrey", new Color(0.16f, 0.13f, 0.11f), new Color(0.05f, 0.04f, 0.035f));
        public static readonly Pal Ember = new Pal("Ember", new Color(1.7f, 0.6f, 0.12f), new Color(0.95f, 0.13f, 0.02f));
        public static readonly Pal Horn = new Pal("Horn", new Color(1.25f, 1.22f, 1.15f), new Color(0.55f, 0.52f, 0.5f));
        // Sólidos (no emiten): bandas de acero, monedas.
        public static readonly Pal Metal = new Pal("Metal", new Color(0.72f, 0.75f, 0.82f), new Color(0.32f, 0.34f, 0.42f));
        public static readonly Pal CoinGold = new Pal("CoinGold", new Color(1.7f, 1.2f, 0.25f), new Color(0.9f, 0.42f, 0.04f));
        // Lote 3.
        // Agujas verdes del anime (Pin Misil): núcleo casi blanco y borde verde intenso.
        public static readonly Pal NeedleGreen = new Pal("NeedleGreen", new Color(2.3f, 2.9f, 2.1f), new Color(0.15f, 1.9f, 0.12f));
        // Colmillos amarillo-naranja de Mordisco (juego).
        public static readonly Pal FangGold = new Pal("FangGold", new Color(3.0f, 2.3f, 0.45f), new Color(2.2f, 0.75f, 0.02f));
        // Agua: núcleo blanco azulado, borde azul.
        public static readonly Pal Water = new Pal("Water", new Color(1.9f, 2.6f, 3.0f), new Color(0.05f, 0.75f, 2.6f));
        // Psicorrayo / Canto (juego): magenta.
        public static readonly Pal Magenta = new Pal("Magenta", new Color(2.9f, 1.8f, 2.9f), new Color(2.1f, 0.1f, 1.9f));
        // Ácido: violeta oscuro y espeso (anime).
        public static readonly Pal AcidViolet = new Pal("AcidViolet", new Color(1.1f, 0.4f, 1.5f), new Color(0.4f, 0.04f, 0.75f));
        // Lote 4.
        // Pétalos de Danza Pétalo (juego): rosa claro con borde rosa; valores sólidos (con HDR alto se lavan a blanco).
        public static readonly Pal Petal = new Pal("Petal", new Color(1.15f, 0.5f, 0.7f), new Color(0.95f, 0.18f, 0.45f));
        // Somnífero: polvo turquesa.
        public static readonly Pal Teal = new Pal("Teal", new Color(2.0f, 2.9f, 2.6f), new Color(0.08f, 1.55f, 1.15f));
        // Paralizador: polen amarillo pálido (más claro que Gold).
        public static readonly Pal Pollen = new Pal("Pollen", new Color(3.0f, 2.85f, 1.7f), new Color(2.1f, 1.55f, 0.15f));
        // Rayo Solar: núcleo blanco y borde amarillo dorado.
        public static readonly Pal Sun = new Pal("Sun", new Color(3.0f, 2.95f, 2.2f), new Color(2.6f, 1.7f, 0.1f));
        // Hiperrayo: núcleo blanco y borde naranja-rosado (anime blanco/rosa, juego amarillo/naranja).
        public static readonly Pal HyperWarm = new Pal("HyperWarm", new Color(3.0f, 2.85f, 2.4f), new Color(2.6f, 0.6f, 0.4f));
        // Absorber / Megaagotar: verde brillante de las motas del juego.
        public static readonly Pal DrainGreen = new Pal("DrainGreen", new Color(2.3f, 3.0f, 1.9f), new Color(0.2f, 1.9f, 0.2f));

        // ------------------------------------------------------------ materiales

        static readonly HashSet<string> made = new HashSet<string>();

        /// <summary>Lo llama EmeraldMoves.Build al empezar: rehace materiales y mallas de esta biblioteca.</summary>
        public static void BeginBuild()
        {
            made.Clear();
            EnsureMeshes();
        }

        static Material Existing(string name) => made.Contains(name) ? BotwMaterials.Get(name) : null;

        /// <summary>
        /// Material de la paleta. Tipos: Spark (huso), Star, Glow (círculo con ruido), Ring (aro),
        /// Shock (anillo de suelo), Blade (mallas media luna/espada), Line (líneas y estelas),
        /// Stripe (cintas de tornado), Solid (mallas opacas), Shell (casco de aura, Energy Sphere).
        /// </summary>
        public static Material Mat(Pal p, string kind)
        {
            string name = $"EMP_{p.key}_{kind}";
            var m = Existing(name);
            if (m != null)
                return m;
            var noise = BotwTextures.Load("T_Noise");
            switch (kind)
            {
                case "Spark":
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Ray"), p.core, p.edge, 0.35f);
                    break;
                case "Star":
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Star"), p.core, p.edge, 0.25f);
                    m.SetFloat("_CameraOffset", 1f);
                    break;
                case "Glow":
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_SoftCircle"), p.core, p.edge, 0.3f, noise, 0.4f,
                        tiling: new Vector2(2f, 2f), scroll: new Vector2(0f, 1.2f));
                    m.SetFloat("_CameraOffset", 0.8f);
                    break;
                case "Ring":
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Ring"), p.core, p.edge, 0.3f, noise, 0.45f, tiling: new Vector2(3f, 3f));
                    m.SetFloat("_Erosion", 0.15f);
                    break;
                case "Shock":
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.35f, noise, 0.55f,
                        tiling: new Vector2(6f, 1f), scroll: new Vector2(0.2f, 0f));
                    m.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
                    break;
                case "Blade":
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Ray"), p.core, p.edge, 0.3f, noise, 0.3f,
                        tiling: new Vector2(1f, 2f), scroll: new Vector2(0f, -1.5f));
                    break;
                case "Line":
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.4f, noise, 0.45f,
                        tiling: new Vector2(4f, 1f), scroll: new Vector2(-4f, 0f));
                    m.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
                    break;
                case "Bolt":
                    // Rayo: núcleo blanco ancho y borde de color, poco ruido (no se deshace en trozos).
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.45f, noise, 0.25f,
                        tiling: new Vector2(3f, 1f), scroll: new Vector2(-6f, 0f));
                    m.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
                    m.SetFloat("_AlphaErosion", 0f);
                    m.SetFloat("_CameraOffset", 0.6f);
                    break;
                case "Stripe":
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.3f, noise, 0.5f,
                        tiling: new Vector2(4f, 1f), scroll: new Vector2(-3f, 0f));
                    m.SetVector("_BorderFade", new Vector4(0.25f, 0.5f, 0f, 0f));
                    break;
                case "Bubble":
                    // Pompa: aro fino de borde de color con poco ruido (no se rompe en trozos como Ring).
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Ring"), p.core, p.edge, 0.4f, noise, 0.15f, tiling: new Vector2(2f, 2f));
                    m.SetFloat("_CameraOffset", 0.6f);
                    break;
                case "Flat":
                    // Silueta plana sin textura (huella): color de núcleo y disolución con ruido al desaparecer.
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.12f, noise, 0.35f, tiling: new Vector2(2f, 2f));
                    break;
                case "Solid":
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.1f, noise, 0.45f, tiling: new Vector2(2f, 2f));
                    m.SetFloat("_ShadeAmount", 0.5f);
                    m.SetFloat("_ZWrite", 1f);
                    break;
                case "Shell":
                {
                    m = BotwMaterials.Mat(name, "BotwVFX/Energy Sphere");
                    BotwMaterials.Hdr(m, "_BaseColor", new Color(p.edge.r, p.edge.g, p.edge.b, 0.05f));
                    BotwMaterials.Hdr(m, "_EmissiveColor", new Color(p.core.r, p.core.g, p.core.b, 1f));
                    m.SetFloat("_FresnelPower", 1.7f);
                    m.SetFloat("_IntersectionDistance", 0.35f);
                    m.SetFloat("_IntersectionPower", 1.5f);
                    m.SetFloat("_NoiseScale", 2.6f);
                    m.SetFloat("_NoiseSpeed", 2.2f);
                    m.SetFloat("_NoiseAmount", 0.6f);
                    m.SetFloat("_Threshold", 0.5f);
                    m.SetFloat("_Softness", 0f);
                    m.SetFloat("_Opacity", 1f);
                    m.SetFloat("_Dissolve", 0f);
                    m.SetFloat("_Wobble", 0.04f);
                    break;
                }
                // Lote 6.
                case "Pane":
                case "Frame":
                    // Panel translúcido (pantallas, barreras, espejos) para la malla Quad: relleno del color de núcleo con poca
                    // opacidad (Frame = sin relleno) y marco del color de borde.
                    m = BotwMaterials.Toon(name, null, p.core, p.edge, 0.3f, noise, 0.15f, tiling: new Vector2(2f, 2f), scroll: new Vector2(0f, 0.3f));
                    m.SetVector("_BorderFade", new Vector4(0.2f, 0.2f, 0f, 0f));
                    BotwMaterials.Hdr(m, "_Color", new Color(p.core.r, p.core.g, p.core.b, kind == "Pane" ? 0.45f : 0f));
                    break;
                case "Facets":
                    // Facetas de un panel (líneas de Voronoi de T_Caustics, celdas transparentes): el centro de cada línea
                    // lleva el color de borde de la paleta (el brillante en Pane/Frame) y su caída el de núcleo.
                    m = BotwMaterials.Toon(name, BotwTextures.Load("T_Caustics"), p.edge, p.core, 0.35f);
                    m.SetVector("_BorderFade", new Vector4(0.08f, 0.08f, 0f, 0f));
                    break;
                default:
                    throw new System.ArgumentException($"Tipo de material desconocido: {kind}");
            }
            made.Add(name);
            return m;
        }

        /// <summary>Sombra en el suelo (círculo oscuro semitransparente, borde duro toon).</summary>
        public static Material ShadowMat()
        {
            const string name = "EMP_Shadow";
            var m = Existing(name);
            if (m != null)
                return m;
            m = BotwMaterials.Toon(name, BotwTextures.Load("T_SoftCircle"), new Color(0.02f, 0.025f, 0.05f), new Color(0.06f, 0.07f, 0.12f), 0.25f);
            BotwMaterials.Hdr(m, "_Color", new Color(0.02f, 0.025f, 0.05f, 0.55f));
            BotwMaterials.Hdr(m, "_EdgeColor", new Color(0.06f, 0.07f, 0.12f, 0.35f));
            made.Add(name);
            return m;
        }

        static Material M(string name) => BotwMaterials.Get(name);

        // ------------------------------------------------------------ mallas

        public static Mesh Crescent => LoadMesh("M_EM_Crescent");
        public static Mesh Quad => LoadMesh("M_EM_Quad");
        public static Mesh Blade => LoadMesh("M_EM_Blade");
        public static Mesh Coin => LoadMesh("M_EM_Coin");
        public static Mesh Band => LoadMesh("M_EM_Band");
        public static Mesh Foot => LoadMesh("M_EM_Foot");
        public static Mesh Helix => LoadMesh("M_EM_Helix");
        public static Mesh Cone => LoadMesh("M_EM_Cone");
        public static Mesh Drill => LoadMesh("M_EM_Drill");
        public static Mesh ThinRing => LoadMesh("M_EM_ThinRing");
        public static Mesh Finger => LoadMesh("M_EM_Finger");
        // Lote 7.
        public static Mesh Star5 => LoadMesh("M_EM_Star5");
        public static Mesh FlameStar => LoadMesh("M_EM_FlameStar");
        public static Mesh Bone => LoadMesh("M_EM_Bone");
        public static Mesh EggShell => LoadMesh("M_EM_EggShell");
        public static Mesh SpoonHead => LoadMesh("M_EM_SpoonHead");
        public static Mesh SpoonHandle => LoadMesh("M_EM_SpoonHandle");
        public static Mesh HelixThin => LoadMesh("M_EM_HelixThin");

        static Mesh LoadMesh(string name) => AssetDatabase.LoadAssetAtPath<Mesh>($"{BotwMeshes.Folder}/{name}.asset");

        static void EnsureMeshes()
        {
            Directory.CreateDirectory(BotwMeshes.Folder);
            SaveMesh("M_EM_Crescent", BuildCrescent());
            SaveMesh("M_EM_Quad", BuildQuads(1));
            SaveMesh("M_EM_Blade", BuildQuads(2));
            SaveMesh("M_EM_Coin", BuildCoin(32));
            SaveMesh("M_EM_Band", BuildBand(1f, 0.15f, 48, 10));
            SaveMesh("M_EM_Foot", BuildFoot());
            SaveMesh("M_EM_Helix", BuildHelix(3f, 0.1f, 8));
            SaveMesh("M_EM_Cone", BuildCone(0f));
            SaveMesh("M_EM_Drill", BuildCone(0.2f));
            SaveMesh("M_EM_ThinRing", BuildThinRing(0.88f, 64));
            SaveMesh("M_EM_Finger", BuildFinger());
            // Lote 7.
            SaveMesh("M_EM_Star5", BuildStar5());
            SaveMesh("M_EM_FlameStar", BuildFlameStar());
            SaveMesh("M_EM_Bone", BuildBone());
            SaveMesh("M_EM_EggShell", BuildEggShell());
            SaveMesh("M_EM_SpoonHead", BuildSpoon(true));
            SaveMesh("M_EM_SpoonHandle", BuildSpoon(false));
            SaveMesh("M_EM_HelixThin", BuildHelix(4f, 0.045f, 8));
            AssetDatabase.SaveAssets();
        }

        // Igual que BotwMeshes.SaveMesh: reutiliza el asset para no romper referencias.
        static void SaveMesh(string name, Mesh mesh)
        {
            string path = $"{BotwMeshes.Folder}/{name}.asset";
            mesh.name = name;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                EditorUtility.CopySerialized(mesh, existing);
                existing.name = name;
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(mesh);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        static Mesh Finish(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> tris)
        {
            var colors = new Color32[v.Count];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = new Color32(255, 255, 255, 255);
            var mesh = new Mesh();
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetUVs(0, uv);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Media luna en el plano XY, convexa hacia +X, centrada en el origen (alto ~2,1).
        // UV.x = a lo ancho (0 interior, 1 exterior), UV.y = a lo largo: con T_Ray se afila en las puntas.
        static Mesh BuildCrescent()
        {
            const int segments = 32;
            const float arc = 65f * Mathf.Deg2Rad;
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float a = Mathf.Lerp(-arc, arc, u);
                var dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                v.Add(dir * 0.8f - Vector3.right);
                v.Add(dir * 1.2f - Vector3.right);
                n.Add(Vector3.back);
                n.Add(Vector3.back);
                uv.Add(new Vector2(0f, u));
                uv.Add(new Vector2(1f, u));
                if (s < segments)
                {
                    int i = s * 2;
                    tris.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                }
            }
            return Finish(v, n, uv, tris);
        }

        // 1 = quad XY centrado (lado 1). 2 = dos quads cruzados en Y (hoja/espada vista desde cualquier lado),
        // con la base en y = 0 y la punta en y = 1. UV.x = ancho, UV.y = largo.
        static Mesh BuildQuads(int count)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int q = 0; q < count; q++)
            {
                var side = q == 0 ? Vector3.right : Vector3.forward;
                var normal = q == 0 ? Vector3.back : Vector3.right;
                float y0 = count == 1 ? -0.5f : 0f, y1 = count == 1 ? 0.5f : 1f;
                int i = v.Count;
                v.Add(-side * 0.5f + Vector3.up * y0);
                v.Add(side * 0.5f + Vector3.up * y0);
                v.Add(side * 0.5f + Vector3.up * y1);
                v.Add(-side * 0.5f + Vector3.up * y1);
                for (int k = 0; k < 4; k++)
                    n.Add(normal);
                uv.Add(new Vector2(0f, 0f));
                uv.Add(new Vector2(1f, 0f));
                uv.Add(new Vector2(1f, 1f));
                uv.Add(new Vector2(0f, 1f));
                tris.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            }
            return Finish(v, n, uv, tris);
        }

        // Moneda: cilindro plano (radio 0,5, grosor 0,08) con caras y canto.
        static Mesh BuildCoin(int segments)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            const float h = 0.04f, r = 0.5f;
            foreach (float y in new[] { h, -h })
            {
                int c = v.Count;
                v.Add(new Vector3(0f, y, 0f));
                n.Add(new Vector3(0f, Mathf.Sign(y), 0f));
                uv.Add(new Vector2(0.5f, 0.5f));
                for (int s = 0; s <= segments; s++)
                {
                    float a = s * Mathf.PI * 2f / segments;
                    v.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
                    n.Add(new Vector3(0f, Mathf.Sign(y), 0f));
                    uv.Add(new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f));
                    if (s < segments)
                    {
                        if (y > 0f)
                            tris.AddRange(new[] { c, c + s + 2, c + s + 1 });
                        else
                            tris.AddRange(new[] { c, c + s + 1, c + s + 2 });
                    }
                }
            }
            int rim = v.Count;
            for (int s = 0; s <= segments; s++)
            {
                float a = s * Mathf.PI * 2f / segments;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                v.Add(d * r + Vector3.up * h);
                v.Add(d * r - Vector3.up * h);
                n.Add(d);
                n.Add(d);
                uv.Add(new Vector2((float)s / segments, 1f));
                uv.Add(new Vector2((float)s / segments, 0f));
                if (s < segments)
                {
                    int i = rim + s * 2;
                    tris.AddRange(new[] { i, i + 2, i + 1, i + 1, i + 2, i + 3 });
                }
            }
            return Finish(v, n, uv, tris);
        }

        // Toroide grueso (bandas de Atadura).
        static Mesh BuildBand(float radius, float tube, int segments, int sides)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float a = (float)s / segments * Mathf.PI * 2f;
                var center = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
                for (int k = 0; k <= sides; k++)
                {
                    float b = (float)k / sides * Mathf.PI * 2f;
                    // Sección aplanada (más alta que gruesa): parece una banda, no un tubo.
                    var nn = Mathf.Cos(b) * center.normalized + Mathf.Sin(b) * Vector3.up;
                    v.Add(center + Mathf.Cos(b) * center.normalized * tube * 0.6f + Mathf.Sin(b) * Vector3.up * tube * 1.3f);
                    n.Add(nn);
                    uv.Add(new Vector2((float)s / segments, (float)k / sides));
                }
            }
            int row = sides + 1;
            for (int s = 0; s < segments; s++)
            for (int k = 0; k < sides; k++)
            {
                int i = s * row + k;
                tris.AddRange(new[] { i, i + 1, i + row + 1, i, i + row + 1, i + row });
            }
            return Finish(v, n, uv, tris);
        }

        static void Disk(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> tris, Vector2 c, Vector2 r, int segments)
        {
            int center = v.Count;
            v.Add(new Vector3(c.x, c.y, 0f));
            n.Add(Vector3.back);
            uv.Add(new Vector2(c.x + 0.5f, c.y + 0.5f));
            for (int s = 0; s <= segments; s++)
            {
                float a = s * Mathf.PI * 2f / segments;
                var p = new Vector2(c.x + Mathf.Cos(a) * r.x, c.y + Mathf.Sin(a) * r.y);
                v.Add(new Vector3(p.x, p.y, 0f));
                n.Add(Vector3.back);
                uv.Add(new Vector2(p.x + 0.5f, p.y + 0.5f));
                if (s < segments)
                    tris.AddRange(new[] { center, center + s + 2, center + s + 1 });
            }
        }

        // Huella de pie en el plano XY (planta + cinco dedos), alto ~1,1, centrada; como la malla media luna.
        static Mesh BuildFoot()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            // Planta: talón estrecho abajo, metatarso ancho arriba (dos óvalos solapados).
            Disk(v, n, uv, tris, new Vector2(0.02f, -0.3f), new Vector2(0.17f, 0.22f), 24);
            Disk(v, n, uv, tris, new Vector2(0.04f, 0.02f), new Vector2(0.23f, 0.26f), 24);
            // Dedos: el gordo a la izquierda.
            float[,] toes = { { -0.15f, 0.41f, 0.1f }, { 0.0f, 0.46f, 0.075f }, { 0.12f, 0.45f, 0.065f }, { 0.22f, 0.39f, 0.058f }, { 0.29f, 0.3f, 0.05f } };
            for (int i = 0; i < 5; i++)
                Disk(v, n, uv, tris, new Vector2(toes[i, 0], toes[i, 1]), new Vector2(toes[i, 2], toes[i, 2] * 1.1f), 14);
            return Finish(v, n, uv, tris);
        }

        // Espiral (muelle) alrededor del eje Y: radio 1, alto 1 centrado; sección más alta que gruesa (cinta).
        static Mesh BuildHelix(float turns, float tube, int sides)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            int segments = Mathf.RoundToInt(turns * 48f);
            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float a = u * turns * Mathf.PI * 2f;
                var radial = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var center = radial + Vector3.up * (u - 0.5f);
                // Las puntas se afinan.
                float taper = Mathf.Clamp01(Mathf.Min(u, 1f - u) * 12f);
                for (int k = 0; k <= sides; k++)
                {
                    float b = (float)k / sides * Mathf.PI * 2f;
                    v.Add(center + (Mathf.Cos(b) * radial * tube * 0.7f + Mathf.Sin(b) * Vector3.up * tube * 1.6f) * taper);
                    n.Add(Mathf.Cos(b) * radial + Mathf.Sin(b) * Vector3.up);
                    uv.Add(new Vector2(u, (float)k / sides));
                }
            }
            int row = sides + 1;
            for (int s = 0; s < segments; s++)
            for (int k = 0; k < sides; k++)
            {
                int i = s * row + k;
                tris.AddRange(new[] { i, i + row + 1, i + 1, i, i + row, i + row + 1 });
            }
            return Finish(v, n, uv, tris);
        }

        // Cono (cuerno) a lo largo de +Y: base de radio 0,5 en y = 0 (con tapa), punta en y = 1.
        // ridge > 0 = estrías en espiral (taladro): con el sombreado toon se ven como bandas que giran.
        static Mesh BuildCone(float ridge)
        {
            const int sides = 40, rings = 24;
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float h = (float)r / rings;
                for (int s = 0; s <= sides; s++)
                {
                    float a = (float)s / sides * Mathf.PI * 2f;
                    float rad = 0.5f * Mathf.Pow(1f - h, 0.85f) * (1f + ridge * Mathf.Sin(2f * a - h * 22f));
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    v.Add(dir * rad + Vector3.up * h);
                    n.Add((dir + Vector3.up * 0.45f + new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)) * ridge * 2f * Mathf.Cos(2f * a - h * 22f)).normalized);
                    uv.Add(new Vector2((float)s / sides, h));
                }
            }
            int row = sides + 1;
            for (int r = 0; r < rings; r++)
            for (int s = 0; s < sides; s++)
            {
                int i = r * row + s;
                tris.AddRange(new[] { i, i + row, i + 1, i + 1, i + row, i + row + 1 });
            }
            int c = v.Count;
            v.Add(Vector3.zero);
            n.Add(Vector3.down);
            uv.Add(new Vector2(0.5f, 0f));
            for (int s = 0; s <= sides; s++)
            {
                float a = (float)s / sides * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.5f * (1f + ridge * Mathf.Sin(2f * a)));
                n.Add(Vector3.down);
                uv.Add(new Vector2(0.5f, 0f));
                if (s < sides)
                    tris.AddRange(new[] { c, c + s + 1, c + s + 2 });
            }
            return Finish(v, n, uv, tris);
        }

        // Aro fino en el plano XZ (radio exterior 1, interior <paramref name="inner"/>): UV.x = alrededor, UV.y = 0 dentro, 1 fuera.
        static Mesh BuildThinRing(float inner, int segments)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float a = u * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                v.Add(dir * inner);
                v.Add(dir);
                n.Add(Vector3.up);
                n.Add(Vector3.up);
                uv.Add(new Vector2(u, 0f));
                uv.Add(new Vector2(u, 1f));
                if (s < segments)
                {
                    int i = s * 2;
                    tris.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                }
            }
            return Finish(v, n, uv, tris);
        }

        // Mano con el índice levantado en el plano XY (Metrónomo): muñeca en el origen (pivote del vaivén), punta en y ≈ 1.
        static Mesh BuildFinger()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            Disk(v, n, uv, tris, new Vector2(0f, 0.08f), new Vector2(0.13f, 0.1f), 16);     // muñeca
            Disk(v, n, uv, tris, new Vector2(0.02f, 0.3f), new Vector2(0.22f, 0.2f), 24);   // palma
            // Nudillos de los dedos doblados (a la derecha) y pulgar (a la izquierda).
            Disk(v, n, uv, tris, new Vector2(0.1f, 0.47f), new Vector2(0.085f, 0.08f), 14);
            Disk(v, n, uv, tris, new Vector2(0.2f, 0.4f), new Vector2(0.075f, 0.075f), 14);
            Disk(v, n, uv, tris, new Vector2(0.24f, 0.29f), new Vector2(0.065f, 0.065f), 14);
            Disk(v, n, uv, tris, new Vector2(-0.21f, 0.3f), new Vector2(0.07f, 0.11f), 14);
            // Índice: discos solapados hasta la punta.
            for (int i = 0; i <= 10; i++)
                Disk(v, n, uv, tris, new Vector2(-0.07f, 0.45f + i * 0.05f), new Vector2(0.08f, 0.08f), 14);
            return Finish(v, n, uv, tris);
        }

        // ---- lote 7

        // Estrella rellena de cinco puntas en el plano XY (radio exterior 0,5, punta hacia +Y): Meteoros.
        static Mesh BuildStar5()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            v.Add(Vector3.zero);
            n.Add(Vector3.back);
            uv.Add(new Vector2(0.5f, 0.5f));
            for (int s = 0; s <= 10; s++)
            {
                float a = Mathf.PI * 0.5f + s * Mathf.PI / 5f;
                float r = s % 2 == 0 ? 0.5f : 0.21f;
                var p = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
                v.Add(p);
                n.Add(Vector3.back);
                uv.Add(new Vector2(p.x + 0.5f, p.y + 0.5f));
                if (s < 10)
                    tris.AddRange(new[] { 0, s + 2, s + 1 });
            }
            return Finish(v, n, uv, tris);
        }

        // Cinco brazos (tiras) en estrella en el plano XY (figura 大 de Llamarada), radio 0,5. Cada brazo empieza algo antes del
        // centro: UV.x = a lo ancho, UV.y = a lo largo; con T_Ray (material Blade) cada brazo es un huso que se afila en la punta.
        static Mesh BuildFlameStar()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            for (int k = 0; k < 5; k++)
            {
                float a = Mathf.PI * 0.5f + k * Mathf.PI * 2f / 5f;
                var d = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                var side = new Vector3(-d.y, d.x, 0f);
                // Las piernas (brazos de abajo) algo más largas, como el trazo del kanji.
                float len = k == 2 || k == 3 ? 0.5f : 0.44f;
                int i = v.Count;
                v.Add(d * -0.2f - side * 0.13f);
                v.Add(d * -0.2f + side * 0.13f);
                v.Add(d * len + side * 0.13f);
                v.Add(d * len - side * 0.13f);
                for (int q = 0; q < 4; q++)
                    n.Add(Vector3.back);
                uv.Add(new Vector2(0f, 0f));
                uv.Add(new Vector2(1f, 0f));
                uv.Add(new Vector2(1f, 1f));
                uv.Add(new Vector2(0f, 1f));
                tris.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            }
            return Finish(v, n, uv, tris);
        }

        static void AddSphere(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> tris, Vector3 c, float r, int rings, int segs)
        {
            int start = v.Count;
            for (int i = 0; i <= rings; i++)
            {
                float th = Mathf.PI * i / rings;
                for (int s = 0; s <= segs; s++)
                {
                    float ph = Mathf.PI * 2f * s / segs;
                    var d = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph));
                    v.Add(c + d * r);
                    n.Add(d);
                    uv.Add(new Vector2((float)s / segs, (float)i / rings));
                }
            }
            int row = segs + 1;
            for (int i = 0; i < rings; i++)
            for (int s = 0; s < segs; s++)
            {
                int a = start + i * row + s;
                tris.AddRange(new[] { a, a + 1, a + row, a + 1, a + row + 1, a + row });
            }
        }

        // Hueso a lo largo de +Y (de 0 a 1): caña cilíndrica y dos nudos dobles en cada extremo.
        static Mesh BuildBone()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            const int sides = 14;
            const float r = 0.055f;
            int start = v.Count;
            for (int k = 0; k <= 1; k++)
            {
                float y = k == 0 ? 0.08f : 0.92f;
                for (int s = 0; s <= sides; s++)
                {
                    float a = Mathf.PI * 2f * s / sides;
                    var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    v.Add(d * r + Vector3.up * y);
                    n.Add(d);
                    uv.Add(new Vector2((float)s / sides, k));
                }
            }
            for (int s = 0; s < sides; s++)
            {
                int a = start + s, b = start + sides + 1 + s;
                tris.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
            }
            foreach (float y in new[] { 0.05f, 0.95f })
            foreach (float x in new[] { -0.07f, 0.07f })
                AddSphere(v, n, uv, tris, new Vector3(x, y, 0f), 0.09f, 8, 14);
            return Finish(v, n, uv, tris);
        }

        // Media cáscara de huevo: mitad inferior de un elipsoide (radio 0,5, alto 0,65) con el borde superior en zigzag.
        // El origen está en el centro del ecuador: la mitad de arriba es la misma malla girada 180° en X (y 15° en Y para que
        // los dientes encajen).
        static Mesh BuildEggShell()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            const int rings = 10, segs = 24;
            for (int i = 0; i <= rings; i++)
            {
                float th = -Mathf.PI * 0.5f + Mathf.PI * 0.5f * i / rings;
                for (int s = 0; s <= segs; s++)
                {
                    float ph = Mathf.PI * 2f * s / segs;
                    float zig = i == rings ? (s % 2 == 0 ? 0.1f : -0.07f) : 0f;
                    var d = new Vector3(Mathf.Cos(th) * Mathf.Cos(ph), Mathf.Sin(th), Mathf.Cos(th) * Mathf.Sin(ph));
                    v.Add(new Vector3(d.x * 0.5f, d.y * 0.65f + zig, d.z * 0.5f));
                    n.Add(new Vector3(d.x / 0.5f, d.y / 0.65f, d.z / 0.5f).normalized);
                    uv.Add(new Vector2((float)s / segs, (float)i / rings));
                }
            }
            int row = segs + 1;
            for (int i = 0; i < rings; i++)
            for (int s = 0; s < segs; s++)
            {
                int a = i * row + s;
                tris.AddRange(new[] { a, a + row, a + 1, a + 1, a + row, a + row + 1 });
            }
            return Finish(v, n, uv, tris);
        }

        // Cuchara plana en el plano XY con el pivote en el punto de doblado (origen): head = cuello y cazoleta (+Y),
        // si no, el mango (-Y). Kinético dobla la cabeza alrededor del pivote.
        static Mesh BuildSpoon(bool head)
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tris = new List<int>();
            if (head)
            {
                Disk(v, n, uv, tris, new Vector2(0f, 0.13f), new Vector2(0.035f, 0.16f), 16);
                Disk(v, n, uv, tris, new Vector2(0f, 0.42f), new Vector2(0.12f, 0.17f), 28);
            }
            else
            {
                Disk(v, n, uv, tris, new Vector2(0f, -0.28f), new Vector2(0.045f, 0.3f), 20);
                Disk(v, n, uv, tris, new Vector2(0f, -0.56f), new Vector2(0.065f, 0.07f), 18);
            }
            return Finish(v, n, uv, tris);
        }

        // ------------------------------------------------------------ curvas

        /// <summary>Curva escalonada (sin interpolación): pares (tiempo, valor).</summary>
        public static AnimationCurve Step(params float[] tv)
        {
            var keys = new Keyframe[tv.Length / 2];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = new Keyframe(tv[i * 2], tv[i * 2 + 1], float.PositiveInfinity, float.PositiveInfinity);
            return new AnimationCurve(keys);
        }

        /// <summary>Parpadeo escalonado entre on/off con periodo fijo dentro de [t0, t1].</summary>
        public static AnimationCurve Flicker(float t0, float t1, float period, int phase, int count, float on, float off)
        {
            var tv = new List<float> { -1f, off };
            int i = 0;
            for (float t = t0; t < t1; t += period, i++)
            {
                tv.Add(t);
                tv.Add(i % count == phase ? on : off);
            }
            tv.Add(t1);
            tv.Add(off);
            return Step(tv.ToArray());
        }

        // ------------------------------------------------------------ bloques: impacto

        /// <summary>Destello corto: estrella + círculo (fotograma de impacto).</summary>
        public static void Flash(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float size)
        {
            b.Cue(b.Ps("Flash", Mat(p, "Star"), pos)
                .Life(0.14f).Size(size).Burst(1).Rotation(0f, 45f).AlphaOverLife(0f, 1f, 1f, 0f).Order(9), t);
            b.Cue(b.Ps("FlashGlow", Mat(p, "Glow"), pos)
                .Life(0.12f).Size(size * 0.6f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(8), t);
        }

        /// <summary>
        /// Estrella de impacto grande: estrella que gira, núcleo, rayos radiales largos y chispas.
        /// size ~ diámetro en metros (4 = tan alta como dos veces el atrezo).
        /// </summary>
        public static void ImpactStar(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float size, int rays = 10, int sparks = 30, float life = 0.24f)
        {
            float k = size / 4f;
            b.Cue(b.Ps("ImpactStar", Mat(p, "Star"), pos)
                .Life(life).Size(size).Burst(1).Rotation(0f, 45f).Spin(-50f, 50f)
                .SizeOverLife(C(0f, 0.45f, 0.15f, 1.1f, 1f, 0.85f)).AlphaOverLife(0f, 1f, 0.45f, 0.85f, 1f, 0f).Order(7), t);
            b.Cue(b.Ps("ImpactCore", Mat(p, "Glow"), pos)
                .Life(life * 0.6f).Size(size * 0.42f).Burst(1)
                .SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 0.7f)).AlphaOverLife(0f, 1f, 0.6f, 0.7f, 1f, 0f).Order(8), t);
            if (rays > 0)
                b.Cue(b.Ps("ImpactRays", Mat(p, "Spark"), pos)
                    .Life(0.32f).Speed(0.01f).Size(0.3f * k).Sphere(0.1f).Burst(rays)
                    .Stretch(16f).AlphaOverLife(0f, 1f, 0.35f, 0f, 1f, 0f).Order(6), t);
            if (sparks > 0)
                Sparks(b, pos, p, t, sparks, 6f * k, 13f * k);
        }

        /// <summary>Rayos radiales largos (fotograma de impacto manga), sin estrella.</summary>
        public static Fx Rays(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, int count, float length, float life = 0.3f)
        {
            return b.Cue(b.Ps("Rays", Mat(p, "Spark"), pos)
                .Life(life).Speed(0.01f).Size(length / 16f).Sphere(0.1f).Burst(count)
                .Stretch(16f).AlphaOverLife(0f, 1f, 0.4f, 0.2f, 1f, 0f).Order(6), t);
        }

        public static void Sparks(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, int count, float speedMin, float speedMax, float gravity = 0.8f, float size = 1f)
        {
            b.Cue(b.Ps("Sparks", Mat(p, "Spark"), pos)
                .Sphere(0.3f).Speed(speedMin, speedMax).Life(0.25f, 0.6f).Size(0.08f * size, 0.15f * size)
                .Burst(count).Gravity(gravity).Drag(2f).Stretch(5f, 0.04f)
                .AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(5), t);
        }

        /// <summary>Onda en el suelo (malla de anillo) + aro en el aire.</summary>
        public static void Shockwave(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t, float radius, bool air = false)
        {
            b.Cue(b.Ps("GroundShock", Mat(p, "Shock"), ground + new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.5f).Size(1f).Burst(1)
                .Rotation3D(Vector3.zero, new Vector3(0f, 360f, 0f))
                .SizeOverLife(C(0f, 0.25f * radius, 0.3f, 0.8f * radius, 1f, radius))
                .AlphaOverLife(0f, 1f, 0.4f, 0.85f, 1f, 0f).Order(0), t);
            if (air)
                b.Cue(b.Ps("AirRing", Mat(p, "Ring"), ground + new Vector3(0f, 0.9f, 0f))
                    .Life(0.2f).Size(radius * 1.1f).Burst(1)
                    .SizeOverLife(C(0f, 0.2f, 0.3f, 0.85f, 1f, 1.1f)).AlphaOverLife(0f, 1f, 0.3f, 0.6f, 1f, 0f).Order(4), t + 0.01f);
        }

        /// <summary>Anillo de polvo toon que sale en el suelo.</summary>
        public static void Dust(EmeraldMoveBuilder b, Vector3 ground, float t, float radius, int count, float size = 1f, Material mat = null)
        {
            b.Cue(b.Ps("Dust", mat != null ? mat : M("EM_Dust"), ground + new Vector3(0f, 0.25f, 0f))
                .GroundCircle(radius, 0.3f).Speed(1.4f * size, 3.2f * size).Drag(2.5f).Life(0.8f, 1.4f).Size(0.8f * size, 1.5f * size)
                .Burst(count).Velocity(new Vector3(0f, 0.5f, 0f)).SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0), t);
        }

        /// <summary>Humo toon (cualquier material de humo: EX_Smoke, EX_DarkSmoke, EM_Mist...).</summary>
        public static void Smoke(EmeraldMoveBuilder b, Vector3 pos, Material mat, float t, int count, float size, float rise = 0.6f, float spread = 0.7f, float life = 1.5f)
        {
            b.Cue(b.Ps("Smoke", mat, pos)
                .Sphere(spread).Speed(0.8f * size, 2.2f * size).Drag(2.2f).Life(life * 0.75f, life * 1.15f).Size(1.0f * size, 1.8f * size)
                .Burst(count).Velocity(new Vector3(0f, rise, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.45f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), t);
        }

        public static void Debris(EmeraldMoveBuilder b, Vector3 pos, float t, int count, float speed, float size = 1f, Material mat = null)
        {
            b.Cue(b.Ps("Debris", mat != null ? mat : M("EX_Debris"), pos)
                .Mesh(BotwMeshes.Rock).Hemisphere(0.4f).Speed(speed * 0.5f, speed).Life(1.2f, 1.8f)
                .Size(0.1f * size, 0.26f * size).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-400f, 400f, true)
                .Gravity(2.2f).Burst(count).Collide(0.35f, 0.45f)
                .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(0), t);
        }

        /// <summary>Líneas de velocidad (manga) que convergen en un punto.</summary>
        public static void SpeedLines(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float duration, float radius, float rate, float width = 1f)
        {
            b.Cue(b.Ps("SpeedLines", Mat(p, "Spark"), pos)
                .Sphere(radius, 0f).Radial(-radius * 2.2f).Life(0.22f).Size(0.06f * width, 0.11f * width)
                .Rate(rate).Duration(duration).Stretch(10f, 0.06f)
                .AlphaOverLife(0f, 0.2f, 0.25f, 1f, 1f, 0.3f).Order(2), t);
        }

        /// <summary>Rayas paralelas que cruzan en una dirección (embestida, ráfaga, estelas de velocidad).</summary>
        public static void Streaks(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float spread, float width = 1f, float life = 0.25f)
        {
            var dir = to - from;
            b.Cue(b.Ps("Streaks", Mat(p, "Spark"), from)
                .Sphere(spread).Velocity(dir / life).Life(life * 0.8f, life).Size(0.05f * width, 0.1f * width)
                .Rate(rate).Duration(duration).Stretch(6f, 0.08f)
                .AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(3), t);
        }

        /// <summary>Destellos flotantes (estrellitas que titilan y suben).</summary>
        public static void Glints(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float duration, float radius, float rate, float size = 1f)
        {
            b.Cue(b.Ps("Glints", Mat(p, "Star"), pos)
                .Sphere(radius).Speed(0.1f, 0.5f).Velocity(new Vector3(0f, 0.5f, 0f)).Life(0.35f, 0.7f)
                .Size(0.18f * size, 0.42f * size).Rate(rate).Duration(duration).Rotation(0f, 90f)
                .AlphaOverLife(0f, 0f, 0.2f, 1f, 0.6f, 1f, 1f, 0f).Order(5), t);
        }

        // ------------------------------------------------------------ bloques: carga y auras

        /// <summary>Carga: chispas que convergen, orbe que crece y aros que se cierran (como el Guardián).</summary>
        public static void Charge(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float duration, float radius = 1.6f, float orb = 1.4f, bool rings = true)
        {
            b.Cue(b.Ps("ChargeSparks", Mat(p, "Spark"), pos)
                .Sphere(radius, 0f).Radial(-radius / 0.28f).Life(0.28f).Size(0.06f, 0.12f).Rate(70f).Duration(duration)
                .Stretch(4f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0.6f).Order(4), t);
            b.Cue(b.Ps("ChargeOrb", Mat(p, "Glow"), pos)
                .Life(duration + 0.08f).Size(orb).Burst(1)
                .SizeOverLife(C(0f, 0.1f, 0.75f, 0.8f, 0.92f, 1f, 1f, 0.3f)).Order(5), t);
            if (rings)
                b.Cue(b.Ps("ChargeRings", Mat(p, "Ring"), pos)
                    .Life(0.35f).Size(radius * 1.3f).Rate(5f).Duration(Mathf.Max(0.1f, duration - 0.3f))
                    .SizeOverLife(C(0f, 1f, 1f, 0.1f)).AlphaOverLife(0f, 0f, 0.3f, 1f, 1f, 0.3f).Order(3), t);
        }

        /// <summary>
        /// Aura alrededor de un personaje (pies en <paramref name="ground"/>): casco de contorno
        /// (Energy Sphere con fresnel) + lenguas que suben + motas. Es el "outline glow" de las referencias.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack Aura(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float t1, float scale = 1f, float flameRate = 45f, bool shell = true)
        {
            EmeraldMoveVfx.PartTrack tr = null;
            if (shell)
            {
                var baseScale = new Vector3(0.6f, 0.98f, 0.6f) * scale;
                var r = b.MeshPart("AuraShell", BotwMeshes.Sphere, Mat(p, "Shell"), ground + new Vector3(0f, 0.82f * scale, 0f), Vector3.zero, baseScale, order: 1);
                tr = b.Track(r.transform, r, t0, t1);
                tr.scaleFrom = baseScale * 0.8f;
                tr.scaleTo = baseScale;
                tr.scaleCurve = C(t0, 0f, t0 + 0.15f, 1.06f, t0 + 0.3f, 1f, t1 - 0.25f, 1f, t1, 1.12f);
                tr.property = "_Dissolve";
                tr.propertyCurve = C(t0, 0.9f, t0 + 0.12f, 0f, t1 - 0.3f, 0f, t1, 1f);
            }
            if (flameRate > 0f)
            {
                b.Cue(b.Ps("AuraFlames", Mat(p, "Spark"), ground + new Vector3(0f, 0.05f, 0f))
                    .GroundCircle(0.5f * scale, 0.2f).Velocity(new Vector3(0f, 2.6f * scale, 0f)).Life(0.35f, 0.6f)
                    .Size(0.2f * scale, 0.38f * scale).Rate(flameRate).Duration(Mathf.Max(0.1f, t1 - t0 - 0.25f))
                    .Stretch(1.4f, 0.2f).AlphaOverLife(0f, 0.2f, 0.25f, 1f, 1f, 0f).Order(2), t0);
                b.Cue(b.Ps("AuraMotes", Mat(p, "Star"), ground + new Vector3(0f, 0.8f * scale, 0f))
                    .Sphere(0.6f * scale).Velocity(new Vector3(0f, 0.9f, 0f)).Life(0.5f, 0.9f).Size(0.1f, 0.2f)
                    .Rate(flameRate * 0.2f).Duration(Mathf.Max(0.1f, t1 - t0 - 0.25f)).Spin(-180f, 180f)
                    .AlphaOverLife(0f, 0f, 0.2f, 1f, 0.6f, 1f, 1f, 0f).Order(3), t0);
            }
            return tr;
        }

        /// <summary>Destello de contorno sobre el personaje golpeado (casco que se infla y se rompe).</summary>
        public static MeshRenderer HitShell(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t, float duration, float scale = 1f)
        {
            var baseScale = new Vector3(0.62f, 1f, 0.62f) * scale;
            var r = b.MeshPart("HitShell", BotwMeshes.Sphere, Mat(p, "Shell"), ground + new Vector3(0f, 0.82f * scale, 0f), Vector3.zero, baseScale, order: 2);
            var tr = b.Track(r.transform, r, t, t + duration);
            tr.scaleFrom = baseScale * 0.95f;
            tr.scaleTo = baseScale * 1.18f;
            tr.scaleCurve = C(t, 0f, t + duration, 1f);
            tr.property = "_Dissolve";
            tr.propertyCurve = C(t, 0f, t + duration * 0.35f, 0f, t + duration, 1f);
            return r;
        }

        /// <summary>Aro brillante en el suelo bajo un personaje (girando).</summary>
        public static void GroundGlow(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float t1, float radius)
        {
            var r = b.MeshPart("GroundGlow", BotwMeshes.Ring, Mat(p, "Shock"), ground + new Vector3(0f, 0.05f, 0f), Vector3.zero, Vector3.one * radius, order: 0);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = Vector3.one * radius * 0.3f;
            tr.scaleTo = Vector3.one * radius;
            tr.scaleCurve = C(t0, 0f, t0 + 0.25f, 1f, t1, 1.05f);
            tr.spin = new Vector3(0f, 120f, 0f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0.6f, t0 + 0.2f, 0f, t1 - 0.3f, 0f, t1, 1f);
        }

        /// <summary>Flechas/lenguas que suben alrededor del personaje (subida de estadística).</summary>
        public static void RisingArrows(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float duration, float rate = 14f)
        {
            b.Cue(b.Ps("RisingArrows", Mat(p, "Spark"), ground + new Vector3(0f, 0.1f, 0f))
                .GroundCircle(0.75f, 0.3f).Velocity(new Vector3(0f, 3.2f, 0f)).Life(0.5f, 0.75f)
                .Size(0.22f, 0.34f).Rate(rate).Duration(duration).Stretch(2.2f, 0.12f)
                .AlphaOverLife(0f, 0.2f, 0.2f, 1f, 0.7f, 1f, 1f, 0f).Order(3), t0);
        }

        /// <summary>Sombra en el suelo que crece (algo cae desde arriba).</summary>
        public static void GroundShadow(EmeraldMoveBuilder b, Vector3 ground, float t0, float t1, float fromSize, float toSize)
        {
            var r = b.MeshPart("Shadow", Quad, ShadowMat(), ground + new Vector3(0f, 0.04f, 0f), new Vector3(90f, 0f, 0f), Vector3.one * fromSize, order: 0);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = Vector3.one * fromSize;
            tr.scaleTo = Vector3.one * toSize;
            tr.scaleCurve = C(t0, 0f, t1, 1f);
        }

        // ------------------------------------------------------------ bloques: cortes

        /// <summary>
        /// Tajo en media luna (malla orientada a cámara). angle = giro en pantalla (0 = convexa a la derecha),
        /// size = alto aproximado / 2,1. sweep = giro durante la vida (grados/s).
        /// </summary>
        public static Fx Slash(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float angle, float size, float life = 0.3f, float sweep = 0f)
        {
            var f = b.Ps("Slash", Mat(p, "Blade"), pos)
                .Mesh(Crescent, ParticleSystemRenderSpace.View).Life(life).Size(size).Burst(1).Rotation(angle, angle)
                .SizeOverLife(C(0f, 0.7f, 0.2f, 1.05f, 1f, 1.12f)).AlphaOverLife(0f, 1f, 0.5f, 0.9f, 1f, 0f).Order(6);
            if (sweep != 0f)
                f.Spin(sweep, sweep);
            return b.Cue(f, t);
        }

        /// <summary>Línea de corte larga y fina (huso estirado) que destella.</summary>
        public static void CutLine(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float angle, float length, float width = 0.22f, float life = 0.28f)
        {
            b.Cue(b.Ps("CutLine", Mat(p, "Spark"), pos)
                .Life(life).Size3D(new Vector3(width, length, 1f)).Burst(1).Rotation(angle, angle)
                .SizeOverLife(C(0f, 0.3f, 0.2f, 1f, 1f, 1.05f)).AlphaOverLife(0f, 1f, 0.5f, 0.9f, 1f, 0f).Order(8), t);
        }

        // ------------------------------------------------------------ bloques: elementos

        /// <summary>Explosión de fuego toon (bola de pinchos, lenguas, ascuas, humo caliente) de 80.lv.</summary>
        public static void FireBurst(EmeraldMoveBuilder b, Vector3 pos, float t, float size, bool smoke = true)
        {
            b.Cue(b.Ps("FireBall", M("EX_Ball"), pos)
                .Mesh(BotwMeshes.SpikyBall).Life(0.5f).Size(1.4f * size).Burst(1)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-90f, 90f, true)
                .SizeOverLife(C(0f, 0.15f, 0.22f, 1f, 1f, 1.2f))
                .Custom(Curve(0f, 0f, 0.3f, 0.05f, 1f, 1f), Rand(0f, 1f), Const(0f), Const(0f)).Order(3), t);
            var tongues = b.Ps("FireTongues", M("EX_Tongue"), pos)
                .HalfDonut(1.0f * size, 0.25f * size).Speed(6f * size, 10f * size).Life(0.22f, 0.4f).Size(0.45f * size, 0.8f * size).Burst(22)
                .Stretch(3.2f, 0.03f).Drag(3f).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(2);
            tongues.ps.gameObject.AddComponent<FaceCamera>();
            b.Cue(tongues, t);
            b.Cue(b.Ps("Embers", M("EX_Ember"), pos)
                .Sphere(0.5f * size).Speed(6f * size, 14f * size).Life(0.4f, 1.1f).Size(0.06f, 0.13f).Burst(40)
                .Gravity(0.8f).Drag(2f).Stretch(5f, 0.04f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(4), t + 0.02f);
            if (smoke)
                b.Cue(b.Ps("FireSmoke", M("EX_Smoke"), pos)
                .Sphere(0.6f * size).Speed(2.5f * size, 5f * size).Drag(3.5f).Life(1.3f, 2.0f).Size(1.1f * size, 1.9f * size).Burst(14)
                .Velocity(new Vector3(0f, 0.7f, 0f)).SizeOverLife(C(0f, 0.45f, 0.15f, 1f, 1f, 1.25f))
                .Custom(Curve(0f, 0f, 0.12f, 0.12f, 0.45f, 1f), Rand(-0.05f, 0.08f), Curve(0f, 0f, 0.55f, 0.08f, 1f, 1f), Rand(0f, 1f))
                .Order(1), t + 0.04f);
        }

        /// <summary>Llamas continuas que suben desde un punto (puño en llamas, quemadura).</summary>
        public static void Flames(EmeraldMoveBuilder b, Vector3 pos, float t, float duration, float radius, float size, float rate = 50f)
        {
            b.Cue(b.Ps("Flames", M("EX_Tongue"), pos)
                .Sphere(radius).Velocity(new Vector3(0f, 2.2f * size, 0f)).Speed(0.2f, 0.8f).Life(0.25f, 0.45f)
                .Size(0.3f * size, 0.55f * size).Rate(rate).Duration(duration).Stretch(1.6f, 0.15f)
                .AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(4), t);
            b.Cue(b.Ps("FlameGlow", Mat(Fire, "Glow"), pos)
                .Life(0.3f).Size(1.3f * size).Rate(8f).Duration(duration)
                .SizeOverLife(C(0f, 0.7f, 0.5f, 1f, 1f, 0.8f)).AlphaOverLife(0f, 1f, 1f, 0.2f).Order(3), t);
        }

        public static void Embers(EmeraldMoveBuilder b, Vector3 pos, float t, float duration, float radius, float rate)
        {
            b.Cue(b.Ps("RisingEmbers", M("EX_Ember"), pos)
                .Sphere(radius).Velocity(new Vector3(0f, 1.4f, 0f)).Speed(0.2f, 0.9f).Life(0.6f, 1.2f).Size(0.05f, 0.1f)
                .Rate(rate).Duration(duration).Stretch(3f, 0.05f).AlphaOverLife(0f, 1f, 0.7f, 0.8f, 1f, 0f).Order(4), t);
        }

        /// <summary>Estallido de hielo: esquirlas, copos y escarcha.</summary>
        public static void IceBurst(EmeraldMoveBuilder b, Vector3 pos, float t, float size, int shards = 18)
        {
            b.Cue(b.Ps("IceShards", M("EM_IceShard"), pos)
                .Mesh(BotwMeshes.Rock).Sphere(0.35f * size).Speed(4f * size, 8f * size).Life(0.7f, 1.2f)
                .Size3D(new Vector3(0.14f, 0.45f, 0.14f) * size).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-300f, 300f, true)
                .Gravity(1.6f).Drag(1f).Burst(shards).SizeOverLife(C(0f, 1f, 0.8f, 1f, 1f, 0f)).Order(2), t);
            Snowflakes(b, pos, t, 26, 0.6f * size, size);
            Smoke(b, pos, M("EM_Mist"), t + 0.05f, 9, 1.0f * size, 0.3f, 0.7f * size, 1.4f);
        }

        public static void Snowflakes(EmeraldMoveBuilder b, Vector3 pos, float t, int count, float radius, float size = 1f)
        {
            b.Cue(b.Ps("Snowflakes", Mat(Ice, "Star"), pos)
                .Sphere(radius).Speed(1.5f * size, 4.5f * size).Drag(2.2f).Life(0.8f, 1.5f).Size(0.3f, 0.55f)
                .Burst(count).Gravity(0.12f).Rotation(0f, 90f).Spin(-120f, 120f)
                .AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(5), t);
        }

        /// <summary>Puntos de un rayo quebrado entre from y to (semilla fija).</summary>
        public static Vector3[] BoltPoints(Vector3 from, Vector3 to, int segments, float jitter, int seed)
        {
            var rng = new System.Random(seed);
            var dir = to - from;
            var side = Vector3.Cross(dir.normalized, Vector3.up);
            if (side.sqrMagnitude < 1e-4f)
                side = Vector3.right;
            side.Normalize();
            var up = Vector3.Cross(side, dir.normalized).normalized;
            var pts = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float u = (float)i / segments;
                float amp = jitter * Mathf.Sin(Mathf.PI * u);
                float a = (float)(rng.NextDouble() * 2.0 - 1.0) * amp;
                float c = (float)(rng.NextDouble() * 2.0 - 1.0) * amp * 0.5f;
                pts[i] = from + dir * u + (i == 0 || i == segments ? Vector3.zero : side * a + up * c);
            }
            return pts;
        }

        /// <summary>
        /// Rayo quebrado que parpadea entre <paramref name="variants"/> formas distintas (LineRenderer
        /// con erosión escalonada) durante [t0, t1].
        /// </summary>
        public static void Bolt(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t0, float t1, float width = 0.3f,
            int segments = 9, float jitter = 0.35f, int variants = 3, int seed = 1, float period = 0.05f)
        {
            var mat = Mat(p, "Bolt");
            var w = C(0f, 0.6f, 0.15f, 1f, 0.7f, 0.85f, 1f, 0.3f);
            for (int v = 0; v < variants; v++)
            {
                var line = b.LinePart("Bolt", mat, BoltPoints(from, to, segments, jitter, seed * 97 + v * 13), w);
                line.widthMultiplier = width;
                var tr = b.Track(line.transform, line, t0, t1);
                tr.property = "_Erosion";
                tr.propertyCurve = Flicker(t0, t1, period, v, variants, 0f, 1f);
            }
        }

        /// <summary>Rayos quebrados que salen en estrella desde un punto.</summary>
        public static void RadialBolts(EmeraldMoveBuilder b, Vector3 center, Pal p, float t0, float t1, int count, float length, int seed = 3, float width = 0.26f)
        {
            var rng = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float a = (i + (float)rng.NextDouble() * 0.6f) / count * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a) * 0.55f, Mathf.Sin(a), Mathf.Cos(a) * -0.8f).normalized;
                float len = length * (0.7f + 0.5f * (float)rng.NextDouble());
                float start = t0 + (float)rng.NextDouble() * 0.08f;
                Bolt(b, center + dir * 0.25f, center + dir * len, p, start, t1, width, 6, len * 0.18f, 2, seed * 31 + i, 0.06f);
            }
        }

        /// <summary>Crepitar eléctrico (chispas cortas muy rápidas).</summary>
        public static void Crackle(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float duration, float radius, float rate)
        {
            b.Cue(b.Ps("Crackle", Mat(p, "Spark"), pos)
                .Sphere(radius).Speed(8f, 14f).Drag(6f).Life(0.08f, 0.2f).Size(0.05f, 0.1f).Rate(rate).Duration(duration)
                .Stretch(7f, 0.06f).AlphaOverLife(0f, 1f, 1f, 0.4f).Order(5), t);
        }

        // ------------------------------------------------------------ bloques: proyectiles y viento

        static void Trail(Fx f, Material mat, float ratio, float width)
        {
            var tr = f.ps.trails;
            tr.enabled = true;
            tr.mode = ParticleSystemTrailMode.PerParticle;
            tr.ratio = 1f;
            tr.lifetime = new MinMaxCurve(ratio);
            tr.minVertexDistance = 0.05f;
            tr.dieWithParticles = false;
            tr.sizeAffectsWidth = false;
            tr.widthOverTrail = new MinMaxCurve(width, C(0f, 1f, 1f, 0f));
            tr.textureMode = ParticleSystemTrailTextureMode.Stretch;
            tr.colorOverTrail = new ParticleSystem.MinMaxGradient(Color.white);
            f.renderer.trailMaterial = mat;
        }

        /// <summary>Proyectil (destello) con estela que viaja de from a to en <paramref name="travel"/> s.</summary>
        public static Fx Projectile(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float travel, float size, float trailWidth, string kind = "Glow", Vector3 arc = default)
        {
            var f = b.Ps("Projectile", Mat(p, kind), from)
                .Life(travel).Size(size).Burst(1).Velocity((to - from) / travel)
                .AlphaOverLife(0f, 1f, 0.85f, 1f, 1f, 0.6f).Order(6);
            if (arc != default)
            {
                // Parábola: velocidad inicial hacia arriba y gravedad que la devuelve al destino.
                // (Unity exige que x, y, z estén en el mismo modo: las tres como curva.)
                var vel = f.ps.velocityOverLifetime;
                var v = (to - from) / travel;
                float up = 4f * arc.y / travel;
                vel.x = new MinMaxCurve(1f, C(0f, v.x, 1f, v.x));
                vel.y = new MinMaxCurve(1f, C(0f, v.y + up, 1f, v.y - up));
                vel.z = new MinMaxCurve(1f, C(0f, v.z, 1f, v.z));
            }
            if (trailWidth > 0f)
                Trail(f, Mat(p, "Line"), 0.6f, trailWidth);
            return b.Cue(f, t);
        }

        /// <summary>Cuchilla de viento en media luna que viaja de from a to.</summary>
        public static void WindBlade(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float travel, float size, float angle = 0f)
        {
            b.Cue(b.Ps("WindBlade", Mat(p, "Blade"), from)
                .Mesh(Crescent, ParticleSystemRenderSpace.View).Life(travel).Size(size).Burst(1).Rotation(angle, angle)
                .Velocity((to - from) / travel).SizeOverLife(C(0f, 0.5f, 0.25f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.9f, 1f, 1f, 0.5f).Order(6), t);
            var core = b.Ps("WindBladeTrail", Mat(p, "Spark"), from)
                .Life(travel).Size(0.01f).Burst(1).Velocity((to - from) / travel).Order(5);
            Trail(core, Mat(p, "Line"), 0.35f, size * 0.55f);
            b.Cue(core, t);
        }

        /// <summary>
        /// Tornado de cintas (malla Arc girando alrededor del eje vertical) + rayas que orbitan + polvo.
        /// </summary>
        public static void Tornado(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float t1, float height, float radius, float rate = 26f, bool dust = true)
        {
            float d = Mathf.Max(0.1f, t1 - t0 - 0.3f);
            for (int layer = 0; layer < 2; layer++)
            {
                float h = height * (layer == 0 ? 0.55f : 0.5f);
                float y = layer == 0 ? h * 0.5f : height * 0.5f + h * 0.5f;
                float r = radius * (layer == 0 ? 0.8f : 1.25f);
                var f = b.Ps("TornadoStripes", Mat(p, "Stripe"), ground + new Vector3(0f, y, 0f))
                    .LocalSpace().Mesh(BotwMeshes.Arc, ParticleSystemRenderSpace.Local).Box(new Vector3(0f, 0f, h))
                    .Life(0.3f, 0.5f).Size(r * 0.85f, r * 1.15f).Rotation3D(Vector3.zero, new Vector3(0f, 0f, 360f))
                    .Spin(-760f, -520f).Rate(rate).Duration(d)
                    .SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 1.15f)).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3);
                f.ps.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                b.Cue(f, t0);
            }
            var streaks = b.Ps("TornadoStreaks", Mat(p, "Spark"), ground + new Vector3(0f, 0.2f, 0f))
                .GroundCircle(radius, 0.4f).Radial(0.6f, 7f).Life(0.4f, 0.7f).Size(0.06f, 0.11f)
                .Rate(rate * 2f).Duration(d).Stretch(4f, 0.12f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(4);
            var vel = streaks.ps.velocityOverLifetime;
            vel.y = new MinMaxCurve(height * 1.6f);
            b.Cue(streaks, t0);
            if (dust)
                b.Cue(b.Ps("TornadoDust", M("EM_Dust"), ground + new Vector3(0f, 0.25f, 0f))
                    .GroundCircle(radius * 0.9f, 0.3f).Radial(0.5f, 4f).Life(0.8f, 1.2f).Size(0.6f, 1.1f).Rate(10f).Duration(d)
                    .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                    .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0), t0);
        }

        /// <summary>Hojas/escombros ligeros arrastrados por el viento en espiral.</summary>
        public static void SwirlLeaves(EmeraldMoveBuilder b, Vector3 ground, float t0, float duration, float radius, float height, float rate, Material mat = null)
        {
            var f = b.Ps("SwirlLeaves", mat != null ? mat : M("EM_Leaf"), ground + new Vector3(0f, 0.3f, 0f))
                .GroundCircle(radius, 0.5f).Radial(0.2f, 5f).Life(0.8f, 1.2f).Size(0.16f, 0.28f).Rate(rate).Duration(duration)
                .Rotation(0f, 360f).Spin(-360f, 360f).AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(3);
            var vel = f.ps.velocityOverLifetime;
            vel.y = new MinMaxCurve(height);
            b.Cue(f, t0);
        }
            // ------------------------------------------------------------ bloques: lote 2 (golpes múltiples, huellas, lianas...)

        /// <summary>Añade ráfagas extra a un sistema ya creado (tiempos absolutos; el primero es el del cue).</summary>
        public static Fx Bursts(Fx f, int count, params float[] times)
        {
            var e = f.ps.emission;
            e.SetBursts(new ParticleSystem.Burst[0]);
            for (int i = 0; i < times.Length; i++)
                f.Burst(count, times[i] - times[0]);
            f.Duration(times[times.Length - 1] - times[0] + 0.05f);
            return f;
        }

        /// <summary>Tiñe el color inicial de las partículas (multiplica la paleta del material).</summary>
        public static Fx Tint(Fx f, Color c)
        {
            var m = f.ps.main;
            m.startColor = c;
            return f;
        }

        /// <summary>
        /// Estrellas de impacto para golpes repetidos con solo tres sistemas (estrella, núcleo y chispas),
        /// una ráfaga por instante de <paramref name="times"/>; spread = dispersión de la posición.
        /// </summary>
        public static void HitStars(EmeraldMoveBuilder b, Vector3 pos, Pal p, float[] times, float size, int sparks = 10, float spread = 0.25f, float life = 0.22f)
        {
            float t0 = times[0];
            b.Cue(Bursts(b.Ps("HitStar", Mat(p, "Star"), pos)
                .Sphere(spread).Life(life).Size(size * 0.85f, size * 1.1f).Rotation(0f, 45f).Spin(-60f, 60f)
                .SizeOverLife(C(0f, 0.45f, 0.15f, 1.1f, 1f, 0.85f)).AlphaOverLife(0f, 1f, 0.45f, 0.85f, 1f, 0f).Order(7), 1, times), t0);
            b.Cue(Bursts(b.Ps("HitCore", Mat(p, "Glow"), pos)
                .Life(life * 0.6f).Size(size * 0.42f)
                .SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 0.7f)).AlphaOverLife(0f, 1f, 0.6f, 0.7f, 1f, 0f).Order(8), 1, times), t0);
            if (sparks > 0)
                b.Cue(Bursts(b.Ps("HitSparks", Mat(p, "Spark"), pos)
                    .Sphere(0.3f).Speed(5f * size / 3f, 11f * size / 3f).Life(0.2f, 0.45f).Size(0.08f, 0.15f)
                    .Gravity(0.8f).Drag(2f).Stretch(5f, 0.04f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(5), sparks, times), t0);
        }

        /// <summary>Huella de pie plana (orientada a cámara) que se estampa con un rebote; una por instante.</summary>
        public static Fx Footprint(EmeraldMoveBuilder b, Vector3 pos, Pal p, float[] times, float size, float angle = -20f, float life = 0.5f)
        {
            return b.Cue(Bursts(b.Ps("Footprint", Mat(p, "Flat"), pos)
                .Mesh(Foot, ParticleSystemRenderSpace.View).Life(life).Size(size).Rotation(angle, angle)
                .SizeOverLife(C(0f, 1.45f, 0.1f, 0.92f, 0.22f, 1.02f, 1f, 1.06f)).AlphaOverLife(0f, 1f, 0.65f, 1f, 1f, 0f).Order(8), 1, times), times[0]);
        }

        /// <summary>Corona de "pétalos" (husos gruesos) que salen en abanico desde un círculo en el suelo.</summary>
        public static void Petals(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t, int count, float radius, float speed, float size = 1f)
        {
            b.Cue(b.Ps("Petals", Mat(p, "Spark"), ground + new Vector3(0f, 0.35f, 0f))
                .GroundCircle(radius, 0.1f).Speed(speed * 0.8f, speed).Velocity(new Vector3(0f, speed * 0.4f, 0f)).Drag(3.5f)
                .Life(0.32f, 0.5f).Size(0.32f * size, 0.5f * size).Burst(count).Stretch(2.4f, 0.04f)
                .AlphaOverLife(0f, 1f, 0.6f, 0.9f, 1f, 0f).Order(6), t);
        }

        /// <summary>Aros concéntricos orientados a cámara que se abren uno tras otro (golpe en remolino).</summary>
        public static void RingPulses(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, int count, float interval, float size)
        {
            var f = b.Ps("RingPulses", Mat(p, "Ring"), pos).Life(0.32f).Size(size)
                .SizeOverLife(C(0f, 0.25f, 0.4f, 0.9f, 1f, 1.15f)).AlphaOverLife(0f, 1f, 0.4f, 0.85f, 1f, 0f).Order(4);
            var times = new float[count];
            for (int i = 0; i < count; i++)
                times[i] = t + i * interval;
            b.Cue(Bursts(f, 1, times), t);
        }

        /// <summary>
        /// Arco giratorio horizontal (malla Arc) alrededor de un eje vertical: barrido de pierna, remolino de giro.
        /// </summary>
        public static MeshRenderer SpinArc(EmeraldMoveBuilder b, Vector3 center, Pal p, float t0, float t1, float radius, float width = 1f, float spin = -900f, float tilt = 0f)
        {
            var scale = new Vector3(radius, radius, width);
            var r = b.MeshPart("SpinArc", BotwMeshes.Arc, Mat(p, "Stripe"), center, new Vector3(-90f + tilt, 0f, 0f), scale, order: 4);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = scale * 0.6f;
            tr.scaleTo = scale;
            tr.scaleCurve = C(t0, 0f, t0 + 0.12f, 1f, t1, 1.15f);
            tr.spin = new Vector3(0f, spin, 0f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0.5f, t0 + 0.06f, 0f, t1 - 0.12f, 0.1f, t1, 1f);
            return r;
        }

        /// <summary>
        /// Liana/látigo: línea ondulada que sale de <paramref name="from"/>, se estira hasta <paramref name="to"/>
        /// en <paramref name="hit"/>, restalla y se recoge en <paramref name="t1"/>.
        /// </summary>
        public static void Vine(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t0, float hit, float t1, float width = 0.14f, float wave = 0.4f, float phase = 0f)
        {
            var dir = to - from;
            float len = dir.magnitude;
            var pivot = b.Node("Vine", from);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.right, dir / len);
            const int n = 28;
            var pts = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float u = (float)i / (n - 1);
                // Ondulación en S que se anula en los extremos + rizo cerca de la punta.
                float s = Mathf.Sin(Mathf.PI * u);
                pts[i] = new Vector3(u * len, wave * s * Mathf.Sin(u * Mathf.PI * 2.2f + phase), wave * 0.7f * s * Mathf.Cos(u * Mathf.PI * 3f + phase));
            }
            var line = b.LinePart("VineLine", Mat(p, "Bolt"), pts, C(0f, 1f, 0.75f, 0.75f, 1f, 0.3f), pivot, 5);
            line.widthMultiplier = width;
            var tr = b.Track(pivot, line, t0, t1);
            tr.scaleFrom = new Vector3(0.03f, 1f, 1f);
            tr.scaleTo = Vector3.one;
            tr.scaleCurve = C(t0, 0f, hit - 0.05f, 0.9f, hit, 1.04f, hit + 0.1f, 0.98f, t1, 0f);
            // Restallido: la liana baja de golpe al llegar.
            tr.eulerFrom = pivot.localEulerAngles + new Vector3(0f, 0f, 18f);
            tr.eulerTo = pivot.localEulerAngles;
            tr.rotCurve = C(t0, 0f, hit - 0.06f, 0.2f, hit, 1f, t1, 1f);
        }

        /// <summary>Destellos de cuatro puntas de varios colores (color inicial aleatorio entre <paramref name="colors"/>).</summary>
        public static void Sparkles(EmeraldMoveBuilder b, Vector3 pos, float t, float duration, float radius, float rate, float size, params Color[] colors)
        {
            var f = b.Ps("Sparkles", Mat(White, "Star"), pos)
                .Sphere(radius).Speed(0.05f, 0.3f).Velocity(new Vector3(0f, 0.35f, 0f)).Life(0.45f, 0.8f)
                .Size(0.25f * size, 0.55f * size).Rate(rate).Duration(duration).Rotation(0f, 0f).Spin(-40f, 40f)
                .SizeOverLife(C(0f, 0.2f, 0.25f, 1f, 0.7f, 0.9f, 1f, 0f)).AlphaOverLife(0f, 0f, 0.15f, 1f, 0.7f, 1f, 1f, 0f).Order(5);
            var g = new Gradient();
            var keys = new GradientColorKey[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                keys[i] = new GradientColorKey(colors[i], colors.Length == 1 ? 0f : (float)i / (colors.Length - 1));
            g.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            g.mode = GradientMode.Fixed;
            var main = f.ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
            b.Cue(f, t);
        }

        /// <summary>Estrellas de mareo/confusión girando sobre una cabeza.</summary>
        public static void Dizzy(EmeraldMoveBuilder b, Vector3 head, Pal p, float t, float duration)
        {
            b.Cue(b.Ps("Dizzy", Mat(p, "Star"), head + new Vector3(0f, 0.45f, 0f))
                .GroundCircle(0.45f, 0f).Radial(0f, 6f).Life(1f).Size(0.22f, 0.3f).Rate(6f).Duration(duration).Spin(-200f, 200f)
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f).Order(5), t);
        }

        /// <summary>Chorro de nubes y granos (arena, polvo) que viaja de from a to.</summary>
        public static void CloudStream(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal grains, Material cloud, Color tint, float t, float duration, float size = 1f)
        {
            var dir = to - from;
            const float travel = 0.45f;
            b.Cue(Tint(b.Ps("StreamClouds", cloud, from)
                .Sphere(0.25f * size).Velocity(dir / travel).Speed(0.3f, 1.2f).Life(travel * 0.9f, travel * 1.25f)
                .Size(0.6f * size, 1.1f * size).Rate(26f).Duration(duration)
                .SizeOverLife(C(0f, 0.35f, 0.4f, 1f, 1f, 1.3f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.55f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(2), tint), t);
            b.Cue(b.Ps("StreamGrains", Mat(grains, "Spark"), from)
                .Sphere(0.35f * size).Velocity(dir / (travel * 0.85f)).Speed(0.2f, 1.5f).Life(travel * 0.8f, travel * 1.1f)
                .Size(0.05f, 0.1f).Rate(110f).Duration(duration).Gravity(0.3f).Stretch(4f, 0.05f)
                .AlphaOverLife(0f, 0.4f, 0.15f, 1f, 1f, 0f).Order(3), t);
        }

        /// <summary>Agujas (husos estirados) que vuelan de from a to; cada aguja va rodeada de aros que aparecen a su paso.</summary>
        public static void Needles(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, Pal rings, float t, float duration, float rate, float travel = 0.35f, float size = 1f)
        {
            var dir = to - from;
            b.Cue(b.Ps("Needles", Mat(p, "Spark"), from)
                .Sphere(0.3f).Velocity(dir / travel).Life(travel).Size(0.22f * size, 0.28f * size)
                .Rate(rate).Duration(duration).Stretch(5.5f, 0.02f).AlphaOverLife(0f, 0.6f, 0.1f, 1f, 1f, 1f).Order(6), t);
            var ringFx = b.Ps("NeedleRings", Mat(rings, "Ring"), from + dir * 0.5f)
                .Box(new Vector3(dir.magnitude * 0.85f, 0.5f, 0.5f)).Life(0.16f, 0.24f).Size(0.3f * size, 0.45f * size)
                .Rate(rate * 4f).Duration(duration + travel * 0.6f)
                .SizeOverLife(C(0f, 0.4f, 1f, 1.1f)).AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(5);
            // La caja de los aros sigue la recta from-to.
            ringFx.ps.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir.normalized);
            b.Cue(ringFx, t + travel * 0.3f);
        }

        // ------------------------------------------------------------ bloques: lote 3 (mandíbulas, ondas, chorros, haces, salpicaduras, bruma)

        /// <summary>
        /// Mandíbulas: dos filas de colmillos (husos) que aparecen abiertas en <paramref name="t"/> y se cierran
        /// sobre <paramref name="center"/> en <paramref name="close"/>. width = ancho de la fila, gap = media apertura inicial.
        /// </summary>
        public static void Jaws(EmeraldMoveBuilder b, Vector3 center, Pal p, float t, float close, float width, float gap, int teeth = 7, float tooth = 0.8f, float life = 0.75f)
        {
            float travel = Mathf.Max(0.05f, close - t);
            float k = Mathf.Clamp01(travel / life);
            for (int row = 0; row < 2; row++)
            {
                float sign = row == 0 ? 1f : -1f; // 0 = fila de arriba (cierra hacia abajo)
                var f = b.Ps(row == 0 ? "FangsUpper" : "FangsLower", Mat(p, "Spark"), center + new Vector3(0f, sign * (gap + tooth * 0.45f), 0f))
                    .Life(life).Size3D(new Vector3(tooth * 0.42f, tooth, 1f)).Burst(teeth).Rotation(0f, 0f)
                    .SizeOverLife(C(0f, 0.35f, 0.1f, 1f, 1f, 1f)).AlphaOverLife(0f, 1f, 0.75f, 1f, 1f, 0f).Order(7);
                // Fila recta y regular (reparto uniforme de la ráfaga sobre una arista) en el eje "derecha de pantalla".
                var sh = f.ps.shape;
                sh.enabled = true;
                sh.shapeType = ParticleSystemShapeType.SingleSidedEdge;
                sh.radius = width * 0.5f;
                sh.radiusMode = ParticleSystemShapeMultiModeValue.BurstSpread;
                sh.rotation = new Vector3(0f, 37f, 0f);
                // Se cierran a velocidad constante y se quedan clavadas.
                var vel = f.ps.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.World;
                float v = -sign * gap / travel;
                vel.x = new MinMaxCurve(1f, C(0f, 0f, 1f, 0f));
                vel.y = new MinMaxCurve(1f, Step(0f, v, k, 0f));
                vel.z = new MinMaxCurve(1f, C(0f, 0f, 1f, 0f));
                b.Cue(f, t);
            }
        }

        /// <summary>Ondas en media luna (malla orientada a cámara) que viajan de from a to y crecen (sonido, rugido, onda de choque).</summary>
        public static Fx ArcWaves(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float travel, float size0, float size1, float angle = 0f)
        {
            return b.Cue(b.Ps("ArcWaves", Mat(p, "Blade"), from)
                .Mesh(Crescent, ParticleSystemRenderSpace.View).Velocity((to - from) / travel).Life(travel).Size(size0).Rotation(angle, angle)
                .Rate(rate).Duration(duration)
                .SizeOverLife(C(0f, 1f, 1f, size1 / size0)).AlphaOverLife(0f, 0f, 0.12f, 1f, 0.7f, 0.85f, 1f, 0f).Order(5), t);
        }

        /// <summary>Aros orientados a cámara que viajan de from a to y crecen (ultrasonido, canto, pulsos de presión).</summary>
        public static Fx RingStream(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float travel, float size0, float size1)
        {
            return b.Cue(b.Ps("RingStream", Mat(p, "Ring"), from)
                .Velocity((to - from) / travel).Life(travel).Size(size0).Rate(rate).Duration(duration)
                .SizeOverLife(C(0f, 1f, 1f, size1 / size0)).AlphaOverLife(0f, 0f, 0.1f, 1f, 0.75f, 0.9f, 1f, 0f).Order(5), t);
        }

        /// <summary>
        /// Chorro continuo de partículas de from a to que se ensancha (size0 → size1): fuego, agua, ácido, nieve.
        /// stretch &gt; 0 = husos estirados en la dirección del chorro.
        /// </summary>
        public static Fx Stream(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Material mat, float t, float duration, float rate, float travel,
            float size0, float size1, float stretch = 0f, float spread = 0.12f, int order = 4)
        {
            var f = b.Ps("Stream", mat, from)
                .Sphere(spread).Velocity((to - from) / travel).Speed(0.1f, 0.6f).Life(travel * 0.9f, travel * 1.05f)
                .Size(size0).Rate(rate).Duration(duration).Rotation(0f, 360f)
                .SizeOverLife(C(0f, 1f, 1f, size1 / size0)).AlphaOverLife(0f, 0.6f, 0.08f, 1f, 0.8f, 1f, 1f, 0f).Order(order);
            if (stretch > 0f)
                f.Stretch(stretch, 0.02f);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Haz recto (línea con núcleo blanco y borde de color) que se extiende de from a to en <paramref name="grow"/> s,
        /// se mantiene y se deshace al final.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack Beam(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t0, float t1, float width, float grow = 0.12f)
        {
            var dir = to - from;
            float len = dir.magnitude;
            var pivot = b.Node("Beam", from);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.right, dir / len);
            var pts = new Vector3[8];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new Vector3(len * i / (pts.Length - 1f), 0f, 0f);
            var line = b.LinePart("BeamLine", Mat(p, "Bolt"), pts, C(0f, 0.6f, 0.08f, 1f, 0.92f, 1f, 1f, 0.75f), pivot, 6);
            line.widthMultiplier = width;
            var tr = b.Track(pivot, line, t0, t1);
            tr.scaleFrom = new Vector3(0.02f, 1f, 1f);
            tr.scaleTo = Vector3.one;
            tr.scaleCurve = C(t0, 0f, t0 + grow, 1f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0f, t1 - 0.18f, 0f, t1, 1f);
            return tr;
        }

        /// <summary>Salpicadura: gotas que saltan con gravedad, gotas gruesas y espuma (material de humo; null = EM_Mist).</summary>
        public static void Splash(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float size, int drops = 30, Material foam = null)
        {
            b.Cue(b.Ps("SplashDrops", Mat(p, "Spark"), pos)
                .Hemisphere(0.3f * size).Speed(3f * size, 7f * size).Life(0.4f, 0.8f).Size(0.1f * size, 0.2f * size)
                .Burst(drops).Gravity(1.4f).Drag(1f).Stretch(3f, 0.05f)
                .AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(5), t);
            b.Cue(b.Ps("SplashBlobs", Mat(p, "Glow"), pos)
                .Sphere(0.3f * size).Speed(1.5f * size, 4f * size).Life(0.35f, 0.6f).Size(0.25f * size, 0.5f * size)
                .Burst(Mathf.Max(4, drops / 3)).Gravity(0.8f).Drag(2f)
                .SizeOverLife(C(0f, 1f, 1f, 0.3f)).AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(4), t);
            Smoke(b, pos, foam != null ? foam : M("EM_Mist"), t + 0.03f, 8, 0.9f * size, 0.4f, 0.6f * size, 1.2f);
        }

        /// <summary>Bruma continua: bocanadas de humo toon que giran despacio alrededor de un personaje y suben.</summary>
        public static Fx Fog(EmeraldMoveBuilder b, Vector3 ground, Material mat, float t, float duration, float radius, float rate, float size, float height = 0.5f, float life = 1.6f)
        {
            var f = b.Ps("Fog", mat, ground + new Vector3(0f, height, 0f))
                .GroundCircle(radius, 0.6f).Radial(0.3f, 0.7f).Life(life * 0.8f, life * 1.2f).Size(0.9f * size, 1.6f * size)
                .Rate(rate).Duration(duration).Rotation(0f, 360f)
                .SizeOverLife(C(0f, 0.4f, 0.35f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.5f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1);
            var vel = f.ps.velocityOverLifetime;
            vel.y = new MinMaxCurve(0.35f);
            return b.Cue(f, t);
        }

        // ------------------------------------------------------------ bloques: lote 4 (conos de chorro, lenguas de fuego, cintas, pompas, drenaje)

        /// <summary>
        /// Degradado de color y alpha para ColorOverLife: rgb = cuartetos (t, r, g, b), alpha = pares (t, a).
        /// El color multiplica la paleta del material (blanco = sin cambio).
        /// </summary>
        public static Gradient Grad(float[] rgb, float[] alpha)
        {
            var ck = new GradientColorKey[rgb.Length / 4];
            for (int i = 0; i < ck.Length; i++)
                ck[i] = new GradientColorKey(new Color(rgb[i * 4 + 1], rgb[i * 4 + 2], rgb[i * 4 + 3]), rgb[i * 4]);
            var ak = new GradientAlphaKey[alpha.Length / 2];
            for (int i = 0; i < ak.Length; i++)
                ak[i] = new GradientAlphaKey(alpha[i * 2 + 1], alpha[i * 2]);
            var g = new Gradient();
            g.SetKeys(ck, ak);
            return g;
        }

        /// <summary>
        /// Chorro en cono de from a to: las partículas salen en abanico (semiángulo <paramref name="angle"/> en grados)
        /// y crecen de size0 a size1, así que la masa se ensancha hacia el objetivo (lanzallamas, ventisca, nubes de polvo).
        /// color = degradado sobre la vida (null = solo fundido). stretch &gt; 0 = husos estirados en la dirección del chorro.
        /// aspect &gt; 0 = billboards alargados (alto = ancho·aspect) girados <paramref name="screenRot"/> ± <paramref name="rotJitter"/>
        /// grados en pantalla: en la vista de combate el chorro se aleja de la cámara y los husos estirados por la velocidad
        /// se ven de punta (redondos); así las lenguas/copos conservan su forma alargada.
        /// </summary>
        public static Fx ConeSpray(EmeraldMoveBuilder b, string name, Vector3 from, Vector3 to, Material mat, float t, float duration, float rate,
            float angle, float size0, float size1, float travel, Gradient color = null, float stretch = 0f, float spin = 0f, int order = 4,
            float aspect = 0f, float screenRot = 0f, float rotJitter = 0f)
        {
            var dir = to - from;
            float speed = dir.magnitude / travel;
            var f = b.Ps(name, mat, from)
                .Speed(speed * 0.85f, speed * 1.1f).Life(travel * 0.85f, travel * 1.1f)
                .Size(size0 * 0.8f, size0 * 1.2f).Rate(rate).Duration(duration).Rotation(0f, 360f)
                .SizeOverLife(C(0f, 1f, 0.45f, 0.35f + 0.65f * size1 / size0, 1f, size1 / size0)).Order(order);
            var sh = f.ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle = angle;
            sh.radius = 0.12f;
            sh.radiusThickness = 1f;
            f.ps.transform.localRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            f.ColorOverLife(color ?? Grad(new[] { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }, new[] { 0f, 0.5f, 0.1f, 1f, 0.75f, 1f, 1f, 0f }));
            if (stretch > 0f)
                f.Stretch(stretch, 0.015f);
            if (aspect > 0f)
            {
                var m = f.ps.main;
                m.startSize3D = true;
                m.startSizeX = new MinMaxCurve(size0 * 0.8f, size0 * 1.2f);
                m.startSizeY = new MinMaxCurve(size0 * aspect * 0.8f, size0 * aspect * 1.2f);
                m.startSizeZ = new MinMaxCurve(1f);
                f.Rotation(screenRot - rotJitter, screenRot + rotJitter);
            }
            if (spin > 0f)
                f.Spin(-spin, spin);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Lenguas de fuego en cono (lanzallamas): husos de llama amarillo-blancos en la boca que pasan a naranja y
        /// acaban rojos y anchos en el objetivo, más una masa de resplandor y un borde exterior rojo más ancho.
        /// width = ancho aproximado de la masa al llegar (m).
        /// </summary>
        public static void FlameTongues(EmeraldMoveBuilder b, Vector3 from, Vector3 to, float t, float duration, float width, float rate = 90f, float travel = 0.45f)
        {
            float dist = (to - from).magnitude;
            float angle = Mathf.Atan2(width * 0.42f, dist) * Mathf.Rad2Deg;
            var flame = Grad(new[] { 0f, 1f, 1f, 1f, 0.35f, 1f, 0.85f, 0.6f, 0.7f, 1f, 0.55f, 0.35f, 1f, 0.95f, 0.32f, 0.22f },
                new[] { 0f, 0.6f, 0.08f, 1f, 0.75f, 1f, 1f, 0f });
            // Lenguas: husos horizontales en pantalla (de 2,6 de largo por 1 de ancho), algo inclinados al azar.
            ConeSpray(b, "FlameTongues", from, to, M("EX_Tongue"), t, duration, rate, angle, width * 0.08f, width * 0.36f, travel, flame, 0f, 0f, 5, 2.6f, 90f, 22f);
            // Núcleo amarillo: resplandor ancho que llena el cono (sin él se ven huecos entre lenguas).
            var core = Grad(new[] { 0f, 1f, 1f, 1f, 0.6f, 1f, 0.8f, 0.5f, 1f, 1f, 0.5f, 0.3f }, new[] { 0f, 0.7f, 0.1f, 1f, 0.7f, 0.9f, 1f, 0f });
            ConeSpray(b, "FlameCore", from, to, Mat(Fire, "Glow"), t, duration, rate * 0.45f, angle * 0.8f, width * 0.12f, width * 0.55f, travel * 1.05f, core, 0f, 0f, 4);
            // Borde exterior rojo, más ancho y más tardío.
            var rim = Grad(new[] { 0f, 1f, 0.7f, 0.5f, 1f, 1f, 0.45f, 0.35f }, new[] { 0f, 0f, 0.25f, 1f, 0.8f, 0.9f, 1f, 0f });
            ConeSpray(b, "FlameRim", from, to, Mat(Red, "Glow"), t + 0.05f, duration, rate * 0.3f, angle * 1.25f, width * 0.15f, width * 0.6f, travel * 1.1f, rim, 0f, 0f, 3);
        }

        /// <summary>
        /// Cinta ondulante (línea con núcleo blanco y borde de color) de from a to: onda senoidal de amplitud
        /// <paramref name="amp"/> en el plano perpendicular girado <paramref name="roll"/> grados; crece como Beam y se deshace al final.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack Ribbon(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t0, float t1, float width,
            float amp, float waves, float phase = 0f, float roll = 0f, float grow = 0.15f)
        {
            var dir = to - from;
            float len = dir.magnitude;
            var pivot = b.Node("Ribbon", from);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.right, dir / len) * Quaternion.Euler(roll, 0f, 0f);
            const int n = 40;
            var pts = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float u = (float)i / (n - 1);
                // La onda nace en la boca (amplitud 0) y se abre hacia el objetivo.
                float a = amp * Mathf.Sin(Mathf.PI * 0.5f * Mathf.Min(1f, u * 2.5f));
                pts[i] = new Vector3(u * len, a * Mathf.Sin(u * Mathf.PI * 2f * waves + phase), 0f);
            }
            var line = b.LinePart("RibbonLine", Mat(p, "Bolt"), pts, C(0f, 0.5f, 0.1f, 1f, 0.9f, 1f, 1f, 0.7f), pivot, 6);
            line.widthMultiplier = width;
            var tr = b.Track(pivot, line, t0, t1);
            tr.scaleFrom = new Vector3(0.02f, 1f, 1f);
            tr.scaleTo = Vector3.one;
            tr.scaleCurve = C(t0, 0f, t0 + grow, 1f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0f, t1 - 0.2f, 0f, t1, 1f);
            return tr;
        }

        /// <summary>Pompas (aros finos de borde de color con brillo) que viajan de from a to, crecen y se bambolean.</summary>
        public static Fx Bubbles(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float travel, float size0, float size1, float spread = 0.35f)
        {
            var f = b.Ps("Bubbles", Mat(p, "Bubble"), from)
                .Sphere(spread).Velocity((to - from) / travel).Speed(0.2f, 0.9f).Life(travel * 0.9f, travel * 1.1f)
                .Size(size0 * 0.7f, size0 * 1.3f).Rate(rate).Duration(duration)
                .SizeOverLife(C(0f, 0.6f, 0.2f, 1f, 1f, size1 / size0)).AlphaOverLife(0f, 0.5f, 0.1f, 1f, 0.85f, 1f, 1f, 0f).Order(5);
            return b.Cue(f, t);
        }

        /// <summary>Estallido de pompas: aros que se abren y gotitas (al llegar al objetivo).</summary>
        public static void BubblePops(EmeraldMoveBuilder b, Vector3 pos, Pal p, float[] times, float size, int drops = 10)
        {
            float t0 = times[0];
            b.Cue(Bursts(b.Ps("BubblePops", Mat(p, "Bubble"), pos)
                .Sphere(0.5f * size).Life(0.22f, 0.3f).Size(0.5f * size, 0.9f * size)
                .SizeOverLife(C(0f, 0.7f, 1f, 1.5f)).AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(6), 3, times), t0);
            if (drops > 0)
                b.Cue(Bursts(b.Ps("BubbleDrops", Mat(p, "Spark"), pos)
                    .Sphere(0.4f * size).Speed(3f * size, 6f * size).Life(0.25f, 0.45f).Size(0.06f, 0.12f)
                    .Gravity(1f).Drag(2f).Stretch(4f, 0.04f).AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(5), drops, times), t0);
        }

        /// <summary>
        /// Drenaje: motas que salen del objetivo y vuelven al usuario serpenteando (velocidad orbital alrededor del eje del chorro).
        /// </summary>
        public static Fx DrainMotes(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float travel, float size = 1f, string kind = "Glow", float spread = 0.5f)
        {
            var dir = to - from;
            var f = b.Ps("DrainMotes", Mat(p, kind), from)
                .Sphere(spread).Life(travel * 0.9f, travel * 1.1f).Size(0.12f * size, 0.26f * size)
                .Rate(rate).Duration(duration).Rotation(0f, 90f)
                .SizeOverLife(C(0f, 0.5f, 0.2f, 1f, 0.85f, 1f, 1f, 0.3f)).AlphaOverLife(0f, 0f, 0.12f, 1f, 0.85f, 1f, 1f, 0f).Order(6);
            // Avanzan hacia el usuario y la nube se cierra (radial negativo) mientras gira (orbital).
            var vel = f.ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            var local = Quaternion.Inverse(Quaternion.LookRotation(dir.normalized, Vector3.up)) * (dir / travel);
            vel.x = new MinMaxCurve(local.x);
            vel.y = new MinMaxCurve(local.y);
            vel.z = new MinMaxCurve(local.z);
            vel.orbitalZ = new MinMaxCurve(5f);
            vel.radial = new MinMaxCurve(-spread / travel * 0.8f);
            f.ps.transform.localRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            return b.Cue(f, t);
        }

        // ------------------------------------------------------------ bloques: lote 5 (vueltas de hilo, grietas, rocas, lajas, residuales)

        /// <summary>
        /// Vueltas apiladas alrededor de un personaje (hilo que lo envuelve, órbitas): <paramref name="count"/> arcos
        /// giratorios (SpinArc) repartidos en <paramref name="height"/> m, con entradas escalonadas cada <paramref name="stagger"/> s.
        /// Sin sistemas de partículas (solo mallas).
        /// </summary>
        public static void Coils(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float t1, int count, float radius, float height,
            float width = 0.35f, float stagger = 0.06f, float spin = -900f, float tilt = 8f)
        {
            for (int i = 0; i < count; i++)
            {
                float u = count == 1 ? 0.5f : (float)i / (count - 1);
                float sign = i % 2 == 0 ? 1f : -1f;
                // El centro más ancho que los extremos (capullo).
                float r = radius * (1f - 0.3f * Mathf.Abs(u - 0.5f));
                SpinArc(b, ground + new Vector3(0f, 0.2f + u * height, 0f), p, t0 + i * stagger, t1, r, width, spin * (i % 2 == 0 ? 1f : 1.2f), sign * tilt);
            }
        }

        // Punto del plano del suelo (x = a lo largo, z = de lado, en el marco del pivote) en el marco del hijo tumbado 90° en X.
        static Vector3 OnGround(float x, float side) => new Vector3(x, side, 0f);

        static Vector3[] JaggedPoints(System.Random rng, Vector3 start, float angle, float length, int n, float jitter)
        {
            var pts = new Vector3[n];
            var d = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            var side = new Vector3(-d.y, d.x, 0f);
            for (int i = 0; i < n; i++)
            {
                float u = (float)i / (n - 1);
                float j = i == 0 ? 0f : (float)(rng.NextDouble() * 2.0 - 1.0) * jitter * Mathf.Sin(Mathf.PI * Mathf.Min(1f, u * 1.6f));
                pts[i] = start + d * (u * length) + side * j;
            }
            return pts;
        }

        /// <summary>
        /// Grieta plana en el suelo (líneas quebradas tumbadas, no orientadas a cámara) que se abre de from a to en
        /// <paramref name="grow"/> s con <paramref name="branches"/> ramas, se mantiene y se cierra (erosión) en t1.
        /// Material Flat de la paleta (p. ej. casi negro). Sin sistemas de partículas.
        /// </summary>
        public static void GroundCrack(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t0, float grow, float t1, float width,
            int branches = 4, int seed = 1, float jitter = 0.3f)
        {
            var dir = to - from;
            dir.y = 0f;
            float len = dir.magnitude;
            var pivot = b.Node("Crack", new Vector3(from.x, from.y + 0.03f, from.z));
            pivot.localRotation = Quaternion.FromToRotation(Vector3.right, dir / len);
            var flat = b.Node("CrackFlat", Vector3.zero, pivot);
            flat.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var mat = Mat(p, "Flat");
            var rng = new System.Random(seed);
            var erosion = C(t0, 0f, t1 - 0.3f, 0f, t1, 1f);

            void Line(Vector3[] pts, AnimationCurve w, float mul)
            {
                var line = b.LinePart("CrackLine", mat, pts, w, flat, 1);
                line.alignment = LineAlignment.TransformZ;
                line.widthMultiplier = mul;
                line.numCapVertices = 0;
                var tr = b.Track(line.transform, line, t0, t1);
                tr.property = "_Erosion";
                tr.propertyCurve = erosion;
            }

            var main = JaggedPoints(rng, OnGround(0f, 0f), 0f, len, 16, jitter);
            Line(main, C(0f, 0.25f, 0.15f, 1f, 0.6f, 0.85f, 1f, 0.15f), width);
            for (int k = 0; k < branches; k++)
            {
                int j = 3 + rng.Next(main.Length - 6);
                float side = k % 2 == 0 ? 1f : -1f;
                float a = side * (0.5f + 0.5f * (float)rng.NextDouble());
                float bl = len * (0.12f + 0.12f * (float)rng.NextDouble());
                Line(JaggedPoints(rng, main[j], a, bl, 6, jitter * 0.5f), C(0f, 1f, 1f, 0.1f), width * 0.45f);
            }
            // Se abre a lo largo (del usuario hacia el objetivo).
            var open = b.Track(pivot, null, t0, t1);
            open.scaleFrom = new Vector3(0.02f, 1f, 1f);
            open.scaleTo = Vector3.one;
            open.scaleCurve = C(t0, 0f, t0 + grow, 1f);
        }

        /// <summary>
        /// Roca facetada (malla sólida) lanzada en parábola de from a to en <paramref name="travel"/> s, girando, con estela
        /// fina de la paleta <paramref name="trail"/> (null = sin estela). arcHeight = flecha de la parábola (m).
        /// </summary>
        public static Fx ThrownRock(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Material mat, float t, float travel, float size, float arcHeight,
            Pal? trail = null, float trailWidth = 0.3f)
        {
            var v = (to - from) / travel;
            var f = b.Ps("ThrownRock", mat, from)
                .Mesh(BotwMeshes.Rock).Life(travel).Size(size).Burst(1).Velocity(v)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-360f, 360f, true)
                .SizeOverLife(C(0f, 0.5f, 0.12f, 1f, 1f, 1f)).Order(3);
            var vel = f.ps.velocityOverLifetime;
            float up = 4f * arcHeight / travel;
            vel.x = new MinMaxCurve(1f, C(0f, v.x, 1f, v.x));
            vel.y = new MinMaxCurve(1f, C(0f, v.y + up, 1f, v.y - up));
            vel.z = new MinMaxCurve(1f, C(0f, v.z, 1f, v.z));
            if (trail.HasValue)
                Trail(f, Mat(trail.Value, "Line"), 0.3f, trailWidth);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Lajas de roca (conos sólidos) que brotan del suelo en una franja de from a to (ancho 2·<paramref name="spread"/>),
        /// crecen de golpe, se quedan y se hunden. height = alto de las lajas (m).
        /// </summary>
        public static Fx RockSpikes(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Material mat, float t, float duration, int count, float height,
            float spread = 0.5f, float life = 0.9f, float thickness = 0.55f)
        {
            var dir = to - from;
            dir.y = 0f;
            var f = b.Ps("RockSpikes", mat, (from + to) * 0.5f)
                .Mesh(Cone).Box(new Vector3(dir.magnitude, 0f, spread * 2f)).Life(life * 0.85f, life * 1.1f)
                .Rotation3D(new Vector3(-22f, 0f, -22f), new Vector3(22f, 360f, 22f))
                .Rate(count / Mathf.Max(0.05f, duration)).Duration(duration)
                .SizeOverLife(C(0f, 0f, 0.12f, 1.08f, 0.22f, 1f, 0.75f, 1f, 1f, 0f)).Order(2);
            var m = f.ps.main;
            m.startSize3D = true;
            m.startSizeX = new MinMaxCurve(thickness * 0.7f, thickness * 1.2f);
            m.startSizeY = new MinMaxCurve(height * 0.6f, height * 1.1f);
            m.startSizeZ = new MinMaxCurve(thickness * 0.7f, thickness * 1.2f);
            f.ps.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir.normalized);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Imágenes residuales: cascos de contorno (Energy Sphere) del tamaño del atrezo que quedan atrás, uno cada
        /// <paramref name="interval"/> s desde t0, desplazados <paramref name="step"/> cada uno, y se desvanecen en <paramref name="life"/> s.
        /// Sin sistemas de partículas.
        /// </summary>
        public static void Afterimages(EmeraldMoveBuilder b, Vector3 ground, Vector3 step, Pal p, float t0, int count, float interval, float life, float scale = 1f)
        {
            var baseScale = new Vector3(0.6f, 0.98f, 0.6f) * scale;
            for (int i = 0; i < count; i++)
            {
                float s = t0 + i * interval;
                var r = b.MeshPart("Afterimage", BotwMeshes.Sphere, Mat(p, "Shell"), ground + step * (i + 1) + new Vector3(0f, 0.82f * scale, 0f),
                    Vector3.zero, baseScale, order: 1);
                var tr = b.Track(r.transform, r, s, s + life);
                tr.scaleFrom = baseScale;
                tr.scaleTo = Vector3.Scale(baseScale, new Vector3(1.25f, 1.02f, 1.25f));
                tr.scaleCurve = C(s, 0f, s + life, 1f);
                tr.property = "_Dissolve";
                tr.propertyCurve = C(s, 0f, s + life * 0.35f, 0.15f, s + life, 1f);
            }
        }

        // ------------------------------------------------------------ bloques: lote 6 (paneles, túneles de aros, órbitas, convergencia, dedo)

        /// <summary>
        /// Panel translúcido plano (malla Quad, no orientado a cámara) de <paramref name="size"/> m centrado en <paramref name="center"/>
        /// y girado <paramref name="yaw"/> grados en Y (90 = perpendicular a la recta usuario-rival; 60 mira más a la cámara de combate).
        /// Se abre como una persiana en <paramref name="grow"/> s, se mantiene y se deshace en t1. fill = false deja solo el marco;
        /// facets = superpone facetas brillantes (líneas de Voronoi) que aparecen tras abrirse. Sin sistemas de partículas.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack Panel(EmeraldMoveBuilder b, Vector3 center, Vector2 size, float yaw, Pal p, float t0, float t1,
            float grow = 0.25f, bool fill = true, bool facets = true)
        {
            var euler = new Vector3(0f, yaw, 0f);
            var scale = new Vector3(size.x, size.y, 1f);
            var r = b.MeshPart("Panel", Quad, Mat(p, fill ? "Pane" : "Frame"), center, euler, scale, order: 2);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = new Vector3(size.x * 0.04f, size.y * 0.7f, 1f);
            tr.scaleTo = scale;
            tr.scaleCurve = C(t0, 0f, t0 + grow, 1.04f, t0 + grow + 0.1f, 1f, t1, 1f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0.3f, t0 + grow, 0f, t1 - 0.35f, 0f, t1, 1f);
            if (facets)
            {
                var f = b.MeshPart("PanelFacets", Quad, Mat(p, "Facets"), center, euler, scale, order: 3);
                var ft = b.Track(f.transform, f, t0, t1);
                ft.scaleFrom = tr.scaleFrom;
                ft.scaleTo = scale;
                ft.scaleCurve = tr.scaleCurve;
                ft.property = "_Erosion";
                // Suelo de erosión 0,45: quedan las facetas grandes y desaparece la red fina (más tenue) de T_Caustics.
                ft.propertyCurve = C(t0, 1f, t0 + grow, 0.95f, t0 + grow + 0.25f, 0.45f, t1 - 0.35f, 0.45f, t1, 1f);
            }
            return tr;
        }

        /// <summary>
        /// Túnel de aros finos (malla ThinRing perpendicular a la recta from-to) que viajan de from a to y crecen de size0 a size1
        /// (radio en m): ondas sonoras estrechas y rápidas, chirridos.
        /// </summary>
        public static Fx RingTunnel(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float t, float duration, float rate, float travel,
            float size0, float size1)
        {
            var dir = to - from;
            var f = b.Ps("RingTunnel", Mat(p, "Shock"), from)
                .Mesh(ThinRing, ParticleSystemRenderSpace.Local).Velocity(dir / travel).Life(travel).Size(size0).Rate(rate).Duration(duration)
                .SizeOverLife(C(0f, 1f, 1f, size1 / size0)).AlphaOverLife(0f, 0f, 0.1f, 1f, 0.7f, 0.9f, 1f, 0f).Order(5);
            // El aro (plano XZ de la malla) queda perpendicular al chorro.
            f.ps.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Motas que giran alrededor de <paramref name="center"/> en un círculo de <paramref name="radius"/> m algo inclinado
        /// (orbital = velocidad orbital), aparecen y se apagan en <paramref name="life"/> s. colors = color inicial aleatorio entre
        /// ellos (multiplica la paleta; con White se ve el color tal cual).
        /// </summary>
        public static Fx Orbit(EmeraldMoveBuilder b, Vector3 center, Pal p, float t, float life, int count, float radius, float orbital, float size,
            string kind = "Glow", float tilt = 12f, params Color[] colors)
        {
            var f = b.Ps("Orbit", Mat(p, kind), center)
                .GroundCircle(radius, 0f).Radial(0f, orbital).Life(life * 0.9f, life).Size(size * 0.7f, size * 1.2f).Burst(count).Rotation(0f, 90f)
                .SizeOverLife(C(0f, 0.2f, 0.12f, 1f, 0.85f, 1f, 1f, 0.2f)).AlphaOverLife(0f, 0f, 0.1f, 1f, 0.85f, 1f, 1f, 0f).Order(6);
            f.ps.transform.localRotation = Quaternion.Euler(tilt, 0f, tilt * 0.6f);
            if (colors != null && colors.Length > 0)
            {
                var g = new Gradient();
                var keys = new GradientColorKey[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                    keys[i] = new GradientColorKey(colors[i], colors.Length == 1 ? 0f : (float)i / (colors.Length - 1));
                g.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
                g.mode = GradientMode.Fixed;
                var main = f.ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
            }
            return b.Cue(f, t);
        }

        /// <summary>
        /// Convergencia: motas que nacen en una esfera de <paramref name="radius"/> m y llegan a <paramref name="pos"/> al final de su
        /// vida (curación, concentración, absorción).
        /// </summary>
        public static Fx Gather(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t, float duration, float radius, float rate, float size = 1f,
            string kind = "Glow", float life = 0.55f)
        {
            return b.Cue(b.Ps("Gather", Mat(p, kind), pos)
                .Sphere(radius, 0f).Radial(-radius / life * 0.95f).Life(life).Size(0.14f * size, 0.3f * size).Rate(rate).Duration(duration)
                .Rotation(0f, 90f).SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 0.3f)).AlphaOverLife(0f, 0f, 0.2f, 1f, 0.85f, 1f, 1f, 0f).Order(5), t);
        }

        /// <summary>
        /// Mano con el índice levantado (malla Finger, material Flat de la paleta) que se balancea ±<paramref name="amplitude"/> grados
        /// alrededor de la muñeca (<paramref name="wrist"/>) con periodo <paramref name="period"/> s entre t0 y t1. yaw = giro en Y
        /// del plano de la mano (37 ≈ de cara a la cámara de combate). Sin sistemas de partículas.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack WagFinger(EmeraldMoveBuilder b, Vector3 wrist, Pal p, float t0, float t1, float size,
            float yaw = 37f, float amplitude = 25f, float period = 0.5f)
        {
            var pivot = b.Node("FingerPivot", wrist);
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var r = b.MeshPart("Finger", Finger, Mat(p, "Flat"), Vector3.zero, Vector3.zero, Vector3.one * size, pivot, 7);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = Vector3.one * size * 0.2f;
            tr.scaleTo = Vector3.one * size;
            tr.scaleCurve = C(t0, 0f, t0 + 0.08f, 1.08f, t0 + 0.14f, 1f, t1, 1f);
            tr.eulerFrom = new Vector3(0f, 0f, -amplitude);
            tr.eulerTo = new Vector3(0f, 0f, amplitude);
            var tv = new List<float>();
            for (float t = t0; t <= t1 + 1e-4f; t += period / 8f)
            {
                tv.Add(t);
                tv.Add(0.5f + 0.5f * Mathf.Sin((t - t0) * 2f * Mathf.PI / period));
            }
            tr.rotCurve = C(tv.ToArray());
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0f, t1 - 0.2f, 0f, t1, 1f);
            return tr;
        }

        // ------------------------------------------------------------ bloques: lote 7 (objetos lanzados, estrellas, ráfagas de púas,
        // interrogantes, espirales que aprietan, objetos que golpean en arco, huevo que se abre, cuchara que se dobla)

        /// <summary>
        /// Objeto sólido (malla cualquiera, tamaño 3D) lanzado en parábola de from a to en <paramref name="travel"/> s, dando vueltas
        /// (<paramref name="spin"/> grados/s como mucho en cada eje). Igual que ThrownRock pero con malla y proporciones propias
        /// (huevos, bolas). Para varios lanzamientos por el mismo camino: Bursts(f, 1, tiempos...).
        /// </summary>
        public static Fx Lob(EmeraldMoveBuilder b, Mesh mesh, Material mat, Vector3 from, Vector3 to, float t, float travel, Vector3 size,
            float arcHeight, float spin = 360f, Pal? trail = null, float trailWidth = 0.3f)
        {
            var v = (to - from) / travel;
            var f = b.Ps("Lob", mat, from)
                .Mesh(mesh).Life(travel).Size3D(size).Burst(1).Velocity(v)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-spin, spin, true)
                .SizeOverLife(C(0f, 0.5f, 0.12f, 1f, 1f, 1f)).Order(3);
            var vel = f.ps.velocityOverLifetime;
            float up = 4f * arcHeight / travel;
            vel.x = new MinMaxCurve(1f, C(0f, v.x, 1f, v.x));
            vel.y = new MinMaxCurve(1f, C(0f, v.y + up, 1f, v.y - up));
            vel.z = new MinMaxCurve(1f, C(0f, v.z, 1f, v.z));
            if (trail.HasValue)
                Trail(f, Mat(trail.Value, "Line"), 0.3f, trailWidth);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Chorro de estrellas (malla orientada a cámara, p. ej. Star5) de from a to: giran, se reparten en una esfera de
        /// <paramref name="spread"/> m y dejan una estela fina de la paleta <paramref name="trail"/> (trailWidth 0 = sin estela).
        /// </summary>
        public static Fx StarStream(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Mesh mesh, Material mat, Pal trail, float t, float duration,
            float rate, float travel, float size, float spread = 0.5f, float trailWidth = 0.25f)
        {
            var f = b.Ps("StarStream", mat, from)
                .Mesh(mesh, ParticleSystemRenderSpace.View).Sphere(spread).Velocity((to - from) / travel).Speed(0.2f, 1.0f)
                .Life(travel * 0.9f, travel * 1.1f).Size(size * 0.7f, size * 1.2f).Rate(rate).Duration(duration).Rotation(0f, 72f).Spin(-240f, 240f)
                .SizeOverLife(C(0f, 0.4f, 0.15f, 1f, 0.85f, 1f, 1f, 0.6f)).AlphaOverLife(0f, 1f, 0.85f, 1f, 1f, 0f).Order(6);
            if (trailWidth > 0f)
                Trail(f, Mat(trail, "Line"), 0.35f, trailWidth);
            return b.Cue(f, t);
        }

        /// <summary>
        /// Andanadas de púas (husos estirados en la dirección de vuelo, con estela corta): <paramref name="count"/> por instante de
        /// <paramref name="times"/>, de from a to en <paramref name="travel"/> s.
        /// </summary>
        public static Fx Volley(EmeraldMoveBuilder b, Vector3 from, Vector3 to, Pal p, float[] times, int count, float travel, float size = 1f,
            float spread = 0.25f, float trailWidth = 0.12f)
        {
            var f = b.Ps("Volley", Mat(p, "Spark"), from)
                .Sphere(spread).Velocity((to - from) / travel).Life(travel).Size(0.16f * size, 0.22f * size)
                .Stretch(4.5f, 0.02f).AlphaOverLife(0f, 1f, 0.9f, 1f, 1f, 0f).Order(6);
            if (trailWidth > 0f)
                Trail(f, Mat(p, "Line"), 0.12f, trailWidth);
            return b.Cue(Bursts(f, count, times), times[0]);
        }

        /// <summary>
        /// Signo de interrogación trazado con líneas (gancho + punto) de <paramref name="size"/> m de alto, de cara a la cámara de
        /// combate (yaw 37), con la base en <paramref name="pos"/>: aparece con un rebote en t0, flota y se deshace en t1.
        /// tilt = inclinación en pantalla (grados). Sin sistemas de partículas.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack QuestionMark(EmeraldMoveBuilder b, Vector3 pos, Pal p, float t0, float t1, float size, float tilt = 0f,
            float width = 0.11f)
        {
            var pivot = b.Node("Question", pos);
            pivot.localRotation = Quaternion.Euler(0f, 37f, 0f);
            var glyph = b.Node("QuestionGlyph", Vector3.zero, pivot);
            glyph.localRotation = Quaternion.Euler(0f, 0f, -tilt);
            var pts = new List<Vector3>();
            var c = new Vector2(0f, 0.7f);
            const float r = 0.23f;
            for (int i = 0; i <= 14; i++)
            {
                // De la izquierda (155°) por arriba hasta abajo (-90°), sentido horario.
                float a = Mathf.Lerp(155f, -90f, i / 14f) * Mathf.Deg2Rad;
                pts.Add(new Vector3(c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r, 0f));
            }
            pts.Add(new Vector3(0f, 0.36f, 0f));
            pts.Add(new Vector3(0f, 0.3f, 0f));
            var mat = Mat(p, "Bolt");
            var erosion = C(t0, 0f, t1 - 0.25f, 0f, t1, 1f);
            var hook = b.LinePart("QuestionHook", mat, pts.ToArray(), C(0f, 0.8f, 0.2f, 1f, 1f, 1f), glyph, 7);
            hook.widthMultiplier = width;
            hook.numCapVertices = 4;
            hook.numCornerVertices = 3;
            var ht = b.Track(hook.transform, hook, t0, t1);
            ht.property = "_Erosion";
            ht.propertyCurve = erosion;
            var dot = b.LinePart("QuestionDot", mat, new[] { new Vector3(0f, 0.1f, 0f), new Vector3(0f, 0.12f, 0f) }, C(0f, 1f, 1f, 1f), glyph, 7);
            dot.widthMultiplier = width * 1.35f;
            dot.numCapVertices = 6;
            var dt = b.Track(dot.transform, dot, t0, t1);
            dt.property = "_Erosion";
            dt.propertyCurve = erosion;
            // Aparece con un rebote y sube un poco mientras flota.
            var tr = b.Track(pivot, null, t0, t1);
            tr.scaleFrom = Vector3.one * size * 0.2f;
            tr.scaleTo = Vector3.one * size;
            tr.scaleCurve = C(t0, 0f, t0 + 0.1f, 1.15f, t0 + 0.18f, 0.95f, t0 + 0.26f, 1f, t1, 1f);
            tr.posFrom = pos;
            tr.posTo = pos + new Vector3(0f, 0.25f, 0f);
            tr.posCurve = C(t0, 0f, t1, 1f);
            return tr;
        }

        /// <summary>
        /// Espiral sólida fina (malla HelixThin, 4 vueltas) alrededor de <paramref name="ground"/> que se cierra de <paramref name="radius0"/> a
        /// <paramref name="radius1"/> m entre t0 y tSqueeze mientras gira, aprieta con un latido y se deshace en t1 (zarcillos que
        /// envuelven y comprimen). Sin sistemas de partículas.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack HelixWrap(EmeraldMoveBuilder b, Vector3 ground, Pal p, float t0, float tSqueeze, float t1,
            float radius0, float radius1, float height, float spin = 300f, float phase = 0f)
        {
            var center = ground + new Vector3(0f, height * 0.5f + 0.05f, 0f);
            var to = new Vector3(radius1, height, radius1);
            var r = b.MeshPart("HelixWrap", HelixThin, Mat(p, "Solid"), center, new Vector3(0f, phase, 0f), to, order: 4);
            var tr = b.Track(r.transform, r, t0, t1);
            tr.scaleFrom = new Vector3(radius0, height * 1.25f, radius0);
            tr.scaleTo = to;
            tr.scaleCurve = C(t0, 0f, tSqueeze, 1f, tSqueeze + 0.08f, 1.08f, tSqueeze + 0.18f, 0.98f, tSqueeze + 0.3f, 1f, t1, 1f);
            tr.spin = new Vector3(0f, spin, 0f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(t0, 0.6f, t0 + 0.12f, 0f, t1 - 0.25f, 0f, t1, 1f);
            return tr;
        }

        /// <summary>
        /// Objeto (malla a lo largo de +Y desde el pivote, p. ej. Bone) que golpea en arco: el pivote (empuñadura) está en
        /// <paramref name="grip"/>, de cara a la cámara de combate; gira en pantalla de <paramref name="angleFrom"/> a
        /// <paramref name="angleTo"/> grados (positivo = punta hacia la izquierda) acelerando hasta <paramref name="hit"/>,
        /// rebota y se deshace en t1. Sin sistemas de partículas.
        /// </summary>
        public static EmeraldMoveVfx.PartTrack SwingProp(EmeraldMoveBuilder b, Mesh mesh, Material mat, Vector3 grip, Vector3 scale, float t0, float hit,
            float t1, float angleFrom, float angleTo)
        {
            var pivot = b.Node("SwingPivot", grip);
            pivot.localRotation = Quaternion.Euler(0f, 37f, 0f);
            var swing = b.Node("Swing", Vector3.zero, pivot);
            var r = b.MeshPart("SwingProp", mesh, mat, Vector3.zero, Vector3.zero, scale, swing, 6);
            var tr = b.Track(swing, r, t0, t1);
            tr.eulerFrom = new Vector3(0f, 0f, angleFrom);
            tr.eulerTo = new Vector3(0f, 0f, angleTo);
            // Se echa atrás, baja de golpe (ease-in) hasta el impacto y rebota un poco.
            tr.rotCurve = C(t0, 0f, t0 + (hit - t0) * 0.55f, -0.08f, hit - 0.06f, 0.55f, hit, 1f, hit + 0.08f, 0.94f, hit + 0.18f, 0.98f, t1, 0.98f);
            tr.scaleFrom = scale * 0.2f;
            tr.scaleTo = scale;
            tr.scaleCurve = C(t0, 0f, t0 + 0.1f, 1.08f, t0 + 0.16f, 1f, t1 - 0.12f, 1f, t1, 0.2f);
            return tr;
        }

        /// <summary>
        /// Huevo de dos medias cáscaras (malla EggShell, material sólido) en <paramref name="center"/>: aparece con un rebote en t0,
        /// tiembla y en <paramref name="crack"/> la mitad de arriba salta inclinándose y la de abajo cae un poco; se deshacen en t1.
        /// size = escala (1 = 1 m de ancho y 1,3 m de alto). Sin sistemas de partículas.
        /// </summary>
        public static void EggCrack(EmeraldMoveBuilder b, Vector3 center, Material mat, float t0, float crack, float t1, float size = 1f)
        {
            var one = Vector3.one * size;
            var erosion = C(t0, 0f, t1 - 0.3f, 0f, t1, 1f);
            for (int half = 0; half < 2; half++)
            {
                bool top = half == 1;
                var euler = top ? new Vector3(180f, 15f, 0f) : Vector3.zero;
                var r = b.MeshPart(top ? "EggTop" : "EggBottom", EggShell, mat, center, euler, one, order: 3);
                var tr = b.Track(r.transform, r, t0, t1);
                tr.scaleFrom = one * 0.2f;
                tr.scaleTo = one;
                tr.scaleCurve = C(t0, 0f, t0 + 0.12f, 1.1f, t0 + 0.2f, 0.96f, t0 + 0.28f, 1f, t1, 1f);
                tr.posFrom = center;
                tr.posTo = center + (top ? new Vector3(-0.35f, 0.9f, -0.35f) : new Vector3(0.15f, -0.25f, 0.1f)) * size;
                tr.posCurve = C(t0, 0f, crack, 0f, crack + 0.25f, 0.85f, t1, 1f);
                tr.eulerFrom = euler;
                tr.eulerTo = euler + (top ? new Vector3(0f, 0f, 35f) : new Vector3(0f, 0f, -12f));
                // Tiembla antes de abrirse.
                tr.rotCurve = C(t0, 0f, crack - 0.3f, 0f, crack - 0.24f, 0.15f, crack - 0.18f, -0.15f, crack - 0.12f, 0.15f, crack - 0.06f, -0.1f,
                    crack, 0f, crack + 0.3f, 1f, t1, 1f);
                tr.property = "_Erosion";
                tr.propertyCurve = erosion;
            }
        }

        /// <summary>
        /// Cuchara plana (mallas SpoonHandle y SpoonHead, material Flat de la paleta) de cara a la cámara de combate con el punto de
        /// doblado en <paramref name="bend"/>: aparece en t0, la cabeza se dobla <paramref name="angle"/> grados entre tBend0 y
        /// tBend1 temblando, y se deshace en t1. Sin sistemas de partículas.
        /// </summary>
        public static void BendSpoon(EmeraldMoveBuilder b, Vector3 bend, Pal p, float t0, float tBend0, float tBend1, float t1, float size, float angle = -60f)
        {
            var pivot = b.Node("SpoonPivot", bend);
            pivot.localRotation = Quaternion.Euler(0f, 37f, 0f);
            var mat = Mat(p, "Flat");
            var erosion = C(t0, 0f, t1 - 0.25f, 0f, t1, 1f);
            var one = Vector3.one * size;
            var handle = b.MeshPart("SpoonHandle", SpoonHandle, mat, Vector3.zero, Vector3.zero, one, pivot, 6);
            var ht = b.Track(handle.transform, handle, t0, t1);
            ht.property = "_Erosion";
            ht.propertyCurve = erosion;
            var head = b.MeshPart("SpoonHead", SpoonHead, mat, Vector3.zero, Vector3.zero, one, pivot, 6);
            var tr = b.Track(head.transform, head, t0, t1);
            tr.eulerFrom = Vector3.zero;
            tr.eulerTo = new Vector3(0f, 0f, angle);
            tr.rotCurve = C(t0, 0f, tBend0, 0f, tBend0 + (tBend1 - tBend0) * 0.3f, 0.2f, tBend0 + (tBend1 - tBend0) * 0.45f, 0.12f,
                tBend0 + (tBend1 - tBend0) * 0.7f, 0.6f, tBend1, 1f, t1, 1f);
            tr.property = "_Erosion";
            tr.propertyCurve = erosion;
            // Toda la cuchara aparece con un rebote y vibra.
            var pt = b.Track(pivot, null, t0, t1);
            pt.scaleFrom = Vector3.one * 0.2f;
            pt.scaleTo = Vector3.one;
            pt.scaleCurve = C(t0, 0f, t0 + 0.1f, 1.1f, t0 + 0.18f, 1f, t1, 1f);
            pt.eulerFrom = new Vector3(0f, 37f, -4f);
            pt.eulerTo = new Vector3(0f, 37f, 4f);
            pt.rotCurve = Flicker(tBend0, tBend1 + 0.2f, 0.04f, 0, 2, 1f, 0.5f);
        }
    }
}
