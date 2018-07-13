using UnityEditor;
using UnityEngine;


namespace Kayac.VisualArts
{

	public class KamakuraInspectorUtility
	{
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
					smallBoxScopeStyle = new GUIStyle(EditorStyles.textField);
					var p = new RectOffset();
					p.right = 0;
					p.top = 0;
					p.left = 0;
					p.bottom = 0;
					smallBoxScopeStyle.margin = p;
					smallBoxScopeStyle.padding = p;
				}
				return smallBoxScopeStyle;
			}
		}
	}

}
