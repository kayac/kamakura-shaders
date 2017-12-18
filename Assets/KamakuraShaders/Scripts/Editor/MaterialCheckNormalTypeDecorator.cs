using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Kayac.VisualArts
{

	public class MaterialCheckNormalTypeDecorator : MaterialPropertyDrawer
	{

		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (prop.type != MaterialProperty.PropType.Texture)
			{
				return;
			}

			if (prop.textureValue == null)
			{
				return;
			}

			var path = AssetDatabase.GetAssetPath(prop.textureValue);
			var importer = AssetImporter.GetAtPath(path) as TextureImporter;

			if (importer == null)
			{
				return;
			}

			if (importer.textureType != TextureImporterType.NormalMap)
			{
				var bgColor = GUI.backgroundColor;
				GUI.backgroundColor = Color.red;
				EditorGUI.HelpBox(position, "Texture type is not Normal Map!", MessageType.Error);
				GUI.backgroundColor = bgColor;
			}
		}

	}

}