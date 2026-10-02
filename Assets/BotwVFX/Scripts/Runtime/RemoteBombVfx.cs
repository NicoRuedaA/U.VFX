using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Bomba Remota (Runa Sheikah). La esfera azul se anima por código igual que
    /// en el vídeo de Daniel Ilett (allí con un Animation Clip): crece de golpe,
    /// el umbral sube para que solo quede el borde con fresnel, y se encoge.
    /// Rayos, chispas, onda y polvo son partículas lanzadas por la timeline.
    /// </summary>
    public class RemoteBombVfx : VfxTimeline
    {
        [Header("Esfera de energía")]
        public Renderer sphere;
        public Renderer core;
        public AnimationCurve sphereRadius = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        public AnimationCurve sphereThreshold = AnimationCurve.Constant(0f, 1f, 0.45f);
        public AnimationCurve sphereOpacity = AnimationCurve.Constant(0f, 1f, 1f);
        public AnimationCurve sphereDissolve = AnimationCurve.Constant(0f, 1f, 0f);
        public AnimationCurve coreRadius = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Header("Bomba (solo para la demo)")]
        [Tooltip("Modelo que se oculta al explotar y reaparece en respawnTime. Puede quedar vacío.")]
        public Transform bombModel;
        public float respawnTime = 1.6f;

        static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
        static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        MaterialPropertyBlock block;

        protected override void Evaluate(float t)
        {
            bool active = t >= 0f;
            block ??= new MaterialPropertyBlock();

            float r = active ? sphereRadius.Evaluate(t) : 0f;
            if (sphere != null)
            {
                SetVisible(sphere, r > 0.01f);
                sphere.transform.localScale = Vector3.one * Mathf.Max(r, 0.001f);
                sphere.GetPropertyBlock(block);
                block.SetFloat(ThresholdId, sphereThreshold.Evaluate(Mathf.Max(t, 0f)));
                block.SetFloat(OpacityId, sphereOpacity.Evaluate(Mathf.Max(t, 0f)));
                block.SetFloat(DissolveId, sphereDissolve.Evaluate(Mathf.Max(t, 0f)));
                sphere.SetPropertyBlock(block);
            }

            float c = active ? coreRadius.Evaluate(t) : 0f;
            if (core != null)
            {
                SetVisible(core, c > 0.01f);
                core.transform.localScale = Vector3.one * Mathf.Max(c, 0.001f);
            }

            if (bombModel != null)
            {
                // Reaparece con un pequeño "pop" para poder repetir la demo.
                float pop = !active ? 1f : EaseOutBack(Mathf.Clamp01((t - respawnTime) / 0.25f));
                bool visible = !active || t >= respawnTime;
                if (bombModel.gameObject.activeSelf != visible)
                    bombModel.gameObject.SetActive(visible);
                bombModel.localScale = Vector3.one * Mathf.Max(pop, 0.001f);
            }
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
