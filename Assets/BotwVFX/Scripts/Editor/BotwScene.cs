using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Construye la escena de demostración: pradera toon, cielo degradado,
    /// árboles, rocas, un Guardián de atrezo y una estación por efecto.
    /// </summary>
    public static class BotwScene
    {
        public const string ScenePath = "Assets/BotwVFX/Scenes/BotwVFX_Demo.unity";
        public const string ProfilePath = "Assets/BotwVFX/Settings/PP_BotwVFX.asset";

        public static readonly Vector3[] Stations =
        {
            new Vector3(-36f, 0f, 0f),
            new Vector3(-12f, 0f, 0f),
            new Vector3(12f, 0f, 0f),
            new Vector3(36f, 0f, 0f),
        };

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rng = new System.Random(1234);

            // ---------------- Luz y cielo
            BuildLighting();

            // ---------------- Terreno
            var env = new GameObject("Environment").transform;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(env, false);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = BotwMaterials.Get("MAT_Ground");

            foreach (var s in Stations)
            {
                var dirt = CreatePrimitive(PrimitiveType.Cylinder, "DirtPatch", env, BotwMaterials.Get("MAT_Dirt"));
                dirt.transform.position = s + new Vector3(0f, 0.005f, 0f);
                dirt.transform.localScale = new Vector3(9f, 0.005f, 9f);
                dirt.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            // Rocas medianas (fuera de las estaciones y del recorrido del rayo).
            var rockMat = BotwMaterials.Get("MAT_Rock");
            int placed = 0;
            for (int i = 0; i < 400 && placed < 34; i++)
            {
                var p = new Vector3(Range(rng, -70f, 70f), 0f, Range(rng, -6f, 45f));
                if (!IsFree(p, 7f))
                    continue;
                float size = Range(rng, 0.6f, 2.6f);
                AddRock(env, rockMat, p, size, rng);
                placed++;
            }
            // Mesetas lejanas (la niebla las funde con el horizonte).
            for (int i = 0; i < 9; i++)
            {
                var p = new Vector3(-140f + i * 35f + Range(rng, -10f, 10f), -2f, Range(rng, 95f, 130f));
                var rock = AddRock(env, rockMat, p, 1f, rng);
                rock.transform.localScale = new Vector3(Range(rng, 16f, 26f), Range(rng, 10f, 18f), Range(rng, 14f, 20f));
            }

            // Árboles redondos.
            var bark = BotwMaterials.Get("MAT_Bark");
            var leaves = BotwMaterials.Get("MAT_Leaves");
            placed = 0;
            for (int i = 0; i < 400 && placed < 26; i++)
            {
                var p = new Vector3(Range(rng, -75f, 75f), 0f, Range(rng, 14f, 55f));
                if (!IsFree(p, 10f))
                    continue;
                AddTree(env, bark, leaves, p, Range(rng, 0.8f, 1.4f), rng);
                placed++;
            }

            // ---------------- Efectos
            var effectsRoot = new GameObject("Effects").transform;
            var bomb = PlaceEffect(BotwEffects.RemoteBombPath, effectsRoot, Stations[0]);
            var explosion = PlaceEffect(BotwEffects.ExplosionPath, effectsRoot, Stations[1]);
            var guardian = PlaceEffect(BotwEffects.GuardianBeamPath, effectsRoot, Stations[2]);
            var arrow = PlaceEffect(BotwEffects.AncientArrowPath, effectsRoot, Stations[3]);
            BuildGuardianStandIn(env, Stations[2] + BotwEffects.GuardianEyeLocal, Stations[2] + new Vector3(0f, 0.6f, 0f));

            // ---------------- Cámara y post-proceso
            var cam = BuildCamera();
            AddVolume(BuildProfile());

            new GameObject("VfxDirector").AddComponent<VfxDirector>();

            var demo = new GameObject("Demo").AddComponent<DemoController>();
            demo.targetCamera = cam;
            demo.stations = new[]
            {
                new DemoController.Station
                {
                    name = "Bomba remota", effect = bomb,
                    description = "Esfera con fresnel + brillo de intersección con el suelo, 8 rayos largos y 50 chispas (vídeo de Daniel Ilett).",
                    focus = Stations[0] + new Vector3(0f, 1.4f, 0f), yaw = 15f, pitch = 14f, distance = 12f,
                },
                new DemoController.Station
                {
                    name = "Explosión", effect = explosion,
                    description = "5 fases: bola de pinchos, onda expansiva, escombros, humo caliente toon y disipación lenta (80.lv).",
                    focus = Stations[1] + new Vector3(0f, 2.4f, 0f), yaw = 12f, pitch = 12f, distance = 17f,
                },
                new DemoController.Station
                {
                    name = "Rayo Guardián", effect = guardian,
                    description = "Láser de apuntado, carga, rayo con tiras de energía, lens flares e impacto con cámara lenta (80.lv).",
                    focus = Stations[2] + new Vector3(-4.2f, 2.6f, 3.6f), yaw = 42f, pitch = 10f, distance = 19f,
                },
                new DemoController.Station
                {
                    name = "Flecha ancestral", effect = arrow,
                    description = "Lente Sheikah con cáusticas, portal en UV polares que lo absorbe todo y colapso final (80.lv).",
                    focus = Stations[3] + new Vector3(-2.5f, 1.8f, 0f), yaw = 8f, pitch = 8f, distance = 14f,
                },
            };
            DemoController.PoseCamera(cam, demo.stations[0]);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath, first: true);
        }

        /// <summary>
        /// Añade la escena a Build Settings sin quitar las demás (la demo BotW queda la primera).
        /// </summary>
        internal static void AddToBuildSettings(string path, bool first)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            var entry = new EditorBuildSettingsScene(path, true);
            if (first)
                scenes.Insert(0, entry);
            else
                scenes.Add(entry);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // Sol, cielo degradado, luz ambiente y niebla (compartido con la escena Emerald).
        internal static Light BuildLighting()
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(38f, 40f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 1f;

            var sky = BotwMaterials.Get("MAT_Sky");
            sky.SetVector("_SunDirection", -sunGo.transform.forward);
            RenderSettings.skybox = sky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.75f, 0.92f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.68f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.33f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.8f, 0.9f, 0.97f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 220f;
            return sun;
        }

        // Cámara HDR con post-proceso y SMAA (compartida con la escena Emerald).
        internal static Camera BuildCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 45f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            cam.allowHDR = true;
            cam.clearFlags = CameraClearFlags.Skybox;
            camGo.AddComponent<AudioListener>();
            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return cam;
        }

        internal static Volume AddVolume(VolumeProfile profile)
        {
            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            return volume;
        }

        /// <summary>Perfil de post-proceso existente (no lo regenera); lo crea si falta.</summary>
        internal static VolumeProfile LoadOrBuildProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            return profile != null ? profile : BuildProfile();
        }

        static VfxTimeline PlaceEffect(string prefabPath, Transform parent, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = position;
            return go.GetComponent<VfxTimeline>();
        }

        static VolumeProfile BuildProfile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath));
            AssetDatabase.DeleteAsset(ProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1f);
            bloom.intensity.Override(1f);
            bloom.scatter.Override(0.6f);
            bloom.highQualityFiltering.Override(true);

            var tonemap = profile.Add<Tonemapping>(true);
            tonemap.mode.Override(TonemappingMode.None);

            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(12f);
            color.contrast.Override(6f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.5f);

            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        // ------------------------------------------------------------ atrezo

        static bool IsFree(Vector3 p, float stationRadius)
        {
            foreach (var s in Stations)
            {
                if (Vector2.Distance(new Vector2(p.x, p.z), new Vector2(s.x, s.z)) < stationRadius)
                    return false;
            }
            // Pasillo del rayo Guardián (del ojo al objetivo, proyectado en el suelo).
            var a = new Vector2(Stations[2].x + BotwEffects.GuardianEyeLocal.x, Stations[2].z + BotwEffects.GuardianEyeLocal.z);
            var b = new Vector2(Stations[2].x, Stations[2].z);
            if (DistanceToSegment(new Vector2(p.x, p.z), a, b) < 5f)
                return false;
            // Zona delante de las cámaras.
            return p.z > 5f || Mathf.Abs(p.x) > 55f;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        internal static GameObject AddRock(Transform parent, Material mat, Vector3 position, float size, System.Random rng)
        {
            var go = new GameObject("Rock");
            go.transform.SetParent(parent, false);
            go.transform.position = position + new Vector3(0f, size * 0.25f, 0f);
            go.transform.rotation = Quaternion.Euler(Range(rng, -10f, 10f), Range(rng, 0f, 360f), Range(rng, -10f, 10f));
            go.transform.localScale = new Vector3(size * Range(rng, 0.9f, 1.4f), size * Range(rng, 0.7f, 1.1f), size);
            go.AddComponent<MeshFilter>().sharedMesh = BotwMeshes.Rock;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        internal static void AddTree(Transform parent, Material bark, Material leaves, Vector3 position, float scale, System.Random rng)
        {
            var tree = new GameObject("Tree").transform;
            tree.SetParent(parent, false);
            tree.position = position;
            tree.localScale = Vector3.one * scale;
            var trunk = CreatePrimitive(PrimitiveType.Cylinder, "Trunk", tree, bark);
            trunk.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            trunk.transform.localScale = new Vector3(0.45f, 1.6f, 0.45f);
            for (int i = 0; i < 3; i++)
            {
                var blob = new GameObject("Canopy");
                blob.transform.SetParent(tree, false);
                blob.transform.localPosition = new Vector3(Range(rng, -0.8f, 0.8f), 3.6f + i * 0.7f + Range(rng, 0f, 0.4f), Range(rng, -0.8f, 0.8f));
                blob.transform.localScale = Vector3.one * Range(rng, 1.6f, 2.3f) * (1f - i * 0.18f);
                blob.AddComponent<MeshFilter>().sharedMesh = BotwMeshes.Sphere;
                blob.AddComponent<MeshRenderer>().sharedMaterial = leaves;
            }
        }

        // Guardián de atrezo: cúpula + ojo + cuatro patas. Solo para dar contexto al rayo.
        static void BuildGuardianStandIn(Transform parent, Vector3 eye, Vector3 target)
        {
            var root = new GameObject("Guardian (atrezo)").transform;
            root.SetParent(parent, false);
            var toTarget = (target - eye).normalized;
            Vector3 bodyCenter = eye - toTarget * 1.45f + Vector3.up * 0.25f;

            var body = new GameObject("Body");
            body.transform.SetParent(root, false);
            body.transform.position = bodyCenter;
            body.transform.localScale = new Vector3(1.6f, 1.2f, 1.6f);
            body.AddComponent<MeshFilter>().sharedMesh = BotwMeshes.Sphere;
            body.AddComponent<MeshRenderer>().sharedMaterial = BotwMaterials.Get("MAT_GuardianShell");

            // Faldón oscuro bajo la cúpula.
            var skirt = new GameObject("Skirt");
            skirt.transform.SetParent(root, false);
            skirt.transform.position = bodyCenter - Vector3.up * 0.35f;
            skirt.transform.localScale = new Vector3(1.55f, 2.2f, 1.55f);
            skirt.AddComponent<MeshFilter>().sharedMesh = BotwMeshes.Torus;
            skirt.AddComponent<MeshRenderer>().sharedMaterial = BotwMaterials.Get("MAT_GuardianLeg");

            var lens = new GameObject("EyeLens");
            lens.transform.SetParent(root, false);
            lens.transform.position = eye;
            lens.transform.localScale = Vector3.one * 0.32f;
            lens.AddComponent<MeshFilter>().sharedMesh = BotwMeshes.Sphere;
            var lensRenderer = lens.AddComponent<MeshRenderer>();
            lensRenderer.sharedMaterial = BotwMaterials.Get("GB_EyeLens");
            lensRenderer.shadowCastingMode = ShadowCastingMode.Off;

            var legMat = BotwMaterials.Get("MAT_GuardianLeg");
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 hip = bodyCenter + dir * 0.9f - Vector3.up * 0.5f;
                Vector3 knee = bodyCenter + dir * 2.4f + Vector3.up * 0.6f;
                Vector3 foot = new Vector3(bodyCenter.x, 0f, bodyCenter.z) + dir * 3.4f;
                AddLimb(root, legMat, hip, knee, 0.32f);
                AddLimb(root, legMat, knee, foot, 0.26f);
            }
        }

        static void AddLimb(Transform parent, Material mat, Vector3 a, Vector3 b, float thickness)
        {
            var limb = CreatePrimitive(PrimitiveType.Cylinder, "Leg", parent, mat);
            limb.transform.position = (a + b) * 0.5f;
            limb.transform.rotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
            limb.transform.localScale = new Vector3(thickness, Vector3.Distance(a, b) * 0.5f, thickness);
        }

        internal static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        internal static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);
    }
}
