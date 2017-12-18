using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


namespace Kayac.VisualArts
{

	[CanEditMultipleObjects]
	[CustomEditor(typeof(Kamakura2DParams), true)]
	public class Kamakura2DParamsInspector : Editor
	{

		SerializedProperty m_hueProp;
		SerializedProperty m_saturationProp;
		SerializedProperty m_brightnessProp;
		SerializedProperty m_alphaProp;
		SerializedProperty m_outlineColorProp;
		SerializedProperty m_outlineThicknessProp;
		SerializedProperty m_outlineSoftnessProp;
		SerializedProperty m_outlineOffsetProp;

		void OnEnable()
		{
			m_hueProp = serializedObject.FindProperty("m_hue");
			m_saturationProp = serializedObject.FindProperty("m_saturation");
			m_brightnessProp = serializedObject.FindProperty("m_brightness");
			m_alphaProp = serializedObject.FindProperty("m_alpha");

			m_outlineColorProp = serializedObject.FindProperty("m_outlineColor");

			m_outlineThicknessProp = serializedObject.FindProperty("m_outlineThickness");
			m_outlineSoftnessProp = serializedObject.FindProperty("m_outlineSoftness");
			m_outlineOffsetProp = serializedObject.FindProperty("m_outlineOffset");
		}


		public override void OnInspectorGUI()
		{
			var comps = targets.Cast<Kamakura2DParams>().ToList();

			if (comps.Any((img) => img.graphic == null && img.spriteRenderer == null))
			{
				EditorGUILayout.HelpBox("This component needs a SpriteRenderer or Graphic component like Image", MessageType.Warning);
				return;
			}
			if (comps.Any((img) => img.sharedMaterial.shader.name != "Kayac/Kamakura2D"))
			{
				EditorGUILayout.HelpBox("This component can only work with Material using KamakuraUI shader", MessageType.Warning);
				return;
			}
			if (comps.Any((img) => img.sharedMaterial.GetFloat("_VertexColorAs") < 0.9f))
			{
				EditorGUILayout.HelpBox("Material's \"Use Vertex Color As\" property must be set to value other than \"Nothing\"", MessageType.Info);
				return;
			}
			if (comps.Select((img) => (int)img.sharedMaterial.GetFloat("_VertexColorAs")).Distinct().Count() > 1)
			{
				EditorGUILayout.HelpBox("Materials' \"Use Vertex Color As\" property contains mixed value", MessageType.Info);
				return;
			}

			var mode = (Kamakura2DParams.VertexColorUsage) comps.Select((img) => (int)img.sharedMaterial.GetFloat("_VertexColorAs")).FirstOrDefault();

			EditorGUILayout.LabelField("Mode: " + mode, EditorStyles.label);

			EditorGUI.BeginChangeCheck();
			if (mode == Kamakura2DParams.VertexColorUsage.FilterParams)
			{
				EditorGUILayout.Slider(m_hueProp, -0.5f, 0.5f, "Hue");
				EditorGUILayout.Slider(m_saturationProp, -10f, 10f, "Saturation");
				EditorGUILayout.Slider(m_brightnessProp, -10f, 10f, "Brightness");
				EditorGUILayout.Slider(m_alphaProp, 0f, 1f, "Alpha");
			}
			else if (mode == Kamakura2DParams.VertexColorUsage.OutlineColor)
			{
				EditorGUILayout.PropertyField(m_outlineColorProp);
			}
			else if (mode == Kamakura2DParams.VertexColorUsage.OutlineParams)
			{
				EditorGUILayout.Slider(m_outlineThicknessProp, 0.0f, 0.5f, "Outline Thickness");
				EditorGUILayout.Slider(m_outlineSoftnessProp, 0.01f, 1f, "Outline Softness");
				EditorGUILayout.Slider(m_outlineOffsetProp, 0f, 0.95f, "Outline Offset");
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				foreach (var t in targets)
				{
					var p = t as Kamakura2DParams;
					p.Refresh();
					EditorUtility.SetDirty(t);
				}
			}
		}
	}

}