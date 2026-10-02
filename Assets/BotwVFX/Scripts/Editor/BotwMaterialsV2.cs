using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Materiales de las variaciones (prefijo V2_). Diferencias principales:
    /// rampas de color en vez de dos colores, distorsión de pantalla,
    /// humo iluminado por el sol real y enemigo con disolución.
    /// </summary>
    public static partial class BotwMaterials
    {
        const string SmokeLitShader = "BotwVFX/Toon Smoke Lit";
        const string DistortionShader = "BotwVFX/Distortion";

        static void CreateV2()
        {
            var noise = BotwTextures.Load("T_Noise");
            var caustics = BotwTextures.Load("T_Caustics");
            var circle = BotwTextures.Load("T_SoftCircle");
            var ray = BotwTextures.Load("T_Ray");
            var star = BotwTextures.Load("T_Star");
            var ring = BotwTextures.Load("T_Ring");
            var gradient = BotwTextures.Load("T_Gradient");
            var sheikahRing = BotwTextures.Load("T_SheikahRing");
            var fire = BotwTextures.Load("T_Ramp_Fire");
            var sheikah = BotwTextures.Load("T_Ramp_Sheikah");
            var guardian = BotwTextures.Load("T_Ramp_Guardian");

            // ---------------- Bomba remota V2
            var sphere = Mat("V2_RB_Sphere", SphereShader);
            Hdr(sphere, "_BaseColor", new Color(0.02f, 0.25f, 1.1f, 0.38f));
            Hdr(sphere, "_EmissiveColor", new Color(0.6f, 1.7f, 2.6f, 1f));
            sphere.SetFloat("_FresnelPower", 2.2f);
            sphere.SetFloat("_IntersectionDistance", 0.9f);
            sphere.SetFloat("_IntersectionPower", 1.5f);
            sphere.SetFloat("_NoiseScale", 3.5f);       // borde roto en "llamas"
            sphere.SetFloat("_NoiseSpeed", 3f);
            sphere.SetFloat("_NoiseAmount", 1f);
            sphere.SetFloat("_Threshold", 0.5f);
            sphere.SetFloat("_Wobble", 0.08f);

            Ramp("V2_RB_Ray", ray, sheikah, 1.6f, 0.5f, noise, 0.15f);
            var rbShock = Ramp("V2_RB_Shock", null, sheikah, 1.5f, 0.35f, noise, 0.65f, new Vector2(6f, 1f), new Vector2(0.25f, 0f));
            rbShock.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            rbShock.SetFloat("_Erosion", 0.15f);
            Ramp("V2_RB_Glint", star, sheikah, 1.6f, 0.5f);
            SmokeLit("V2_RB_Dust", new Color(0.86f, 0.93f, 1f), new Color(0.45f, 0.58f, 0.86f), 1f, 0.5f);
            Distort("V2_RB_Distort", ring, 1, 0.05f, new Color(0.3f, 0.7f, 1f, 0.12f));

            // ---------------- Explosión V2
            Ramp("V2_EX_Flash", star, fire, 1.8f, 0.5f).SetFloat("_CameraOffset", 1f);
            Ramp("V2_EX_FlashCircle", circle, fire, 1.7f, 0.5f, noise, 0.3f);
            Ramp("V2_EX_Ball", gradient, fire, 1.7f, 0.45f, noise, 0.45f, new Vector2(2f, 2f), new Vector2(0.2f, 0.6f));
            Ramp("V2_EX_Ember", ray, fire, 1.8f, 0.5f);
            Ramp("V2_EX_Tongue", ray, fire, 1.7f, 0.5f, noise, 0.3f);
            var groundShock = Toon("V2_EX_GroundShock", null, new Color(1.1f, 0.85f, 0.55f), new Color(0.55f, 0.32f, 0.16f), 0.35f, noise, 0.6f,
                tiling: new Vector2(6f, 1f), scroll: new Vector2(0.2f, 0f));
            groundShock.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            groundShock.SetFloat("_Erosion", 0.15f);
            var trail = Toon("V2_EX_DebrisTrail", null, Lin(0.78f, 0.7f, 0.6f), Lin(0.5f, 0.43f, 0.36f), 0.3f, noise, 0.5f,
                tiling: new Vector2(3f, 1f));
            trail.SetVector("_BorderFade", new Vector4(0f, 0.5f, 0f, 0f));
            Distort("V2_EX_ShockDistort", ring, 1, 0.06f, new Color(1f, 0.8f, 0.5f, 0.06f));
            Distort("V2_EX_Heat", circle, 0, 0.018f, new Color(0f, 0f, 0f, 0f), new Vector2(2f, 2f), new Vector2(0f, -0.6f));
            SmokeLit("V2_EX_Smoke", new Color(0.64f, 0.6f, 0.57f), new Color(0.28f, 0.27f, 0.37f), 0f, 0.5f);
            SmokeLit("V2_EX_DarkSmoke", new Color(0.5f, 0.48f, 0.5f), new Color(0.2f, 0.2f, 0.29f), 1f, 0.55f);

            // ---------------- Rayo Guardián V2
            var eyeGlow = Toon("V2_GB_EyeGlow", circle, new Color(2.2f, 2.2f, 2.2f), new Color(1.2f, 1.2f, 1.2f), 0.3f, noise, 0.3f,
                tiling: new Vector2(2f, 2f), scroll: new Vector2(0f, 0.8f));
            eyeGlow.SetFloat("_CameraOffset", 0.45f);
            Ramp("V2_GB_Orb", circle, guardian, 1.7f, 0.45f, noise, 0.5f, new Vector2(2f, 2f), new Vector2(0f, 1.2f)).SetFloat("_CameraOffset", 0.8f);
            Ramp("V2_GB_Spark", ray, guardian, 1.7f, 0.5f);
            Ramp("V2_GB_Ring", ring, guardian, 1.6f, 0.35f, noise, 0.4f, new Vector2(3f, 3f));
            Ramp("V2_GB_Flare", star, guardian, 1.8f, 0.5f, caustics, 0.35f, new Vector2(2f, 2f), new Vector2(0.4f, 0.3f)).SetFloat("_CameraOffset", 1.5f);
            var stripe = Ramp("V2_GB_Stripe", null, sheikah, 1.6f, 0.35f, noise, 0.4f, new Vector2(3f, 1f), new Vector2(-2.5f, 0f));
            stripe.SetVector("_BorderFade", new Vector4(0.2f, 0.5f, 0f, 0f));
            var head = Toon("V2_GB_Head", null, new Color(2.4f, 1.6f, 2.2f), new Color(2.4f, 1.6f, 2.2f), 0f);
            head.SetFloat("_AlphaErosion", 0f);
            var headFlare = Ramp("V2_GB_HeadFlare", star, guardian, 2f, 0.5f);
            headFlare.SetFloat("_AlphaErosion", 0f);
            headFlare.SetFloat("_CameraOffset", 0.6f);
            var heat = Distort("V2_GB_BeamHeat", null, 0, 0.025f, new Color(1f, 0.3f, 0.7f, 0.05f), new Vector2(1f, 4f), new Vector2(0f, 3f));
            heat.SetFloat("_MeshFade", 1.5f);

            // ---------------- Flecha ancestral V2
            Ramp("V2_AA_Lens", sheikahRing, sheikah, 1.3f, 0.3f, noise, 0.3f, new Vector2(3f, 3f), null, additive: true);
            var rim = Ramp("V2_AA_PortalRim", ring, sheikah, 1.6f, 0.35f, noise, 0.45f, new Vector2(4f, 1f), new Vector2(0.3f, 1f));
            Polar(rim, 0.5f);
            Distort("V2_AA_Pull", ring, 2, 0.07f, new Color(0.2f, 0.6f, 1f, 0.08f));
            Ramp("V2_AA_Flash", star, sheikah, 1.8f, 0.5f).SetFloat("_CameraOffset", 1f);
            Ramp("V2_AA_Spark", ray, sheikah, 1.6f, 0.5f);
            Ramp("V2_AA_Mote", star, sheikah, 1.6f, 0.5f);
            Ramp("V2_AA_TrailSpark", star, sheikah, 1.8f, 0.5f);
            Ramp("V2_AA_Shock", ring, sheikah, 1.6f, 0.35f, noise, 0.5f, new Vector2(3f, 3f)).SetFloat("_Erosion", 0.2f);
            Dissolving("V2_Bokoblin", new Color(0.8f, 0.4f, 0.38f));
            Dissolving("V2_BokoblinDark", new Color(0.32f, 0.24f, 0.2f));
        }

        // ------------------------------------------------------------ helpers V2

        /// <summary>ToonParticle con rampa: el color sale de la rampa por la distancia al borde del corte.</summary>
        static Material Ramp(string name, Texture mainTex, Texture ramp, float intensity, float range,
            Texture noise = null, float noiseStrength = 0f, Vector2? tiling = null, Vector2? scroll = null, bool additive = false)
        {
            var m = Toon(name, mainTex, new Color(intensity, intensity, intensity), Color.white, 0f, noise, noiseStrength, tiling, scroll, additive);
            m.SetFloat("_UseRamp", 1f);
            m.EnableKeyword("_RAMP_ON");
            m.SetTexture("_RampTex", ramp);
            m.SetFloat("_RampRange", range);
            return m;
        }

        static Material Distort(string name, Texture mask, int mode, float strength, Color tint, Vector2? tiling = null, Vector2? scroll = null)
        {
            var m = Mat(name, DistortionShader);
            m.SetTexture("_MainTex", mask);
            m.SetTexture("_NoiseTex", BotwTextures.Load("T_Noise"));
            m.SetTextureScale("_NoiseTex", tiling ?? Vector2.one);
            m.SetVector("_NoiseScroll", scroll ?? new Vector2(0f, 0.3f));
            m.SetFloat("_Mode", mode);
            m.SetFloat("_Strength", strength);
            m.SetFloat("_MeshFade", 0f);
            m.SetColor("_Tint", tint);
            m.renderQueue = -1;
            return m;
        }

        static void SmokeLit(string name, Color light, Color shadow, float fireErosion, float cutout)
        {
            var m = Mat(name, SmokeLitShader);
            m.SetTexture("_SmokeAtlas", BotwTextures.Load("T_SmokeAtlas"));
            m.SetFloat("_Tiles", 2f);
            m.SetTexture("_NoiseTex", BotwTextures.Load("T_Noise"));
            m.SetTexture("_FireRamp", BotwTextures.Load("T_Ramp_Fire"));
            m.SetFloat("_FireIntensity", 1.7f);
            m.SetFloat("_FireRange", 0.35f);
            m.SetFloat("_FireErosion", fireErosion);
            m.SetColor("_SmokeLight", light);
            m.SetColor("_SmokeShadow", shadow);
            m.SetFloat("_ShadeCutout", cutout);
            m.SetFloat("_NoiseAmount", 0.3f);
            m.SetFloat("_ErosionStrength", 0.6f);
            m.SetFloat("_DepthFade", 0.7f);
        }

        static void Dissolving(string name, Color color)
        {
            var m = Mat(name, LitShader);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_UseDissolve", 1f);
            m.EnableKeyword("_DISSOLVE_ON");
            Hdr(m, "_DissolveEdgeColor", new Color(0.6f, 2f, 3f, 1f));
            m.SetFloat("_DissolveEdgeWidth", 0.08f);
            m.SetFloat("_DissolveScale", 3f);
        }
    }
}
