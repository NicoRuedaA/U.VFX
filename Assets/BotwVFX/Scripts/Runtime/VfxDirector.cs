using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Sacudida de cámara y cámara lenta compartidas por todos los efectos.
    /// La cámara lenta en el impacto (como hace el artículo de 80.lv al bloquear
    /// el rayo con el escudo) da muchísimo "peso" a los golpes.
    /// </summary>
    public class VfxDirector : MonoBehaviour
    {
        static VfxDirector instance;

        [Tooltip("Velocidad base del juego (la demo la cambia con el slider).")]
        [Range(0.05f, 2f)] public float baseTimeScale = 1f;
        public bool enableSlowMotion = true;
        [Range(0f, 2f)] public float shakeMultiplier = 1f;

        [Tooltip("Si no hay un controlador de cámara propio, aplica la sacudida directamente a Camera.main.")]
        public bool applyShakeToMainCamera;

        [Header("Sonido")]
        [Range(0f, 1f)] public float masterVolume = 0.6f;
        public bool muted;
        [Range(0f, 1f)] public float spatialBlend = 0.35f;

        /// <summary>
        /// Destello de pantalla (color, intensidad 0..1, duración real). Lo escucha
        /// VfxScreenFlash (URP); así este script no depende de ningún pipeline.
        /// </summary>
        public static event System.Action<Color, float, float> FlashRequested;

        public Vector3 ShakeOffset { get; private set; }

        const int AudioVoices = 10;
        AudioSource[] voices;
        int nextVoice;

        float shakeStrength;
        float shakeDuration;
        float shakeElapsed = float.MaxValue;
        float slowUntil;
        float slowScale = 1f;
        Vector3 appliedOffset;

        public static VfxDirector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<VfxDirector>();
                    if (instance == null && Application.isPlaying)
                        instance = new GameObject("VfxDirector").AddComponent<VfxDirector>();
                }
                return instance;
            }
        }

        public static void Shake(float strength, float duration)
        {
            var d = Instance;
            if (d == null)
                return;
            // Si ya hay una sacudida más fuerte en curso, no la pisamos.
            float remaining = d.shakeElapsed < d.shakeDuration ? d.shakeStrength * (1f - d.shakeElapsed / d.shakeDuration) : 0f;
            if (strength < remaining)
                return;
            d.shakeStrength = strength;
            d.shakeDuration = Mathf.Max(0.01f, duration);
            d.shakeElapsed = 0f;
        }

        public static void SlowMotion(float timeScale, float realSeconds)
        {
            var d = Instance;
            if (d == null || !d.enableSlowMotion)
                return;
            d.slowScale = timeScale;
            d.slowUntil = Time.unscaledTime + realSeconds;
        }

        public static void Flash(Color color, float intensity, float realSeconds)
        {
            FlashRequested?.Invoke(color, intensity, realSeconds);
        }

        public static void PlaySound(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            var d = Instance;
            if (d == null || clip == null || d.muted)
                return;
            d.EnsureVoices();
            var source = d.voices[d.nextVoice];
            d.nextVoice = (d.nextVoice + 1) % d.voices.Length;
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume * d.masterVolume;
            // El tono sigue a la cámara lenta: el impacto suena "pesado".
            source.pitch = pitch * Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(Time.timeScale / Mathf.Max(d.baseTimeScale, 0.01f)));
            source.Play();
        }

        void EnsureVoices()
        {
            if (voices != null)
                return;
            voices = new AudioSource[AudioVoices];
            for (int i = 0; i < AudioVoices; i++)
            {
                var go = new GameObject("Voice" + i);
                go.transform.SetParent(transform, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = spatialBlend;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 10f;
                source.maxDistance = 120f;
                source.dopplerLevel = 0f;
                voices[i] = source;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                Time.timeScale = 1f;
            }
        }

        void Update()
        {
            float slow = Time.unscaledTime < slowUntil ? slowScale : 1f;
            Time.timeScale = baseTimeScale * slow;

            if (shakeElapsed < shakeDuration)
            {
                shakeElapsed += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(shakeElapsed / shakeDuration);
                k *= k;
                float t = Time.unscaledTime * 28f;
                var n = new Vector3(
                    Mathf.PerlinNoise(t, 0.31f) - 0.5f,
                    Mathf.PerlinNoise(0.73f, t) - 0.5f,
                    Mathf.PerlinNoise(t * 0.7f, 5.1f) - 0.5f);
                ShakeOffset = n * (2f * shakeStrength * k * shakeMultiplier);
            }
            else
            {
                ShakeOffset = Vector3.zero;
            }
        }

        void LateUpdate()
        {
            if (!applyShakeToMainCamera)
                return;
            var cam = Camera.main;
            if (cam == null)
                return;
            cam.transform.position += ShakeOffset - appliedOffset;
            appliedOffset = ShakeOffset;
        }
    }
}
