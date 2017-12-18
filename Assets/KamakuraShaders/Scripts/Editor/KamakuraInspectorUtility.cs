using UnityEditor;
using UnityEngine;


namespace Kayac.VisualArts
{

	public class KamakuraInspectorUtility
	{
		#region Layout Scopes
		static GUIStyle grayMiniLabel;
		public static GUIStyle GrayMiniLabel {
			get {
				if (grayMiniLabel == null) {
					grayMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
					grayMiniLabel.alignment = TextAnchor.UpperLeft;
				}
				return grayMiniLabel;
			}
		}

		public class LabelWidthScope : System.IDisposable {
			public LabelWidthScope (float minimumLabelWidth = 190f) {
				EditorGUIUtility.labelWidth = minimumLabelWidth;
			}

			public void Dispose () {
				EditorGUIUtility.labelWidth = 0f;
			}
		}

		public class IndentScope : System.IDisposable {
			public IndentScope () { EditorGUI.indentLevel++; }
			public void Dispose () { EditorGUI.indentLevel--; }
		}

		public class BoxScope : System.IDisposable {
			readonly bool indent;

			static GUIStyle boxScopeStyle;
			public static GUIStyle BoxScopeStyle {
				get {
					if (boxScopeStyle == null) {
						boxScopeStyle = new GUIStyle(EditorStyles.helpBox);
						var p = boxScopeStyle.padding;
						p.right += 6;
						p.top += 1;
						p.left += 3;
					}
					return boxScopeStyle;
				}
			}

			static GUIStyle smallBoxScopeStyle;
			public static GUIStyle SmallBoxScopeStyle {
				get {
					if (smallBoxScopeStyle == null) {
						smallBoxScopeStyle = new GUIStyle(EditorStyles.helpBox);
						var p = smallBoxScopeStyle.padding;
						p.right += 2;
						p.top += 0;
						p.left += 1;
					}
					return smallBoxScopeStyle;
				}
			}

			public BoxScope (bool indent = true) {
				this.indent = indent;
				EditorGUILayout.BeginVertical(BoxScopeStyle);
				if (indent) EditorGUI.indentLevel++;
			}

			public void Dispose () {
				if (indent) EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
		}
		#endregion

		#region Button
		const float CenterButtonMaxWidth = 270f;
		const float CenterButtonHeight = 35f;
		static GUIStyle spineButtonStyle;
		static GUIStyle SpineButtonStyle {
			get {
				if (spineButtonStyle == null) {
					spineButtonStyle = new GUIStyle(GUI.skin.button);
					spineButtonStyle.name = "Spine Button";
					spineButtonStyle.padding = new RectOffset(10, 10, 10, 10);
				}
				return spineButtonStyle;
			}
		}

		public static bool LargeCenteredButton (string label, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(label, SpineButtonStyle, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(label, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool LargeCenteredButton (GUIContent content, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, SpineButtonStyle, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(CenterButtonHeight));
			}
		}

		public static bool CenteredButton (GUIContent content, float height = 20f, bool sideSpace = true) {
			if (sideSpace) {
				bool clicked;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space();
					clicked = GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(height));
					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
				return clicked;
			} else {
				return GUILayout.Button(content, GUILayout.MaxWidth(CenterButtonMaxWidth), GUILayout.Height(height));
			}
		}
		#endregion
	}

}
