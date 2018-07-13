using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace Kayac.VisualArts
{

	public class MaterialKamakuraShaderVersionDrawer : MaterialPropertyDrawer
	{
		string _version;

		public MaterialKamakuraShaderVersionDrawer() {
			_version = "Shader Version: " +  VersionDescriptor.Instance;
		}

		public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			var version = prop.vectorValue;
			EditorGUI.LabelField(position, _version);
		}
	}

}