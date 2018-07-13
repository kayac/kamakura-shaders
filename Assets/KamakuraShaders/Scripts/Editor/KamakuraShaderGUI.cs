using UnityEngine;
using UnityEngine.Internal;
using UnityEngineInternal;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Reflection;

namespace Kayac.VisualArts
{

	public class KamakuraShaderGUI : ShaderGUI
	{
		static bool _selectPropertyMode = false;
		static SelectionMode _selectionMode;
		static HashSet<string> _selectedProperties = new HashSet<string>();
		static int _selectablePropertyCount;
		static Dictionary<string, MaterialProperty> _copiedProperties = new Dictionary<string, MaterialProperty>();
		static Dictionary<string, string> _cachedDisabledPropHeader = new Dictionary<string, string>();
		static HashSet<string> _currentMaterialSelectedProps = new HashSet<string>();
		static MaterialKSFlagsDecorator _defaultFlags = new MaterialKSFlagsDecorator("");

		Dictionary<string, HashSet<string>> _cachedPropGroups;

		enum SelectionMode
		{
			None,
			SelectAll,
			Copy,
			Paste,
			Clear,
		}


		[MenuItem("Kayac/Kamakura Shaders/Version", false, 9001)]
		static void ShowVersion()
		{
			EditorUtility.DisplayDialog("About Kamakura Shaders", "Version " + VersionDescriptor.Instance.ToString(), "Close");
		}

		private ShaderReflectionHelper _reflectionHelper;

		public KamakuraShaderGUI()
		{
			_reflectionHelper = new ShaderReflectionHelper();
		}

		void DrawSelectionMode()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			_selectPropertyMode = EditorGUILayout.Toggle("Select Mode", _selectPropertyMode);
			using (var disabledScope = new EditorGUI.DisabledGroupScope(!_selectPropertyMode))
			{
				int selectionCount = _selectedProperties.Count;
				int copiedPropCount = _copiedProperties.Count;
				GUILayout.Label("Selected items: " + _currentMaterialSelectedProps.Count + " / " + _selectablePropertyCount);
				GUILayout.Label("Items on clipboard: " + copiedPropCount);
			}
			EditorGUILayout.EndVertical();
			_selectionMode = SelectionMode.None;
			using (var disabledScope = new EditorGUI.DisabledGroupScope(!_selectPropertyMode))
			{
				EditorGUILayout.BeginVertical();
				if (GUILayout.Button("Select All")) { _selectedProperties.Clear(); _selectionMode = SelectionMode.SelectAll; }
				if (GUILayout.Button("Select None")) { _selectedProperties.Clear(); }
				if (GUILayout.Button("Copy")) { _selectionMode = SelectionMode.Copy; }
				if (GUILayout.Button("Paste")) { _selectionMode = SelectionMode.Paste; }
				if (GUILayout.Button("Clear")) { _selectionMode = SelectionMode.Clear; }
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}

		void UpdateSelectionPropsWithHeader(string propName, MaterialProperty[] props, bool isSelected)
		{
			if (isSelected)
			{
				_selectedProperties.UnionWith(_cachedPropGroups[propName]);
			}
			else
			{
				_selectedProperties.RemoveWhere(n => _cachedPropGroups[propName].Contains(n));
			}
		}

