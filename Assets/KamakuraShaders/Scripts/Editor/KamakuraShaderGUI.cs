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
		static Dictionary<string, string> _dict = new Dictionary<string, string>
		{
			{"_EnableShadowMod", "_ShadowMod"},
			{"_EnableHatch", "_Hatch"},
			{"_EnableRim", "_Rim"},
			{"_EnableOutline", "_Outline"},
			{"_EnableCubeColor", "_CubeColor"},
			{"_EnableNormal", "_Normal"},
			{"_EnableEmission", "_Emission"},
			{"_EnableFilter", "_Filter"},
		};

		static bool _selectPropertyMode = false;

		static SelectionMode _selectionMode;

		static HashSet<string> _selectedProperties = new HashSet<string>();
		static Dictionary<string, MaterialProperty> _copiedProperties = new Dictionary<string, MaterialProperty>();

		public static VersionDescriptor Version = new VersionDescriptor(1, 0, 0);

		const string UseCubeColorPropName = @"_(\w+)UseCubeColor";
		static Dictionary<string, KeyValuePair<string, bool>> _parsedUseCubeColorPropName = new Dictionary<string, KeyValuePair<string, bool>>();

		static GUIStyle RichLabel;

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
			EditorUtility.DisplayDialog("About Kamakura Shaders", "Version " + Version.ToString(), "Close");
		}

		private ShaderReflectionHelper _reflectionHelper;

		public KamakuraShaderGUI()
		{
			_reflectionHelper = new ShaderReflectionHelper();
		}

		void DrawSelectionMode()
		{
			EditorGUILayout.BeginHorizontal();
			_selectPropertyMode = EditorGUILayout.Toggle("Select Mode", _selectPropertyMode);
			_selectionMode = SelectionMode.None;
			if (_selectPropertyMode)
			{
				int selectionCount = _selectedProperties.Count;
				int copiedPropCount = _copiedProperties.Count;
				EditorGUILayout.BeginVertical();
				if (GUILayout.Button("Select All")) { _selectionMode = SelectionMode.SelectAll; }
				if (GUILayout.Button("Select None")) { _selectedProperties.Clear(); }
				if (GUILayout.Button(selectionCount > 0 ? string.Format("Copy ({0})", selectionCount) : "Copy")) { _selectionMode = SelectionMode.Copy; }
				if (GUILayout.Button(copiedPropCount > 0 ? string.Format("Paste ({0})", copiedPropCount) : "Paste")) { _selectionMode = SelectionMode.Paste; }
				if (GUILayout.Button("Clear")) { _selectionMode = SelectionMode.Clear; }
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			Material material = materialEditor.target as Material;
			var materials = materialEditor.targets.Cast<Material>();

			RichLabel = new GUIStyle(EditorStyles.boldLabel);
			RichLabel.richText = true;

			bool parseUseCubeColor = _parsedUseCubeColorPropName.Count == 0;

			EditorGUI.BeginChangeCheck();
			DrawSelectionMode();
			var updateSelectionMode = EditorGUI.EndChangeCheck();

			string disabledPropHeader = "N/A";
			bool insideBoxLayout = false;
			foreach (var prop in properties)
			{
				if (prop.flags == MaterialProperty.PropFlags.HideInInspector || prop.flags == MaterialProperty.PropFlags.PerRendererData)
				{
					continue;
				}
				string propHeader = null;

				if (_reflectionHelper.IsHeader(prop, material.shader))
				{

					if (insideBoxLayout)
					{
						EditorGUILayout.Separator();
						EditorGUILayout.EndVertical();
						insideBoxLayout = false;
					}

					EditorGUILayout.BeginVertical(KamakuraInspectorUtility.BoxScope.BoxScopeStyle);
					insideBoxLayout = true;
				}

				if (_dict.TryGetValue(prop.name, out propHeader))
				{
					bool isParamPropEnabled = !Mathf.Approximately(prop.floatValue, 0f);
					if (isParamPropEnabled)
					{
						disabledPropHeader = "N/A";
					}
					else
					{
						disabledPropHeader = propHeader;
					}
				}

				if (prop.name.StartsWith(disabledPropHeader))
				{
					continue;
				}

				KeyValuePair<string, bool> kvp = default(KeyValuePair<string, bool>);
				bool alreadyParsed = _parsedUseCubeColorPropName.TryGetValue(prop.name, out kvp);
				if (parseUseCubeColor)
				{
					var cubeColorMatch = Regex.Match(prop.name, UseCubeColorPropName);
					if (cubeColorMatch != null && cubeColorMatch.Success)
					{
						kvp = new KeyValuePair<string, bool>(cubeColorMatch.Groups[1].Value, !Mathf.Approximately(prop.floatValue, 0.0f));
						_parsedUseCubeColorPropName.Add(prop.name, kvp);
					}
				}
				else if (alreadyParsed)
				{
					kvp = new KeyValuePair<string, bool>(kvp.Key, !Mathf.Approximately(prop.floatValue, 0.0f));
					_parsedUseCubeColorPropName[prop.name] = kvp;
				}

				if (_selectPropertyMode)
				{
					EditorGUILayout.BeginHorizontal(KamakuraInspectorUtility.BoxScope.SmallBoxScopeStyle);
					bool wasSelected = _selectedProperties.Contains(prop.name);
					bool isSelected = EditorGUILayout.Toggle(wasSelected, EditorStyles.radioButton, GUILayout.MaxWidth(20), GUILayout.ExpandHeight(true)) || _selectionMode == SelectionMode.SelectAll;

					if (isSelected)
					{
						_selectedProperties.Add(prop.name);
					}
					else
					{
						_selectedProperties.Remove(prop.name);
					}
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
				}
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
					foreach (var propName in _selectedProperties)
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

}