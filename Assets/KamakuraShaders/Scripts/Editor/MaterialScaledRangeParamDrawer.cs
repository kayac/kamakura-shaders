using UnityEngine;
using UnityEditor;
using System;


namespace Kayac.VisualArts
{

	public class MaterialScaledRangeParamDrawer : MaterialPropertyDrawer
	{
		private double _minRange;
		private double _maxRange;
		public MaterialScaledRangeParamDrawer(float minRange, float maxRange)
		{
			_minRange = minRange;
			_maxRange = maxRange;
		}
		// Draw the property inside the given rect
		public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			// Setup

			var originalVal = prop.floatValue;
			var adjustedVal = (_maxRange - _minRange) / (prop.rangeLimits.y - prop.rangeLimits.x) * originalVal + _minRange;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0f;
			float newVal = EditorGUI.Slider(position, label, (float)adjustedVal, (float)_minRange, (float)_maxRange);
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;

			if (EditorGUI.EndChangeCheck())
			{
				originalVal = (float)((prop.rangeLimits.y - prop.rangeLimits.x) / (_maxRange - _minRange) * newVal + prop.rangeLimits.x);
				prop.floatValue = originalVal;
			}
		}
	}

}