		void InitCachedPropGroups(MaterialProperty[] properties, Material material)
		{
			if (_cachedPropGroups != null) { return; }
			_cachedPropGroups = new Dictionary<string, HashSet<string>>();
			HashSet<string> propGroup = null;
			foreach (var prop in properties)
			{
				if (prop.flags == MaterialProperty.PropFlags.HideInInspector || prop.flags == MaterialProperty.PropFlags.PerRendererData)
				{
					continue;
				}
				if (propGroup == null || _reflectionHelper.IsHeader(prop, material.shader))
				{
					propGroup = new HashSet<string>();
				}
				propGroup.Add(prop.name);
				_cachedPropGroups[prop.name] = propGroup;
			}
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			Material material = materialEditor.target as Material;
			var materials = materialEditor.targets.Cast<Material>();

			InitCachedPropGroups(properties, material);

			EditorGUI.BeginChangeCheck();
			DrawSelectionMode();
			var updateSelectionMode = EditorGUI.EndChangeCheck();

			string disabledPropHeader = "*";
			bool insideBoxLayout = false;
			_currentMaterialSelectedProps.Clear();
			_selectablePropertyCount = 0;
			string propHeader = null;

			foreach (var prop in properties)
			{
				if (prop.flags == MaterialProperty.PropFlags.HideInInspector || prop.flags == MaterialProperty.PropFlags.PerRendererData)
				{
					continue;
				}

				if (_reflectionHelper.IsHeader(prop, material.shader))
				{
					if (insideBoxLayout)
					{
						EditorGUILayout.Separator();
						EditorGUILayout.EndVertical();
						insideBoxLayout = false;
					}

					EditorGUILayout.BeginVertical(KamakuraInspectorUtility.BoxScopeStyle);
					insideBoxLayout = true;
				}

				var propFlagsHolder = _reflectionHelper.GetPropertyDrawer<MaterialKSFlagsDecorator>(prop, material.shader) ?? _defaultFlags;
				var selectable = !propFlagsHolder.IsFlagSet(KSFlags.NonSelectable);
				bool wasSelected = false;

				if (propFlagsHolder.IsFlagSet(KSFlags.EnableFlag))
				{
					if (!_cachedDisabledPropHeader.TryGetValue(prop.name, out propHeader))
					{
						propHeader = prop.name.Replace("_Enable", "_");
						_cachedDisabledPropHeader[prop.name] = propHeader;
					}
					bool isParamPropEnabled = !Mathf.Approximately(prop.floatValue, 0f);
					disabledPropHeader = isParamPropEnabled ? "*" : propHeader;
				}


				if (_selectPropertyMode && selectable)
				{
					if (_selectionMode == SelectionMode.SelectAll)
					{
						_selectedProperties.Add(prop.name);
					}
					wasSelected = _selectedProperties.Contains(prop.name);
					_selectablePropertyCount += 1;
					if (wasSelected)
					{
						_currentMaterialSelectedProps.Add(prop.name);
					}
				}


				var isDisabled = prop.name.StartsWith(disabledPropHeader, System.StringComparison.Ordinal);
				if (isDisabled)
				{
					continue;
				}

				if (_selectPropertyMode && selectable)
				{
					GUI.color = wasSelected ? Color.green : Color.white;
					EditorGUILayout.BeginHorizontal(KamakuraInspectorUtility.SmallBoxScopeStyle);
					bool isSelected = EditorGUILayout.Toggle(wasSelected, EditorStyles.radioButton, GUILayout.MaxWidth(20), GUILayout.ExpandHeight(true));
					if (isSelected)
					{
						_selectedProperties.Add(prop.name);
					}
					else
					{
						_selectedProperties.Remove(prop.name);
					}
					if (isSelected != wasSelected && Event.current.alt)
					{
						UpdateSelectionPropsWithHeader(prop.name, properties, isSelected);
					}
					GUI.color = Color.white;
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
				}

				MaterialPropertyUtils.BeginChangeCheck(prop);
				if (prop.name.Contains("UseCubeColor"))
				{
					var disabled = materials.Any(m => m.GetFloat("_EnableCubeColor") == 0);
					using (new EditorGUI.DisabledGroupScope(disabled))
					{
						materialEditor.ShaderProperty(prop, prop.displayName);
					}
				}
				else
				{
					materialEditor.ShaderProperty(prop, prop.displayName);
				}
				var propChanged = MaterialPropertyUtils.EndChangeCheck();

				if (propChanged && propFlagsHolder.IsFlagSet(KSFlags.AutoRenderQueue) && !prop.hasMixedValue && prop.floatValue > 0f)
				{
					foreach (var mat in materials)
					{
						var blendMode = (KamakuraBlendMode)(int)mat.GetFloat("_BlendMode");
						MaterialBlendModeUtils.SetMaterialBlendMode(mat, blendMode, true);
					}
				}

				EditorGUILayout.EndHorizontal();

				// TODO find better way to display this description
				if (prop.name.Contains("_CubeColor5"))
				{
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("CubeColor works on world space by default.");
					EditorGUILayout.LabelField("Attach a CubeColorLocalSpaceRoot component");
					EditorGUILayout.LabelField("to make it works locally relative to the root.");
				}
			}

			if (insideBoxLayout)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.EndVertical();
				insideBoxLayout = false;
			}

			materialEditor.RenderQueueField();
			materialEditor.EnableInstancingField();

			if (updateSelectionMode)
			{
				if (_selectionMode == SelectionMode.Copy)
				{
					_copiedProperties.Clear();
					foreach (var propName in _currentMaterialSelectedProps)
					{
						var prop = properties.FirstOrDefault(p => p.name == propName);
						if (prop != null)
						{
							_copiedProperties[propName] = prop;
						}
						else
						{
							Debug.LogWarning("Could not copy property " + propName + " from current material");
						}
					}
				}
				else if (_selectionMode == SelectionMode.Paste)
				{

					foreach (var selectedPropName in _selectedProperties)
					{
						MaterialProperty copiedProp;
						if (_copiedProperties.TryGetValue(selectedPropName, out copiedProp))
						{
							var targetProp = properties.FirstOrDefault(p => p.name == selectedPropName);

							if (targetProp == null)
							{
								Debug.LogWarning("Could not paste copied property " + selectedPropName + " to current material");
								continue;
							}
							targetProp.floatValue = copiedProp.floatValue;
							targetProp.colorValue = copiedProp.colorValue;
							targetProp.vectorValue = copiedProp.vectorValue;
							targetProp.textureValue = copiedProp.textureValue;
							targetProp.textureScaleAndOffset = copiedProp.textureScaleAndOffset;

							_reflectionHelper.ApplyMaterialPropertyHandler(targetProp, material.shader);
						}
					}
				}
				else if (_selectionMode == SelectionMode.Clear)
				{
					_selectedProperties.Clear();
					_copiedProperties.Clear();
				}
			}
		}
	}

	public static class MaterialPropertyUtils
	{

		static MaterialProperty _target;

		static bool _hasMixedValue;
		static float _floatValue;
		static Vector4 _vectorValue;
		static Color _colorValue;
		static Texture _textureValue;
		static Vector4 _textureScaleAndOffsetValue;

		public static void BeginChangeCheck(MaterialProperty prop)
		{
			_target = prop;
			_hasMixedValue = prop.hasMixedValue;
			_floatValue   = prop.floatValue;
			_colorValue   = prop.colorValue;
			_vectorValue  = prop.vectorValue;
			_textureValue = prop.textureValue;
			_textureScaleAndOffsetValue = prop.textureScaleAndOffset;
		}

		public static bool EndChangeCheck()
		{
			if (_target == null) { return false; }
			var changed = _hasMixedValue != _target.hasMixedValue
				|| _floatValue != _target.floatValue
				|| _vectorValue != _target.vectorValue
				|| _colorValue != _target.colorValue
				|| _textureValue != _target.textureValue
				|| _textureScaleAndOffsetValue != _target.textureScaleAndOffset;
			_target = null;
			return changed;
		}
	}

}