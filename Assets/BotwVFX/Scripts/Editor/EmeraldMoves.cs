using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EmeraldArena.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static BotwVfx.EditorTools.Fx;
using Object = UnityEngine.Object;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Importa los 165 movimientos Emerald (Assets/EmeraldMoveVFX) con estilo BotW:
    /// - conserva la animación horneada original (EmeraldVfxPlayer) con el shader "BotwVFX/Emerald Toon",
    /// - calcula el instante de impacto a partir de ImpactStrength,
    /// - si el movimiento tiene receta propia (EmeraldRecipes.M###, diseñada a partir de sus referencias)
    ///   la usa: timeline de 3,6 s, clip horneado recolocado y capas de EmeraldLayers;
    /// - si no, añade las capas Shuriken genéricas por tipo (tabla de datos, reserva hasta tener receta),
    /// - genera la escena de galería EmeraldMoves_Demo.
    /// Menú: Tools/BotW VFX/Import Emerald Moves.
    /// Línea de comandos: -executeMethod BotwVfx.EditorTools.EmeraldMoves.BuildBatch
    /// Necesita las texturas, mallas y materiales BotW ("Rebuild Everything" al menos una vez).
    /// </summary>
    public static class EmeraldMoves
    {
        public const string DataFolder = "Assets/EmeraldMoveVFX/Data";
        public const string PrefabFolder = "Assets/BotwVFX/Prefabs/Emerald";
        public const string ScenePath = "Assets/BotwVFX/Scenes/EmeraldMoves_Demo.unity";

        // Marco local del módulo original (distancia 6).
        public static readonly Vector3 Attacker = new Vector3(-3f, 0f, 0f);
        public static readonly Vector3 Target = new Vector3(3f, 0f, 0f);

        // Impacto = primer fotograma que alcanza esta fracción del pico de ImpactStrength.
        const float StrongFraction = 0.5f;
        // Sin señal de impacto: este instante (fracción de la animación).
        const float FallbackImpact = 0.6f;
        // Cámara lenta solo en los impactos más fuertes: pico >= 0.99 sostenido al menos 3 fotogramas.
        const float HeavyStrength = 0.99f;
        const int HeavyFrames = 3;
        // Si el centro de la acción en el impacto queda a la izquierda de este X, el foco es el atacante.
        const float SelfFocusX = -1.5f;
        // Tiempo que dejan las capas después del impacto.
        const float Tail = 1.8f;

        [Serializable]
        class Entry
        {
            public int id;
            public string name;
            public string type;
            public string color;
            public string family;
            public string description;
            public string file;
            public int frames;
        }

        [Serializable]
        class Catalogue
        {
            public string source;
            public string commit;
            public float duration;
            public Entry[] moves;
        }

        // ------------------------------------------------------------ tabla de tipos

        enum Layer
        {
            ImpactStar, ImpactRays, Dust, Debris, Embers, FireSmoke, Droplets, Mist, Crackle, Glints,
            Leaves, IceShards, Motes, Pulse, Bubbles, PoisonSmoke, GhostSmoke, DarkSmoke, Wind, SmallMotes,
        }

        // Capas que solo tienen sentido con un golpe (los movimientos de estado no las llevan).
        static readonly HashSet<Layer> ImpactOnly = new HashSet<Layer>
        {
            Layer.ImpactStar, Layer.ImpactRays, Layer.Dust, Layer.Debris, Layer.Embers, Layer.FireSmoke,
            Layer.Droplets, Layer.Crackle, Layer.IceShards, Layer.Wind,
        };

        class TypeStyle
        {
            public string key;      // nombre ASCII para los materiales EM_<key>_*
            public Color core;      // HDR lineal
            public Color edge;      // HDR lineal
            public Layer[] layers;
        }

        static TypeStyle T(string key, Color core, Color edge, params Layer[] layers) =>
            new TypeStyle { key = key, core = core, edge = edge, layers = layers };

        // Claves = valores de "type" en catalogue.json. Paleta HDR al estilo de BotwMaterials.
        static readonly Dictionary<string, TypeStyle> Types = new Dictionary<string, TypeStyle>
        {
            ["Normal"] = T("Normal", new Color(2.2f, 2.0f, 1.6f), new Color(1.4f, 0.9f, 0.4f), Layer.ImpactStar, Layer.ImpactRays, Layer.Dust),
            ["Lucha"] = T("Fighting", new Color(2.4f, 1.4f, 0.9f), new Color(1.8f, 0.25f, 0.08f), Layer.ImpactStar, Layer.ImpactRays, Layer.Dust, Layer.Debris),
            ["Volador"] = T("Flying", new Color(1.8f, 2.1f, 2.4f), new Color(0.5f, 0.9f, 1.8f), Layer.Wind, Layer.ImpactRays),
            ["Veneno"] = T("Poison", new Color(2.0f, 1.0f, 2.4f), new Color(0.9f, 0.1f, 1.4f), Layer.Bubbles, Layer.PoisonSmoke),
            ["Tierra"] = T("Ground", new Color(2.2f, 1.7f, 0.9f), new Color(1.0f, 0.5f, 0.15f), Layer.Debris, Layer.Dust),
            ["Roca"] = T("Rock", new Color(2.0f, 1.7f, 1.1f), new Color(0.8f, 0.55f, 0.25f), Layer.Debris, Layer.Dust, Layer.ImpactRays),
            ["Bicho"] = T("Bug", new Color(1.9f, 2.3f, 0.6f), new Color(0.6f, 1.2f, 0.05f), Layer.SmallMotes),
            ["Fantasma"] = T("Ghost", new Color(1.6f, 1.2f, 2.6f), new Color(0.5f, 0.1f, 1.4f), Layer.GhostSmoke, Layer.Motes),
            ["Fuego"] = T("Fire", new Color(2f, 1.25f, 0.3f), new Color(2.2f, 0.25f, 0.02f), Layer.Embers, Layer.FireSmoke),
            ["Agua"] = T("Water", new Color(1.2f, 2.2f, 2.8f), new Color(0.05f, 0.5f, 2.0f), Layer.Droplets, Layer.Mist),
            ["Planta"] = T("Grass", new Color(1.6f, 2.6f, 0.9f), new Color(0.15f, 1.1f, 0.1f), Layer.Leaves),
            ["Eléctrico"] = T("Electric", new Color(2.8f, 2.6f, 1.2f), new Color(2.2f, 1.4f, 0.05f), Layer.Crackle, Layer.Glints),
            ["Psíquico"] = T("Psychic", new Color(2.6f, 1.4f, 2.2f), new Color(2.0f, 0.15f, 0.9f), Layer.Pulse, Layer.Motes),
            ["Hielo"] = T("Ice", new Color(1.6f, 2.5f, 2.8f), new Color(0.3f, 1.3f, 2.2f), Layer.IceShards, Layer.Mist, Layer.Glints),
            ["Dragón"] = T("Dragon", new Color(1.6f, 1.4f, 2.8f), new Color(0.4f, 0.2f, 2.2f), Layer.Embers, Layer.Pulse, Layer.Motes),
            ["Siniestro"] = T("Dark", new Color(1.6f, 1.2f, 1.8f), new Color(0.35f, 0.05f, 0.5f), Layer.DarkSmoke, Layer.Motes),
        };

        // Familias "a distancia" (minúsculas, subcadena): llevan motas de anticipación en el atacante.
        static readonly string[] RangedFamilyWords =
        {
            "proyectil", "onda", "cortes a distancia", "remolino", "torbellino", "abanico", "púas", "aguijonazo",
            "agujas", "rociada", "brasas", "chorro", "ola gigante", "haz", "tormenta", "burbujas", "cinta",
            "drenaje", "semillas", "hojas cortantes", "rayo", "polvo", "seda", "dragón de fuego", "descarga",
            "rocas arrancadas", "pulso", "telequin", "orbe", "lodo", "bumerán", "estrella", "mental", "esporas",
            "lluvia", "humo", "niebla", "pegote", "mirada", "luz", "sello",
        };

        // ------------------------------------------------------------ menú / lotes

        [MenuItem("Tools/BotW VFX/Import Emerald Moves", priority = 30)]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            Build();
        }

        // Unity -batchmode -nographics -executeMethod BotwVfx.EditorTools.EmeraldMoves.BuildBatch -quit
        public static void BuildBatch()
        {
            try
            {
                Build();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            if (BotwTextures.Load("T_Noise") == null || BotwMeshes.Ring == null || BotwMaterials.Get("EX_Smoke") == null)
                throw new InvalidOperationException("Faltan los assets BotW: ejecuta antes Tools/BotW VFX/Rebuild Everything.");

            var catalogue = JsonUtility.FromJson<Catalogue>(File.ReadAllText(DataFolder + "/catalogue.json"));
            if (catalogue?.moves == null || catalogue.moves.Length != 165)
                throw new InvalidDataException("catalogue.json debe contener los 165 movimientos.");
            foreach (var e in catalogue.moves)
            {
                if (!Types.ContainsKey(e.type))
                    throw new InvalidDataException($"Tipo sin estilo en la tabla: '{e.type}' (movimiento {e.id}).");
            }
            float animDuration = catalogue.duration > 0f ? catalogue.duration : 2f;

            // Materiales.
            BotwMaterials.CreateEmeraldShared();
            foreach (var style in Types.Values)
                BotwMaterials.CreateEmeraldType(style.key, style.core, style.edge);
            EmeraldLayers.BeginBuild();
            AssetDatabase.SaveAssets();
            var toon = BotwMaterials.Get("EM_Toon");
            if (ShaderUtil.ShaderHasError(toon.shader))
                throw new InvalidOperationException("El shader BotwVFX/Emerald Toon tiene errores de compilación.");

            // Prefabs.
            Directory.CreateDirectory(PrefabFolder);
            var prefabs = new EmeraldMoveVfx[catalogue.moves.Length];
            int slow = 0, noImpact = 0, selfFocus = 0, recipes = 0;
            for (int i = 0; i < catalogue.moves.Length; i++)
            {
                var e = catalogue.moves[i];
                EditorUtility.DisplayProgressBar("Emerald Moves", $"{e.id:000} {e.name}", (float)i / catalogue.moves.Length);
                var go = BuildMove(e, toon, animDuration, out var info);
                string path = $"{PrefabFolder}/{e.id:000}-{SafeName(e.name)}.prefab";
                var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
                prefabs[i] = saved.GetComponent<EmeraldMoveVfx>();
                if (prefabs[i].hasRecipe)
                {
                    recipes++;
                    if (prefabs[i].slowMotion.Count > 0) slow++;
                    continue;
                }
                if (info.slowMotion) slow++;
                if (info.peak <= 0.01f) noImpact++;
                if (info.focus == Attacker) selfFocus++;
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            Debug.Log($"[BotW VFX] Emerald: {prefabs.Length} prefabs en {PrefabFolder} " +
                      $"(con receta propia: {recipes}; genéricos sin impacto: {noImpact}, foco en atacante: {selfFocus}; con cámara lenta: {slow}).");

            BuildScene(prefabs.OrderBy(p => p.moveId).ToArray());
            AssetDatabase.SaveAssets();
            Debug.Log($"[BotW VFX] Emerald: escena {ScenePath} generada.");
        }

        static string SafeName(string name) => string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '-'));

        // ------------------------------------------------------------ análisis del clip

        struct ClipInfo
        {
            public float impactTime;
            public float peak;
            public int heavyFrames;
            public Vector3 focus;
            public bool slowMotion;
            public Vector2 viewMin, viewMax;
        }

        static ClipInfo Analyze(EmeraldVfxPlayer player, float animDuration)
        {
            int n = player.FrameCount;
            var strength = new float[n];
            for (int f = 0; f < n; f++)
            {
                player.RenderAt((float)f / (n - 1), null, false);
                strength[f] = player.ImpactStrength;
            }

            var info = new ClipInfo { peak = strength.Max() };
            int frame;
            if (info.peak > 0.01f)
            {
                float threshold = info.peak * StrongFraction;
                frame = Array.FindIndex(strength, s => s >= threshold);
            }
            else
            {
                frame = Mathf.RoundToInt(FallbackImpact * (n - 1));
            }
            info.heavyFrames = strength.Count(s => s >= HeavyStrength);
            info.slowMotion = info.heavyFrames >= HeavyFrames;
            info.impactTime = Mathf.Max(0.12f, frame / (n - 1f) * animDuration);
            ScanDraws(player.clip.bytes, frame, out float centroidX, out info.viewMin, out info.viewMax);
            info.focus = centroidX < SelfFocusX ? Attacker : Target;
            return info;
        }

        /// <summary>
        /// Recorre los dibujos horneados con el mismo formato que EmeraldVfxPlayer.LoadClip
        /// (el reproductor no expone sus posiciones): X medio en el fotograma del impacto y
        /// caja XY aproximada del efecto (posición ± escala, percentiles 2-98 %, solo dibujos visibles).
        /// </summary>
        static void ScanDraws(byte[] gzipped, int impactFrame, out float centroidX, out Vector2 viewMin, out Vector2 viewMax)
        {
            using var source = new MemoryStream(gzipped);
            using var gzip = new GZipStream(source, CompressionMode.Decompress);
            using var data = new MemoryStream();
            gzip.CopyTo(data);
            data.Position = 0;
            using var r = new BinaryReader(data);
            r.ReadInt32(); // magic (ya validado por LoadClip)
            r.ReadInt32(); // id
            int frames = r.ReadInt32();
            r.ReadInt32();
            int geometries = r.ReadInt32();
            for (int g = 0; g < geometries; g++)
            {
                for (int k = 0; k < 3; k++)
                    data.Seek(r.ReadInt32() * 4L, SeekOrigin.Current); // posiciones, normales, uv
                data.Seek(r.ReadInt32() * 4L, SeekOrigin.Current);     // índices
            }

            var xs = new List<float>();
            var ys = new List<float>();
            float sum = 0f;
            int count = 0;
            for (int f = 0; f < frames; f++)
            {
                int draws = r.ReadInt32();
                r.ReadSingle(); // ImpactStrength
                for (int d = 0; d < draws; d++)
                {
                    data.Seek(20, SeekOrigin.Current); // 5 enteros: id, geometría, modo, orden, flags
                    float px = r.ReadSingle(), py = r.ReadSingle();
                    data.Seek(4 + 16, SeekOrigin.Current); // z, rotación
                    float scale = Mathf.Max(Mathf.Abs(r.ReadSingle()), Mathf.Max(Mathf.Abs(r.ReadSingle()), Mathf.Abs(r.ReadSingle())));
                    data.Seek(12, SeekOrigin.Current); // rgb
                    float alpha = r.ReadSingle();
                    if (f == impactFrame)
                    {
                        sum += px;
                        count++;
                    }
                    if (alpha < 0.05f)
                        continue;
                    xs.Add(px - scale);
                    xs.Add(px + scale);
                    ys.Add(py - scale);
                    ys.Add(py + scale);
                }
            }
            centroidX = count > 0 ? sum / count : 0f;
            if (xs.Count == 0)
            {
                viewMin = new Vector2(-3f, 0f);
                viewMax = new Vector2(3f, 2f);
                return;
            }
            xs.Sort();
            ys.Sort();
            float Q(List<float> a, float q) => a[Mathf.RoundToInt(q * (a.Count - 1))];
            viewMin = new Vector2(Q(xs, 0.02f), Q(ys, 0.02f));
            viewMax = new Vector2(Q(xs, 0.98f), Q(ys, 0.98f));
        }

        // ------------------------------------------------------------ prefab

        static GameObject BuildMove(Entry e, Material toon, float animDuration, out ClipInfo info)
        {
            var clip = AssetDatabase.LoadAssetAtPath<TextAsset>($"{DataFolder}/{e.file}");
            if (clip == null)
                throw new InvalidDataException($"Falta {DataFolder}/{e.file}");
            var style = Types[e.type];

            var root = new GameObject($"{e.id:000}-{SafeName(e.name)}");
            var fx = root.AddComponent<EmeraldMoveVfx>();
            fx.moveId = e.id;
            fx.moveName = e.name;
            fx.moveType = e.type;
            fx.family = e.family;
            fx.description = e.description;
            fx.animDuration = animDuration;

            // Animación horneada original con el shader BotW.
            var baked = new GameObject("Baked");
            baked.transform.SetParent(root.transform, false);
            var player = baked.AddComponent<EmeraldVfxPlayer>();
            player.clip = clip;
            player.effectMaterial = toon;
            player.duration = animDuration;
            player.playOnEnable = false;
            player.loop = false;
            player.previewTime = 0f;
            player.LoadClip();
            if (player.MoveId != e.id || player.FrameCount != e.frames)
                throw new InvalidDataException($"Cabecera de {e.file} no coincide con el catálogo.");
            info = Analyze(player, animDuration);
            player.enabled = false; // en reposo no dibuja (EmeraldMoveVfx lo activa al reproducir)
            fx.player = player;

            fx.bakedOffset = 0f;
            fx.bakedLength = animDuration;
            fx.bakedVisible = true;
            fx.bakedScale = Vector3.one;

            // Receta propia del movimiento (diseñada a partir de sus referencias).
            if (EmeraldRecipes.TryGet(e.id, out var recipe))
            {
                var builder = new EmeraldMoveBuilder(fx, e.id, e.type, ParseColor(e.color), info.impactTime, info.peak, animDuration);
                recipe(builder);
                if (fx.impactTime <= 0f || fx.peakTime <= fx.impactTime)
                    throw new InvalidDataException($"La receta M{e.id:000} no fija los instantes clave (Keys).");
                return root;
            }

            // Reserva: capas genéricas por tipo.
            fx.impactTime = info.impactTime;
            fx.impactStrength = Mathf.Clamp01(info.peak);
            fx.impactPoint = info.focus;
            fx.viewMin = info.viewMin;
            fx.viewMax = info.viewMax;
            fx.duration = Mathf.Max(animDuration, info.impactTime + Tail);
            fx.anticipationTime = info.impactTime * 0.5f;
            fx.peakTime = Mathf.Min(info.impactTime + 0.15f, fx.duration);
            fx.dissipationTime = Mathf.Min(info.impactTime + 0.6f, fx.duration);

            BuildLayers(fx, e, style, info);
            return root;
        }

        static void BuildLayers(EmeraldMoveVfx fx, Entry e, TypeStyle style, ClipInfo info)
        {
            var impact = new GameObject("Impact").transform;
            impact.SetParent(fx.transform, false);
            impact.localPosition = info.focus;

            bool hit = info.peak > 0.01f;
            float s = Mathf.Clamp01(info.peak);
            float k = hit ? Mathf.Lerp(0.55f, 1f, s) : 0.6f; // escala de cantidades y tamaños
            float ti = info.impactTime;
            var c = new Vector3(0f, 0.9f, 0f);

            Material TM(string suffix) => BotwMaterials.Get($"EM_{style.key}_{suffix}");
            void Cue(Fx f, float delay) => fx.particles.Add(new VfxTimeline.ParticleCue { system = f.ps, time = ti + delay });

            // ---- Capa común: destello + chispas + onda en el suelo + luz.
            if (hit)
            {
                Cue(Create(impact, "Flash", TM("Star"), c)
                    .Life(0.14f).Size(Mathf.Lerp(2.2f, 4.2f, s)).Burst(1).Rotation(0f, 45f)
                    .AlphaOverLife(0f, 1f, 1f, 0f).Order(6), 0f);
                Cue(Create(impact, "GroundShock", TM("Shock"), new Vector3(0f, 0.06f, 0f))
                    .Mesh(BotwMeshes.Ring).Life(0.45f).Size(1f).Burst(1)
                    .Rotation3D(Vector3.zero, new Vector3(0f, 360f, 0f))
                    .SizeOverLife(C(0f, 0.4f, 0.3f, 2.2f * k, 1f, 3f * k))
                    .AlphaOverLife(0f, 1f, 0.4f, 0.85f, 1f, 0f).Order(0), 0.02f);
            }
            var glow = Create(impact, "FlashGlow", TM("Glow"), c)
                .Life(hit ? 0.12f : 0.45f).Size(hit ? 2.4f * k : 2.2f).Burst(1)
                .AlphaOverLife(0f, 1f, 1f, 0f).Order(5);
            if (!hit)
                glow.SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f));
            Cue(glow, 0f);
            Cue(Create(impact, "Sparks", TM("Spark"), c)
                .Sphere(0.25f).Speed(5f * k, 11f * k).Life(0.25f, 0.55f).Size(0.07f, 0.13f)
                .Burst(Mathf.RoundToInt(10 + 30 * s)).Gravity(0.8f).Drag(2f).Stretch(5f, 0.04f)
                .AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3), 0f);

            fx.flashLight = AddLight(impact, c, ParseColor(e.color), 8f);
            fx.lightIntensity = hit
                ? C(0f, 0f, ti - 0.02f, 0f, ti + 0.03f, 2f + 3f * s, ti + 0.2f, 1.2f + s, ti + 0.7f, 0f)
                : C(0f, 0f, ti - 0.1f, 0f, ti + 0.1f, 1.5f, ti + 0.6f, 0f);

            // ---- Capas del tipo (los movimientos de estado solo llevan las "ambientales" + brillo y motas).
            var layers = new List<Layer>(style.layers);
            if (!hit)
            {
                layers.RemoveAll(ImpactOnly.Contains);
                layers.Insert(0, Layer.Pulse);
                layers.Add(Layer.Motes);
            }
            foreach (var layer in layers.Distinct())
                BuildTypeLayer(layer, impact, c, style, k, TM, Cue);

            // ---- Anticipación en el atacante (solo familias a distancia con el foco en el objetivo).
            string family = (e.family ?? "").ToLowerInvariant();
            if (info.focus == Target && RangedFamilyWords.Any(w => family.Contains(w)))
            {
                var attacker = new GameObject("Anticipation").transform;
                attacker.SetParent(fx.transform, false);
                attacker.localPosition = Attacker + new Vector3(0f, 0.9f, 0f);
                float charge = Mathf.Clamp(ti * 0.45f, 0.15f, 0.6f);
                fx.particles.Add(new VfxTimeline.ParticleCue
                {
                    system = Create(attacker, "ChargeSparks", TM("Spark"))
                        .Sphere(1.6f, 0f).Radial(-6f).Life(0.28f).Size(0.06f, 0.11f).Rate(60f).Duration(charge)
                        .Stretch(4f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0.6f).Order(3).ps,
                    time = 0f,
                });
                fx.particles.Add(new VfxTimeline.ParticleCue
                {
                    system = Create(attacker, "ChargeGlow", TM("Glow"))
                        .Life(charge + 0.1f).Size(1.4f).Burst(1).SizeOverLife(C(0f, 0.2f, 0.7f, 1f, 1f, 0.4f)).Order(4).ps,
                    time = 0f,
                });
            }

            // ---- Sacudida proporcional al impacto y cámara lenta solo en los más fuertes.
            if (hit)
                fx.shakes.Add(new VfxTimeline.ShakeCue { time = ti, strength = Mathf.Lerp(0.08f, 0.4f, s), duration = Mathf.Lerp(0.2f, 0.5f, s) });
            if (info.slowMotion)
                fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = ti + 0.01f, timeScale = 0.25f, duration = 0.1f });
        }

        static void BuildTypeLayer(Layer layer, Transform impact, Vector3 c, TypeStyle style, float k,
            Func<string, Material> TM, Action<Fx, float> Cue)
        {
            int N(float baseCount, float extra) => Mathf.RoundToInt(baseCount + extra * k);
            var ground = new Vector3(0f, 0.25f, 0f);
            switch (layer)
            {
                case Layer.ImpactStar:
                    Cue(Create(impact, "ImpactStar", TM("Star"), c)
                        .Life(0.22f).Size(4.5f * k).Burst(1).Rotation(0f, 45f).Spin(-60f, 60f)
                        .SizeOverLife(C(0f, 0.6f, 0.2f, 1f, 1f, 0.8f)).AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(7), 0f);
                    break;
                case Layer.ImpactRays:
                    Cue(Create(impact, "ImpactRays", TM("Spark"), c)
                        .Life(0.3f).Speed(0.01f).Size(0.28f * k).Sphere(0.1f).Burst(8)
                        .Stretch(16f).AlphaOverLife(0f, 1f, 0.33f, 0f, 1f, 0f).Order(4), 0f);
                    break;
                case Layer.Dust:
                    Cue(Create(impact, "Dust", BotwMaterials.Get("EM_Dust"), ground)
                        .GroundCircle(1f, 0.3f).Speed(1.2f * k, 2.8f * k).Drag(2.5f).Life(0.7f, 1.2f).Size(0.7f * k, 1.3f * k)
                        .Burst(N(8, 8)).Velocity(new Vector3(0f, 0.5f, 0f)).SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                        .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0), 0.04f);
                    break;
                case Layer.Debris:
                {
                    float big = style.key == "Rock" ? 1.4f : 1f;
                    Cue(Create(impact, "Debris", BotwMaterials.Get("EX_Debris"), new Vector3(0f, 0.3f, 0f))
                        .Mesh(BotwMeshes.Rock).Hemisphere(0.4f).Speed(4f * k, 8f * k).Life(1.2f, 1.7f)
                        .Size(0.1f * big, 0.26f * big).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-400f, 400f, true)
                        .Gravity(2.2f).Burst(N(8, 8)).Collide(0.35f, 0.45f)
                        .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(0), 0.02f);
                    break;
                }
                case Layer.Embers:
                {
                    var mat = style.key == "Fire" ? BotwMaterials.Get("EX_Ember") : TM("Spark");
                    Cue(Create(impact, "Embers", mat, c)
                        .Sphere(0.45f).Speed(3f * k, 7f * k).Life(0.5f, 1.2f).Size(0.05f, 0.1f).Burst(N(20, 20))
                        .Gravity(-0.15f).Drag(1.8f).Stretch(4f, 0.04f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(2), 0.02f);
                    break;
                }
                case Layer.FireSmoke:
                    Cue(Create(impact, "FireSmoke", BotwMaterials.Get("EX_Smoke"), c)
                        .Sphere(0.5f).Speed(1.5f * k, 3.5f * k).Drag(3f).Life(1f, 1.6f).Size(0.9f * k, 1.6f * k).Burst(N(7, 5))
                        .Velocity(new Vector3(0f, 0.8f, 0f)).SizeOverLife(C(0f, 0.45f, 0.15f, 1f, 1f, 1.25f))
                        .Custom(Curve(0f, 0f, 0.12f, 0.12f, 0.45f, 1f), Rand(-0.05f, 0.08f), Curve(0f, 0f, 0.55f, 0.08f, 1f, 1f), Rand(0f, 1f))
                        .Order(1), 0.04f);
                    break;
                case Layer.Droplets:
                    Cue(Create(impact, "Droplets", TM("Spark"), c)
                        .Hemisphere(0.3f).Speed(4f * k, 8f * k).Life(0.5f, 0.9f).Size(0.06f, 0.12f).Burst(N(18, 20))
                        .Gravity(1.6f).Drag(0.8f).Stretch(3f, 0.05f).AlphaOverLife(0f, 1f, 0.7f, 0.8f, 1f, 0f).Order(3), 0.01f);
                    break;
                case Layer.Mist:
                    Cue(Smoke(impact, "Mist", BotwMaterials.Get("EM_Mist"), c, k, 0.4f), 0.05f);
                    break;
                case Layer.PoisonSmoke:
                    Cue(Smoke(impact, "PoisonSmoke", BotwMaterials.Get("EM_PoisonSmoke"), c, k, 0.5f), 0.06f);
                    break;
                case Layer.GhostSmoke:
                    Cue(Smoke(impact, "GhostSmoke", BotwMaterials.Get("EM_GhostSmoke"), c, k, 0.5f), 0.06f);
                    break;
                case Layer.DarkSmoke:
                    Cue(Smoke(impact, "DarkSmoke", BotwMaterials.Get("EX_DarkSmoke"), c, k, 0.5f), 0.06f);
                    break;
                case Layer.Crackle:
                    Cue(Create(impact, "Crackle", TM("Spark"), c)
                        .Sphere(0.45f).Speed(9f, 15f).Drag(6f).Life(0.08f, 0.2f).Size(0.05f, 0.09f).Rate(140f * k).Duration(0.4f)
                        .Stretch(7f, 0.06f).AlphaOverLife(0f, 1f, 1f, 0.4f).Order(4), 0f);
                    break;
                case Layer.Glints:
                    Cue(Create(impact, "Glints", TM("Star"), c)
                        .Sphere(0.9f).Life(0.06f, 0.16f).Size(0.25f, 0.6f).Rate(40f * k).Duration(0.5f).Rotation(0f, 90f)
                        .AlphaOverLife(0f, 1f, 1f, 0f).Order(5), 0.02f);
                    break;
                case Layer.Leaves:
                    Cue(Create(impact, "Leaves", BotwMaterials.Get("EM_Leaf"), c)
                        .Sphere(0.4f).Speed(2f * k, 5f * k).Drag(1.5f).Life(0.9f, 1.6f).Size(0.18f, 0.32f).Burst(N(12, 10))
                        .Gravity(0.25f).Rotation(0f, 360f).Spin(-360f, 360f).Velocity(new Vector3(0f, 0.3f, 0f))
                        .AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(2), 0.02f);
                    break;
                case Layer.IceShards:
                    Cue(Create(impact, "IceShards", BotwMaterials.Get("EM_IceShard"), new Vector3(0f, 0.5f, 0f))
                        .Mesh(BotwMeshes.Rock).Hemisphere(0.4f).Speed(3f * k, 6f * k).Life(0.8f, 1.3f)
                        .Size3D(new Vector3(0.1f, 0.32f, 0.1f)).Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-300f, 300f, true)
                        .Gravity(1.6f).Burst(N(8, 8)).SizeOverLife(C(0f, 1f, 0.8f, 1f, 1f, 0f)).Order(1), 0.02f);
                    break;
                case Layer.Motes:
                    Cue(Create(impact, "Motes", TM("Star"), c)
                        .Sphere(1f).Speed(0.3f, 1f).Drag(1f).Velocity(new Vector3(0f, 0.7f, 0f)).Life(0.6f, 1.3f)
                        .Size(0.1f, 0.24f).Burst(N(14, 10)).Spin(-180f, 180f)
                        .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.6f, 1f, 1f, 0f).Order(4), 0.08f);
                    break;
                case Layer.Pulse:
                    Cue(Create(impact, "Pulse", TM("Glow"), c)
                        .Life(0.5f).Size(3f * k).Burst(1).SizeOverLife(C(0f, 0.3f, 0.25f, 1f, 1f, 1.2f))
                        .AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(2), 0.03f);
                    break;
                case Layer.Bubbles:
                    Cue(Create(impact, "Bubbles", BotwMaterials.Get("EM_Bubble"), c)
                        .Sphere(0.6f).Speed(0.4f, 1.2f).Velocity(new Vector3(0f, 0.9f, 0f)).Life(0.6f, 1.2f).Size(0.12f, 0.3f)
                        .Burst(N(10, 8)).SizeOverLife(C(0f, 0.4f, 0.2f, 1f, 1f, 1.1f))
                        .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(3), 0.05f);
                    break;
                case Layer.Wind:
                    Cue(Create(impact, "Wind", BotwMaterials.Get("EM_Wind"), c)
                        .Sphere(0.5f).Speed(7f * k, 12f * k).Drag(3f).Life(0.25f, 0.45f).Size(0.06f, 0.1f).Burst(N(12, 10))
                        .Stretch(10f, 0.06f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3), 0f);
                    break;
                case Layer.SmallMotes:
                    Cue(Create(impact, "SmallMotes", TM("Star"), c)
                        .Sphere(0.8f).Radial(0.4f, 3f).Life(0.6f, 1.1f).Size(0.06f, 0.14f).Burst(N(16, 10))
                        .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.6f, 1f, 1f, 0f).Order(4), 0.04f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }

        // Humo toon genérico (bruma, veneno, fantasma, siniestro).
        static Fx Smoke(Transform parent, string name, Material mat, Vector3 pos, float k, float rise) =>
            Create(parent, name, mat, pos)
                .Sphere(0.6f).Speed(0.8f * k, 2f * k).Drag(2f).Life(1f, 1.6f).Size(1f * k, 1.7f * k)
                .Burst(Mathf.RoundToInt(6 + 4 * k)).Velocity(new Vector3(0f, rise, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.4f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1);

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

        static Color ParseColor(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;

        // ------------------------------------------------------------ escena

        static void BuildScene(EmeraldMoveVfx[] prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rng = new System.Random(4321);

            BotwScene.BuildLighting();

            // ---------------- Arena
            var env = new GameObject("Environment").transform;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); // conserva el collider (rebotan los escombros)
            ground.name = "Ground";
            ground.transform.SetParent(env, false);
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = BotwMaterials.Get("MAT_Ground");

            var arena = BotwScene.CreatePrimitive(PrimitiveType.Cylinder, "Arena", env, BotwMaterials.Get("MAT_Dirt"));
            arena.transform.position = new Vector3(0f, 0.005f, 0f);
            arena.transform.localScale = new Vector3(11f, 0.005f, 6.5f);
            arena.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Fondo: rocas y árboles detrás de la arena (fuera del encuadre de los efectos).
            var rockMat = BotwMaterials.Get("MAT_Rock");
            int placed = 0;
            for (int i = 0; i < 300 && placed < 14; i++)
            {
                var p = new Vector3(BotwScene.Range(rng, -32f, 32f), 0f, BotwScene.Range(rng, 7f, 40f));
                if (Mathf.Abs(p.x) < 9f && p.z < 10f)
                    continue;
                BotwScene.AddRock(env, rockMat, p, BotwScene.Range(rng, 0.6f, 2.2f), rng);
                placed++;
            }
            var bark = BotwMaterials.Get("MAT_Bark");
            var leaves = BotwMaterials.Get("MAT_Leaves");
            placed = 0;
            for (int i = 0; i < 300 && placed < 12; i++)
            {
                var p = new Vector3(BotwScene.Range(rng, -36f, 36f), 0f, BotwScene.Range(rng, 11f, 45f));
                BotwScene.AddTree(env, bark, leaves, p, BotwScene.Range(rng, 0.8f, 1.4f), rng);
                placed++;
            }

            // ---------------- Atacante y objetivo (atrezo)
            AddStandIn(env, "Atacante (atrezo)", Attacker, BotwMaterials.Get("MAT_SheikahStone"), 90f);
            AddStandIn(env, "Objetivo (atrezo)", Target, BotwMaterials.Get("MAT_GuardianShell"), -90f);

            // ---------------- Cámara, post-proceso y director
            var cam = BotwScene.BuildCamera();
            BotwScene.AddVolume(BotwScene.LoadOrBuildProfile());
            new GameObject("VfxDirector").AddComponent<VfxDirector>();

            var stage = new GameObject("Stage").transform; // punto medio entre atacante y objetivo
            var gallery = new GameObject("Gallery").AddComponent<EmeraldGallery>();
            gallery.moves = prefabs;
            gallery.stage = stage;
            gallery.targetCamera = cam;
            gallery.PoseFor(prefabs.Length > 0 ? prefabs[0] : null, cam);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            BotwScene.AddToBuildSettings(ScenePath, first: false);
        }

        // Muñeco sencillo: cuerpo (cápsula) + cabeza, mirando al rival.
        static void AddStandIn(Transform parent, string name, Vector3 position, Material mat, float yaw)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = position;
            root.rotation = Quaternion.Euler(0f, yaw, 0f);
            var body = BotwScene.CreatePrimitive(PrimitiveType.Capsule, "Body", root, mat);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(0.75f, 0.55f, 0.75f);
            var head = BotwScene.CreatePrimitive(PrimitiveType.Sphere, "Head", root, mat);
            head.transform.localPosition = new Vector3(0f, 1.3f, 0.05f);
            head.transform.localScale = Vector3.one * 0.55f;
        }
    }
}
