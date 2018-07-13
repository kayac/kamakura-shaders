using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Kayac.VisualArts
{

	public class MaterialCheckNormalTypeDecorator : MaterialPropertyDrawer
	{

		int _textureInstandeId = -1;
		bool _showAlert;

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

			if (_showAlert)
			{
				var bgColor = GUI.backgroundColor;
				GUI.backgroundColor = Color.red;
				EditorGUI.HelpBox(position, "Texture type is not Normal Map!", MessageType.Error);
				GUI.backgroundColor = bgColor;
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			var textureInstandeId = prop.textureValue != null ? prop.textureValue.GetInstanceID() : -1;
			if (textureInstandeId != _textureInstandeId)
			{
				_textureInstandeId = textureInstandeId;

				var path = AssetDatabase.GetAssetPath(prop.textureValue);
				var importer = AssetImporter.GetAtPath(path) as TextureImporter;

				if (importer == null)
				{
					_showAlert = false;
					return 0;
				}
				_showAlert = importer.textureType != TextureImporterType.NormalMap;
			}
			return _showAlert ? base.GetPropertyHeight(prop, label, editor) : 0;
		}

	}

}