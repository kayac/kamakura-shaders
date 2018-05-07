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
		static int _selectablePropertyCount;
		static Dictionary<string, MaterialProperty> _copiedProperties = new Dictionary<string, MaterialProperty>();
		static HashSet<string> _currentMaterialSelectedProps = new HashSet<string>();

		public static readonly VersionDescriptor Version;

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


		static KamakuraShaderGUI()
		{
			var verGuids = AssetDatabase.FindAssets("t:kayac.visualarts.versiondescriptor");
			if (verGuids.Length == 1)
			{
				var path = AssetDatabase.GUIDToAssetPath(verGuids[0]);
				Version = AssetDatabase.LoadAssetAtPath<VersionDescriptor>(path);
			}
			else
			{
				Version = new VersionDescriptor();
				Version.SetVersion(1, 0, 4);
			}
		}

		public KamakuraShaderGUI()
		{
			_reflectionHelper = new ShaderReflectionHelper();
		}

		void DrawSelectionMode()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			_selectPropertyMode = EditorGUILayout.Toggle("Select Mode", _selectPropertyMode);
			if (_selectPropertyMode)
			{
				int selectionCount = _selectedProperties.Count;
				int copiedPropCount = _copiedProperties.Count;
				GUILayout.Label("Selected items: " + _currentMaterialSelectedProps.Count + " / " + _selectablePropertyCount);
				GUILayout.Label("Items on clipboard: " + copiedPropCount);
			}
			EditorGUILayout.EndVertical();
			_selectionMode = SelectionMode.None;
			if (_selectPropertyMode)
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

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			Material material = materialEditor.target as Material;
			var materials = materialEditor.targets.Cast<Material>();

			RichLabel = new GUIStyle(EditorStyles.boldLabel);
			RichLabel.richText = true;

			EditorGUI.BeginChangeCheck();
			DrawSelectionMode();
			var updateSelectionMode = EditorGUI.EndChangeCheck();

			string disabledPropHeader = "N/A";
			bool insideBoxLayout = false;
			_currentMaterialSelectedProps.Clear();
			_selectablePropertyCount = 0;

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

					EditorGUILayout.BeginVertical(KamakuraInspectorUtility.BoxScopeStyle);
					insideBoxLayout = true;
				}

				if (_dict.TryGetValue(prop.name, out propHeader))
				{
					bool isParamPropEnabled = !Mathf.Approximately(prop.floatValue, 0f);
					disabledPropHeader = isParamPropEnabled ? "N/A" : propHeader;
				}

				var selectable = prop.name != "_ShaderVersion";
				bool wasSelected = false;
				bool selectAll = _selectionMode == SelectionMode.SelectAll;

				if (_selectPropertyMode && selectable)
				{
					if (selectAll)
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

				var isDisabled = prop.name.StartsWith(disabledPropHeader);

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
					GUI.color = Color.white;
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

}