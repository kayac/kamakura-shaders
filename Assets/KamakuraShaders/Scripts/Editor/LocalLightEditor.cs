// Name this script "RotateAtPointEditor"
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Kayac.VisualArts
{

    [CustomEditor(typeof(LocalLight))]
    [CanEditMultipleObjects]
    public class LocalLightEditor : UnityEditor.Editor
    {
        List<LocalLight> multiTarget = new List<LocalLight>();

        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (targets.Length > 1)
            {
                return;
            }

            var t = target as LocalLight;
            var renderers = t.renderers;
            var anyMaterialWithEnabledLocalLight = false;

            if (renderers.Count > 0)
            {
                EditorGUILayout.BeginVertical(KamakuraInspectorUtility.BoxScopeStyle);
                GUILayout.Label("Affected Objects", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Mesh And Materials");
                GUILayout.Label("Local Light On", GUILayout.MaxWidth(80));
                EditorGUILayout.EndHorizontal();
                foreach (var r in renderers)
                {
                    foreach (var mat in r.sharedMaterials)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(r.name, mat, typeof(Material), false);
                        var wasLocalLightEnabled = mat.GetFloat("_EnableLocalLight") == 1f;
                        var isLocalLightEnabled = EditorGUILayout.Toggle(wasLocalLightEnabled, GUILayout.MaxWidth(80));
                        anyMaterialWithEnabledLocalLight |= isLocalLightEnabled;
                        if (isLocalLightEnabled != wasLocalLightEnabled)
                        {
                            if (isLocalLightEnabled)
                                { mat.EnableKeyword("KAMAKURA_LOCALLIGHT_ON"); mat.SetFloat("_EnableLocalLight", 1f); }
                            else
                                { mat.DisableKeyword("KAMAKURA_LOCALLIGHT_ON"); mat.SetFloat("_EnableLocalLight", 0f); }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(KamakuraInspectorUtility.BoxScopeStyle);
                GUILayout.Label("Local Light Warnings", EditorStyles.boldLabel);
                if (!anyMaterialWithEnabledLocalLight)
                {
                    EditorGUILayout.HelpBox("Any materials affected by this component are not local light enabled", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("No Local Light warnings", MessageType.Info);
                }
                EditorGUILayout.EndVertical();

            }
        }

        public void OnSceneGUI()
        {
            LocalLight t = target as LocalLight;

            if (!t.enabled)
            {
                return;
            }

            if (!multiTarget.Contains(t))
            {
                multiTarget.Add(t);
            }

            EditorGUI.BeginChangeCheck();
            Quaternion currentRot = t.totalRotationVector;
            Quaternion newRot = Handles.RotationHandle(currentRot, t.transform.position);
            Vector3 tPos = t.transform.position;

            var hColor = Handles.color;
            Handles.color = t.localLightColor;
            Handles.ArrowHandleCap(1, tPos, newRot, HandleUtility.GetHandleSize(tPos) * 1.5f, EventType.Repaint);
            Handles.color = hColor;

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var mt in multiTarget)
                {
                    Undo.RecordObject(mt, "Rotated totalRotationVector");
                    mt.totalRotationVector = newRot;
                    mt.Update();
                }
            }
        }
    }

}