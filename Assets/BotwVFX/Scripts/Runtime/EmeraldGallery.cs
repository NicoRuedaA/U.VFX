using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BotwVfx
{
    /// <summary>
    /// Galería de los 165 movimientos Emerald con estilo BotW.
    /// Instancia un prefab cada vez (en el punto medio entre atacante y objetivo) y lo reproduce.
    /// Flechas izq./dcha.: anterior/siguiente · Espacio o R: repetir · A: avance automático ·
    /// T: cámara lenta · clic derecho: orbitar · rueda: zoom.
    /// </summary>
    public class EmeraldGallery : MonoBehaviour
    {
        [Header("Movimientos")]
        public EmeraldMoveVfx[] moves = Array.Empty<EmeraldMoveVfx>();
        [Tooltip("Punto medio entre atacante (-3,0,0) y objetivo (+3,0,0).")]
        public Transform stage;
        public int startIndex;

        [Header("Reproducción")]
        public bool autoAdvance = true;
        [Tooltip("Pausa (segundos reales) entre el final de un movimiento y el siguiente.")]
        public float autoDelay = 0.8f;
        [Range(0.05f, 1f)] public float slowMotionSpeed = 0.25f;

        [Header("Cámara (frontal / tres cuartos)")]
        public Camera targetCamera;
        public Vector3 focus = new Vector3(0.3f, 1.3f, 0f);
        public float yaw = 12f;
        public float pitch = 9f;
        public float distance = 11f;
        [Tooltip("Margen alrededor de la caja del efecto al alejar la cámara para los movimientos grandes.")]
        public float framingMargin = 0.6f;
        public float maxDistance = 16f;

        public int CurrentIndex { get; private set; } = -1;
        public EmeraldMoveVfx Current { get; private set; }

        float camYaw, camPitch, camDistance;
        float goalYaw, goalPitch, goalDistance;
        Vector3 camPivot, goalPivot;
        bool slowMotion;
        float autoTimer = -1f;
        GUIStyle panelStyle, titleStyle, labelStyle, buttonStyle, hintStyle;
        Texture2D panelTex, buttonTex, activeTex;

        void Start()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            camYaw = goalYaw = yaw;
            camPitch = goalPitch = pitch;
            camDistance = goalDistance = distance;
            camPivot = goalPivot = focus;
            if (moves.Length > 0)
                Show(Mathf.Clamp(startIndex, 0, moves.Length - 1));
            camPivot = goalPivot;
            camDistance = goalDistance;
        }

        /// <summary>Instancia y reproduce el movimiento de la posición index (0..164).</summary>
        public void Show(int index)
        {
            if (moves.Length == 0)
                return;
            index = (index % moves.Length + moves.Length) % moves.Length;
            if (Current != null)
                Destroy(Current.gameObject);
            CurrentIndex = index;
            Current = null;
            autoTimer = -1f;
            var prefab = moves[index];
            if (prefab == null)
                return;
            var parent = stage != null ? stage : transform;
            Current = Instantiate(prefab, parent.position, parent.rotation, parent);
            Frame(Current, targetCamera, out goalPivot, out goalDistance);
            Current.Play();
        }

        public void Next() => Show(CurrentIndex + 1);
        public void Previous() => Show(CurrentIndex - 1);

        public void Replay()
        {
            if (Current != null)
            {
                autoTimer = -1f;
                Current.Play();
            }
            else
            {
                Show(Mathf.Max(CurrentIndex, 0));
            }
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.rightArrowKey.wasPressedThisFrame)
                    Next();
                if (kb.leftArrowKey.wasPressedThisFrame)
                    Previous();
                if (kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame)
                    Replay();
                if (kb.aKey.wasPressedThisFrame)
                    autoAdvance = !autoAdvance;
                if (kb.tKey.wasPressedThisFrame)
                    slowMotion = !slowMotion;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed || mouse.middleButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    goalYaw += delta.x * 0.2f;
                    goalPitch = Mathf.Clamp(goalPitch - delta.y * 0.2f, -5f, 80f);
                }
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    goalDistance = Mathf.Clamp(goalDistance * (1f - Mathf.Sign(scroll) * 0.1f), 3f, 40f);
            }

            var director = VfxDirector.Instance;
            if (director != null)
                director.baseTimeScale = slowMotion ? slowMotionSpeed : 1f;

            // Avance automático: al terminar un movimiento pasa al siguiente.
            if (autoAdvance && Current != null && !Current.IsPlaying)
            {
                if (autoTimer < 0f)
                    autoTimer = 0f;
                autoTimer += Time.unscaledDeltaTime;
                if (autoTimer >= autoDelay)
                    Next();
            }
        }

        void LateUpdate()
        {
            if (targetCamera == null)
                return;
            float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 4f);
            camPivot = Vector3.Lerp(camPivot, goalPivot, k);
            camYaw = Mathf.LerpAngle(camYaw, goalYaw, k);
            camPitch = Mathf.Lerp(camPitch, goalPitch, k);
            camDistance = Mathf.Lerp(camDistance, goalDistance, k);
            Vector3 shake = VfxDirector.Instance != null ? VfxDirector.Instance.ShakeOffset : Vector3.zero;
            PoseCamera(targetCamera, StageOrigin + camPivot, camYaw, camPitch, camDistance, shake);
        }

        Vector3 StageOrigin => stage != null ? stage.position : transform.position;

        /// <summary>
        /// Encuadre para un movimiento: el normal (frontal, cercano) si el efecto cabe;
        /// si no, centra la caja del efecto y aleja la cámara lo justo (los horneados asumen vista frontal).
        /// </summary>
        public void Frame(EmeraldMoveVfx fx, Camera cam, out Vector3 pivot, out float dist)
        {
            pivot = focus;
            dist = distance;
            if (fx == null)
                return;
            Vector2 min = fx.viewMin - Vector2.one * framingMargin;
            Vector2 max = fx.viewMax + Vector2.one * framingMargin;
            float halfV = Mathf.Tan((cam != null ? cam.fieldOfView : 45f) * 0.5f * Mathf.Deg2Rad);
            float aspect = cam != null && cam.aspect > 0.1f ? cam.aspect : 16f / 9f;
            float FitDistance(Vector3 p) => Mathf.Max(
                Mathf.Max(max.x - p.x, p.x - min.x) / (halfV * aspect),
                Mathf.Max(max.y - p.y, p.y - min.y) / halfV);
            if (FitDistance(pivot) <= distance)
                return;
            pivot = new Vector3(Mathf.Clamp((min.x + max.x) * 0.5f, -3f, 3f), Mathf.Clamp((min.y + max.y) * 0.5f, focus.y, 3.5f), 0f);
            dist = Mathf.Clamp(FitDistance(pivot), distance, maxDistance);
        }

        /// <summary>Coloca la cámara en el encuadre de un movimiento sin transición (lo usan las capturas del editor).</summary>
        public void PoseFor(EmeraldMoveVfx fx, Camera cam)
        {
            Frame(fx, cam, out var pivot, out float dist);
            PoseCamera(cam, StageOrigin + pivot, yaw, pitch, dist, Vector3.zero);
        }

        static void PoseCamera(Camera cam, Vector3 pivot, float yawDeg, float pitchDeg, float dist, Vector3 offset)
        {
            var rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
            cam.transform.SetPositionAndRotation(pivot + rot * new Vector3(0f, 0f, -dist) + offset, rot);
        }

        // ---------------------------------------------------------------- UI

        void OnGUI()
        {
            if (moves.Length == 0)
                return;
            EnsureStyles();
            float scale = Mathf.Max(1f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            const float width = 420f;
            GUILayout.BeginArea(new Rect(16f, 16f, width, 330f), panelStyle);
            GUILayout.Label("Movimientos Emerald · estilo BotW", hintStyle);
            var fx = Current;
            GUILayout.Label(fx != null ? fx.Title : "—", titleStyle);
            if (fx != null)
            {
                if (!string.IsNullOrEmpty(fx.family))
                    GUILayout.Label(fx.family, hintStyle);
                GUILayout.Space(4f);
                GUILayout.Label(fx.description, labelStyle);
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Anterior", buttonStyle, GUILayout.Height(32f)))
                Previous();
            if (GUILayout.Button("Repetir", buttonStyle, GUILayout.Height(32f)))
                Replay();
            if (GUILayout.Button("Siguiente ▶", buttonStyle, GUILayout.Height(32f)))
                Next();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            autoAdvance = GUILayout.Toggle(autoAdvance, " Avance automático (A)", labelStyle);
            slowMotion = GUILayout.Toggle(slowMotion, $" Lento x{slowMotionSpeed:0.##} (T)", labelStyle);
            GUILayout.EndHorizontal();
            GUILayout.Label($"{CurrentIndex + 1}/{moves.Length} · ←/→ cambiar · Espacio repetir · clic dcho orbitar", hintStyle);
            GUILayout.EndArea();
        }

        void EnsureStyles()
        {
            if (panelStyle != null)
                return;
            panelTex = MakeTex(new Color(0.03f, 0.08f, 0.12f, 0.78f));
            buttonTex = MakeTex(new Color(0.1f, 0.2f, 0.27f, 0.9f));
            activeTex = MakeTex(new Color(0.2f, 0.62f, 0.78f, 0.95f));

            panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 12, 12) };
            panelStyle.normal.background = panelTex;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.55f, 0.92f, 1f);
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            labelStyle.normal.textColor = new Color(0.9f, 0.95f, 0.95f);
            hintStyle = new GUIStyle(labelStyle) { fontSize = 11 };
            hintStyle.normal.textColor = new Color(0.65f, 0.78f, 0.82f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, padding = new RectOffset(10, 10, 4, 4) };
            buttonStyle.normal.background = buttonTex;
            buttonStyle.hover.background = activeTex;
            buttonStyle.normal.textColor = buttonStyle.hover.textColor = Color.white;
        }

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        void OnDestroy()
        {
            if (panelTex != null) Destroy(panelTex);
            if (buttonTex != null) Destroy(buttonTex);
            if (activeTex != null) Destroy(activeTex);
        }
    }
}
