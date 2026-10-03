using EmeraldArena.Vfx;
using UnityEngine;

namespace BotwVfx
{
    /// <summary>
    /// Movimiento Emerald con estilo BotW: la animación horneada original
    /// (EmeraldVfxPlayer, ahora con el shader "BotwVFX/Emerald Toon") se sincroniza
    /// con la timeline BotW, que añade las capas Shuriken del impacto, la luz,
    /// la sacudida y la cámara lenta.
    /// Marco local (el del módulo original): atacante en (-3,0,0), objetivo en (+3,0,0).
    /// En reposo (t &lt; 0) el reproductor queda desactivado y no dibuja nada.
    /// </summary>
    [DefaultExecutionOrder(-50)] // fija previewTime antes de que el reproductor dibuje en su Update
    public class EmeraldMoveVfx : VfxTimeline
    {
        [Header("Movimiento")]
        public int moveId;
        public string moveName;
        [Tooltip("Tipo tal como aparece en catalogue.json (p. ej. \"Fuego\").")]
        public string moveType;
        public string family;
        [TextArea] public string description;

        [Header("Animación horneada")]
        public EmeraldVfxPlayer player;
        [Tooltip("Duración de la animación horneada en segundos (121 muestras).")]
        [Min(0.05f)] public float animDuration = 2f;

        [Header("Impacto")]
        [Tooltip("Instante del impacto en segundos, calculado a partir de ImpactStrength.")]
        public float impactTime = 1.2f;
        [Range(0f, 1f)] public float impactStrength;
        [Tooltip("Punto local donde se lanzan las capas del impacto.")]
        public Vector3 impactPoint = new Vector3(3f, 0f, 0f);

        [Header("Encuadre")]
        [Tooltip("Caja XY local aproximada de la animación horneada (la usa la galería para alejar la cámara).")]
        public Vector2 viewMin = new Vector2(-3f, 0f);
        public Vector2 viewMax = new Vector2(3f, 2f);

        public string Title => $"{moveId:000} {moveName} — {moveType}";

        protected override void Awake()
        {
            if (player != null)
            {
                player.playOnEnable = false;
                player.loop = false;
                player.duration = animDuration;
            }
            base.Awake();
        }

        protected override void Evaluate(float t)
        {
            if (player == null)
                return;
            bool active = t >= 0f && t <= animDuration;
            if (player.enabled != active)
                player.enabled = active;
            if (active)
                player.previewTime = Mathf.Clamp01(t / animDuration);
        }

        /// <summary>
        /// Encola ahora los dibujos de la animación horneada para una cámara concreta.
        /// En Play no hace falta (el reproductor dibuja en su Update); lo usan las capturas
        /// del editor tras Preview(t), donde no hay bucle de juego entre medias.
        /// </summary>
        public void DrawBakedNow(Camera cam)
        {
            if (player != null && player.enabled)
                player.RenderAt(player.previewTime, cam);
        }
    }
}
