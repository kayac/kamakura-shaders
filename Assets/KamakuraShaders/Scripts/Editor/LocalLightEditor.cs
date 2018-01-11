// Name this script "RotateAtPointEditor"
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Kayac.VisualArts
{

    [CustomEditor(typeof(LocalLight))]
    [CanEditMultipleObjects]
    public class LocalLightEditor : UnityEditor.Editor
    {
        List<LocalLight> multiTarget = new List<LocalLight>();

        public void OnSceneGUI()
        {
            LocalLight t = (target as LocalLight);

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
            Handles.ArrowHandleCap(1, tPos, newRot, 1, EventType.Repaint);
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