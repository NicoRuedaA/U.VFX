using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BotwVfx
{
    /// <summary>
    /// Escena de demostración: cada "estación" tiene un efecto original, su variación
    /// (V2) en el mismo sitio y un encuadre.
    /// 1-4 lanzan los efectos, V alterna original/variación, C los compara seguidos,
    /// Espacio repite, clic derecho orbita, rueda = zoom, M silencia.
    /// </summary>
    public class DemoController : MonoBehaviour
    {
        [Serializable]
        public class Station
        {
            public string name;
            [TextArea] public string description;
            public VfxTimeline effect;
            [TextArea] public string variantDescription;
            public VfxTimeline variant;
            public Vector3 focus;
            public float yaw;
            public float pitch = 12f;
            public float distance = 14f;

            public VfxTimeline Get(bool useVariant) => useVariant && variant != null ? variant : effect;
        }

        public Camera targetCamera;
        public Station[] stations = Array.Empty<Station>();
        [Tooltip("Mostrar la variación (V2) en lugar del original.")]
        public bool showVariant = true;
        public bool autoPlay;
        public float autoDelay = 1.2f;
        [Range(0.05f, 1f)] public float slowMotionSpeed = 0.25f;

        int current;
        float yaw, pitch, distance;
        Vector3 pivot;
        float goalYaw, goalPitch, goalDistance;
        Vector3 goalPivot;
        bool slowMotion;
        float autoTimer = -1f;
        int compareStage;
        float compareTimer;
        GUIStyle panelStyle, titleStyle, labelStyle, buttonStyle, activeButtonStyle, hintStyle;
        Texture2D panelTex, buttonTex, activeTex;

        VfxTimeline CurrentEffect => stations.Length > 0 ? stations[current].Get(showVariant) : null;

        void Start()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            if (stations.Length == 0)
                return;
            ApplyVersion();
            Select(0, snap: true);
            Play(0);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                for (int i = 0; i < stations.Length && i < 9; i++)
                {
                    if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
                        Play(i);
                }
                if (kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame)
                    Play(current);
                if (kb.vKey.wasPressedThisFrame)
                    SetVersion(!showVariant);
                if (kb.cKey.wasPressedThisFrame)
                    StartCompare();
                if (kb.mKey.wasPressedThisFrame && VfxDirector.Instance != null)
                    VfxDirector.Instance.muted = !VfxDirector.Instance.muted;
                if (kb.tKey.wasPressedThisFrame)
                    slowMotion = !slowMotion;
                if (kb.aKey.wasPressedThisFrame)
                    autoPlay = !autoPlay;
                if (kb.rightArrowKey.wasPressedThisFrame)
                    Play((current + 1) % stations.Length);
                if (kb.leftArrowKey.wasPressedThisFrame)
                    Play((current + stations.Length - 1) % stations.Length);
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
                    goalDistance = Mathf.Clamp(goalDistance * (1f - Mathf.Sign(scroll) * 0.1f), 3f, 60f);
            }

            var director = VfxDirector.Instance;
            if (director != null)
                director.baseTimeScale = slowMotion ? slowMotionSpeed : 1f;

            // Comparación: original y, al terminar, la variación.
            if (compareStage == 1 && CurrentEffect != null && !CurrentEffect.IsPlaying)
            {
                compareTimer += Time.unscaledDeltaTime;
                if (compareTimer > 0.6f)
                {
                    compareStage = 0;
                    SetVersion(true);
                }
            }

            // Modo automático: cuando termina un efecto pasa al siguiente.
            if (autoPlay && compareStage == 0 && stations.Length > 0)
            {
                var fx = CurrentEffect;
                if (fx != null && !fx.IsPlaying)
                {
                    if (autoTimer < 0f)
                        autoTimer = 0f;
                    autoTimer += Time.unscaledDeltaTime;
                    if (autoTimer >= autoDelay)
                        Play((current + 1) % stations.Length);
                }
            }
        }

        void LateUpdate()
        {
            if (targetCamera == null)
                return;
            float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 4f);
            pivot = Vector3.Lerp(pivot, goalPivot, k);
            yaw = Mathf.LerpAngle(yaw, goalYaw, k);
            pitch = Mathf.Lerp(pitch, goalPitch, k);
            distance = Mathf.Lerp(distance, goalDistance, k);

            var rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 shake = VfxDirector.Instance != null ? VfxDirector.Instance.ShakeOffset : Vector3.zero;
            targetCamera.transform.SetPositionAndRotation(pivot + rot * new Vector3(0f, 0f, -distance) + shake, rot);
        }

        public void Play(int index)
        {
            if (index < 0 || index >= stations.Length)
                return;
            if (index != current)
                Select(index, snap: false);
            autoTimer = -1f;
            var fx = stations[index].Get(showVariant);
            if (fx != null)
                fx.Play();
        }

        public void SetVersion(bool variant)
        {
            showVariant = variant;
            ApplyVersion();
            Play(current);
        }

        public void StartCompare()
        {
            compareStage = 1;
            compareTimer = 0f;
            SetVersion(false);
        }

        // Solo una de las dos versiones está activa (así no se ven dos bombas o dos enemigos).
        void ApplyVersion()
        {
            foreach (var s in stations)
            {
                var active = s.Get(showVariant);
                foreach (var fx in new[] { s.effect, s.variant })
                {
                    if (fx == null)
                        continue;
                    bool on = fx == active;
                    if (!on && fx.gameObject.activeSelf)
                        fx.StopAndClear();
                    fx.gameObject.SetActive(on);
                }
            }
        }

        void Select(int index, bool snap)
        {
            current = index;
            var s = stations[index];
            goalPivot = s.focus;
            goalYaw = s.yaw;
            goalPitch = s.pitch;
            goalDistance = s.distance;
            if (snap)
            {
                pivot = goalPivot;
                yaw = goalYaw;
                pitch = goalPitch;
                distance = goalDistance;
            }
        }

        /// <summary>Coloca una cámara en el encuadre de una estación (lo usan las capturas del editor).</summary>
        public static void PoseCamera(Camera cam, Station s)
        {
            var rot = Quaternion.Euler(s.pitch, s.yaw, 0f);
            cam.transform.SetPositionAndRotation(s.focus + rot * new Vector3(0f, 0f, -s.distance), rot);
        }

        // ---------------------------------------------------------------- UI

        void OnGUI()
        {
            if (stations.Length == 0)
                return;
            EnsureStyles();
            float scale = Mathf.Max(1f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            const float width = 340f;
            float height = 230f + stations.Length * 40f + 110f;
            GUILayout.BeginArea(new Rect(16f, 16f, width, height), panelStyle);
            GUILayout.Label("BotW VFX", titleStyle);
            GUILayout.Label("Efectos estilo Breath of the Wild", hintStyle);
            GUILayout.Space(8f);

            for (int i = 0; i < stations.Length; i++)
            {
                var style = i == current ? activeButtonStyle : buttonStyle;
                if (GUILayout.Button($"{i + 1}   {stations[i].name}", style, GUILayout.Height(34f)))
                    Play(i);
            }

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Original", showVariant ? buttonStyle : activeButtonStyle, GUILayout.Height(28f)))
                SetVersion(false);
            if (GUILayout.Button("Variación", showVariant ? activeButtonStyle : buttonStyle, GUILayout.Height(28f)))
                SetVersion(true);
            if (GUILayout.Button("Comparar", compareStage > 0 ? activeButtonStyle : buttonStyle, GUILayout.Height(28f)))
                StartCompare();
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            var s = stations[current];
            var desc = showVariant && s.variant != null ? s.variantDescription : s.description;
            if (!string.IsNullOrEmpty(desc))
                GUILayout.Label(desc, labelStyle);

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            autoPlay = GUILayout.Toggle(autoPlay, " Auto (A)", labelStyle);
            slowMotion = GUILayout.Toggle(slowMotion, $" Lento x{slowMotionSpeed:0.##} (T)", labelStyle);
            var director = VfxDirector.Instance;
            if (director != null)
                director.muted = !GUILayout.Toggle(!director.muted, " Sonido (M)", labelStyle);
            GUILayout.EndHorizontal();
            GUILayout.Label("V: original/variación · C: comparar · Espacio: repetir\nClic dcho: orbitar · Rueda: zoom", hintStyle);
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
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = new Color(0.55f, 0.92f, 1f);
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            labelStyle.normal.textColor = new Color(0.9f, 0.95f, 0.95f);
            hintStyle = new GUIStyle(labelStyle) { fontSize = 11 };
            hintStyle.normal.textColor = new Color(0.65f, 0.78f, 0.82f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 8, 4, 4) };
            buttonStyle.normal.background = buttonTex;
            buttonStyle.hover.background = activeTex;
            buttonStyle.normal.textColor = buttonStyle.hover.textColor = Color.white;
            activeButtonStyle = new GUIStyle(buttonStyle) { fontStyle = FontStyle.Bold };
            activeButtonStyle.normal.background = activeTex;
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
