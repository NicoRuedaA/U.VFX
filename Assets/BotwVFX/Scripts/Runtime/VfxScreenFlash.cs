using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BotwVfx
{
    /// <summary>
    /// Destello de pantalla completa de 1-2 fotogramas (solo URP).
    /// Crea un Volume global propio con exposición + filtro de color y anima su peso.
    /// Escucha VfxDirector.FlashRequested, que lanzan los FlashCue de las timelines.
    /// </summary>
    public class VfxScreenFlash : MonoBehaviour
    {
        [Tooltip("Exposición extra con el destello al máximo.")]
        public float exposure = 2f;

        Volume volume;
        ColorAdjustments adjustments;
        VolumeProfile profile;
        float strength;
        float duration = 0.1f;
        float elapsed = float.MaxValue;

        void OnEnable()
        {
            VfxDirector.FlashRequested += OnFlash;
            if (volume == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.hideFlags = HideFlags.DontSave;
                adjustments = profile.Add<ColorAdjustments>(true);
                adjustments.postExposure.Override(exposure);
                adjustments.colorFilter.Override(Color.white);
                volume = gameObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 100f;
                volume.weight = 0f;
                volume.sharedProfile = profile;
                volume.hideFlags = HideFlags.DontSave;
            }
        }

        void OnDisable()
        {
            VfxDirector.FlashRequested -= OnFlash;
            if (volume != null)
                volume.weight = 0f;
        }

        void OnDestroy()
        {
            if (profile != null)
                Destroy(profile);
        }

        void OnFlash(Color color, float intensity, float seconds)
        {
            if (adjustments == null)
                return;
            adjustments.colorFilter.Override(Color.Lerp(Color.white, color, 0.6f));
            adjustments.postExposure.Override(exposure);
            strength = Mathf.Clamp01(intensity);
            duration = Mathf.Max(0.02f, seconds);
            elapsed = 0f;
        }

        void Update()
        {
            if (volume == null)
                return;
            if (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(elapsed / duration);
                volume.weight = strength * k * k;
            }
            else
            {
                volume.weight = 0f;
            }
        }
    }
}
