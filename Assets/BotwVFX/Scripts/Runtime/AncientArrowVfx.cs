using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Flecha Ancestral: la flecha llega, se abre un "objetivo" Sheikah gigante
    /// con cáusticas, un portal absorbe todo hacia su centro y colapsa en un destello.
    /// La flecha, su estela y (opcional) la víctima se animan aquí; el resto son
    /// partículas de la timeline.
    /// </summary>
    public class AncientArrowVfx : VfxTimeline
    {
        public Transform arrowStart;
        public Transform impactPoint;
        public Renderer arrow;
        public LineRenderer trail;
        public float flightTime = 0.25f;
        public float arrowLength = 1.4f;
        public float arrowWidth = 0.07f;
        public float trailLength = 5f;

        [Header("Opcional (V2)")]
        [Tooltip("Emisor que sigue a la punta de la flecha (chispas con Rate over Distance).")]
        public Transform headFollower;
        [Tooltip("Enemigo que se disuelve y es absorbido hacia el centro del portal.")]
        public Transform victim;
        public Renderer[] victimRenderers;
        public Vector2 victimDissolveTime = new Vector2(0.45f, 1.3f);
        public float victimRespawnTime = 2.7f;

        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        MaterialPropertyBlock block;
        Vector3 victimRestPosition;
        bool victimCached;

        protected override void Evaluate(float t)
        {
            if (arrowStart == null || impactPoint == null)
                return;

            Vector3 a = arrowStart.position;
            Vector3 b = impactPoint.position;
            Vector3 dir = (b - a).normalized;
            float k = flightTime > 0f ? Mathf.Clamp01(t / flightTime) : 1f;
            Vector3 head = Vector3.Lerp(a, b, k);

            bool flying = t >= 0f && t <= flightTime;
            SetVisible(arrow, flying);
            if (trail != null)
                trail.enabled = flying;
            if (headFollower != null)
                headFollower.position = t >= 0f ? head : a;

            if (flying)
            {
                if (arrow != null)
                {
                    var tr = arrow.transform;
                    tr.SetPositionAndRotation(head - dir * (arrowLength * 0.5f), Quaternion.LookRotation(dir));
                    tr.localScale = new Vector3(arrowWidth, arrowWidth, arrowLength * 0.5f);
                }
                if (trail != null)
                {
                    float back = Mathf.Min(trailLength, Vector3.Distance(a, head));
                    trail.SetPosition(0, head - dir * back);
                    trail.SetPosition(1, head);
                }
            }

            EvaluateVictim(t, b);
        }

        void EvaluateVictim(float t, Vector3 center)
        {
            if (victim == null)
                return;
            if (!victimCached)
            {
                victimRestPosition = victim.localPosition;
                victimCached = true;
            }

            float dissolve = 0f, pull = 0f, scale = 1f;
            bool visible = true;
            if (t >= 0f)
            {
                float d = Mathf.InverseLerp(victimDissolveTime.x, victimDissolveTime.y, t);
                dissolve = d;
                pull = EaseInCubic(d);
                scale = Mathf.Lerp(1f, 0.15f, pull);
                visible = t < victimDissolveTime.y;
                if (t >= victimRespawnTime)
                {
                    // Reaparece para poder repetir la demo.
                    float pop = Mathf.Clamp01((t - victimRespawnTime) / 0.25f);
                    dissolve = 1f - pop;
                    pull = 0f;
                    scale = 1f;
                    visible = true;
                }
            }

            if (victim.gameObject.activeSelf != visible)
                victim.gameObject.SetActive(visible);
            Vector3 rest = victim.parent != null ? victim.parent.TransformPoint(victimRestPosition) : victimRestPosition;
            victim.position = Vector3.Lerp(rest, center, pull);
            victim.localScale = Vector3.one * scale;

            block ??= new MaterialPropertyBlock();
            if (victimRenderers == null)
                return;
            foreach (var r in victimRenderers)
            {
                if (r == null)
                    continue;
                r.GetPropertyBlock(block);
                block.SetFloat(DissolveId, dissolve);
                r.SetPropertyBlock(block);
            }
        }
    }
}
