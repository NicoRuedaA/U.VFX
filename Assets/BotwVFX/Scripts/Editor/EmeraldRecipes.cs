using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Contexto que recibe cada receta (EmeraldRecipes.M###): el prefab en construcción,
    /// los datos del clip horneado y utilidades para fijar la timeline (3,6 s), alinear el
    /// clip horneado, registrar cues, partes animadas, luz, sacudidas y cámara lenta.
    /// Las capas visuales reutilizables están en EmeraldLayers.
    /// </summary>
    public sealed class EmeraldMoveBuilder
    {
        /// <summary>Duración total de cada movimiento (documento de referencias).</summary>
        public const float Duration = 3.6f;

        // Puntos del marco local (atrezo: cápsula de ~1,6 m).
        public static readonly Vector3 Attacker = new Vector3(-3f, 0f, 0f);
        public static readonly Vector3 Target = new Vector3(3f, 0f, 0f);
        public static readonly Vector3 AttackerChest = new Vector3(-3f, 0.85f, 0f);
        public static readonly Vector3 TargetChest = new Vector3(3f, 0.85f, 0f);
        public static readonly Vector3 AttackerHand = new Vector3(-2.45f, 0.85f, 0f);
        public static readonly Vector3 AttackerHead = new Vector3(-3f, 1.35f, 0f);
        public static readonly Vector3 TargetHead = new Vector3(3f, 1.35f, 0f);

        public readonly EmeraldMoveVfx fx;
        public readonly Transform root;
        public readonly int id;
        public readonly string type;
        /// <summary>Color del catálogo (para la luz).</summary>
        public readonly Color moveColor;
        /// <summary>Instante del impacto dentro del clip horneado, a velocidad original (s).</summary>
        public readonly float clipImpact;
        /// <summary>Pico de ImpactStrength del clip (0 = sin golpe).</summary>
        public readonly float clipPeak;
        readonly float animDuration;
        readonly HashSet<string> names = new HashSet<string>();

        internal EmeraldMoveBuilder(EmeraldMoveVfx fx, int id, string type, Color moveColor, float clipImpact, float clipPeak, float animDuration)
        {
            this.fx = fx;
            root = fx.transform;
            this.id = id;
            this.type = type;
            this.moveColor = moveColor;
            this.clipImpact = clipImpact;
            this.clipPeak = clipPeak;
            this.animDuration = animDuration;
            fx.hasRecipe = true;
            fx.duration = Duration;
            fx.bakedOffset = 0f;
            fx.bakedLength = animDuration;
            fx.bakedVisible = true;
            fx.bakedScale = Vector3.one;
            fx.viewMin = new Vector2(-4.2f, 0f);
            fx.viewMax = new Vector2(4.6f, 3.4f);
        }

        // ------------------------------------------------------------ timeline

        /// <summary>Los 4 instantes clave (anticipación, impacto, pico, disipación) y el punto del impacto.</summary>
        public void Keys(float anticipation, float impact, float peak, float dissipation, Vector3 impactPoint, float strength = 1f)
        {
            if (!(0f <= anticipation && anticipation < impact && impact < peak && peak < dissipation && dissipation <= Duration))
                throw new ArgumentException($"Movimiento {id}: instantes clave fuera de orden.");
            fx.anticipationTime = anticipation;
            fx.impactTime = impact;
            fx.peakTime = peak;
            fx.dissipationTime = dissipation;
            fx.impactPoint = impactPoint;
            fx.impactStrength = Mathf.Clamp01(strength);
        }

        /// <summary>
        /// Coloca el clip horneado para que su impacto caiga en <paramref name="impactAt"/>,
        /// reproducido a <paramref name="speed"/> veces su velocidad original.
        /// </summary>
        public void Baked(float impactAt, float speed = 1f, Vector3? scale = null)
        {
            float length = animDuration / Mathf.Max(0.05f, speed);
            BakedWindow(impactAt - clipImpact * length / animDuration, length, scale);
        }

        public void BakedWindow(float offset, float length, Vector3? scale = null)
        {
            fx.bakedVisible = true;
            fx.bakedOffset = offset;
            fx.bakedLength = Mathf.Max(0.05f, length);
            fx.bakedScale = scale ?? Vector3.one;
            fx.player.duration = fx.bakedLength;
            fx.player.transform.localScale = fx.bakedScale;
        }

        /// <summary>La receta sustituye el clip horneado por completo.</summary>
        public void HideBaked() => fx.bakedVisible = false;

        public void Shake(float time, float strength, float duration) =>
            fx.shakes.Add(new VfxTimeline.ShakeCue { time = time, strength = strength, duration = duration });

        public void SlowMo(float time, float timeScale = 0.25f, float duration = 0.1f) =>
            fx.slowMotion.Add(new VfxTimeline.SlowMotionCue { time = time, timeScale = timeScale, duration = duration });

        /// <summary>Luz de destello (una por movimiento). keys = pares (tiempo, intensidad).</summary>
        public void Light(Vector3 position, Color color, float range, params float[] keys)
        {
            var go = new GameObject("FlashLight");
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.enabled = false;
            fx.flashLight = light;
            fx.lightIntensity = C(keys);
        }

        public void View(Vector2 min, Vector2 max)
        {
            fx.viewMin = min;
            fx.viewMax = max;
        }

        // ------------------------------------------------------------ nodos, cues y partes

        string Unique(string name)
        {
            string n = name;
            for (int i = 2; !names.Add(n); i++)
                n = $"{name}{i}";
            return n;
        }

        public Transform Node(string name, Vector3 localPosition, Transform parent = null)
        {
            var t = new GameObject(Unique(name)).transform;
            t.SetParent(parent != null ? parent : root, false);
            t.localPosition = localPosition;
            return t;
        }

        /// <summary>Crea un sistema Shuriken bajo la raíz (nombre único) en la posición local dada.</summary>
        public Fx Ps(string name, Material material, Vector3 localPosition, Transform parent = null) =>
            Create(parent != null ? parent : root, Unique(name), material, localPosition);

        /// <summary>Registra el sistema en la timeline con semilla fija (Preview(t) determinista).</summary>
        public Fx Cue(Fx f, float time)
        {
            f.ps.useAutoRandomSeed = false;
            f.ps.randomSeed = (uint)(Hash(f.ps.gameObject.name) ^ (id * 7919));
            fx.particles.Add(new VfxTimeline.ParticleCue { system = f.ps, time = Mathf.Max(0f, time) });
            return f;
        }

        static int Hash(string s)
        {
            unchecked
            {
                int h = 23;
                foreach (char c in s)
                    h = h * 31 + c;
                return h & 0x7fffffff;
            }
        }

        public EmeraldMoveVfx.PartTrack Track(Transform part, Renderer renderer, float start, float end)
        {
            var track = new EmeraldMoveVfx.PartTrack
            {
                part = part,
                renderer = renderer,
                start = start,
                end = end,
                eulerFrom = part != null ? part.localEulerAngles : Vector3.zero,
            };
            if (renderer != null)
                renderer.enabled = false; // en reposo no se ve
            fx.tracks.Add(track);
            return track;
        }

        public MeshRenderer MeshPart(string name, Mesh mesh, Material mat, Vector3 localPos, Vector3 euler, Vector3 scale, Transform parent = null, int order = 0)
        {
            var go = new GameObject(Unique(name));
            go.transform.SetParent(parent != null ? parent : root, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            r.sortingOrder = order;
            r.enabled = false;
            return r;
        }

        /// <summary>Línea en espacio local (rayos, cortes largos) con los puntos dados.</summary>
        public LineRenderer LinePart(string name, Material mat, Vector3[] points, AnimationCurve width, Transform parent = null, int order = 4)
        {
            var go = new GameObject(Unique(name));
            go.transform.SetParent(parent != null ? parent : root, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.widthCurve = width;
            line.widthMultiplier = 1f;
            line.sharedMaterial = mat;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.numCapVertices = 2;
            line.numCornerVertices = 1;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sortingOrder = order;
            line.enabled = false;
            return line;
        }
    }

    /// <summary>
    /// Recetas por movimiento: un método estático "M###" por movimiento, agrupados por lotes
    /// en ficheros parciales (EmeraldRecipes.Batch01.cs = 001-020...). Los movimientos sin
    /// receta usan las capas genéricas por tipo de EmeraldMoves.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        static Dictionary<int, Action<EmeraldMoveBuilder>> registry;

        public static bool TryGet(int id, out Action<EmeraldMoveBuilder> recipe)
        {
            if (registry == null)
            {
                registry = new Dictionary<int, Action<EmeraldMoveBuilder>>();
                foreach (var m in typeof(EmeraldRecipes).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (m.Name.Length == 4 && m.Name[0] == 'M' && int.TryParse(m.Name.Substring(1), out int n)
                        && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(EmeraldMoveBuilder))
                        registry[n] = (Action<EmeraldMoveBuilder>)Delegate.CreateDelegate(typeof(Action<EmeraldMoveBuilder>), m);
                }
            }
            return registry.TryGetValue(id, out recipe);
        }

        public static int Count
        {
            get
            {
                TryGet(0, out _);
                return registry.Count;
            }
        }
    }
}
