#ifndef __KAMAKURA_SHARED_CGINC
#define __KAMAKURA_SHARED_CGINC

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Kamakura-CGINC-Extensions.cginc"

#ifdef SHADER_API_MOBILE
// Float types
	#define float_t  half
	#define float2_t half2
	#define float3_t half3
	#define float4_t half4
#else
	#define float_t  float
	#define float2_t float2
	#define float3_t float3
	#define float4_t float4
#endif

KAMAKURA_EXT_SharedExtraParams


	uniform sampler2D _MainTex;
	uniform float4_t _MainTex_ST;

	uniform sampler2D _SpecularTex;
	uniform float_t _SpecularPower;
	uniform fixed _SpecularIntensity;
	uniform fixed4 _SpecularColor;
	uniform fixed _SpecularSmoothness;

	uniform fixed _EnableCutout;
	uniform fixed _AlphaCutoutValue;

	#define KAMAKURA_V2F_POS float4 pos : SV_POSITION;

/// <--- MISC METHODS --->

	inline fixed Phong(fixed3 R, fixed3 L)
	{
		//modified to adjust specular sharpness by _SpecularSmoothness parameter
		fixed val = pow(saturate(dot(R, L)), _SpecularPower);
		return smoothstep(-_SpecularSmoothness * 0.5 + 0.5, _SpecularSmoothness * 0.5 + 0.5, val);
	}

	inline fixed3 Specular(float3_t viewDir, float3_t normalDir, float3_t lightDirection, fixed lightAtten, float2_t uv)
	{
		fixed4 specularTexSample = tex2D(_SpecularTex, uv);
		fixed specularHighlight = Phong(reflect(-viewDir, normalDir), lightDirection) * lightAtten;
		return saturate((specularHighlight * specularHighlight + specularHighlight) * (_SpecularColor * specularTexSample)) * _SpecularIntensity;
	}

	inline fixed Lambert(fixed3 N, fixed3 L)
	{
		return 0.5 * dot(N, L) + 0.5;
	}

	inline float3_t GetViewDir(float4 vertexPos)
	{
	#if defined(SHADER_API_METAL) || defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(SHADER_API_D3D9)
 		float4_t nearPlane = float4_t(unity_CameraWorldClipPlanes[4].xyz, 0);
	#else
		float4_t nearPlane = float4_t(-unity_CameraWorldClipPlanes[4].xyz, 0);
	#endif

		UNITY_BRANCH
		if (unity_OrthoParams.w > 0.0)
		{
			return mul(unity_WorldToObject, nearPlane).xyz;
		}
		else
		{
			return ObjSpaceViewDir(vertexPos);
		}
	}

/// <--- LIGHTRAMP SAMPLING MODULE --->

	uniform sampler2D _LightRampTex;
	uniform fixed _LightRampOffset;
	uniform fixed _LightRampPresetsOffset;
	uniform fixed _LightRampUseGVertexColor;

	inline fixed4 LightRampTexSample(float_t lighting, fixed vertexColorGreen)
	{
		fixed adjuster = _LightRampUseGVertexColor * (2 * vertexColorGreen - 1);
		fixed2 lightUV = fixed2(lighting + _LightRampOffset + adjuster, _LightRampPresetsOffset);
		lightUV = clamp(lightUV, 0.01, 0.99);

		return fixed4(tex2D(_LightRampTex, lightUV).xyz, adjuster);
	}

