using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Rayo del Guardián, siguiendo el desglose de 80.lv:
    /// 1) láser de apuntado que se va "fijando" y parpadea cada vez más rápido,
    /// 2) carga en el ojo (chispas que convergen + orbe),
    /// 3) disparo: cuerpo del rayo con esferas escaladas + tiras de energía
    ///    + lens flares empujados hacia la cámara,
    /// 4) impacto: explosión toon (sub-efecto), cámara lenta y sacudida.
    /// </summary>
    public class GuardianBeamVfx : VfxTimeline
    {
        [Header("Puntos")]
        public Transform eye;
        public Transform target;

        [Header("Láser de apuntado")]
        public LineRenderer laser;
        public float laserWidth = 0.05f;
        public float laserEnd = 2.7f;
        [Tooltip("Cuánto se desvía el láser al principio antes de fijar el objetivo.")]
        public float aimWobble = 1.2f;
        [Tooltip("Emisor del punto rojo; sigue el extremo del láser.")]
        public Transform laserDot;

        [Header("Rayo")]
        public Renderer beamCore;
        public Renderer beamGlow;
        public float fireTime = 2.7f;
        public float travelTime = 0.08f;
        public float beamEnd = 3.3f;
        public AnimationCurve beamWidth = AnimationCurve.Constant(0f, 1f, 0.5f);
        public float glowScale = 2.2f;

        [Header("Tiras de energía (se alinean con el rayo)")]
        public ParticleSystem stripes;

        protected override void Evaluate(float t)
        {
            if (eye == null || target == null)
                return;

            Vector3 a = eye.position;
            Vector3 b = target.position;
            Vector3 dir = b - a;
            float dist = dir.magnitude;

            // --- Láser de apuntado ---
            bool laserOn = t >= 0f && t < laserEnd;
            if (laser != null)
            {
                laser.enabled = laserOn;
                if (laserOn)
                {
                    float lockOn = Mathf.Clamp01(t / (laserEnd * 0.75f));
                    float wobble = (1f - lockOn) * (1f - lockOn) * aimWobble;
                    var jitter = new Vector3(
                        Mathf.PerlinNoise(t * 1.3f, 0.1f) - 0.5f,
                        0f,
                        Mathf.PerlinNoise(0.7f, t * 1.3f) - 0.5f) * (2f * wobble);
                    laser.SetPosition(0, a);
                    laser.SetPosition(1, b + jitter);
                    if (laserDot != null)
                        laserDot.position = b + jitter;

                    // El parpadeo acelera (2 Hz -> 14 Hz), como el pitido del Guardián.
                    float f0 = 2f, f1 = 14f;
                    float phase = Mathf.PI * 2f * (f0 * t + (f1 - f0) * t * t / (2f * laserEnd));
                    float pulse = 0.5f + 0.5f * Mathf.Sin(phase);
                    laser.widthMultiplier = laserWidth * (0.6f + 0.8f * pulse);
                }
            }

            // --- Cuerpo del rayo ---
            float bt = t - fireTime;
            bool beamOn = t >= 0f && bt >= 0f && t < beamEnd;
            SetVisible(beamCore, beamOn);
            SetVisible(beamGlow, beamOn);
            if (beamOn)
            {
                float k = travelTime > 0f ? Mathf.Clamp01(bt / travelTime) : 1f;
                Vector3 tip = Vector3.Lerp(a, b, k);
                float w = beamWidth.Evaluate(bt) * (1f + 0.12f * Mathf.Sin(t * 95f));
                PlaceBeam(beamCore, a, tip, w);
                PlaceBeam(beamGlow, a, tip, w * glowScale);
            }

            // --- Emisor de tiras: una línea del ojo al objetivo ---
            if (stripes != null && dist > 0.01f)
            {
                stripes.transform.SetPositionAndRotation(a, Quaternion.LookRotation(dir));
                var shape = stripes.shape;
                shape.position = new Vector3(0f, 0f, dist * 0.5f);
                shape.scale = new Vector3(0f, 0f, dist);
            }
        }

        static void PlaceBeam(Renderer r, Vector3 a, Vector3 b, float width)
        {
            if (r == null)
                return;
            Vector3 d = b - a;
            float len = d.magnitude;
            var tr = r.transform;
            tr.SetPositionAndRotation((a + b) * 0.5f, len > 0.001f ? Quaternion.LookRotation(d) : Quaternion.identity);
            // La malla es una esfera de radio 1: escalar Z a len/2 la estira de A a B.
            tr.localScale = new Vector3(width, width, Mathf.Max(len * 0.5f, width));
        }
    }
}
