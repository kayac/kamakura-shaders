using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Runtime.CompilerServices;


namespace Kayac.VisualArts
{

	[CustomEditor(typeof(KamakuraRampTexture))]
	public class KamakuraRampTextureEditor : UnityEditor.Editor
	{
		private int _width = 0;
		private List<Gradient> _cachedGradients;
		private Texture2D _cachedTexture;
		private ReorderableList _list;
		private KamakuraRampTexture _rampObj;

		private void OnEnable()
		{
			_list = new ReorderableList(serializedObject, serializedObject.FindProperty("gradients"), true, true, true, true);
			_list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				var element = _list.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;
				EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
			};

			_list.drawHeaderCallback += (Rect rect) =>
			{
				EditorGUI.LabelField(rect, "Gradient Presets");
			};

			_list.onCanRemoveCallback = (ReorderableList l) =>
			{
				return l.count > 1;
			};

			_list.onChangedCallback = (l) =>
			{
				// Gradients Cache needs to be cleared
				UnityEditorInternal.GradientPreviewCache.ClearCache();
			};

			Undo.undoRedoPerformed -= UnityEditorInternal.GradientPreviewCache.ClearCache;
			Undo.undoRedoPerformed += UnityEditorInternal.GradientPreviewCache.ClearCache;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= UnityEditorInternal.GradientPreviewCache.ClearCache;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			_rampObj = target as KamakuraRampTexture;

			var widthProp = serializedObject.FindProperty("width");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.IntSlider(widthProp, 4, 128);
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.BeginChangeCheck();
			_list.DoLayoutList();
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}

			if (NeedsUpdate(_rampObj))
			{
				_cachedTexture = UpdateTex(_rampObj);
			}

			using (var disableScope = new EditorGUI.DisabledScope(_cachedTexture == null))
			{
				if (GUILayout.Button("Export as PNG"))
				{
					ExportToPNG(_cachedTexture);
				}
			}

		}

		private bool NeedsUpdate(KamakuraRampTexture ramp)
		{
			if (_cachedGradients == null)
			{
				_cachedGradients = new List<Gradient>();
				foreach (var g in ramp.gradients)
				{
					var ng = new Gradient();
					ng.SetKeys(g.colorKeys, g.alphaKeys);
					_cachedGradients.Add(ng);
				}
				_width = ramp.width;
				return true;
			}

			var changed = false;

			while (_cachedGradients.Count > ramp.gradients.Length)
			{
				_cachedGradients.RemoveAt(0);
				changed = true;
			}

			while (_cachedGradients.Count < ramp.gradients.Length)
			{
				var ng = new Gradient();
				_cachedGradients.Add(ng);
				changed = true;
			}

			if (!changed)
			{
				for (int i = 0; i < _cachedGradients.Count; ++i)
				{
					var g = ramp.gradients[i];
					var xColors = g.colorKeys;
					var xAlphas = g.alphaKeys;

					var yColors = _cachedGradients[i].colorKeys;
					var yAlphas = _cachedGradients[i].alphaKeys;

					if (g.mode != _cachedGradients[i].mode) { changed = true; break; }
					if (xColors.Length != yColors.Length) { changed = true; break; }
					if (xAlphas.Length != yAlphas.Length) { changed = true; break; }

					for (int t = 0; t < xColors.Length; ++t)
					{
						if (xColors[t].color != yColors[t].color) { changed = true; break; }
						if (xColors[t].time != yColors[t].time) { changed = true; break; }
					}

					if (changed) { break; }

					for (int t = 0; t < xAlphas.Length; ++t)
					{
						if (xAlphas[t].alpha != yAlphas[t].alpha) { changed = true; break; }
						if (xAlphas[t].time != yAlphas[t].time) { changed = true; break; }
					}

					if (changed) { break; }
				}
			}

			if (changed)
			{
				for (int i = 0; i < _cachedGradients.Count; ++i)
				{
					var g = ramp.gradients[i];
					_cachedGradients[i].mode = g.mode;
					_cachedGradients[i].SetKeys(g.colorKeys, g.alphaKeys);
				}
			}

			if (_width != ramp.width)
			{
				_width = ramp.width;
				changed = true;
			}

			return changed;
		}

		private Texture2D UpdateTex(KamakuraRampTexture rampObj)
		{
			var path = AssetDatabase.GetAssetPath(rampObj);

			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			var objs = AssetDatabase.LoadAllAssetsAtPath(path);

			Texture2D tex = null;
			foreach (var o in objs)
			{
				if (o is Texture2D)
				{
					tex = o as Texture2D;
				}
			}

			if (rampObj.gradients.Length == 0)
			{
				return tex;
			}

			if (tex == null)
			{
				tex = new Texture2D(rampObj.width, rampObj.gradients.Length, TextureFormat.RGB24, false);
				tex.wrapMode = TextureWrapMode.Clamp;
				tex.name = "RampTexture";
				AssetDatabase.AddObjectToAsset(tex, path);
				AssetDatabase.ImportAsset(path);
			}
			else if (tex.width != rampObj.width || tex.height != rampObj.gradients.Length)
			{
				tex.Resize(rampObj.width, rampObj.gradients.Length);
			}

			var width = tex.width;
			var height = tex.height;
			var colors = new Color[tex.width * tex.height];
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					colors[y * width + x] = rampObj.gradients[height - y - 1].Evaluate(1.0f * x / (width - 1));
				}
			}
			tex.SetPixels(colors);
			tex.Apply();

			EditorUtility.SetDirty(rampObj);
			return tex;
		}

		private void ExportToPNG(Texture2D rampTex)
		{
			var currentRampObjPath = AssetDatabase.GetAssetPath(_rampObj);
			var defaultDirectory = System.IO.Path.GetDirectoryName(currentRampObjPath);
			var savePath = EditorUtility.SaveFilePanelInProject("Export as PNG file", _rampObj.name, "png", "Set file path for the PNG file", defaultDirectory);
			if (!string.IsNullOrEmpty(savePath))
			{
				var bytes = rampTex.EncodeToPNG();
				System.IO.File.WriteAllBytes(savePath, bytes);
				AssetDatabase.Refresh();
			}
		}

	}

}

namespace UnityEditorInternal
{
	internal sealed class GradientPreviewCache
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void ClearCache();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Texture2D GetPropertyPreview(SerializedProperty property);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Texture2D GetGradientPreview(Gradient curve);
	}
}