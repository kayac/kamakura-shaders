using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kayac.VisualArts
{

	[ExecuteInEditMode]
	public class Kamakura2DParams : MonoBehaviour
	{

		public enum VertexColorUsage
		{
			Nothing = 0,
			FilterParams = 1,
			OutlineColor = 2,
			OutlineParams = 3,
		}

		[SerializeField]
		private float m_hue = 0f;
		private float m_oldHue = 0f;

		[SerializeField]
		private float m_saturation = 1f;
		private float m_oldSaturation = 1f;

		[SerializeField]
		private float m_brightness = 1f;
		private float m_oldBrightness = 1f;

		[SerializeField]
		private float m_alpha = 1f;
		private float m_oldAlpha = 1f;

		[SerializeField]
		private Color m_outlineColor = Color.white;
		private Color m_oldOutlineColor = Color.white;

		[SerializeField]
		private float m_outlineThickness = 0.5f;
		private float m_oldOutlineThickness = 0.5f;

		[SerializeField]
		private float m_outlineSoftness = 1f;
		private float m_oldOutlineSoftness = 1f;

		[SerializeField]
		private float m_outlineOffset = 0.0f;
		private float m_oldOutlineOffset = 0.0f;

		private bool m_isDirty = false;

		public float hue
		{
			get { return m_hue; }
			set { m_isDirty = true; m_hue = value; }
		}

		public float saturation
		{
			get { return m_saturation; }
			set { m_isDirty = true; m_saturation = value; }
		}

		public float brightness
		{
			get { return m_brightness; }
			set { m_isDirty = true; m_brightness = value; }
		}

		public float alpha
		{
			get { return m_alpha; }
			set { m_isDirty = true; m_alpha = value; }
		}

		public Color outlineColor
		{
			get { return m_outlineColor; }
			set { m_isDirty = true; m_outlineColor = value; }
		}

		public float outlineThickness
		{
			get { return m_outlineThickness; }
			set { m_isDirty = true; m_outlineThickness = value; }
		}

		public float outlineSoftness
		{
			get { return m_outlineSoftness; }
			set { m_isDirty = true; m_outlineSoftness = value; }
		}

		public float outlineOffset
		{
			get { return m_outlineOffset; }
			set { m_isDirty = true; m_outlineOffset = value; }
		}

		[SerializeField]
		bool _defaultSettings = true;

		public static Color _defaultColor = Color.white;

		private Color _color
		{
			get
			{
				if (graphic != null) { return graphic.color; }
				if (spriteRenderer != null) { return spriteRenderer.color; }
				return _defaultColor;
			}

			set
			{
				if (graphic != null) { graphic.color = value; return; }
				if (spriteRenderer != null) { spriteRenderer.color = value; return; }
			}
		}

		public Material sharedMaterial
		{
			get
			{
				if (graphic != null) { return graphic.material; }
				if (spriteRenderer != null) { return spriteRenderer.sharedMaterial; }
				return null;
			}
		}

		VertexColorUsage _currentMode;
		public VertexColorUsage mode
		{
			get
			{
				if (sharedMaterial == null)
				{
					return VertexColorUsage.Nothing;
				}
				return (VertexColorUsage)(int)sharedMaterial.GetFloat("_VertexColorAs");
			}
		}

		public UnityEngine.UI.Graphic graphic { get; private set; }
		public SpriteRenderer spriteRenderer { get; private set; }

		#if UNITY_EDITOR
		void Start()
		{
			UnityEditor.Undo.undoRedoPerformed -= Refresh;
			UnityEditor.Undo.undoRedoPerformed += Refresh;
		}
		#endif

		void OnEnable()
		{
			graphic = GetComponent<UnityEngine.UI.Graphic>();
			if (graphic == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }

			if (_defaultSettings)
			{
				_defaultSettings = false;
				_color = new Color(0.5f, 0.55f, 0.55f, _color.a);
			}
		}

		public void Refresh()
		{
			var c = _color;
			switch (mode)
			{
				case VertexColorUsage.FilterParams:
					c.r = m_hue + 0.5f;
					c.g = m_saturation * 0.05f + 0.5f;
					c.b = m_brightness * 0.05f + 0.5f;
					c.a = m_alpha;
					break;
				case VertexColorUsage.OutlineColor:
					c.r = m_outlineColor.r;
					c.g = m_outlineColor.g;
					c.b = m_outlineColor.b;
					c.a = m_outlineColor.a;
					break;
				case VertexColorUsage.OutlineParams:
					c.r = m_outlineThickness;
					c.g = m_outlineSoftness;
					c.b = m_outlineOffset;
					c.a = 1f;
					break;
			}
			_color = c;

			m_isDirty = false;
			m_oldAlpha = m_alpha;
			m_oldHue = m_hue;
			m_oldSaturation = m_saturation;
			m_oldBrightness = m_brightness;
			m_oldOutlineColor = m_outlineColor;
			m_oldOutlineThickness = m_outlineThickness;
			m_oldOutlineSoftness = m_outlineSoftness;
			m_oldOutlineOffset = m_outlineOffset;
		#if UNITY_EDITOR
			_currentMode = mode;
		#endif
		}

		void LateUpdate()
		{
			if (m_isDirty
			|| m_alpha != m_oldAlpha
			|| m_hue != m_oldHue
			|| m_saturation != m_oldSaturation
			|| m_brightness != m_oldBrightness
			|| m_outlineColor != m_oldOutlineColor
			|| m_oldOutlineThickness != m_outlineThickness
			|| m_oldOutlineSoftness != m_outlineSoftness
			|| m_oldOutlineOffset != m_outlineOffset
		#if UNITY_EDITOR
			|| _currentMode != mode
		#endif
			)
			{
				Refresh();
			}
		}
	}

}
