using UnityEngine;
using UnityEditor;
using System;


namespace Kayac.VisualArts
{

	public class MaterialRotationParamDrawer : MaterialPropertyDrawer
	{
		private string _sinParam;
		private string _cosParam;
		public MaterialRotationParamDrawer(string sinParam, string cosParam)
		{
			_sinParam = sinParam;
			_cosParam = cosParam;
		}
		// Draw the property inside the given rect
		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			// Setup

			EditorGUI.BeginChangeCheck();
			editor.RangeProperty(position, prop, label);
			// editor.ShaderProperty(prop, prop.displayName);
			if (EditorGUI.EndChangeCheck())
			{
				Apply(prop);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			foreach (Material mat in prop.targets)
			{
				var radValue = Mathf.Deg2Rad * prop.floatValue;
				mat.SetFloat(_sinParam, Mathf.Sin(radValue));
				mat.SetFloat(_cosParam, Mathf.Cos(radValue));
			}
		}
	}

}