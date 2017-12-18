using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace Kayac.VisualArts
{

	public class MaterialKamakuraShaderVersionDrawer : MaterialPropertyDrawer
	{

		public MaterialKamakuraShaderVersionDrawer()
		{

		}

		// public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		// {
		// 	return 30f;
		// }

		// Draw the property inside the given rect
		public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			var version = prop.vectorValue;
			EditorGUI.LabelField(position, "Shader Version: " +  KamakuraShaderGUI.Version);
			// position.y += 16f;
			// EditorGUI.LabelField(position, "Material Version: " + (int)version.x + "." + (int)version.y + "." + (int)version.z);
		}
	}

}