using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Flecha Ancestral: la flecha llega, se abre un "objetivo" Sheikah gigante
    /// con cáusticas, un portal absorbe todo hacia su centro y colapsa en un destello.
    /// La flecha y su estela se animan aquí; el resto son partículas de la timeline.
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

        protected override void Evaluate(float t)
        {
            if (arrowStart == null || impactPoint == null)
                return;

            bool flying = t >= 0f && t <= flightTime;
            SetVisible(arrow, flying);
            if (trail != null)
                trail.enabled = flying;
            if (!flying)
                return;

            Vector3 a = arrowStart.position;
            Vector3 b = impactPoint.position;
            Vector3 dir = (b - a).normalized;
            float k = flightTime > 0f ? t / flightTime : 1f;
            Vector3 head = Vector3.Lerp(a, b, k);

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
    }
}
