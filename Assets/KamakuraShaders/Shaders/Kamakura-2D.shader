Shader "Kayac/Kamakura2D"
{
	Properties
	{
		[Header(About)]
		[KamakuraShaderVersion] _ShaderVersion ("_ShaderVersion", Vector) = (1, 0, 3, -1)

		[Header(Basic)]
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		_BaseColor ("Base Color", Color) = (1, 1, 1, 1)

		[Header(Advanced)]
		[KeywordEnum(Nothing, FilterParameters, OutlineColor, OutlineParameters)]  _VertexColorAs ("Use Vertex Color As", Float) = 0

		[Header(Outline)]
		[Toggle] _EnableOutline("Enable", Float) = 0.0
		[FormerlySerializedAs(_SDFTex)][NoScaleOffset] _OutlineSDFTex ("SDF Texture", 2D) = "white" {}
		_OutlineColor ("Color", Color) = (1, 1, 1, 1)
		_OutlineThickness ("Thickness", Range(0, 0.5)) = 0.5
		_OutlineSoftness ("Softness", Range(0.01, 1)) = 1
		_OutlineOffset ("Offset", Range(0, 0.95)) = 0
		_OutlineTextureEdgeSmoothness ("Texture Edge Smoothness", Range(0, 0.05)) = 0


		[Header(Filter)]
		[Toggle] _EnableFilter ("Enable", Float) = 0.0
		_FilterHue("Hue", Range(-0.5,0.5)) = 0.0
		_FilterSaturation("Saturation", Float) = 1.0
		_FilterBrightness("Brightness", Float) = 1.0
		_FilterColorModifier ("Color Modifier", Color) = (0,0,0,0)

	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"RenderType"="Sprite"
			"AlphaDepth"="False"
			"CanUseSpriteAtlas"="True"
			"IgnoreProjector"="True"
		}
		LOD 100

		Pass
		{
			Lighting Off
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ _ENABLEFILTER_ON
			#pragma multi_compile _ _ENABLEOUTLINE_ON
			#pragma multi_compile _VERTEXCOLORAS_NOTHING _VERTEXCOLORAS_FILTERPARAMETERS _VERTEXCOLORAS_OUTLINECOLOR _VERTEXCOLORAS_OUTLINEPARAMETERS
			#include "UnityCG.cginc"

		#if defined(_VERTEXCOLORAS_FILTERPARAMETERS) || defined(_VERTEXCOLORAS_OUTLINECOLOR) || defined(_VERTEXCOLORAS_OUTLINEPARAMETERS)
			#define _VERTEXCOLOR_USED_AS_PARAM
		#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1; // xyz = hue saturation brigtnesss
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _BaseColor;

			uniform fixed _FilterHue;
			uniform fixed _FilterSaturation;
			uniform fixed _FilterBrightness;
			uniform fixed4 _FilterColorModifier;

		#ifdef _ENABLEOUTLINE_ON
			uniform sampler2D _OutlineSDFTex;
			uniform fixed4 _OutlineColor;
			uniform half _OutlineThickness;
			uniform half _OutlineSoftness;
			uniform half _OutlineOffset;
			uniform half _OutlineTextureEdgeSmoothness;
		#endif

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			#if defined(_ENABLEFILTER_ON) && defined(_VERTEXCOLORAS_FILTERPARAMETERS)
				o.color.x = v.color.r - 0.5;
				o.color.yz = 20 * v.color.gb - 10;
				o.color.a = v.color.a;
			#else
				o.color = v.color;
			#endif
				return o;
			}

			inline half3 rgbToHsv(fixed3 c)
			{
				half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
				half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

				half d = q.x - min(q.w, q.y);
				half e = 1.0e-10;
				return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			inline fixed3 hsvToRgb(half3 c)
			{
				half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			inline fixed4 adjustColor(fixed4 col, float4 params)
			{
				half3 hsv = rgbToHsv(col);
			#ifdef _VERTEXCOLORAS_FILTERPARAMETERS
				hsv.x += params.x;
				hsv.y *= params.y;
				hsv.z *= params.z;
				return fixed4(hsvToRgb(hsv), col.a * params.a);
			#elif defined(_VERTEXCOLOR_USED_AS_PARAM)
				hsv.x += _FilterHue;
				hsv.y *= _FilterSaturation;
				hsv.z *= _FilterBrightness;
				return fixed4(hsvToRgb(hsv), col.a * params.a);
			#else
				hsv.x += _FilterHue;
				hsv.y *= _FilterSaturation;
				hsv.z *= _FilterBrightness;
				return fixed4(hsvToRgb(hsv) * params.rgb, col.a * params.a);
			#endif
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			#ifdef _ENABLEFILTER_ON
				col = adjustColor(col, i.color);
				col.rgb = lerp(col.rgb, _FilterColorModifier.rgb, _FilterColorModifier.a);
			#endif

		#ifdef _ENABLEOUTLINE_ON
				fixed4 c = 0;
				fixed4 sdf = tex2D(_OutlineSDFTex, i.uv);

			#ifdef _VERTEXCOLORAS_OUTLINECOLOR
				c.rgb = i.color.rgb;
			#else
				c.rgb = _OutlineColor;
			#endif

			#ifdef _VERTEXCOLORAS_OUTLINEPARAMETERS
				half thickness = i.color.r;
				half softness = i.color.g;
				half offset = i.color.b;
			#else
				half thickness = _OutlineThickness;
				half softness = _OutlineSoftness;
				half offset = _OutlineOffset;
			#endif
				fixed dist = sdf.a;
				half outlineOffset = offset * 0.5 + 0.5;
				half outlineSize = outlineOffset - thickness;
				half diffInv = 1 / (outlineOffset - outlineSize);
				fixed outlineAlpha = saturate((dist - outlineSize) * diffInv);
				c.a = smoothstep(0, softness, outlineAlpha) * _OutlineColor.a * i.color.a;

			#if defined(_VERTEXCOLORAS_FILTERPARAMETERS) || defined(_VERTEXCOLORAS_OUTLINECOLOR) || defined(_VERTEXCOLORAS_OUTLINEPARAMETERS)
				return lerp(c, col, smoothstep(outlineOffset - _OutlineTextureEdgeSmoothness, outlineOffset + _OutlineTextureEdgeSmoothness, dist)) * _BaseColor;
			#else // _ENABLEOUTLINE_ON && !_VERTEXCOLORAS_FILTERPARAMETERS && !_VERTEXCOLORAS_OUTLINECOLOR && !_VERTEXCOLORAS_OUTLINEPARAMETERS
				return lerp(c, col * i.color, smoothstep(outlineOffset - _OutlineTextureEdgeSmoothness, outlineOffset + _OutlineTextureEdgeSmoothness, dist)) * _BaseColor;
			#endif // _ENABLEOUTLINE_ON && _VERTEXCOLORAS_FILTERPARAMETERS
		#else
				return col * _BaseColor;
		#endif // !_ENABLEOUTLINE_ON
			}
			ENDCG
		}
	}
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"

}
