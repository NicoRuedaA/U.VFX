using System.Collections.Generic;
using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Pequeño "builder" fluido para configurar ParticleSystems por código
    /// sin repetir 20 líneas por sistema.
    /// </summary>
    public class Fx
    {
        public readonly ParticleSystem ps;
        public readonly ParticleSystemRenderer renderer;

        static readonly List<ParticleSystemVertexStream> Streams = new List<ParticleSystemVertexStream>
        {
            ParticleSystemVertexStream.Position,
            ParticleSystemVertexStream.Normal,
            ParticleSystemVertexStream.Color,
            ParticleSystemVertexStream.UV,
            ParticleSystemVertexStream.Custom1XYZW,
        };

        Fx(ParticleSystem ps)
        {
            this.ps = ps;
            renderer = ps.GetComponent<ParticleSystemRenderer>();
        }

        public static Fx Create(Transform parent, string name, Material material, Vector3 localPosition = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var fx = new Fx(ps);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startDelay = 0f;
            main.startSpeed = 0f;
            main.startLifetime = 1f;
            main.startSize = 1f;
            main.startColor = UnityEngine.Color.white;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 500;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;

            var shape = ps.shape;
            shape.enabled = false;

            var r = fx.renderer;
            r.sharedMaterial = material;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.alignment = ParticleSystemRenderSpace.View;
            r.maxParticleSize = 10f;
            r.minParticleSize = 0f;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.SetActiveVertexStreams(Streams);
            return fx;
        }

        // ------------------------------------------------------------ main

        public Fx Life(float min, float max = -1f)
        {
            var m = ps.main;
            m.startLifetime = max < 0f ? new MinMaxCurve(min) : new MinMaxCurve(min, max);
            return this;
        }

        public Fx Speed(float min, float max = -1f)
        {
            var m = ps.main;
            m.startSpeed = max < 0f ? new MinMaxCurve(min) : new MinMaxCurve(min, max);
            return this;
        }

        public Fx Size(float min, float max = -1f)
        {
            var m = ps.main;
            m.startSize3D = false;
            m.startSize = max < 0f ? new MinMaxCurve(min) : new MinMaxCurve(min, max);
            return this;
        }

        public Fx Size3D(Vector3 size)
        {
            var m = ps.main;
            m.startSize3D = true;
            m.startSizeX = size.x;
            m.startSizeY = size.y;
            m.startSizeZ = size.z;
            return this;
        }

        /// <summary>Rotación inicial en grados (aleatoria entre min y max).</summary>
        public Fx Rotation(float minDeg, float maxDeg)
        {
            var m = ps.main;
            m.startRotation3D = false;
            m.startRotation = new MinMaxCurve(minDeg * Mathf.Deg2Rad, maxDeg * Mathf.Deg2Rad);
            return this;
        }

        public Fx Rotation3D(Vector3 minDeg, Vector3 maxDeg)
        {
            var m = ps.main;
            m.startRotation3D = true;
            m.startRotationX = new MinMaxCurve(minDeg.x * Mathf.Deg2Rad, maxDeg.x * Mathf.Deg2Rad);
            m.startRotationY = new MinMaxCurve(minDeg.y * Mathf.Deg2Rad, maxDeg.y * Mathf.Deg2Rad);
            m.startRotationZ = new MinMaxCurve(minDeg.z * Mathf.Deg2Rad, maxDeg.z * Mathf.Deg2Rad);
            return this;
        }

        public Fx Gravity(float g)
        {
            var m = ps.main;
            m.gravityModifier = g;
            return this;
        }

        public Fx Duration(float d)
        {
            var m = ps.main;
            m.duration = d;
            return this;
        }

        public Fx LocalSpace()
        {
            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.Local;
            return this;
        }

        // ------------------------------------------------------------ emisión

        public Fx Burst(int count, float time = 0f)
        {
            var e = ps.emission;
            var bursts = new ParticleSystem.Burst[e.burstCount + 1];
            e.GetBursts(bursts);
            bursts[e.burstCount] = new ParticleSystem.Burst(time, (short)count);
            e.SetBursts(bursts);
            return this;
        }

        public Fx Rate(float perSecond)
        {
            var e = ps.emission;
            e.rateOverTime = perSecond;
            return this;
        }

        // ------------------------------------------------------------ forma

        public Fx Sphere(float radius, float thickness = 1f)
        {
            var s = ps.shape;
            s.enabled = true;
            s.shapeType = ParticleSystemShapeType.Sphere;
            s.radius = radius;
            s.radiusThickness = thickness;
            return this;
        }

        public Fx Hemisphere(float radius, float thickness = 1f)
        {
            var s = ps.shape;
            s.enabled = true;
            s.shapeType = ParticleSystemShapeType.Hemisphere;
            s.radius = radius;
            s.radiusThickness = thickness;
            s.rotation = new Vector3(-90f, 0f, 0f); // la semiesfera apunta hacia arriba (+Y)
            return this;
        }

        /// <summary>Círculo tumbado en el suelo (plano XZ).</summary>
        public Fx GroundCircle(float radius, float thickness = 1f)
        {
            var s = ps.shape;
            s.enabled = true;
            s.shapeType = ParticleSystemShapeType.Circle;
            s.radius = radius;
            s.radiusThickness = thickness;
            s.rotation = new Vector3(90f, 0f, 0f);
            return this;
        }

        /// <summary>Medio toroide en el plano XY (arco superior).</summary>
        public Fx HalfDonut(float radius, float donutRadius)
        {
            var s = ps.shape;
            s.enabled = true;
            s.shapeType = ParticleSystemShapeType.Donut;
            s.radius = radius;
            s.donutRadius = donutRadius;
            s.radiusThickness = 1f;
            s.arc = 180f;
            return this;
        }

        public Fx Box(Vector3 scale)
        {
            var s = ps.shape;
            s.enabled = true;
            s.shapeType = ParticleSystemShapeType.Box;
            s.scale = scale;
            return this;
        }

        // ------------------------------------------------------------ módulos

        public Fx ColorOverLife(Gradient g)
        {
            var c = ps.colorOverLifetime;
            c.enabled = true;
            c.color = new ParticleSystem.MinMaxGradient(g);
            return this;
        }

        /// <summary>Alpha sobre la vida. Con _AlphaErosion = 1 el alpha se convierte en erosión.</summary>
        public Fx AlphaOverLife(params float[] timeAlpha)
        {
            var colorKeys = new[] { new GradientColorKey(UnityEngine.Color.white, 0f), new GradientColorKey(UnityEngine.Color.white, 1f) };
            var alphaKeys = new GradientAlphaKey[timeAlpha.Length / 2];
            for (int i = 0; i < alphaKeys.Length; i++)
                alphaKeys[i] = new GradientAlphaKey(timeAlpha[i * 2 + 1], timeAlpha[i * 2]);
            var g = new Gradient();
            g.SetKeys(colorKeys, alphaKeys);
            return ColorOverLife(g);
        }

        public Fx SizeOverLife(AnimationCurve curve)
        {
            var s = ps.sizeOverLifetime;
            s.enabled = true;
            s.separateAxes = false;
            s.size = new MinMaxCurve(1f, curve);
            return this;
        }

        /// <summary>Velocidad angular aleatoria en grados/segundo (eje Z o los tres ejes).</summary>
        public Fx Spin(float minDeg, float maxDeg, bool allAxes = false)
        {
            var r = ps.rotationOverLifetime;
            r.enabled = true;
            r.separateAxes = allAxes;
            var curve = new MinMaxCurve(minDeg * Mathf.Deg2Rad, maxDeg * Mathf.Deg2Rad);
            if (allAxes)
            {
                r.x = curve;
                r.y = curve;
            }
            r.z = curve;
            return this;
        }

        public Fx Velocity(Vector3 v)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new MinMaxCurve(v.x);
            vel.y = new MinMaxCurve(v.y);
            vel.z = new MinMaxCurve(v.z);
            return this;
        }

        public Fx Radial(float radial, float orbitalY = 0f)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new MinMaxCurve(0f);
            vel.y = new MinMaxCurve(0f);
            vel.z = new MinMaxCurve(0f);
            vel.radial = new MinMaxCurve(radial);
            vel.orbitalY = new MinMaxCurve(orbitalY);
            return this;
        }

        /// <summary>Rozamiento: los fragmentos salen rápido y frenan (sensación "ease-out").</summary>
        public Fx Drag(float drag)
        {
            var l = ps.limitVelocityOverLifetime;
            l.enabled = true;
            l.limit = 1000f;
            l.dampen = 0f;
            l.drag = drag;
            l.multiplyDragByParticleSize = false;
            l.multiplyDragByParticleVelocity = false;
            return this;
        }

        public Fx Custom(MinMaxCurve x, MinMaxCurve y, MinMaxCurve z, MinMaxCurve w)
        {
            var cd = ps.customData;
            cd.enabled = true;
            cd.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
            cd.SetVectorComponentCount(ParticleSystemCustomData.Custom1, 4);
            cd.SetVector(ParticleSystemCustomData.Custom1, 0, x);
            cd.SetVector(ParticleSystemCustomData.Custom1, 1, y);
            cd.SetVector(ParticleSystemCustomData.Custom1, 2, z);
            cd.SetVector(ParticleSystemCustomData.Custom1, 3, w);
            return this;
        }

        public Fx Collide(float bounce, float dampen)
        {
            var c = ps.collision;
            c.enabled = true;
            c.type = ParticleSystemCollisionType.World;
            c.mode = ParticleSystemCollisionMode.Collision3D;
            c.bounce = bounce;
            c.dampen = dampen;
            c.lifetimeLoss = 0f;
            c.quality = ParticleSystemCollisionQuality.Medium;
            c.radiusScale = 0.6f;
            return this;
        }

        // ------------------------------------------------------------ render

        public Fx Stretch(float lengthScale, float velocityScale = 0f)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = lengthScale;
            renderer.velocityScale = velocityScale;
            renderer.cameraVelocityScale = 0f;
            return this;
        }

        public Fx Mesh(Mesh mesh, ParticleSystemRenderSpace alignment = ParticleSystemRenderSpace.World)
        {
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = mesh;
            renderer.alignment = alignment;
            return this;
        }

        public Fx Order(int sortingOrder)
        {
            renderer.sortingOrder = sortingOrder;
            return this;
        }

        // ------------------------------------------------------------ curvas

        /// <summary>Curva a partir de pares (tiempo, valor) con tangentes suaves sin rebasar.</summary>
        public static AnimationCurve C(params float[] tv)
        {
            int n = tv.Length / 2;
            var keys = new Keyframe[n];
            for (int i = 0; i < n; i++)
                keys[i] = new Keyframe(tv[i * 2], tv[i * 2 + 1]);
            for (int i = 0; i < n; i++)
            {
                float tangent;
                if (n == 1)
                    tangent = 0f;
                else if (i == 0)
                    tangent = (keys[1].value - keys[0].value) / (keys[1].time - keys[0].time);
                else if (i == n - 1)
                    tangent = (keys[i].value - keys[i - 1].value) / (keys[i].time - keys[i - 1].time);
                else
                {
                    float d0 = keys[i].value - keys[i - 1].value;
                    float d1 = keys[i + 1].value - keys[i].value;
                    // En los extremos locales la tangente es plana (sin overshoot).
                    tangent = d0 * d1 <= 0f ? 0f : (keys[i + 1].value - keys[i - 1].value) / (keys[i + 1].time - keys[i - 1].time);
                }
                keys[i].inTangent = tangent;
                keys[i].outTangent = tangent;
            }
            return new AnimationCurve(keys);
        }

        public static MinMaxCurve Curve(params float[] tv) => new MinMaxCurve(1f, C(tv));
        public static MinMaxCurve Const(float v) => new MinMaxCurve(v);
        public static MinMaxCurve Rand(float min, float max) => new MinMaxCurve(min, max);
    }
}