/// <--- FILTER MODULE --->
	inline float3_t RgbToHsv(fixed3 c)
	{
		float4_t K = float4_t(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4_t p = lerp(float4_t(c.bg, K.wz), float4_t(c.gb, K.xy), step(c.b, c.g));
		float4_t q = lerp(float4_t(p.xyw, c.r), float4_t(c.r, p.yzx), step(p.x, c.r));

		float_t d = q.x - min(q.w, q.y);
		float_t e = 1.0e-10;
		return float3_t(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	inline fixed3 HsvToRgb(float3_t c)
	{
		float4_t K = float4_t(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float3_t p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
		return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	}

	uniform float_t _FilterHue;
	uniform float_t _FilterSaturation;
	uniform float_t _FilterBrightness;
	uniform float_t _FilterContrast;
	uniform float_t _FilterContrastMidPoint;
	uniform fixed _EnableFilter;

	fixed3 AdjustColor(fixed3 col)
	{
		UNITY_BRANCH
		if (_EnableFilter > 0.5)
		{
			float3_t hsv = RgbToHsv(col);
			hsv.x += _FilterHue;
			hsv.y *= _FilterSaturation;
			hsv.z *= _FilterBrightness;
			float3_t rgb = HsvToRgb(hsv);
			rgb = (rgb - _FilterContrastMidPoint) * _FilterContrast + _FilterContrastMidPoint;
			return saturate(rgb);
		}
		else
		{
			return col;
		}
	}

/// <--- SHADOW MOD MODULE --->
#ifdef KAMAKURA_SHADOWMOD_ON
	uniform sampler2D _ShadowModTex;
	uniform half4 _ShadowModTex_ST;
	uniform fixed4 _ShadowModColor;
	uniform fixed _ShadowModSpecularUseDiffuseTexture;
	uniform fixed _ShadowModRimUseDiffuseTexture;
	uniform fixed _ShadowModIntensity;
	uniform fixed _ShadowModUseFilter;
	uniform fixed _ShadowModBlendMode;

	inline fixed3 ShadowMod(float2_t uv, fixed3 texSample, fixed3 lightRampTexSample, fixed3 lightColor, fixed3 localLightRampTexSample, fixed3 localLightColor, fixed3 ambient)
	{
		fixed3 shadowModTexSample = tex2D(_ShadowModTex, uv) * _ShadowModColor;
		UNITY_BRANCH
		if (_ShadowModUseFilter > 0.5)
		{
			shadowModTexSample = AdjustColor(shadowModTexSample);
		}

		fixed3 diffuse = (_ShadowModIntensity * lightRampTexSample - (_ShadowModIntensity - 1)) * lightColor + ambient;
	#ifdef KAMAKURA_LOCALLIGHT_ON
		diffuse += (_ShadowModIntensity * localLightRampTexSample - (_ShadowModIntensity - 1)) * localLightColor;
	#endif
		return lerp((1 - _ShadowModBlendMode) * shadowModTexSample + _ShadowModBlendMode * shadowModTexSample * texSample, texSample, diffuse);
	}
#endif // KAMAKURA_SHADOWMOD_ON

/// <--- HATCH MODULE --->
#ifdef KAMAKURA_HATCH_ON
	uniform sampler2D _HatchMask;
	uniform fixed _HatchScreenSpace;
	uniform sampler2D _HatchTex;
	uniform half4 _HatchTex_ST;
	uniform fixed _HatchIntensity;
	uniform fixed _HatchOffset;
	uniform fixed4 _Hatch1Color;
	uniform fixed4 _Hatch2Color;
	uniform fixed4 _Hatch3Color;
	uniform fixed _HatchRangeStretch;
	uniform fixed _HatchRotSin;
	uniform fixed _HatchRotCos;
	uniform fixed _HatchBlendMode;


	inline fixed3 HatchDiffuse(float4_t pos, float2_t uv, float_t nDotV, float intensity, fixed3 diffuse)
	{
	#ifdef SHADER_API_D3D9
		float2_t hatchUVsrc = uv - 0.5;
	#else
		float2_t hatchUVsrc = _HatchScreenSpace * (pos.xy / _ScreenParams.xy - 0.5) + (1 - _HatchScreenSpace) * (uv - 0.5);
	#endif
		float2_t hatchCosSin = float2_t(_HatchRotCos, -_HatchRotSin);
		float2_t hatchSinCos = float2_t(_HatchRotSin, _HatchRotCos);

		hatchUVsrc = float2_t(dot(hatchUVsrc, hatchCosSin), dot(hatchUVsrc, hatchSinCos)) + 0.5;

		float2_t hatchUVs = TRANSFORM_TEX(hatchUVsrc, _HatchTex);
		fixed4 hatchSample = tex2D(_HatchTex, hatchUVs);

		fixed hatchLevel = saturate(nDotV * (1 / _HatchRangeStretch) + 0.33 + _HatchOffset) * 4;
		fixed4 hatchWeight1 = fixed4(0.0, 0.0, 0.0, 0.0);

		if (hatchLevel > 3.0)
		{
			hatchWeight1.x = 1.0;
		}
		else if (hatchLevel > 2.0)
		{
			hatchWeight1.x = hatchLevel - 2.0;
			hatchWeight1.y = 1.0 - hatchWeight1.x;
		}
		else if (hatchLevel > 1.0)
		{
			hatchWeight1.y = hatchLevel - 1.0;
			hatchWeight1.z = 1.0 - hatchWeight1.y;
		}
		else
		{
			hatchWeight1.z = hatchLevel;
			hatchWeight1.w = 1.0 - hatchWeight1.z;
		}

		fixed hatching = hatchWeight1.x;
		hatching = hatchSample.x * hatchWeight1.y + hatching;
		hatching = hatchSample.y * hatchWeight1.z + hatching;
		hatching = hatchSample.z * hatchWeight1.w + hatching;

		float3_t hatchColor = float3_t((hatchWeight1.x * _Hatch1Color.a) * _Hatch1Color.rgb);
		hatchColor = ((hatchWeight1.y * _Hatch1Color.a) * _Hatch1Color.rgb) + hatchColor;
		hatchColor = ((hatchWeight1.z * _Hatch2Color.a) * _Hatch2Color.rgb) + hatchColor;
		hatchColor = ((hatchWeight1.w * _Hatch3Color.a) * _Hatch3Color.rgb) + hatchColor;
		fixed4 hatchMaskSample = tex2D(_HatchMask, uv);

		return lerp(diffuse, (1 - _HatchBlendMode) * hatchColor + _HatchBlendMode * hatchColor * diffuse, intensity * -hatchMaskSample.r * (_HatchIntensity * hatching - _HatchIntensity));
	}
#endif

/// <--- CUBE COLOR MODULE --->
	uniform fixed _EnableCubeColor;

	uniform fixed3 _CubeColor0;
	uniform fixed3 _CubeColor1;
	uniform fixed3 _CubeColor2;
	uniform fixed3 _CubeColor3;
	uniform fixed3 _CubeColor4;
	uniform fixed3 _CubeColor5;

	uniform fixed _CubeColorUseLocalSpace;

	uniform float3_t _CubeColorLocalSpaceMatrixRow0;
	uniform float3_t _CubeColorLocalSpaceMatrixRow1;
	uniform float3_t _CubeColorLocalSpaceMatrixRow2;
	uniform float3_t _CubeColorLocalSpaceMatrixRow3;

	fixed3 GetCubeColor(float3_t worldNormal)
	{
		UNITY_BRANCH
		if (_EnableCubeColor > 0.5)
		{
			UNITY_BRANCH
			if (_CubeColorUseLocalSpace > 0.5)
			{
				float3x3 localSpaceMatrix = float3x3(_CubeColorLocalSpaceMatrixRow0, _CubeColorLocalSpaceMatrixRow1, _CubeColorLocalSpaceMatrixRow2);
				worldNormal = normalize(mul(worldNormal, localSpaceMatrix));
			}
			float3_t normalSqr = normalize(worldNormal * worldNormal);
			fixed3 color = normalSqr.x * ((worldNormal.x >= 0) ? _CubeColor1 : _CubeColor3)
				+ normalSqr.y * ((worldNormal.y >= 0) ? _CubeColor0 : _CubeColor5)
				+ normalSqr.z * ((worldNormal.z >= 0) ? _CubeColor2 : _CubeColor4);
			return saturate(color);
		}
		else
		{
			return 0;
		}
	}

/// <--- AMBIENT MODULE --->
	uniform fixed3 _AmbientColor;
	uniform fixed _AmbientUseCubeColor;
	uniform fixed _AmbientIntensity;
	uniform fixed _AmbientUnitySHIntensity;

	fixed3 GetAmbient(fixed3 cubeColor, fixed3 shAmbient)
	{
		fixed3 val;
		val = (_AmbientUseCubeColor * cubeColor + (1 - _AmbientUseCubeColor) * _AmbientColor) * _AmbientIntensity;
		val = val * _EnableCubeColor + (1 - _EnableCubeColor) * _AmbientColor * _AmbientIntensity;
		val += shAmbient * _AmbientUnitySHIntensity;
		return val;
	}


/// <--- RIM MODULE --->
#ifdef KAMAKURA_RIM_ON
	uniform fixed3 _RimColor;
	uniform fixed _RimSoftness;
	uniform fixed _RimIntensity;
	uniform float_t _RimSize;
	uniform fixed _RimUseCubeColor;
	uniform sampler2D _RimNoiseTex;
	uniform float4_t _RimNoiseTex_ST;
	uniform fixed _RimBlendingMode;
	uniform fixed _RimUnitySHIntensity;

	inline fixed3 ApplyRim(fixed3 outColor, float2_t uv, float_t nDotV, fixed3 sampledCubeColor, fixed3 shAmbient)
	{
		fixed inverseRimSize = 1 - _RimSize;
		float2_t rimUVs = TRANSFORM_TEX(uv, _RimNoiseTex);
		_RimSoftness = _RimSoftness * tex2D(_RimNoiseTex, rimUVs).r;
		fixed rimAmount = smoothstep(inverseRimSize - _RimSoftness, inverseRimSize + _RimSoftness, 0.75 - nDotV) * _RimIntensity;

		fixed3 rimColor = _RimColor;
		rimColor = _RimUseCubeColor * sampledCubeColor + (1 - _RimUseCubeColor) * _RimColor;
		rimColor = _EnableCubeColor * rimColor + (1 - _EnableCubeColor) * _RimColor;
		rimColor += _RimUnitySHIntensity * shAmbient;

		return _RimBlendingMode * lerp(outColor, rimColor, rimAmount) + (1 - _RimBlendingMode) * (rimAmount * rimColor + outColor);
	}
#endif // KAMAKURA_RIM_ON

/// <--- EMISSION MODULE --->
#ifdef KAMAKURA_EMISSION_ON
	uniform sampler2D _EmissionTex;
	uniform fixed4 _EmissionColor;
	uniform float_t _EmissionPower;

	inline fixed4 ApplyEmission(fixed4 outColor, float2_t uv)
	{
		fixed4 emissionTexSample = tex2D(_EmissionTex, uv);
		return outColor + emissionTexSample * _EmissionColor * _EmissionPower;
	}
#endif // KAMAKURA_EMISSION_ON

#ifdef KAMAKURA_NORMALMAP_ON
	uniform sampler2D _NormalTex;
	uniform fixed _NormalIntensity;

	inline float3_t ApplyNormalMap(float3_t normalDir, float4_t tangentDir, float2_t uv)
	{
		float3_t binormalDir = normalize(cross (normalDir, tangentDir.xyz) * tangentDir.w);
		half3x3 normalBase = half3x3(tangentDir.xyz, binormalDir, normalDir);
		float4_t normal = tex2D(_NormalTex, uv);
		float3_t normalSample = UnpackNormal(normal);
		normalSample.xy *= _NormalIntensity;
		return normalize(mul(normalSample, normalBase));
	}
#endif

#endif // __KAMAKURA_SHARED_CGINC