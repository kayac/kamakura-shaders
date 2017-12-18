using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Kayac.VisualArts
{

	public class MaterialKamakuraBlendModeDrawer : MaterialPropertyDrawer
	{

		static readonly int kSolidQueue = 2000;
		static readonly int kAlphaTestQueue = 2450;
		// static readonly int kTransparentQueue = 3000;

		public MaterialKamakuraBlendModeDrawer()
		{

		}

		// Draw the property inside the given rect
		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			// Setup

			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			int num = (int)prop.floatValue;
			var mode = (KamakuraBlendMode)EditorGUI.EnumPopup(position, label, (KamakuraBlendMode)num);
			num = (int)mode;

			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = num;
				Apply(prop);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			var mode = (KamakuraBlendMode)(int)prop.floatValue;
			foreach (Material material in prop.targets)
			{
				switch(mode)
				{
					case KamakuraBlendMode.Opaque:
					{
						material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
						material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
						material.SetOverrideTag("RenderType", "Opaque");
						material.renderQueue = kSolidQueue;
					}
					break;
					case KamakuraBlendMode.Additive:
					{
						material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
						material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
						material.SetOverrideTag("RenderType", "TransparentCutout");
						material.renderQueue = kAlphaTestQueue;
					}
					break;
					case KamakuraBlendMode.Transparent:
					{
						material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
						material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
						material.SetOverrideTag("RenderType", "TransparentCutout");
						material.renderQueue = kAlphaTestQueue;
					}
					break;
					// case KamakuraBlendMode.Multiply:
					// {
					// 	material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					// 	material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
					// 	material.SetOverrideTag("RenderType", "TransparentCutout");
					// 	material.renderQueue = kAlphaTestQueue;
					// }
					// break;
				}
			}
		}
	}

}