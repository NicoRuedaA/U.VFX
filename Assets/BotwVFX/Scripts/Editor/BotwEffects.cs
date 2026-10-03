using System.IO;
using UnityEditor;
using UnityEngine;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Construye los prefabs de los efectos (Assets/BotwVFX/Prefabs).
    /// Cada efecto sigue la "curva de clímax" del artículo de 80.lv:
    /// anticipación -> pico muy rápido -> disipación lenta.
    /// Escala de referencia: 1 unidad = 1 metro (Link mide ~1,7).
    /// </summary>
    public static class BotwEffects
    {
        public const string Folder = "Assets/BotwVFX/Prefabs";

        public const string RemoteBombPath = Folder + "/VFX_RemoteBomb.prefab";
        public const string ExplosionPath = Folder + "/VFX_Explosion.prefab";
        public const string GuardianBeamPath = Folder + "/VFX_GuardianBeam.prefab";
        public const string AncientArrowPath = Folder + "/VFX_AncientArrow.prefab";

        // Posiciones locales que la escena también usa.
        public static readonly Vector3 GuardianEyeLocal = new Vector3(-9f, 5.2f, 8f);
        public static readonly Vector3 ArrowStartLocal = new Vector3(-13f, 2.6f, -4f);

        public static void BuildAll()
        {
            Directory.CreateDirectory(Folder);
            Save(BuildRemoteBomb(), RemoteBombPath);
            Save(BuildExplosion(), ExplosionPath);
            Save(BuildGuardianBeam(), GuardianBeamPath);
            Save(BuildAncientArrow(), AncientArrowPath);
        }

        static void Save(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        static Material M(string name) => BotwMaterials.Get(name);

        static VfxTimeline.ParticleCue Cue(Fx fx, float time) => new VfxTimeline.ParticleCue { system = fx.ps, time = time };

        static Light AddLight(Transform parent, Vector3 localPos, Color color, float range)
        {
            var go = new GameObject("FlashLight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.enabled = false;
            return light;
        }

        static MeshRenderer AddMesh(Transform parent, string name, Mesh mesh, Material mat, Vector3 localPos, Vector3 scale, bool enabled = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = enabled;
            return r;
        }

        static LineRenderer AddLine(Transform parent, string name, Material mat, float width, Gradient colors)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier = width;
            line.sharedMaterial = mat;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.numCapVertices = 0;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.colorGradient = colors;
            line.enabled = false;
            return line;
        }

        static Gradient AlphaGradient(float start, float end)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(start, 0f), new GradientAlphaKey(end, 1f) });
            return g;
        }

        // =====================================================================
        // 1. BOMBA REMOTA  (vídeo de Daniel Ilett: Shader Graph + VFX Graph)
        // =====================================================================
        static GameObject BuildRemoteBomb()
        {
            var root = new GameObject("VFX_RemoteBomb");
            var fx = root.AddComponent<RemoteBombVfx>();
            fx.duration = 2f;
            fx.respawnTime = 1.4f;
            var center = new Vector3(0f, 0.45f, 0f);

            // Modelo de la bomba (solo para la demo).
            var bomb = new GameObject("BombModel").transform;
            bomb.SetParent(root.transform, false);
            bomb.localPosition = center;
            AddMesh(bomb, "Body", BotwMeshes.Sphere, M("MAT_SheikahStone"), Vector3.zero, Vector3.one * 0.42f).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            AddMesh(bomb, "BandA", BotwMeshes.Torus, M("RB_BombGlow"), Vector3.zero, Vector3.one * 0.425f);
            var bandB = AddMesh(bomb, "BandB", BotwMeshes.Torus, M("RB_BombGlow"), Vector3.zero, Vector3.one * 0.425f);
            bandB.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            fx.bombModel = bomb;

            // Esfera de energía + núcleo blanco (animados por RemoteBombVfx).
            fx.sphere = AddMesh(root.transform, "EnergySphere", BotwMeshes.Sphere, M("RB_Sphere"), center, Vector3.one, false);
            fx.sphere.sortingOrder = 1;
            fx.core = AddMesh(root.transform, "Core", BotwMeshes.Sphere, M("RB_Core"), center, Vector3.one, false);
            fx.core.sortingOrder = 2;
            fx.sphereRadius = C(0f, 0.3f, 0.06f, 2.4f, 0.12f, 2.9f, 0.4f, 3.2f, 0.6f, 2.6f, 0.74f, 0.8f, 0.8f, 0f);
            fx.sphereThreshold = C(0f, -0.1f, 0.03f, -0.1f, 0.08f, 0.3f, 0.25f, 0.48f, 0.8f, 0.6f);
            fx.sphereOpacity = C(0f, 1f, 0.6f, 1f, 0.8f, 0.7f);
            fx.sphereDissolve = C(0f, 0f, 0.55f, 0f, 0.8f, 0.5f);
            fx.coreRadius = C(0f, 0.4f, 0.04f, 1.8f, 0.1f, 1.4f, 0.22f, 0f);

            fx.flashLight = AddLight(root.transform, center, new Color(0.45f, 0.8f, 1f), 10f);
            fx.lightIntensity = C(0f, 0f, 0.03f, 4f, 0.15f, 2.5f, 0.7f, 0f);

            // 8 rayos largos que duran 0,3 s y se erosionan al 33 % de su vida.
            var rays = Create(root.transform, "Rays", M("RB_Ray"), center)
                .Life(0.3f).Speed(0.01f).Size(0.32f).Sphere(0.1f).Burst(8)
                .Stretch(20f).AlphaOverLife(0f, 1f, 0.33f, 0f, 1f, 0f).Order(3);

            // 50 chispas cortas con gravedad.
            var sparks = Create(root.transform, "Sparks", M("RB_Ray"), center)
                .Life(0.5f, 1f).Speed(9f, 14f).Size(0.1f, 0.16f).Sphere(0.2f).Burst(50)
                .Gravity(1f).Drag(1.5f).Stretch(6f, 0.05f).AlphaOverLife(0f, 1f, 0.5f, 0.6f, 1f, 0f).Order(3);

            // Onda expansiva en el suelo.
            var shock = Create(root.transform, "Shockwave", M("RB_Shock"), new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.45f).Size(1f).Burst(1)
                .Rotation3D(Vector3.zero, new Vector3(0f, 360f, 0f))
                .SizeOverLife(C(0f, 0.6f, 0.25f, 5f, 1f, 6.5f)).AlphaOverLife(0f, 1f, 0.4f, 0.85f, 1f, 0f).Order(0);

            // Polvo azulado (humo toon sin fuego).
            var dust = Create(root.transform, "Dust", M("RB_Dust"), new Vector3(0f, 0.3f, 0f))
                .GroundCircle(2f, 0.3f).Speed(1.5f, 3.5f).Drag(2.5f).Life(0.8f, 1.3f).Size(1f, 1.8f).Burst(16)
                .Velocity(new Vector3(0f, 0.6f, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0);

            // Destellos Sheikah flotando.
            var glints = Create(root.transform, "Glints", M("RB_Glint"), center)
                .Sphere(2.6f).Life(0.5f, 1.1f).Size(0.12f, 0.3f).Speed(0.2f, 0.8f).Burst(24)
                .Velocity(new Vector3(0f, 0.8f, 0f)).Spin(-180f, 180f)
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.6f, 1f, 1f, 0f).Order(2);

            fx.particles.Add(Cue(rays, 0f));
            fx.particles.Add(Cue(sparks, 0f));
            fx.particles.Add(Cue(shock, 0.02f));
            fx.particles.Add(Cue(dust, 0.05f));
            fx.particles.Add(Cue(glints, 0.12f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0f, strength = 0.25f, duration = 0.35f });
            return root;
        }

        // =====================================================================
        // 2. EXPLOSIÓN TOON  (80.lv: bola de pinchos, onda, escombros,
        //    humo caliente y disipación lenta)
        // =====================================================================
        static GameObject BuildExplosion()
        {
            var root = new GameObject("VFX_Explosion");
            var fx = root.AddComponent<VfxTimeline>();
            fx.duration = 3.6f;
            var c = new Vector3(0f, 0.9f, 0f);

            // --- Destello inicial
            var flash = Create(root.transform, "Flash", M("EX_Flash"), c)
                .Life(0.14f).Size(5.5f).Burst(1).Rotation(0f, 45f).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);
            var flashCircle = Create(root.transform, "FlashCircle", M("EX_FlashCircle"), c)
                .Life(0.1f).Size(3.2f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(5);

            // --- Fase 1: bola de pinchos que crece y se erosiona desde las puntas
            var ball = Create(root.transform, "SpikyBall", M("EX_Ball"), c)
                .Mesh(BotwMeshes.SpikyBall).Life(0.5f).Size(1.6f).Burst(1)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-90f, 90f, true)
                .SizeOverLife(C(0f, 0.12f, 0.22f, 1f, 1f, 1.2f))
                .Custom(Curve(0f, 0f, 0.3f, 0.05f, 1f, 1f), Rand(0f, 1f), Const(0f), Const(0f)).Order(2);
            var ballB = Create(root.transform, "SpikyBallB", M("EX_Ball"), c + new Vector3(0.2f, 0.3f, 0f))
                .Mesh(BotwMeshes.SpikyBall).Life(0.42f).Size(1.15f).Burst(1)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-120f, 120f, true)
                .SizeOverLife(C(0f, 0.15f, 0.25f, 1f, 1f, 1.15f))
                .Custom(Curve(0f, 0f, 0.25f, 0.05f, 1f, 1f), Rand(0f, 1f), Const(0f), Const(0f)).Order(3);

            // --- Fase 2: onda expansiva (aire + suelo)
            var shock = Create(root.transform, "Shockwave", M("EX_Shock"), c)
                .Life(0.25f).Size(1f).Burst(1).SizeOverLife(C(0f, 1f, 0.3f, 7f, 1f, 9.5f))
                .AlphaOverLife(0f, 1f, 0.5f, 0.7f, 1f, 0f).Order(4);
            var groundShock = Create(root.transform, "GroundShock", M("EX_GroundShock"), new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.5f).Size(1f).Burst(1)
                .SizeOverLife(C(0f, 1f, 0.3f, 6f, 1f, 8f)).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(0);

            // Lenguas de fuego: emisores de medio toroide que miran a cámara (truco de 80.lv).
            var tongues = Create(root.transform, "FireTongues", M("EX_Tongue"), c)
                .HalfDonut(1.1f, 0.25f).Speed(6f, 10f).Life(0.22f, 0.38f).Size(0.45f, 0.75f).Burst(22)
                .Stretch(3.2f, 0.03f).Drag(3f).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(1);
            tongues.ps.gameObject.AddComponent<FaceCamera>();
            var tongues2 = Create(root.transform, "FireTonguesBig", M("EX_Tongue"), c)
                .HalfDonut(1.6f, 0.3f).Speed(5f, 8f).Life(0.3f, 0.45f).Size(0.6f, 1f).Burst(14)
                .Stretch(3f, 0.03f).Drag(3f).AlphaOverLife(0f, 1f, 0.4f, 0.8f, 1f, 0f).Order(1);
            tongues2.ps.gameObject.AddComponent<FaceCamera>();

            // --- Fase 3: escombros y ascuas (sensación de impacto)
            var debris = Create(root.transform, "Debris", M("EX_Debris"), new Vector3(0f, 0.3f, 0f))
                .Mesh(BotwMeshes.Rock).Hemisphere(0.6f).Speed(7f, 13f).Life(1.6f, 2.4f).Size(0.14f, 0.38f)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-400f, 400f, true)
                .Gravity(2.4f).Burst(16).Collide(0.35f, 0.45f)
                .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(0);
            var embers = Create(root.transform, "Embers", M("EX_Ember"), c)
                .Sphere(0.6f).Speed(8f, 18f).Life(0.4f, 1.1f).Size(0.07f, 0.14f).Burst(45)
                .Gravity(0.9f).Drag(2f).Stretch(5f, 0.04f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(2);

            // --- Fase 4: humo caliente (el fuego se retira hacia dentro y queda humo)
            var smoke = Create(root.transform, "FireSmoke", M("EX_Smoke"), c)
                .Sphere(0.9f).Speed(3f, 6.5f).Drag(3.5f).Life(1.5f, 2.3f).Size(1.4f, 2.4f).Burst(22)
                .Velocity(new Vector3(0f, 0.7f, 0f))
                .SizeOverLife(C(0f, 0.45f, 0.15f, 1f, 1f, 1.25f))
                .Custom(Curve(0f, 0f, 0.12f, 0.12f, 0.45f, 1f), Rand(-0.05f, 0.08f), Curve(0f, 0f, 0.55f, 0.08f, 1f, 1f), Rand(0f, 1f))
                .Order(1);

            // --- Fase 5: disipación lenta con remolino (rotación del ruido sobre la vida)
            var darkSmoke = Create(root.transform, "DarkSmoke", M("EX_DarkSmoke"), c + new Vector3(0f, 0.8f, 0f))
                .Sphere(1f).Speed(0.5f, 1.5f).Life(2.4f, 3.2f).Size(2.2f, 3.2f).Burst(7)
                .Velocity(new Vector3(0f, 1f, 0f))
                .SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 1.3f))
                .Custom(Const(1f), Rand(0f, 0.1f), Curve(0f, 0f, 0.6f, 0.1f, 1f, 1f),
                    new ParticleSystem.MinMaxCurve(1f, C(0f, 0f, 1f, 0.15f), C(0f, 1f, 1f, 1.35f)))
                .Order(0);

            fx.flashLight = AddLight(root.transform, c, new Color(1f, 0.62f, 0.3f), 14f);
            fx.lightIntensity = C(0f, 0f, 0.03f, 6f, 0.2f, 3f, 0.9f, 0f);

            fx.particles.Add(Cue(flash, 0f));
            fx.particles.Add(Cue(flashCircle, 0f));
            fx.particles.Add(Cue(ball, 0f));
            fx.particles.Add(Cue(ballB, 0.03f));
            fx.particles.Add(Cue(tongues, 0f));
            fx.particles.Add(Cue(tongues2, 0.06f));
            fx.particles.Add(Cue(shock, 0.03f));
            fx.particles.Add(Cue(groundShock, 0.03f));
            fx.particles.Add(Cue(debris, 0.02f));
            fx.particles.Add(Cue(embers, 0.02f));
            fx.particles.Add(Cue(smoke, 0.04f));
            fx.particles.Add(Cue(darkSmoke, 0.25f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0f, strength = 0.5f, duration = 0.5f });
            return root;
        }

        // =====================================================================
        // 3. RAYO GUARDIÁN  (80.lv: láser, carga, rayo + tiras, flares, impacto)
        // =====================================================================
        static GameObject BuildGuardianBeam()
        {
            var root = new GameObject("VFX_GuardianBeam");
            var fx = root.AddComponent<GuardianBeamVfx>();
            const float fire = 2.7f;
            fx.fireTime = fire;
            fx.laserEnd = fire;
            fx.travelTime = 0.08f;
            fx.beamEnd = fire + 0.6f;
            fx.duration = fire + 0.08f + 3.6f;
            fx.beamWidth = C(0f, 0.06f, 0.04f, 0.42f, 0.1f, 0.3f, 0.4f, 0.28f, 0.6f, 0f);
            fx.glowScale = 1.9f;

            var eye = new GameObject("Eye").transform;
            eye.SetParent(root.transform, false);
            eye.localPosition = GuardianEyeLocal;
            var target = new GameObject("Target").transform;
            target.SetParent(root.transform, false);
            target.localPosition = new Vector3(0f, 0.6f, 0f);
            fx.eye = eye;
            fx.target = target;

            fx.laser = AddLine(root.transform, "Laser", M("GB_Laser"), 0.05f, AlphaGradient(1f, 1f));
            fx.beamGlow = AddMesh(root.transform, "BeamGlow", BotwMeshes.Sphere, M("GB_BeamGlow"), Vector3.zero, Vector3.one, false);
            fx.beamGlow.sortingOrder = 1;
            fx.beamCore = AddMesh(root.transform, "BeamCore", BotwMeshes.Sphere, M("GB_Beam"), Vector3.zero, Vector3.one, false);
            fx.beamCore.sortingOrder = 2;

            // Punto del láser en el objetivo.
            var dot = Create(root.transform, "LaserDot", M("GB_Dot"), target.localPosition)
                .Life(0.12f, 0.2f).Size(0.5f, 0.9f).Rate(30f).Duration(fire - 0.05f).Rotation(0f, 90f)
                .AlphaOverLife(0f, 1f, 1f, 0.2f).Order(3);
            fx.laserDot = dot.ps.transform;

            // Carga en el ojo.
            var chargeSparks = Create(eye, "ChargeSparks", M("GB_Spark"))
                .Sphere(2.2f, 0f).Radial(-7f).Life(0.3f).Size(0.08f, 0.14f).Rate(70f).Duration(1.15f)
                .Stretch(4f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0.6f).Order(3);
            var chargeOrb = Create(eye, "ChargeOrb", M("GB_Orb"))
                .Life(1.3f).Size(1.8f).Burst(1).SizeOverLife(C(0f, 0.1f, 0.7f, 0.75f, 0.9f, 1f, 1f, 0.6f)).Order(4);
            var chargeRings = Create(eye, "ChargeRings", M("GB_Ring"))
                .Life(0.35f).Size(4f).Rate(5f).Duration(0.9f).SizeOverLife(C(0f, 1f, 1f, 0.1f))
                .AlphaOverLife(0f, 0f, 0.3f, 1f, 1f, 0.3f).Order(2);

            // Disparo: dos lens flares (uno pequeño y uno alargado que cruza la pantalla).
            var muzzle = Create(eye, "MuzzleFlare", M("GB_Flare"))
                .Life(0.45f).Size(4.5f).Burst(1).Rotation(0f, 30f)
                .SizeOverLife(C(0f, 1.2f, 0.2f, 0.8f, 1f, 0.5f)).AlphaOverLife(0f, 1f, 0.6f, 0.7f, 1f, 0f).Order(6);
            var wideFlare = Create(eye, "WideFlare", M("GB_Flare"))
                .Life(0.3f).Size3D(new Vector3(14f, 0.7f, 1f)).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(7);

            // Tiras de energía alrededor del rayo (malla curva, rotación y escala alternas).
            var stripes = Create(root.transform, "Stripes", M("GB_Stripe"))
                .LocalSpace().Mesh(BotwMeshes.Arc, ParticleSystemRenderSpace.Local).Box(new Vector3(0f, 0f, 1f))
                .Life(0.12f, 0.26f).Size(0.9f, 1.5f).Rotation3D(Vector3.zero, new Vector3(0f, 0f, 360f))
                .Spin(-900f, 900f).Rate(70f).Duration(0.55f).Burst(14)
                .SizeOverLife(C(0f, 0.5f, 0.5f, 1.1f, 1f, 0.8f)).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3);
            fx.stripes = stripes.ps;

            var impactFlare = Create(target, "ImpactFlare", M("GB_Flare"))
                .Life(0.3f).Size(6f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);

            // Impacto: la explosión toon como sub-efecto anidado.
            var explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionPath);
            var explosion = (GameObject)PrefabUtility.InstantiatePrefab(explosionPrefab, root.transform);
            explosion.transform.localPosition = Vector3.zero;

            fx.flashLight = AddLight(eye, Vector3.zero, new Color(1f, 0.35f, 0.7f), 8f);
            fx.lightIntensity = C(0f, 0f, 1.5f, 0f, fire - 0.05f, 2.5f, fire + 0.02f, 5f, fire + 0.2f, 1.5f, fire + 0.6f, 0f);

            fx.particles.Add(Cue(dot, 0f));
            fx.particles.Add(Cue(chargeSparks, 1.5f));
            fx.particles.Add(Cue(chargeOrb, 1.45f));
            fx.particles.Add(Cue(chargeRings, 1.7f));
            fx.particles.Add(Cue(muzzle, fire));
            fx.particles.Add(Cue(wideFlare, fire));
            fx.particles.Add(Cue(stripes, fire + 0.02f));
            fx.particles.Add(Cue(impactFlare, fire + 0.07f));
            fx.subEffects.Add(new VfxTimeline.TimelineCue { timeline = explosion.GetComponent<VfxTimeline>(), time = fire + 0.08f });
            fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = fire + 0.08f, timeScale = 0.15f, duration = 0.18f });
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = fire + 0.08f, strength = 0.6f, duration = 0.6f });
            return root;
        }

        // =====================================================================
        // 4. FLECHA ANCESTRAL  (80.lv: carga, lente gigante con cáusticas,
        //    portal que se abre/cierra y objetos absorbidos al centro)
        // =====================================================================
        static GameObject BuildAncientArrow()
        {
            var root = new GameObject("VFX_AncientArrow");
            var fx = root.AddComponent<AncientArrowVfx>();
            fx.duration = 3.2f;
            fx.flightTime = 0.25f;
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
            fx.trail = AddLine(root.transform, "ArrowTrail", M("AA_Trail"), 0.14f, AlphaGradient(0f, 1f));

            var impactFlash = Create(impact, "ImpactFlash", M("AA_Flash"))
                .Life(0.18f).Size(4f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);

            // Lente gigante con el marco Sheikah, girando.
            var lens = Create(impact, "Lens", M("AA_Lens"))
                .Life(1.25f).Size(5.6f).Burst(1).Spin(70f, 70f)
                .SizeOverLife(C(0f, 0f, 0.1f, 1.08f, 0.18f, 1f, 0.85f, 1.02f, 1f, 1.15f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(2);
            var lensCaustics = Create(impact, "LensCaustics", M("AA_Caustics"))
                .Life(1.15f).Size(4.8f).Burst(1).SizeOverLife(C(0f, 0f, 0.12f, 1f, 1f, 1f))
                .AlphaOverLife(0f, 1f, 0.85f, 1f, 1f, 0f).Order(3);

            // Portal con UV polares: el ruido gira y cae hacia el centro.
            var portalSize = C(0f, 0f, 0.15f, 1.1f, 0.25f, 1f, 0.85f, 0.95f, 1f, 0f);
            var portal = Create(impact, "Portal", M("AA_Portal"))
                .Life(1.05f).Size(3f).Burst(1).Spin(-200f, -200f).SizeOverLife(portalSize).Order(4);
            var portalRim = Create(impact, "PortalRim", M("AA_PortalRim"))
                .Life(1.05f).Size(3.7f).Burst(1).Spin(-120f, -120f).SizeOverLife(portalSize).Order(5);

            // Todo es absorbido hacia el centro.
            var converge = Create(impact, "Converge", M("AA_Spark"))
                .Sphere(5f, 0f).Radial(-15f, 2f).Life(0.32f).Size(0.08f, 0.15f).Rate(110f).Duration(0.85f)
                .Stretch(4f, 0.04f).AlphaOverLife(0f, 0.4f, 0.2f, 1f, 1f, 0.5f).Order(3);
            var pebbles = Create(root.transform, "Pebbles", M("EX_Debris"), new Vector3(0f, 0.2f, 0f))
                .Mesh(BotwMeshes.Rock).GroundCircle(4f, 0.3f).Radial(-8f).Life(0.5f).Size(0.1f, 0.22f)
                .Rate(18f).Duration(0.8f).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-500f, 500f, true)
                .SizeOverLife(C(0f, 1f, 0.7f, 1f, 1f, 0f)).Order(0);
            var pebbleVel = pebbles.ps.velocityOverLifetime;
            pebbleVel.y = new ParticleSystem.MinMaxCurve(2.4f);

            // Colapso final.
            var collapse = Create(impact, "CollapseFlash", M("AA_Flash"))
                .Life(0.22f).Size(5f).Burst(1).AlphaOverLife(0f, 1f, 1f, 0f).Order(6);
            var collapseRing = Create(impact, "CollapseRing", M("AA_Shock"))
                .Life(0.4f).Size(1f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.3f, 6f, 1f, 8f))
                .AlphaOverLife(0f, 1f, 0.5f, 0.7f, 1f, 0f).Order(5);
            var motes = Create(impact, "Motes", M("AA_Mote"))
                .Sphere(1.5f).Speed(1f, 3f).Drag(1.5f).Velocity(new Vector3(0f, 0.6f, 0f)).Life(0.8f, 1.6f)
                .Size(0.1f, 0.25f).Burst(30).Spin(-180f, 180f).AlphaOverLife(0f, 1f, 0.7f, 0.8f, 1f, 0f).Order(4);

            fx.flashLight = AddLight(impact, Vector3.zero, new Color(0.45f, 0.85f, 1f), 10f);
            fx.lightIntensity = C(0f, 0f, 0.25f, 0f, 0.28f, 4f, 0.5f, 2f, 1.3f, 2.5f, 1.42f, 5f, 1.8f, 0f);

            fx.particles.Add(Cue(impactFlash, 0.25f));
            fx.particles.Add(Cue(lens, 0.3f));
            fx.particles.Add(Cue(lensCaustics, 0.33f));
            fx.particles.Add(Cue(portal, 0.38f));
            fx.particles.Add(Cue(portalRim, 0.38f));
            fx.particles.Add(Cue(converge, 0.4f));
            fx.particles.Add(Cue(pebbles, 0.4f));
            fx.particles.Add(Cue(collapse, 1.38f));
            fx.particles.Add(Cue(collapseRing, 1.4f));
            fx.particles.Add(Cue(motes, 1.42f));
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 0.25f, strength = 0.2f, duration = 0.25f });
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = 1.4f, strength = 0.35f, duration = 0.4f });
            fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = 0.26f, timeScale = 0.3f, duration = 0.08f });
            return root;
        }
    }
}
