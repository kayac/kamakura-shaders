using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace Kayac.VisualArts
{

	public class MaterialFormerlySerializedAsDecorator : MaterialPropertyDrawer
	{
		private string _oldSerializedName;

		public MaterialFormerlySerializedAsDecorator(string oldSerializedName)
		{
			_oldSerializedName = oldSerializedName;
		}

		public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			Apply(prop);
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return 0;
		}


		private HashSet<int> _cachedId = new HashSet<int>();

		public bool HasOldSerializedProp(MaterialProperty prop)
		{
			bool needsUpdate = false;
			if (prop.targets.Length != _cachedId.Count)
			{
				needsUpdate = true;
			}
			else
			{
				foreach (var mat in prop.targets)
				{
					if (!_cachedId.Contains(mat.GetInstanceID()))
					{
						needsUpdate = true;
						break;
					}
				}
			}

			if (!needsUpdate)
			{
				return false;
			}

			_cachedId.Clear();

			foreach (Material mat in prop.targets)
			{
				_cachedId.Add(mat.GetInstanceID());
				var matObj = new SerializedObject(mat);
				var savedProps = matObj.FindProperty("m_SavedProperties");
				foreach (SerializedProperty p in savedProps)
				{
					if (p.propertyType == SerializedPropertyType.String && p.stringValue == _oldSerializedName)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);

			if (!HasOldSerializedProp(prop))
			{
				return;
			}

			foreach (Material mat in prop.targets)
			{
				Debug.Log("Converts old property " + _oldSerializedName + " to " + prop.name);
				switch (prop.type)
				{
					case MaterialProperty.PropType.Color:
						var oldColor = (Vector4)GetSavedProp(mat, _oldSerializedName, "m_Colors", true);
						Debug.Log("Color: " + mat.GetColor(prop.name) + " -> " + oldColor);
						mat.SetColor(prop.name, oldColor);
						break;
					case MaterialProperty.PropType.Vector:
						var oldVector = (Vector4)GetSavedProp(mat, _oldSerializedName, "m_Colors", true);
						Debug.Log("Vector: " + mat.GetVector(prop.name) + " -> " + oldVector);
						mat.SetVector(prop.name, oldVector);
						break;
					case MaterialProperty.PropType.Float:
					case MaterialProperty.PropType.Range:
						var oldFloat = (float)GetSavedProp(mat, _oldSerializedName, "m_Floats", true);
						Debug.Log("Float: " + mat.GetFloat(prop.name) + " -> " + oldFloat);
						mat.SetFloat(prop.name, oldFloat);
						break;
					case MaterialProperty.PropType.Texture:
						var data = GetSavedProp(mat, _oldSerializedName, "m_TexEnvs", true) as List<object>;
						var newPropTexture = mat.GetTexture(prop.name);
						var oldPropTexture = ((Texture)data[0]);
						var newPropTextureName = newPropTexture != null ? newPropTexture.name : "(none)";
						if (oldPropTexture != null)
						{
							Debug.Log("Texture: " + newPropTextureName + " -> " + oldPropTexture.name);
							mat.SetTexture(prop.name, oldPropTexture);
							mat.SetTextureScale(prop.name, (Vector2)data[1]);
							mat.SetTextureOffset(prop.name, (Vector2)data[2]);
						}
						break;
				}
			}
		}

		object GetSavedProp(Material mat, string name, string dataType, bool removeProp)
		{
			var obj = new SerializedObject(mat);
			var savedProps = obj.FindProperty("m_SavedProperties");

			var elements = savedProps.FindPropertyRelative(dataType);

			object returnValue = null;

			int propIndex = -1;

			for (int i = 0; i < elements.arraySize; ++i)
			{
				var entry = elements.GetArrayElementAtIndex(i);
				SerializedProperty data = null;

			#if UNITY_5_6_OR_NEWER
				var nameProp = entry.FindPropertyRelative("first");
			#else
				var nameProp = entry.FindPropertyRelative("first").FindPropertyRelative("name");
			#endif

				if (nameProp.stringValue != name)
				{
					continue;
				}
				data = entry.FindPropertyRelative("second");
				if (data != null)
				{
					var val = GetValue(data);
					if (val != null)
					{
						propIndex = i;
						returnValue = val;
						break;
					}
					else if (data.hasChildren)
					{
						var otherDataList = new List<object>();
						otherDataList.Add(GetValue(data.FindPropertyRelative("m_Texture")));
						otherDataList.Add(GetValue(data.FindPropertyRelative("m_Scale")));
						otherDataList.Add(GetValue(data.FindPropertyRelative("m_Offset")));
						propIndex = i;
						returnValue = otherDataList;
						break;
					}
				}
			}

			if (removeProp && propIndex > -1)
			{
				elements.DeleteArrayElementAtIndex(propIndex);
			}
			obj.ApplyModifiedPropertiesWithoutUndo();
			obj.Dispose();
			return returnValue;
		}

		object GetValue(SerializedProperty data)
		{
			switch (data.propertyType)
			{
				case SerializedPropertyType.Float:
					return data.floatValue;
				case SerializedPropertyType.Color:
					return data.colorValue;
				case SerializedPropertyType.ObjectReference:
					return data.objectReferenceValue;
				case SerializedPropertyType.Vector2:
					return data.vector2Value;
				case SerializedPropertyType.Vector3:
					return data.vector3Value;
				case SerializedPropertyType.Vector4:
					return data.vector4Value;
				case SerializedPropertyType.Generic:
				default:
					return null;
			}

		}
	}

}