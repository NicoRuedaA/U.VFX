using System.IO;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Crea (o actualiza) todos los materiales en Assets/BotwVFX/Materials.
    ///
    /// Sobre el color: los valores HDR están en espacio lineal. Sin tonemapping,
    /// cada canal se recorta a 1 en pantalla, así que un color como (2.2, 0.3, 0.03)
    /// se ve naranja saturado y lo que pasa de 1 alimenta al bloom. Es la forma más
    /// fácil de conseguir el look "plano pero brillante" de BotW: núcleo casi blanco,
    /// borde de color puro y halo de bloom del mismo tono.
    /// </summary>
    public static class BotwMaterials
    {
        public const string Folder = "Assets/BotwVFX/Materials";

        const string ToonShader = "BotwVFX/Toon Particle";
        const string SmokeShader = "BotwVFX/Toon Smoke";
        const string SphereShader = "BotwVFX/Energy Sphere";
        const string LitShader = "BotwVFX/Environment/Toon Lit";
        const string SkyShader = "BotwVFX/Environment/Gradient Sky";

        // ---------------- Paleta (lineal / HDR)
        // Azul Sheikah: runas, bomba remota, flecha ancestral.
        static readonly Color SheikahCore = new Color(0.9f, 2.0f, 2.6f);
        static readonly Color SheikahEdge = new Color(0.02f, 0.3f, 1.8f);
        // Fuego toon.
        static readonly Color FireCore = new Color(2f, 1.25f, 0.3f);
        static readonly Color FireEdge = new Color(2.2f, 0.25f, 0.02f);
        // Guardián: núcleo rosa casi blanco, borde magenta.
        static readonly Color GuardianCore = new Color(2.4f, 1.1f, 2.0f);
        static readonly Color GuardianEdge = new Color(2f, 0.1f, 0.9f);

        public static Material Get(string name) => AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat");

        public static void CreateAll()
        {
            Directory.CreateDirectory(Folder);
            var noise = BotwTextures.Load("T_Noise");
            var caustics = BotwTextures.Load("T_Caustics");
            var circle = BotwTextures.Load("T_SoftCircle");
            var ray = BotwTextures.Load("T_Ray");
            var star = BotwTextures.Load("T_Star");
            var ring = BotwTextures.Load("T_Ring");
            var gradient = BotwTextures.Load("T_Gradient");
            var puff = BotwTextures.Load("T_SmokePuff");
            var sheikah = BotwTextures.Load("T_SheikahRing");

            // ---------------- Entorno
            Sky("MAT_Sky", new Color(0.33f, 0.6f, 0.93f), new Color(0.84f, 0.93f, 0.98f), new Color(0.62f, 0.68f, 0.62f));
            Lit("MAT_Ground", new Color(0.46f, 0.66f, 0.3f));
            Lit("MAT_Dirt", new Color(0.62f, 0.55f, 0.42f));
            Lit("MAT_Rock", new Color(0.6f, 0.58f, 0.54f));
            Lit("MAT_Bark", new Color(0.45f, 0.33f, 0.24f));
            Lit("MAT_Leaves", new Color(0.3f, 0.55f, 0.26f));
            Lit("MAT_GuardianShell", new Color(0.62f, 0.56f, 0.46f));
            Lit("MAT_GuardianLeg", new Color(0.35f, 0.3f, 0.26f));
            Lit("MAT_SheikahStone", new Color(0.2f, 0.25f, 0.33f));

            // ---------------- Bomba remota (azul Sheikah)
            var sphere = Mat("RB_Sphere", SphereShader);
            Hdr(sphere, "_BaseColor", new Color(0.03f, 0.3f, 1.2f, 0.42f));
            Hdr(sphere, "_EmissiveColor", new Color(0.7f, 1.8f, 2.6f, 1f));
            sphere.SetFloat("_FresnelPower", 1.6f);
            sphere.SetFloat("_IntersectionDistance", 0.9f);
            sphere.SetFloat("_IntersectionPower", 1.5f);
            sphere.SetFloat("_NoiseScale", 2.2f);
            sphere.SetFloat("_NoiseSpeed", 2.5f);
            sphere.SetFloat("_NoiseAmount", 0.8f);
            sphere.SetFloat("_Threshold", 0.45f);
            sphere.SetFloat("_Wobble", 0.06f);

            var core = Mat("RB_Core", SphereShader);
            Hdr(core, "_BaseColor", new Color(2f, 2.5f, 3f, 0.95f));
            Hdr(core, "_EmissiveColor", new Color(2f, 2.5f, 3f, 1f));
            core.SetFloat("_NoiseAmount", 0f);
            core.SetFloat("_Threshold", 1.5f);
            core.SetFloat("_Wobble", 0f);

            Toon("RB_Ray", ray, SheikahCore, SheikahEdge, 0.3f, noise, 0.15f);
            var rbShock = Toon("RB_Shock", null, SheikahCore, SheikahEdge, 0.3f, noise, 0.6f,
                tiling: new Vector2(6f, 1f), scroll: new Vector2(0.25f, 0f));
            rbShock.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            Toon("RB_Glint", star, SheikahCore, SheikahEdge, 0.25f);
            var bombGlow = Toon("RB_BombGlow", null, new Color(0.3f, 1.4f, 2.4f), new Color(0.3f, 1.4f, 2.4f), 0f);
            bombGlow.SetFloat("_AlphaErosion", 0f);
            Smoke("RB_Dust", puff, new Color(0.82f, 0.9f, 0.98f), new Color(0.5f, 0.62f, 0.82f), fireErosion: 1f, cutout: 0.5f);

            // ---------------- Explosión (fuego toon)
            var exFlash = Toon("EX_Flash", star, new Color(2.6f, 2.3f, 1.4f), FireEdge, 0.25f);
            exFlash.SetFloat("_CameraOffset", 1f);
            Toon("EX_FlashCircle", circle, FireCore, FireEdge, 0.25f, noise, 0.3f);
            var ball = Toon("EX_Ball", gradient, FireCore, FireEdge, 0.4f, noise, 0.45f,
                tiling: new Vector2(2f, 2f), scroll: new Vector2(0.2f, 0.6f));
            ball.SetFloat("_Cull", 0f);
            var shock = Toon("EX_Shock", ring, new Color(1.6f, 1.4f, 1.1f), new Color(1.2f, 0.6f, 0.25f), 0.25f, noise, 0.5f, tiling: new Vector2(3f, 3f));
            shock.SetFloat("_Erosion", 0.42f);
            var groundShock = Toon("EX_GroundShock", null, new Color(1.1f, 0.85f, 0.55f), new Color(0.55f, 0.32f, 0.16f), 0.35f, noise, 0.55f,
                tiling: new Vector2(6f, 1f), scroll: new Vector2(0.2f, 0f));
            groundShock.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            var debris = Toon("EX_Debris", null, Lin(0.36f, 0.3f, 0.26f), Lin(0.36f, 0.3f, 0.26f), 0f);
            debris.SetFloat("_AlphaErosion", 0f);
            debris.SetFloat("_ShadeAmount", 0.5f);
            debris.SetFloat("_ZWrite", 1f);
            Toon("EX_Ember", ray, FireCore, FireEdge, 0.35f);
            Toon("EX_Tongue", ray, FireCore, FireEdge, 0.35f, noise, 0.3f);
            Smoke("EX_Smoke", puff, new Color(0.56f, 0.52f, 0.5f), new Color(0.25f, 0.22f, 0.24f), 0f, 0.45f);
            Smoke("EX_DarkSmoke", puff, new Color(0.44f, 0.42f, 0.42f), new Color(0.19f, 0.18f, 0.2f), 1f, 0.5f);

            // ---------------- Rayo Guardián (rosa/magenta + tiras azuladas)
            var laser = Toon("GB_Laser", null, new Color(3f, 0.35f, 0.9f), new Color(1.5f, 0.03f, 0.2f), 0.35f);
            laser.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            laser.SetFloat("_AlphaErosion", 0f);

            var beam = Mat("GB_Beam", SphereShader);
            Hdr(beam, "_BaseColor", new Color(2.4f, 1.6f, 2.2f, 0.97f));
            Hdr(beam, "_EmissiveColor", new Color(2f, 0.15f, 1f, 1f));
            beam.SetFloat("_FresnelPower", 1.3f);
            beam.SetFloat("_Threshold", 0.5f);
            beam.SetFloat("_NoiseScale", 1.5f);
            beam.SetFloat("_NoiseSpeed", 6f);
            beam.SetFloat("_NoiseAmount", 0.6f);
            beam.SetFloat("_IntersectionDistance", 0.5f);
            beam.SetFloat("_Wobble", 0.02f);

            var beamGlow = Mat("GB_BeamGlow", SphereShader);
            Hdr(beamGlow, "_BaseColor", new Color(1.2f, 0.05f, 0.6f, 0.3f));
            Hdr(beamGlow, "_EmissiveColor", new Color(2f, 0.2f, 1.2f, 0.8f));
            beamGlow.SetFloat("_FresnelPower", 2f);
            beamGlow.SetFloat("_Threshold", 0.6f);
            beamGlow.SetFloat("_NoiseScale", 1.2f);
            beamGlow.SetFloat("_NoiseSpeed", 5f);
            beamGlow.SetFloat("_IntersectionDistance", 0.5f);
            beamGlow.SetFloat("_Wobble", 0.03f);

            var stripe = Toon("GB_Stripe", null, SheikahCore, new Color(0.3f, 0.6f, 2.2f), 0.3f, noise, 0.5f,
                tiling: new Vector2(4f, 1f), scroll: new Vector2(-3f, 0f));
            stripe.SetVector("_BorderFade", new Vector4(0.25f, 0.5f, 0f, 0f));
            var flare = Toon("GB_Flare", star, GuardianCore, GuardianEdge, 0.25f, caustics, 0.35f,
                tiling: new Vector2(2f, 2f), scroll: new Vector2(0.4f, 0.3f));
            flare.SetFloat("_CameraOffset", 1.5f);
            var orb = Toon("GB_Orb", circle, GuardianCore, GuardianEdge, 0.3f, noise, 0.5f,
                tiling: new Vector2(2f, 2f), scroll: new Vector2(0f, 1.2f));
            orb.SetFloat("_CameraOffset", 0.8f);
            Toon("GB_Spark", ray, GuardianCore, GuardianEdge, 0.35f);
            Toon("GB_Ring", ring, GuardianCore, GuardianEdge, 0.3f, noise, 0.4f, tiling: new Vector2(3f, 3f));
            Toon("GB_Dot", star, new Color(3f, 0.35f, 0.9f), new Color(1.5f, 0.03f, 0.2f), 0.3f);
            var eyeLens = Toon("GB_EyeLens", null, new Color(2.2f, 0.4f, 1.3f), new Color(2.2f, 0.4f, 1.3f), 0f);
            eyeLens.SetFloat("_AlphaErosion", 0f);

            // ---------------- Flecha ancestral (azul Sheikah)
            var arrow = Toon("AA_Arrow", null, new Color(1.6f, 2.4f, 3f), new Color(1.6f, 2.4f, 3f), 0f);
            arrow.SetFloat("_AlphaErosion", 0f);
            Toon("AA_Trail", null, SheikahCore, SheikahEdge, 0.35f).SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            Toon("AA_Flash", star, new Color(1.8f, 2.4f, 3f), SheikahEdge, 0.25f).SetFloat("_CameraOffset", 1f);
            Toon("AA_Spark", ray, SheikahCore, SheikahEdge, 0.35f);
            Toon("AA_Lens", sheikah, new Color(0.5f, 1.5f, 2.6f), SheikahEdge, 0.2f, noise, 0.3f, tiling: new Vector2(3f, 3f));
            var aaCaustics = Toon("AA_Caustics", circle, new Color(0.25f, 0.9f, 1.8f), new Color(0.02f, 0.2f, 1f), 0.25f, caustics, 1f,
                tiling: new Vector2(1.5f, 1.5f), scroll: new Vector2(0.1f, -0.15f), additive: true);
            aaCaustics.SetFloat("_Erosion", 0.2f);
            var portal = Toon("AA_Portal", circle, new Color(0.005f, 0.02f, 0.1f, 1f), new Color(0.3f, 1.5f, 2.6f), 0.22f, noise, 0.7f,
                tiling: new Vector2(3f, 1.5f), scroll: new Vector2(0.15f, 1.2f));
            Polar(portal, 0.8f);
            var portalRim = Toon("AA_PortalRim", ring, SheikahCore, SheikahEdge, 0.3f, noise, 0.45f,
                tiling: new Vector2(4f, 1f), scroll: new Vector2(0.3f, 1f));
            Polar(portalRim, 0.5f);
            Toon("AA_Mote", star, SheikahCore, SheikahEdge, 0.25f);
            Toon("AA_Shock", ring, SheikahCore, SheikahEdge, 0.3f, noise, 0.5f, tiling: new Vector2(3f, 3f)).SetFloat("_Erosion", 0.2f);

            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------ Emerald
        // Materiales de los movimientos Emerald (los usa EmeraldMoves, no "Rebuild Everything").

        const string EmeraldShader = "BotwVFX/Emerald Toon";

        /// <summary>Material de la animación horneada y materiales compartidos por todos los tipos.</summary>
        public static void CreateEmeraldShared()
        {
            Directory.CreateDirectory(Folder);
            var noise = BotwTextures.Load("T_Noise");
            var ray = BotwTextures.Load("T_Ray");
            var ring = BotwTextures.Load("T_Ring");
            var puff = BotwTextures.Load("T_SmokePuff");

            // Animación horneada: el reproductor fija _Color/_Mode/_Cull/_ZWrite/_ZTest; aquí solo el estilo.
            var toon = Mat("EM_Toon", EmeraldShader);
            toon.SetFloat("_EmissionBoost", 1.6f);
            toon.SetFloat("_CoreBoost", 1.4f);
            toon.SetFloat("_BodyMax", 1.2f);
            toon.SetFloat("_CoreWhite", 0.9f);
            toon.SetFloat("_Saturation", 1.5f);
            toon.SetFloat("_NoiseScale", 2.2f);
            toon.SetFloat("_NoiseSpeed", 1.8f);
            toon.SetFloat("_NoiseAmount", 0.5f);
            toon.SetFloat("_EdgeErosion", 0.08f);
            toon.SetFloat("_AlphaErosion", 0.35f);
            toon.SetFloat("_IntersectionDistance", 0.3f);
            toon.SetFloat("_IntersectionStrength", 0.8f);
            toon.SetColor("_ShadowColor", new Color(0.55f, 0.62f, 0.8f, 1f)); // como MAT_* (Toon Lit)
            toon.SetColor("_RimColor", new Color(1f, 0.93f, 0.78f, 0.35f));

            Smoke("EM_Dust", puff, new Color(0.93f, 0.88f, 0.78f), new Color(0.72f, 0.64f, 0.52f), fireErosion: 1f, cutout: 0.5f);
            Smoke("EM_Mist", puff, new Color(0.88f, 0.95f, 1f), new Color(0.55f, 0.7f, 0.86f), fireErosion: 1f, cutout: 0.5f);
            Smoke("EM_PoisonSmoke", puff, new Color(0.7f, 0.5f, 0.82f), new Color(0.38f, 0.2f, 0.5f), fireErosion: 1f, cutout: 0.5f);
            Smoke("EM_GhostSmoke", puff, new Color(0.45f, 0.36f, 0.62f), new Color(0.18f, 0.12f, 0.3f), fireErosion: 1f, cutout: 0.5f);

            var leaf = Toon("EM_Leaf", ray, new Color(0.9f, 1.8f, 0.45f), new Color(0.08f, 0.55f, 0.06f), 0.45f);
            leaf.SetFloat("_ShadeAmount", 0f);
            var shard = Toon("EM_IceShard", null, new Color(1.3f, 2f, 2.4f), new Color(1.3f, 2f, 2.4f), 0f);
            shard.SetFloat("_AlphaErosion", 0f);
            shard.SetFloat("_ShadeAmount", 0.45f);
            shard.SetFloat("_ZWrite", 1f);
            Toon("EM_Bubble", ring, new Color(2f, 1.2f, 2.4f), new Color(0.8f, 0.1f, 1.2f), 0.3f, noise, 0.3f);
            Toon("EM_Wind", ray, new Color(1.9f, 2.1f, 2.2f), new Color(0.6f, 1.1f, 1.6f), 0.35f, noise, 0.25f);
        }

        /// <summary>
        /// Juego de materiales de un tipo (EM_&lt;tipo&gt;_Spark/Star/Glow/Shock) con su paleta HDR lineal:
        /// núcleo casi blanco y borde saturado, como el resto de efectos BotW.
        /// </summary>
        public static void CreateEmeraldType(string key, Color core, Color edge)
        {
            var noise = BotwTextures.Load("T_Noise");
            var circle = BotwTextures.Load("T_SoftCircle");
            var ray = BotwTextures.Load("T_Ray");
            var star = BotwTextures.Load("T_Star");

            Toon($"EM_{key}_Spark", ray, core, edge, 0.35f);
            Toon($"EM_{key}_Star", star, core, edge, 0.25f).SetFloat("_CameraOffset", 1f);
            var glow = Toon($"EM_{key}_Glow", circle, core, edge, 0.3f, noise, 0.4f,
                tiling: new Vector2(2f, 2f), scroll: new Vector2(0f, 1.2f));
            glow.SetFloat("_CameraOffset", 0.8f);
            var shock = Toon($"EM_{key}_Shock", null, core, edge, 0.35f, noise, 0.55f,
                tiling: new Vector2(6f, 1f), scroll: new Vector2(0.2f, 0f));
            shock.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
        }

        // ------------------------------------------------------------ helpers

        internal static Material Mat(string name, string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
                throw new System.Exception($"No se encuentra el shader '{shaderName}'. ¿Hay errores de compilación?");
            string path = $"{Folder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>Asigna un color lineal/HDR (SetColor espera el valor en gamma en proyectos lineales).</summary>
        internal static void Hdr(Material m, string prop, Color linear) => m.SetColor(prop, linear.gamma);

        internal static Color Lin(float r, float g, float b) => new Color(r, g, b).linear;

        internal static Material Toon(string name, Texture mainTex, Color color, Color edge, float edgeWidth,
            Texture noise = null, float noiseStrength = 0f, Vector2? tiling = null, Vector2? scroll = null, bool additive = false)
        {
            var m = Mat(name, ToonShader);
            m.SetTexture("_MainTex", mainTex);
            m.SetTexture("_NoiseTex", noise);
            m.SetTextureScale("_NoiseTex", tiling ?? Vector2.one);
            m.SetVector("_NoiseScroll", scroll ?? Vector2.zero);
            m.SetFloat("_NoiseStrength", noise != null ? noiseStrength : 0f);
            Hdr(m, "_Color", new Color(color.r, color.g, color.b, 1f));
            Hdr(m, "_EdgeColor", new Color(edge.r, edge.g, edge.b, 1f));
            m.SetFloat("_EdgeWidth", edgeWidth);
            m.SetFloat("_Erosion", 0f);
            m.SetFloat("_Softness", 0f);
            m.SetFloat("_AlphaErosion", 1f);
            m.SetFloat("_ShadeAmount", 0f);
            m.SetFloat("_CameraOffset", 0f);
            m.SetVector("_BorderFade", Vector4.zero);
            m.SetVector("_MainScroll", Vector4.zero);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Polar", 0f);
            m.DisableKeyword("_POLAR_ON");
            m.renderQueue = -1;
            return m;
        }

        static void Polar(Material m, float twist)
        {
            m.SetFloat("_Polar", 1f);
            m.SetFloat("_PolarTwist", twist);
            m.EnableKeyword("_POLAR_ON");
        }

        internal static Material Smoke(string name, Texture puff, Color light, Color shadow, float fireErosion, float cutout)
        {
            var m = Mat(name, SmokeShader);
            m.SetTexture("_SmokeTex", puff);
            Hdr(m, "_FireColor", new Color(FireCore.r, FireCore.g, FireCore.b, 1f));
            Hdr(m, "_FireEdgeColor", new Color(FireEdge.r, FireEdge.g, FireEdge.b, 1f));
            m.SetFloat("_FireEdgeWidth", 0.12f);
            m.SetFloat("_FireErosion", fireErosion);
            m.SetColor("_SmokeLight", light);   // colores normales: se escriben en sRGB
            m.SetColor("_SmokeShadow", shadow);
            m.SetFloat("_ShadeCutout", cutout);
            m.SetFloat("_NoiseAmount", 0.35f);
            m.SetFloat("_ErosionStrength", 0.6f);
            return m;
        }

        static void Lit(string name, Color color)
        {
            var m = Mat(name, LitShader);
            m.SetColor("_BaseColor", color);
        }

        static void Sky(string name, Color top, Color horizon, Color bottom)
        {
            var m = Mat(name, SkyShader);
            m.SetColor("_TopColor", top);
            m.SetColor("_HorizonColor", horizon);
            m.SetColor("_BottomColor", bottom);
        }
    }
}
