using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Añade al inspector de cualquier efecto una barra para "rebobinar" y ver
    /// cualquier instante sin entrar en Play (como la timeline de un programa de VFX).
    /// </summary>
    [CustomEditor(typeof(VfxTimeline), true)]
    public class VfxTimelineEditor : Editor
    {
        float previewTime;
        bool previewing;

        public override void OnInspectorGUI()
        {
            var fx = (VfxTimeline)target;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Previsualización", EditorStyles.boldLabel);
                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Play", GUILayout.Height(24f)))
                        fx.Play();
                    EditorGUILayout.LabelField(fx.IsPlaying ? $"t = {fx.CurrentTime:0.00} s" : "En reposo");
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    previewTime = EditorGUILayout.Slider("Tiempo", previewTime, 0f, fx.duration);
                    if (EditorGUI.EndChangeCheck())
                    {
                        previewing = true;
                        fx.Preview(previewTime);
                        SceneView.RepaintAll();
                    }
                    if (previewing && GUILayout.Button("Reset"))
                    {
                        previewing = false;
                        fx.Preview(-1f);
                        SceneView.RepaintAll();
                    }
                }
            }
            EditorGUILayout.Space(4f);
            DrawDefaultInspector();
        }

        void OnDisable()
        {
            if (previewing && target != null && !Application.isPlaying)
                ((VfxTimeline)target).Preview(-1f);
        }
    }
}
