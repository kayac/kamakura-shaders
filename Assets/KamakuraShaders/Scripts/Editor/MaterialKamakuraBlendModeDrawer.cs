using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace Kayac.VisualArts
{

	public class MaterialKamakuraBlendModeDrawer : MaterialPropertyDrawer
	{

		// static readonly int kTransparentQueue = 3000;

		const string kAutoAdjustRenderQueueKey = "_AutoAdjustRenderQueue";
		const float kDefaultEnumMask = (float)(KamakuraBlendMode.Opaque | KamakuraBlendMode.Transparent | KamakuraBlendMode.Additive);
		const float kAdjustRenderQueue = 1.0f;

		string srcBlendKey;
		string dstBlendKey;
		int enumMask;

		int[] enumValues;
		string[] labels;

		KamakuraBlendMode defaultBlendMode;

		bool adjustRenderQueue;


		public MaterialKamakuraBlendModeDrawer() : this(
			MaterialBlendModeUtils.DefaultSrcBlendKey,
			MaterialBlendModeUtils.DefaultDstBlendKey,
			(float)KamakuraBlendMode.Opaque,
			kDefaultEnumMask,
			kAdjustRenderQueue)
		{
		}

		public MaterialKamakuraBlendModeDrawer(string srcBlendKey, string dstBlendKey, float defaultBlendMode, float enumMask, float adjustRenderQueue)
		{
			this.srcBlendKey = srcBlendKey;
			this.dstBlendKey = dstBlendKey;
			this.enumMask = (int)enumMask;
			this.defaultBlendMode = (KamakuraBlendMode)(int)defaultBlendMode;
			this.enumValues = System.Enum.GetValues(typeof(KamakuraBlendMode)).Cast<int>().Where(t => (this.enumMask & t) != 0).ToArray();
			var labels = from num in enumValues select ((KamakuraBlendMode)num).ToString();
			this.labels = labels.ToArray();
			this.adjustRenderQueue = adjustRenderQueue != 0f;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return base.GetPropertyHeight(prop, label, editor);
		}

		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			int num = (int)prop.floatValue;
			var valueChanged = false;
			if ((KamakuraBlendMode)num == KamakuraBlendMode.Default)
			{
				num = (int)defaultBlendMode;
				valueChanged = true;
			}
			num = EditorGUI.IntPopup(position, label, num, labels, enumValues);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck() || valueChanged)
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
				var adjustRenderQueue = this.adjustRenderQueue && (material.GetFloat(kAutoAdjustRenderQueueKey) != 0f);
				MaterialBlendModeUtils.SetMaterialBlendMode(material, mode, adjustRenderQueue, srcBlendKey, dstBlendKey);
			}
		}
	}

	public static class MaterialBlendModeUtils
	{
		public const string DefaultSrcBlendKey = "_SrcBlend";
		public const string DefaultDstBlendKey = "_DstBlend";

		static readonly int kSolidQueue = 2000;
		static readonly int kAlphaTestQueue = 2450;

		public static void SetMaterialBlendMode(Material material, KamakuraBlendMode mode, bool adjustRenderQueue, string srcBlendKey = DefaultSrcBlendKey, string dstBlendKey = DefaultDstBlendKey)
		{
			switch (mode)
			{
				case KamakuraBlendMode.Default:
				case KamakuraBlendMode.Opaque:
				{
					material.SetInt(srcBlendKey, (int)UnityEngine.Rendering.BlendMode.One);
					material.SetInt(dstBlendKey, (int)UnityEngine.Rendering.BlendMode.Zero);
					if (adjustRenderQueue)
					{
						material.SetOverrideTag("RenderType", "Opaque");
						material.renderQueue = kSolidQueue;
					}
				}
				break;
				case KamakuraBlendMode.Additive:
				{
					material.SetInt(srcBlendKey, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					material.SetInt(dstBlendKey, (int)UnityEngine.Rendering.BlendMode.One);
					if (adjustRenderQueue)
					{
						material.SetOverrideTag("RenderType", "TransparentCutout");
						material.renderQueue = kAlphaTestQueue;
					}
				}
				break;
				case KamakuraBlendMode.Transparent:
				{
					material.SetInt(srcBlendKey, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					material.SetInt(dstBlendKey, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					if (adjustRenderQueue)
					{
						material.SetOverrideTag("RenderType", "TransparentCutout");
						material.renderQueue = kAlphaTestQueue;
					}
				}
				break;
				case KamakuraBlendMode.Multiply:
				{
					material.SetInt(srcBlendKey, (int)UnityEngine.Rendering.BlendMode.Zero);
					material.SetInt(dstBlendKey, (int)UnityEngine.Rendering.BlendMode.SrcColor);
					if (adjustRenderQueue)
					{
						material.SetOverrideTag("RenderType", "TransparentCutout");
						material.renderQueue = kAlphaTestQueue;
					}
				}
				break;
			}
		}
	}
}