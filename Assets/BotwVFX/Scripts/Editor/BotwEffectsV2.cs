using UnityEditor;
using UnityEngine;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Variaciones (V2) de los cuatro efectos. Los originales no se tocan: así se
    /// pueden comparar en la misma estación de la demo (tecla V / C).
    ///
    /// Mejoras aplicadas en todas:
    ///  - Rampas de color (blanco -> color -> oscuro) en vez de núcleo/borde.
    ///  - Distorsión de pantalla (ondas, calor, succión del portal).
    ///  - Humo con 4 formas, iluminado por el sol real y que se erosiona contra el suelo.
    ///  - Destello de pantalla y sonido.
    /// </summary>
    public static partial class BotwEffects
    {
        public const string RemoteBombV2Path = Folder + "/VFX_RemoteBomb_V2.prefab";
        public const string ExplosionV2Path = Folder + "/VFX_Explosion_V2.prefab";
        public const string GuardianBeamV2Path = Folder + "/VFX_GuardianBeam_V2.prefab";
        public const string AncientArrowV2Path = Folder + "/VFX_AncientArrow_V2.prefab";

        public const float GuardianV2Fire = 2.7f;
        public const float GuardianV2Travel = 0.22f;

        static void BuildAllV2()
        {
            Save(BuildRemoteBombV2(), RemoteBombV2Path);
            Save(BuildExplosionV2(), ExplosionV2Path);
            Save(BuildGuardianBeamV2(), GuardianBeamV2Path);
            Save(BuildAncientArrowV2(), AncientArrowV2Path);
        }

        static void Sound(VfxTimeline fx, string clip, float time, float volume = 1f)
        {
            fx.sounds.Add(new VfxTimeline.AudioCue { clip = BotwAudio.Load(clip), time = time, volume = volume });
        }

        static void ScreenFlash(VfxTimeline fx, float time, Color color, float intensity, float duration)
        {
            fx.screenFlashes.Add(new VfxTimeline.FlashCue { time = time, color = color, intensity = intensity, duration = duration });
        }

        // =====================================================================
        // 1. BOMBA REMOTA V2
        //    Borde de la esfera roto en llamas, rampa Sheikah, onda más fina,
        //    distorsión expansiva, polvo iluminado, destello y sonido.
        // =====================================================================
        static GameObject BuildRemoteBombV2()
        {
            var root = new GameObject("VFX_RemoteBomb_V2");
            var fx = root.AddComponent<RemoteBombVfx>();
            fx.duration = 2f;
            fx.respawnTime = 1.4f;
            var center = new Vector3(0f, 0.45f, 0f);

            var bomb = new GameObject("BombModel").transform;
            bomb.SetParent(root.transform, false);
            bomb.localPosition = center;
            AddMesh(bomb, "Body", BotwMeshes.Sphere, M("MAT_SheikahStone"), Vector3.zero, Vector3.one * 0.42f).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            AddMesh(bomb, "BandA", BotwMeshes.Torus, M("RB_BombGlow"), Vector3.zero, Vector3.one * 0.425f);
            AddMesh(bomb, "BandB", BotwMeshes.Torus, M("RB_BombGlow"), Vector3.zero, Vector3.one * 0.425f).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            fx.bombModel = bomb;

            fx.sphere = AddMesh(root.transform, "EnergySphere", BotwMeshes.Sphere, M("V2_RB_Sphere"), center, Vector3.one, false);
            fx.sphere.sortingOrder = 1;
            fx.core = AddMesh(root.transform, "Core", BotwMeshes.Sphere, M("RB_Core"), center, Vector3.one, false);
            fx.core.sortingOrder = 2;
            fx.sphereRadius = C(0f, 0.3f, 0.05f, 2.5f, 0.11f, 3f, 0.4f, 3.25f, 0.6f, 2.6f, 0.74f, 0.8f, 0.8f, 0f);
            fx.sphereThreshold = C(0f, -0.1f, 0.03f, -0.1f, 0.08f, 0.35f, 0.25f, 0.52f, 0.8f, 0.65f);
            fx.sphereOpacity = C(0f, 1f, 0.6f, 1f, 0.8f, 0.7f);
            fx.sphereDissolve = C(0f, 0f, 0.5f, 0f, 0.8f, 0.6f);
            fx.coreRadius = C(0f, 0.4f, 0.04f, 1.8f, 0.1f, 1.4f, 0.22f, 0f);

            fx.flashLight = AddLight(root.transform, center, new Color(0.45f, 0.8f, 1f), 10f);
            fx.lightIntensity = C(0f, 0f, 0.03f, 4f, 0.15f, 2.5f, 0.7f, 0f);

            var distort = Create(root.transform, "ShockDistortion", M("V2_RB_Distort"), center)
                .Life(0.35f).Size(1f).Burst(1).SizeOverLife(C(0f, 1f, 0.4f, 9f, 1f, 11f)).AlphaOverLife(0f, 1f, 1f, 0f);
            var rays = Create(root.transform, "Rays", M("V2_RB_Ray"), center)
                .Life(0.3f).Speed(0.01f).Size(0.32f).Sphere(0.1f).Burst(8)
                .Stretch(20f).AlphaOverLife(0f, 1f, 0.33f, 0f, 1f, 0f).Order(3);
            var sparks = Create(root.transform, "Sparks", M("V2_RB_Ray"), center)
                .Life(0.5f, 1f).Speed(9f, 14f).Size(0.1f, 0.16f).Sphere(0.2f).Burst(50)
                .Gravity(1f).Drag(1.5f).Stretch(6f, 0.05f).AlphaOverLife(0f, 1f, 0.5f, 0.6f, 1f, 0f).Order(3);
            var shock = Create(root.transform, "Shockwave", M("V2_RB_Shock"), new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.35f).Size(1f).Burst(1)
                .Rotation3D(Vector3.zero, new Vector3(0f, 360f, 0f))
                .SizeOverLife(C(0f, 0.6f, 0.25f, 4.5f, 1f, 5.5f)).AlphaOverLife(0f, 1f, 0.3f, 0.6f, 1f, 0f).Order(0);
            var dust = Create(root.transform, "Dust", M("V2_RB_Dust"), new Vector3(0f, 0.3f, 0f))
                .GroundCircle(2f, 0.3f).Speed(1.5f, 3.5f).Drag(2.5f).Life(0.8f, 1.3f).Size(1f, 1.8f).Burst(16)
                .Velocity(new Vector3(0f, 0.6f, 0f)).RandomFrame(2, 2)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0);
            var glints = Create(root.transform, "Glints", M("V2_RB_Glint"), center)
                .Sphere(2.6f).Life(0.5f, 1.1f).Size(0.12f, 0.3f).Speed(0.2f, 0.8f).Burst(24)
                .Velocity(new Vector3(0f, 0.8f, 0f)).Spin(-180f, 180f)
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.6f, 1f, 1f, 0f).Order(2);

            fx.particles.Add(Cue(distort, 0f));
            fx.particles.Add(Cue(rays, 0f));
            fx.particles.Add(Cue(sparks, 0f));
            fx.particles.Add(Cue(shock, 0.02f));
            fx.particles.Add(Cue(dust, 0.05f));
            fx.particles.Add(Cue(glints, 0.12f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0f, strength = 0.25f, duration = 0.35f });
            ScreenFlash(fx, 0f, new Color(0.6f, 0.85f, 1f), 0.5f, 0.12f);
            Sound(fx, "SFX_RemoteBomb", 0f);
            return root;
        }

        // =====================================================================
        // 2. EXPLOSIÓN V2
        //    Fuego con rampa, onda de aire = distorsión, calor que sube,
        //    humo iluminado con 4 formas, estelas de polvo en los escombros.
        // =====================================================================
        static GameObject BuildExplosionV2()
        {
            var root = new GameObject("VFX_Explosion_V2");
            var fx = root.AddComponent<VfxTimeline>();
            fx.duration = 3.6f;
            var c = new Vector3(0f, 0.9f, 0f);

            var flash = Create(root.transform, "Flash", M("V2_EX_Flash"), c)
                .Life(0.14f).Size(5.5f).Burst(1).Rotation(0f, 45f).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);
            var flashCircle = Create(root.transform, "FlashCircle", M("V2_EX_FlashCircle"), c)
                .Life(0.1f).Size(3.2f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(5);

            var ball = Create(root.transform, "SpikyBall", M("V2_EX_Ball"), c)
                .Mesh(BotwMeshes.SpikyBall).Life(0.5f).Size(1.6f).Burst(1)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-90f, 90f, true)
                .SizeOverLife(C(0f, 0.12f, 0.22f, 1f, 1f, 1.2f))
                .Custom(Curve(0f, 0f, 0.3f, 0.05f, 1f, 1f), Rand(0f, 1f), Const(0f), Const(0f)).Order(2);
            var ballB = Create(root.transform, "SpikyBallB", M("V2_EX_Ball"), c + new Vector3(0.2f, 0.3f, 0f))
                .Mesh(BotwMeshes.SpikyBall).Life(0.42f).Size(1.15f).Burst(1)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-120f, 120f, true)
                .SizeOverLife(C(0f, 0.15f, 0.25f, 1f, 1f, 1.15f))
                .Custom(Curve(0f, 0f, 0.25f, 0.05f, 1f, 1f), Rand(0f, 1f), Const(0f), Const(0f)).Order(3);

            // La onda de aire ya no es un anillo blanco: deforma lo que hay detrás.
            var shockDistort = Create(root.transform, "ShockDistortion", M("V2_EX_ShockDistort"), c)
                .Life(0.4f).Size(1f).Burst(1).SizeOverLife(C(0f, 1f, 0.35f, 10f, 1f, 13f)).AlphaOverLife(0f, 1f, 1f, 0f);
            var groundShock = Create(root.transform, "GroundShock", M("V2_EX_GroundShock"), new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.45f).Size(1f).Burst(1)
                .SizeOverLife(C(0f, 1f, 0.3f, 6f, 1f, 8f)).AlphaOverLife(0f, 1f, 0.4f, 0.75f, 1f, 0f).Order(0);

            var tongues = Create(root.transform, "FireTongues", M("V2_EX_Tongue"), c)
                .HalfDonut(1.1f, 0.25f).Speed(6f, 10f).Life(0.22f, 0.38f).Size(0.45f, 0.75f).Burst(22)
                .Stretch(3.2f, 0.03f).Drag(3f).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(1);
            tongues.ps.gameObject.AddComponent<FaceCamera>();
            var tongues2 = Create(root.transform, "FireTonguesBig", M("V2_EX_Tongue"), c)
                .HalfDonut(1.6f, 0.3f).Speed(5f, 8f).Life(0.3f, 0.45f).Size(0.6f, 1f).Burst(14)
                .Stretch(3f, 0.03f).Drag(3f).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(1);
            tongues2.ps.gameObject.AddComponent<FaceCamera>();

            // Escombros con estela de polvo (Trails module).
            var debris = Create(root.transform, "Debris", M("EX_Debris"), new Vector3(0f, 0.3f, 0f))
                .Mesh(BotwMeshes.Rock).Hemisphere(0.6f).Speed(7f, 13f).Life(1.6f, 2.4f).Size(0.14f, 0.38f)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-400f, 400f, true)
                .Gravity(2.4f).Burst(16).Collide(0.35f, 0.45f)
                .Trails(M("V2_EX_DebrisTrail"), 0.22f, 1.6f)
                .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(0);
            var embers = Create(root.transform, "Embers", M("V2_EX_Ember"), c)
                .Sphere(0.6f).Speed(8f, 18f).Life(0.4f, 1.1f).Size(0.07f, 0.14f).Burst(45)
                .Gravity(0.9f).Drag(2f).Stretch(5f, 0.04f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(2);

            var smoke = Create(root.transform, "FireSmoke", M("V2_EX_Smoke"), c)
                .Sphere(0.9f).Speed(3f, 6.5f).Drag(3.5f).Life(1.5f, 2.3f).Size(1.4f, 2.4f).Burst(22)
                .Velocity(new Vector3(0f, 0.7f, 0f)).RandomFrame(2, 2)
                .SizeOverLife(C(0f, 0.45f, 0.15f, 1f, 1f, 1.25f))
                .Custom(Curve(0f, 0f, 0.12f, 0.12f, 0.45f, 1f), Rand(-0.05f, 0.08f), Curve(0f, 0f, 0.55f, 0.08f, 1f, 1f), Rand(0f, 1f))
                .Order(1);
            var darkSmoke = Create(root.transform, "DarkSmoke", M("V2_EX_DarkSmoke"), c + new Vector3(0f, 0.8f, 0f))
                .Sphere(1f).Speed(0.5f, 1.5f).Life(2.4f, 3.2f).Size(2.2f, 3.2f).Burst(7)
                .Velocity(new Vector3(0f, 1f, 0f)).RandomFrame(2, 2)
                .SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 1.3f))
                .Custom(Const(1f), Rand(0f, 0.1f), Curve(0f, 0f, 0.6f, 0.1f, 1f, 1f),
                    new ParticleSystem.MinMaxCurve(1f, C(0f, 0f, 1f, 0.15f), C(0f, 1f, 1f, 1.35f)))
                .Order(0);

            // Aire caliente que sube sobre el fuego.
            var heat = Create(root.transform, "HeatHaze", M("V2_EX_Heat"), c + new Vector3(0f, 0.5f, 0f))
                .Sphere(1f).Speed(0.3f, 0.8f).Velocity(new Vector3(0f, 1.6f, 0f)).Life(0.8f, 1.2f).Size(2.5f, 3.5f)
                .Rate(10f).Duration(1.4f).Rotation(0f, 360f).AlphaOverLife(0f, 0f, 0.3f, 1f, 1f, 0f);

            fx.flashLight = AddLight(root.transform, c, new Color(1f, 0.62f, 0.3f), 14f);
            fx.lightIntensity = C(0f, 0f, 0.03f, 6f, 0.2f, 3f, 0.9f, 0f);

            fx.particles.Add(Cue(flash, 0f));
            fx.particles.Add(Cue(flashCircle, 0f));
            fx.particles.Add(Cue(ball, 0f));
            fx.particles.Add(Cue(ballB, 0.03f));
            fx.particles.Add(Cue(tongues, 0f));
            fx.particles.Add(Cue(tongues2, 0.06f));
            fx.particles.Add(Cue(shockDistort, 0.02f));
            fx.particles.Add(Cue(groundShock, 0.03f));
            fx.particles.Add(Cue(debris, 0.02f));
            fx.particles.Add(Cue(embers, 0.02f));
            fx.particles.Add(Cue(smoke, 0.04f));
            fx.particles.Add(Cue(darkSmoke, 0.25f));
            fx.particles.Add(Cue(heat, 0.1f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0f, strength = 0.5f, duration = 0.5f });
            ScreenFlash(fx, 0f, new Color(1f, 0.8f, 0.5f), 0.6f, 0.15f);
            Sound(fx, "SFX_Explosion", 0f);
            return root;
        }

        // =====================================================================
        // 3. RAYO GUARDIÁN V2
        //    Ojo azul -> rosa, pitidos que aceleran, proyectil visible que viaja,
        //    tiras de energía grandes, calor alrededor del rayo, impacto V2.
        // =====================================================================
        static GameObject BuildGuardianBeamV2()
        {
            var root = new GameObject("VFX_GuardianBeam_V2");
            var fx = root.AddComponent<GuardianBeamVfx>();
            const float fire = GuardianV2Fire;
            const float travel = GuardianV2Travel;
            const float impact = fire + travel + 0.02f;
            fx.fireTime = fire;
            fx.laserEnd = fire;
            fx.travelTime = travel;
            fx.beamEnd = fire + 0.8f;
            fx.duration = impact + 3.6f;
            fx.beamWidth = C(0f, 0.06f, 0.05f, 0.32f, 0.22f, 0.42f, 0.32f, 0.3f, 0.6f, 0.27f, 0.8f, 0f);
            fx.glowScale = 1.9f;
            fx.heatScale = 3.2f;

            var eye = new GameObject("Eye").transform;
            eye.SetParent(root.transform, false);
            eye.localPosition = GuardianEyeLocal;
            var target = new GameObject("Target").transform;
            target.SetParent(root.transform, false);
            target.localPosition = new Vector3(0f, 0.6f, 0f);
            fx.eye = eye;
            fx.target = target;

            fx.laser = AddLine(root.transform, "Laser", M("GB_Laser"), 0.05f, AlphaGradient(1f, 1f));
            fx.beamHeat = AddMesh(root.transform, "BeamHeat", BotwMeshes.Sphere, M("V2_GB_BeamHeat"), Vector3.zero, Vector3.one, false);
            fx.beamGlow = AddMesh(root.transform, "BeamGlow", BotwMeshes.Sphere, M("GB_BeamGlow"), Vector3.zero, Vector3.one, false);
            fx.beamGlow.sortingOrder = 1;
            fx.beamCore = AddMesh(root.transform, "BeamCore", BotwMeshes.Sphere, M("GB_Beam"), Vector3.zero, Vector3.one, false);
            fx.beamCore.sortingOrder = 2;

            // Ojo: azul (vigilando) -> rosa (te ha visto).
            var eyeGlow = Create(eye, "EyeGlow", M("V2_GB_EyeGlow"))
                .Life(fire + 0.6f).Size(1.2f).Burst(1)
                .ColorKeys((0f, new Color(0.25f, 0.7f, 1f)), (0.04f, new Color(0.25f, 0.7f, 1f)), (0.09f, new Color(1f, 0.3f, 0.7f)), (1f, new Color(1f, 0.3f, 0.7f)))
                .Order(5);

            var dot = Create(root.transform, "LaserDot", M("GB_Dot"), target.localPosition)
                .Life(0.12f, 0.2f).Size(0.5f, 0.9f).Rate(30f).Duration(fire - 0.05f).Rotation(0f, 90f)
                .AlphaOverLife(0f, 1f, 1f, 0.2f).Order(3);
            fx.laserDot = dot.ps.transform;

            var chargeSparks = Create(eye, "ChargeSparks", M("V2_GB_Spark"))
                .Sphere(2.2f, 0f).Radial(-7f).Life(0.3f).Size(0.08f, 0.14f).Rate(70f).Duration(1.15f)
                .Stretch(4f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0.6f).Order(3);
            var chargeOrb = Create(eye, "ChargeOrb", M("V2_GB_Orb"))
                .Life(1.3f).Size(1.8f).Burst(1).SizeOverLife(C(0f, 0.1f, 0.7f, 0.75f, 0.9f, 1f, 1f, 0.6f)).Order(4);
            var chargeRings = Create(eye, "ChargeRings", M("V2_GB_Ring"))
                .Life(0.35f).Size(4f).Rate(5f).Duration(0.9f).SizeOverLife(C(0f, 1f, 1f, 0.1f))
                .AlphaOverLife(0f, 0f, 0.3f, 1f, 1f, 0.3f).Order(2);

            var muzzle = Create(eye, "MuzzleFlare", M("V2_GB_Flare"))
                .Life(0.45f).Size(4.5f).Burst(1).Rotation(0f, 30f)
                .SizeOverLife(C(0f, 1.2f, 0.2f, 0.8f, 1f, 0.5f)).AlphaOverLife(0f, 1f, 0.6f, 0.7f, 1f, 0f).Order(6);
            var wideFlare = Create(eye, "WideFlare", M("V2_GB_Flare"))
                .Life(0.3f).Size3D(new Vector3(14f, 0.7f, 1f)).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(7);

            // Proyectil: esfera + destello que viajan del ojo al objetivo, dejando chispas.
            var head = new GameObject("ProjectileHead").transform;
            head.SetParent(root.transform, false);
            head.localPosition = GuardianEyeLocal;
            AddMesh(head, "Core", BotwMeshes.Sphere, M("V2_GB_Head"), Vector3.zero, Vector3.one * 0.38f, false).sortingOrder = 4;
            var flareQuad = AddMesh(head, "Flare", Resources.GetBuiltinResource<Mesh>("Quad.fbx"), M("V2_GB_HeadFlare"), Vector3.zero, Vector3.one * 2.4f, false);
            flareQuad.sortingOrder = 5;
            flareQuad.gameObject.AddComponent<FaceCamera>();
            var headTrail = Create(head, "HeadTrail", M("V2_GB_Spark"))
                .RateOverDistance(14f).Duration(travel + 0.05f).Life(0.2f, 0.4f).Speed(0.5f, 1.5f).Sphere(0.15f)
                .Size(0.08f, 0.16f).Stretch(4f, 0.05f).AlphaOverLife(0f, 1f, 1f, 0f).Order(3);
            fx.projectileHead = head;

            // Tiras: menos, más grandes y gruesas, con la escala alternando.
            var stripes = Create(root.transform, "Stripes", M("V2_GB_Stripe"))
                .LocalSpace().Mesh(BotwMeshes.ArcWide, ParticleSystemRenderSpace.Local).Box(new Vector3(0f, 0f, 1f))
                .Life(0.15f, 0.3f).Size(1.3f, 1.9f).Rotation3D(Vector3.zero, new Vector3(0f, 0f, 360f))
                .Spin(-500f, 500f).Rate(30f).Duration(0.5f).Burst(8)
                .SizeOverLife(C(0f, 0.4f, 0.3f, 1.15f, 0.6f, 0.85f, 1f, 1.1f)).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3);
            fx.stripes = stripes.ps;

            var impactFlare = Create(target, "ImpactFlare", M("V2_GB_Flare"))
                .Life(0.3f).Size(6f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);

            var explosion = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionV2Path), root.transform);
            explosion.transform.localPosition = Vector3.zero;

            fx.flashLight = AddLight(eye, Vector3.zero, new Color(1f, 0.35f, 0.7f), 8f);
            fx.lightIntensity = C(0f, 0f, 1.5f, 0f, fire - 0.05f, 2.5f, fire + 0.02f, 5f, fire + 0.2f, 1.5f, fire + 0.6f, 0f);

            fx.particles.Add(Cue(eyeGlow, 0f));
            fx.particles.Add(Cue(dot, 0f));
            fx.particles.Add(Cue(chargeSparks, 1.5f));
            fx.particles.Add(Cue(chargeOrb, 1.45f));
            fx.particles.Add(Cue(chargeRings, 1.7f));
            fx.particles.Add(Cue(muzzle, fire));
            fx.particles.Add(Cue(wideFlare, fire));
            fx.particles.Add(Cue(headTrail, fire));
            fx.particles.Add(Cue(stripes, fire + travel));
            fx.particles.Add(Cue(impactFlare, fire + travel));
            fx.subEffects.Add(new VfxTimeline.TimelineCue { timeline = explosion.GetComponent<VfxTimeline>(), time = impact });
            fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = impact, timeScale = 0.15f, duration = 0.18f });
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = impact, strength = 0.6f, duration = 0.6f });
            ScreenFlash(fx, fire, new Color(1f, 0.5f, 0.85f), 0.35f, 0.08f);
            Sound(fx, "SFX_GuardianBeeps", 0f, 0.7f);
            Sound(fx, "SFX_GuardianCharge", 1.45f, 0.8f);
            Sound(fx, "SFX_GuardianShot", fire);
            return root;
        }

        // =====================================================================
        // 4. FLECHA ANCESTRAL V2
        //    Enemigo que se disuelve y es absorbido, lente inclinada en 3D y
        //    aditiva, distorsión que tira hacia el portal, estela con chispas.
        // =====================================================================
        static GameObject BuildAncientArrowV2()
        {
            var root = new GameObject("VFX_AncientArrow_V2");
            var fx = root.AddComponent<AncientArrowVfx>();
            fx.duration = 3.2f;
            fx.flightTime = 0.25f;
            fx.victimDissolveTime = new Vector2(0.45f, 1.3f);
            fx.victimRespawnTime = 2.7f;
            var c = new Vector3(0f, 1.3f, 0f);

            var start = new GameObject("ArrowStart").transform;
            start.SetParent(root.transform, false);
            start.localPosition = ArrowStartLocal;
            var impact = new GameObject("Impact").transform;
            impact.SetParent(root.transform, false);
            impact.localPosition = c;
            fx.arrowStart = start;
            fx.impactPoint = impact;
            fx.arrow = AddMesh(root.transform, "Arrow", BotwMeshes.Sphere, M("AA_Arrow"), Vector3.zero, Vector3.one, false);
            fx.trail = AddLine(root.transform, "ArrowTrail", M("AA_Trail"), 0.24f, AlphaGradient(0f, 1f));

            var follower = new GameObject("ArrowHead").transform;
            follower.SetParent(root.transform, false);
            follower.localPosition = ArrowStartLocal;
            var trailSparks = Create(follower, "TrailSparks", M("V2_AA_TrailSpark"))
                .RateOverDistance(8f).Duration(0.3f).Life(0.3f, 0.6f).Speed(0.2f, 0.8f).Sphere(0.1f)
                .Size(0.1f, 0.22f).Spin(-180f, 180f).AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(3);
            fx.headFollower = follower;

            // Víctima: un "bokoblin" de primitivas que se disuelve hacia el centro.
            var victim = new GameObject("Victim").transform;
            victim.SetParent(root.transform, false);
            var skin = M("V2_Bokoblin");
            var dark = M("V2_BokoblinDark");
            var renderers = new System.Collections.Generic.List<Renderer>
            {
                VictimPart(victim, PrimitiveType.Capsule, skin, new Vector3(0f, 0.85f, 0f), new Vector3(0.85f, 0.7f, 0.75f)),
                VictimPart(victim, PrimitiveType.Sphere, skin, new Vector3(0f, 1.75f, 0.05f), Vector3.one * 0.75f),
                VictimPart(victim, PrimitiveType.Cylinder, dark, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 0.18f, 0.8f)),
                VictimPart(victim, PrimitiveType.Capsule, dark, new Vector3(0f, 2.15f, 0f), new Vector3(0.12f, 0.2f, 0.12f)),
            };
            fx.victim = victim;
            fx.victimRenderers = renderers.ToArray();

            var impactFlash = Create(impact, "ImpactFlash", M("V2_AA_Flash"))
                .Life(0.18f).Size(4f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);

            // Lente: quad orientado a cámara pero inclinado en 3D, aditivo (más "luz" que sólido).
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            var lens = Create(impact, "Lens", M("V2_AA_Lens"))
                .Mesh(quad, ParticleSystemRenderSpace.View).Life(1.25f).Size(5.6f).Burst(1)
                .Rotation3D(new Vector3(22f, -18f, 0f), new Vector3(22f, -18f, 0f)).Spin(70f, 70f)
                .SizeOverLife(C(0f, 0f, 0.1f, 1.08f, 0.18f, 1f, 0.85f, 1.02f, 1f, 1.15f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(2);
            var lensCaustics = Create(impact, "LensCaustics", M("AA_Caustics"))
                .Mesh(quad, ParticleSystemRenderSpace.View).Life(1.15f).Size(4.8f).Burst(1)
                .Rotation3D(new Vector3(22f, -18f, 0f), new Vector3(22f, -18f, 0f))
                .SizeOverLife(C(0f, 0f, 0.12f, 1f, 1f, 1f)).AlphaOverLife(0f, 1f, 0.85f, 1f, 1f, 0f).Order(3);

            var portalSize = C(0f, 0f, 0.15f, 1.1f, 0.25f, 1f, 0.85f, 0.95f, 1f, 0f);
            var pull = Create(impact, "PullDistortion", M("V2_AA_Pull"))
                .Life(1.05f).Size(6.5f).Burst(1).SizeOverLife(portalSize).AlphaOverLife(0f, 1f, 0.9f, 1f, 1f, 0f);
            var portal = Create(impact, "Portal", M("AA_Portal"))
                .Life(1.05f).Size(3f).Burst(1).Spin(-200f, -200f).SizeOverLife(portalSize).Order(4);
            var portalRim = Create(impact, "PortalRim", M("V2_AA_PortalRim"))
                .Life(1.05f).Size(3.7f).Burst(1).Spin(-120f, -120f).SizeOverLife(portalSize).Order(5);

            var converge = Create(impact, "Converge", M("V2_AA_Spark"))
                .Sphere(5f, 0f).Radial(-15f, 2f).Life(0.32f).Size(0.08f, 0.15f).Rate(110f).Duration(0.85f)
                .Stretch(4f, 0.04f).AlphaOverLife(0f, 0.4f, 0.2f, 1f, 1f, 0.5f).Order(3);
            var pebbles = Create(root.transform, "Pebbles", M("EX_Debris"), new Vector3(0f, 0.2f, 0f))
                .Mesh(BotwMeshes.Rock).GroundCircle(4f, 0.3f).Radial(-8f).Life(0.5f).Size(0.1f, 0.22f)
                .Rate(18f).Duration(0.8f).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-500f, 500f, true)
                .SizeOverLife(C(0f, 1f, 0.7f, 1f, 1f, 0f)).Order(0);
            var pebbleVel = pebbles.ps.velocityOverLifetime;
            pebbleVel.y = new ParticleSystem.MinMaxCurve(2.4f);

            var collapse = Create(impact, "CollapseFlash", M("V2_AA_Flash"))
                .Life(0.22f).Size(5f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);
            var collapseRing = Create(impact, "CollapseRing", M("V2_AA_Shock"))
                .Life(0.4f).Size(1f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.3f, 6f, 1f, 8f))
                .AlphaOverLife(0f, 1f, 0.5f, 0.7f, 1f, 0f).Order(5);
            var collapseDistort = Create(impact, "CollapseDistortion", M("V2_RB_Distort"))
                .Life(0.35f).Size(1f).Burst(1).SizeOverLife(C(0f, 1f, 0.4f, 8f, 1f, 10f)).AlphaOverLife(0f, 1f, 1f, 0f);
            var motes = Create(impact, "Motes", M("V2_AA_Mote"))
                .Sphere(1.5f).Speed(1f, 3f).Drag(1.5f).Velocity(new Vector3(0f, 0.6f, 0f)).Life(0.8f, 1.6f)
                .Size(0.1f, 0.25f).Burst(30).Spin(-180f, 180f).AlphaOverLife(0f, 1f, 0.7f, 0.8f, 1f, 0f).Order(4);

            fx.flashLight = AddLight(impact, Vector3.zero, new Color(0.45f, 0.85f, 1f), 10f);
            fx.lightIntensity = C(0f, 0f, 0.25f, 0f, 0.28f, 4f, 0.5f, 2f, 1.3f, 2.5f, 1.42f, 5f, 1.8f, 0f);

            fx.particles.Add(Cue(trailSparks, 0f));
            fx.particles.Add(Cue(impactFlash, 0.25f));
            fx.particles.Add(Cue(lens, 0.3f));
            fx.particles.Add(Cue(lensCaustics, 0.33f));
            fx.particles.Add(Cue(pull, 0.38f));
            fx.particles.Add(Cue(portal, 0.38f));
            fx.particles.Add(Cue(portalRim, 0.38f));
            fx.particles.Add(Cue(converge, 0.4f));
            fx.particles.Add(Cue(pebbles, 0.4f));
            fx.particles.Add(Cue(collapse, 1.38f));
            fx.particles.Add(Cue(collapseRing, 1.4f));
            fx.particles.Add(Cue(collapseDistort, 1.38f));
            fx.particles.Add(Cue(motes, 1.42f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0.25f, strength = 0.2f, duration = 0.25f });
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 1.4f, strength = 0.35f, duration = 0.4f });
            fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = 0.26f, timeScale = 0.3f, duration = 0.08f });
            ScreenFlash(fx, 1.38f, new Color(0.5f, 0.85f, 1f), 0.45f, 0.12f);
            Sound(fx, "SFX_ArrowWhoosh", 0f, 0.8f);
            Sound(fx, "SFX_ArrowPortal", 0.3f);
            return root;
        }

        static Renderer VictimPart(Transform parent, PrimitiveType type, Material mat, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = type.ToString();
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            return r;
        }
    }
}
