#ifndef __KAMAKURA_HAIR_CGINC
#define __KAMAKURA_HAIR_CGINC

#include "Kamakura-CGINC-Shared.cginc"

	uniform fixed4 _DiffuseColor;

	uniform fixed3 _LightColor0;

	uniform sampler2D _SpecularMap;
	uniform half4 _SpecularMap_ST;
	uniform half _SpecularPower1;
	uniform half _SpecularPower2;
	uniform fixed4 _SpecularColor1;
	uniform fixed4 _SpecularColor2;
	uniform fixed _SpecularShiftIntensity;
	uniform fixed _StrandTexIntensity;

#ifdef KAMAKURA_LOCALLIGHT_ON
	uniform fixed3 _LocalLightVec;
	uniform fixed _LocalLightIntensity;
	uniform fixed _LocalLightVecAsMain;
#endif
	uniform fixed3 _LocalLightColor;
	uniform fixed _GlobalLightIntensity;

	uniform fixed _UseCubeColorForSpecular;

	uniform sampler2D _HairSpecShift;
	uniform half4 _HairSpecShift_ST;
	uniform sampler2D _HairStrandTex;
	uniform half4 _HairStrandTex_ST;


	uniform fixed _PrimarySpecularIntensity;
	uniform fixed _PrimarySpecularSmoothness;
	uniform fixed _PrimarySpecularShadowAffection;
	uniform half _PrimaryShift;

	uniform fixed _SecondarySpecularIntensity;
	uniform fixed _SecondarySpecularSmoothness;
	uniform fixed _SecondarySpecularShadowAffection;
	uniform half _SecondaryShift;

	uniform fixed _BinormalRotSin;
	uniform fixed _BinormalRotCos;

	uniform fixed _UsingRightMirroredMesh;

	struct appdata_hair {
		float4 vertex 		: POSITION;
		fixed2 texcoord 	: TEXCOORD0;
		fixed2 texcoord2 	: TEXCOORD1;
		fixed4 normal 		: NORMAL;
		fixed4 tangent 		: TANGENT;
		fixed4 color 		: COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f_hair
	{
		KAMAKURA_V2F_POS
		fixed4 color 					:	COLOR0;
		float2_t uv 					:	TEXCOORD0;
		float3_t normal 				:	TEXCOORD1;
		fixed3 viewDir 					:	TEXCOORD2;
		float3_t lightDir 				:	TEXCOORD3;
		float3_t binormal 				:	TEXCOORD4;
		fixed3 ambient 					:	TEXCOORD5;
		LIGHTING_COORDS(6, 7)
		fixed3 sampledCubeColor 		:	TEXCOORD8;
	#ifdef KAMAKURA_LOCALLIGHT_ON
		float3_t localLightDir 			:	TEXCOORD9;
	#endif
	};

	struct v2f_hair_fwd_add
	{
		KAMAKURA_V2F_POS
		fixed4 color 					:	COLOR0;
		float2_t uv 					:	TEXCOORD0;
		float3_t normal 				:	TEXCOORD1;
		fixed3 viewDir 					:	TEXCOORD2;
		float3_t lightDir 				:	TEXCOORD3;
		float3_t binormal 				:	TEXCOORD4;
		LIGHTING_COORDS(5, 6)
		fixed3 sampledCubeColor 		:	TEXCOORD7;
	#ifdef KAMAKURA_LOCALLIGHT_ON
		float3_t localLightDir 			:	TEXCOORD8;
	#endif
	};

	inline float3_t ShiftBinormal(float3_t binormal, float3_t normal, float_t shift)
	{
		float3_t shiftedBinormal = binormal + shift * normal;
		return normalize(shiftedBinormal);
	}

	inline fixed StrandSpecular(fixed3 binormal, fixed3 view, fixed3 light, float_t exponent)
	{
		float3_t halfDir = normalize(light + view);
		float_t dotBH = dot(binormal, halfDir);
		float_t sinBH = sqrt(-dotBH * dotBH + 1.0);
		float_t dirAtten = smoothstep(-1.0, 0.0, dotBH);
		return dirAtten * pow (sinBH, exponent);
	}

	v2f_hair VertexShaderForwardBase(appdata_hair v)
	{
		v2f_hair OUT;
		UNITY_SETUP_INSTANCE_ID(v);
		float4x4 WorldInverseTranspose = unity_WorldToObject;
		float4x4 World = unity_ObjectToWorld;

		float4_t vertex = v.vertex;
		float3_t normal = v.normal;

	#ifdef KAMAKURA_VERTEXANIM_ON
		float2_t uvSample = GetSamplingUV(v.texcoord2);
		vertex += GetVertexAnimationDisplacement(uvSample);
		normal = GetVertexAnimationNormal(uvSample);
	#endif

		OUT.pos = UnityObjectToClipPos(vertex);
		OUT.uv = v.texcoord;
		OUT.normal = normal;
		OUT.color = v.color;
		OUT.lightDir = normalize(ObjSpaceLightDir(vertex));
		float3_t worldNormal = UnityObjectToWorldNormal(normal);
		OUT.ambient = ShadeSH9(float4(worldNormal, 1.0)) * _AmbientUnitySHIntensity;
	#ifdef KAMAKURA_LOCALLIGHT_ON
		OUT.localLightDir = normalize(mul((float3x3)unity_WorldToObject, _LocalLightVec).xyz);
	#endif
		OUT.sampledCubeColor = GetCubeColor(normal);
		OUT.viewDir = normalize(GetViewDir(vertex));
		OUT.binormal = -cross(v.tangent, normal) * v.tangent.w;
		OUT.binormal = _UsingRightMirroredMesh > 0.5 ? -OUT.binormal : OUT.binormal;
		TRANSFER_VERTEX_TO_FRAGMENT(OUT)
		KAMAKURA_EXT_HairVertexFwdBase(v, OUT)
		return OUT;
	}

	v2f_hair_fwd_add VertexShaderForwardAdd(appdata_hair v)
	{
		v2f_hair_fwd_add OUT;
		UNITY_SETUP_INSTANCE_ID(v);
		float4x4 WorldInverseTranspose = unity_WorldToObject;
		float4x4 World = unity_ObjectToWorld;

		float4_t vertex = v.vertex;
		float3_t normal = v.normal;

	#ifdef KAMAKURA_VERTEXANIM_ON
		float2_t uvSample = GetSamplingUV(v.texcoord2);
		vertex += GetVertexAnimationDisplacement(uvSample);
		normal = GetVertexAnimationNormal(uvSample);
	#endif

		OUT.pos = UnityObjectToClipPos(vertex);
		OUT.uv = v.texcoord;
		OUT.normal = normal;
		OUT.color = v.color;
		OUT.lightDir = normalize(ObjSpaceLightDir(vertex));
	#ifdef KAMAKURA_LOCALLIGHT_ON
		OUT.localLightDir = normalize(mul((float3x3)unity_WorldToObject, _LocalLightVec).xyz);
	#endif
		OUT.sampledCubeColor = GetCubeColor(normal);
		OUT.viewDir = normalize(GetViewDir(vertex));
		OUT.binormal = -cross(v.tangent, normal) * v.tangent.w;
		OUT.binormal = _UsingRightMirroredMesh > 0.5 ? -OUT.binormal : OUT.binormal;
		TRANSFER_VERTEX_TO_FRAGMENT(OUT)
		KAMAKURA_EXT_HairVertexFwdAdd(v, OUT)
		return OUT;
	}

	fixed4 PixelShaderForwardBase(v2f_hair IN): SV_TARGET
	{
		fixed3 binormalDir = IN.binormal;
		fixed3 normalDir = IN.normal;
		fixed3 viewDir = IN.viewDir;

		binormalDir = binormalDir * _BinormalRotCos + (dot(binormalDir, normalDir) * normalDir * (1 - _BinormalRotCos)) + (cross(normalDir, binormalDir) * _BinormalRotSin);

		fixed3 sampledCubeColor = IN.sampledCubeColor;
		fixed3 ambient = GetAmbient(sampledCubeColor, IN.ambient);

		float_t lightAtten = LIGHT_ATTENUATION(IN);

		fixed3 lightDirection = normalize(IN.lightDir);
		float_t adjuster = _LightRampUseGVertexColor * (2 * IN.color.g - 1) ;
		float_t lighting = Lambert(normalDir, lightDirection);
		fixed2 lightUV = fixed2(lighting + _LightRampOffset + adjuster, _LightRampPresetsOffset);
		fixed3 lightRampTexSample = tex2D(_LightRampTex, lightUV).xyz;
		fixed3 diffuse = lightAtten * _GlobalLightIntensity * (_LightColor0.xyz * lightRampTexSample) + ambient;

	#ifdef KAMAKURA_LOCALLIGHT_ON
		fixed3 localLightDirection = normalize(IN.localLightDir);
		float_t localLighting = Lambert(normalDir, localLightDirection);
		fixed2 localLightUV = fixed2(localLighting + _LightRampOffset + adjuster, _LightRampPresetsOffset);
		fixed3 localLightRampTexSample = tex2D(_LightRampTex, localLightUV).xyz;
		diffuse = localLightRampTexSample * _LocalLightColor.xyz + diffuse;
		lightDirection = _LocalLightVecAsMain > 0.5 ? localLightDirection : lightDirection;
		fixed3 localLightDiffuse = _LocalLightIntensity * localLightRampTexSample;
		fixed unmappedLocalLightDiffuse = _LocalLightIntensity * localLighting;
	#else
		fixed3 localLightDiffuse = fixed3(0, 0, 0);
		fixed unmappedLocalLightDiffuse = 0;
	#endif

		fixed4 outColor;
		float2_t diffuseUV = TRANSFORM_TEX(IN.uv, _MainTex);
		fixed4 texSample = tex2D(_MainTex, diffuseUV);

		texSample.rgb = AdjustColor(texSample.rgb);

	#ifdef KAMAKURA_SHADOWMOD_ON
		diffuse = ShadowMod(IN.uv, texSample * _DiffuseColor, _GlobalLightIntensity * lightRampTexSample, _LightColor0.xyz, localLightDiffuse, _LocalLightColor.xyz, ambient);
	#else
		diffuse *= texSample.xyz * _DiffuseColor;
	#endif

	#ifdef KAMAKURA_HATCH_ON
		float_t hatchDiffuseParam = _GlobalLightIntensity * lighting + unmappedLocalLightDiffuse;
		diffuse = HatchDiffuse(IN.pos, IN.uv, hatchDiffuseParam * 2 - 1, 1, diffuse);
	#endif


		float2_t hairSpecUV = TRANSFORM_TEX(IN.uv, _HairSpecShift);
		fixed hairSpecShift = (tex2D(_HairSpecShift, hairSpecUV).r - 0.5) * _SpecularShiftIntensity;

		float3_t t1 = ShiftBinormal(binormalDir, normalDir, _PrimaryShift + hairSpecShift);
		float3_t t2 = ShiftBinormal(binormalDir, normalDir, _SecondaryShift + hairSpecShift);

		float2_t specMaskUV = TRANSFORM_TEX(IN.uv, _SpecularMap);
		fixed specMaskSample = tex2D(_SpecularMap, specMaskUV).r;

		fixed primarySpecular = StrandSpecular(t1, viewDir, lightDirection, _SpecularPower1);
		primarySpecular = smoothstep(- 0.5 * _PrimarySpecularSmoothness + 0.5, _PrimarySpecularSmoothness * 0.5 + 0.5, primarySpecular);

		fixed3 specular = (primarySpecular * specMaskSample) * _SpecularColor1;
		fixed totalLightRamp = (_GlobalLightIntensity * lightRampTexSample + localLightDiffuse);

		fixed primarySpecularShadowAffection = lerp(1, totalLightRamp, _PrimarySpecularShadowAffection);
		specular = specular * primarySpecularShadowAffection * _PrimarySpecularIntensity;

		fixed secondarySpecular = StrandSpecular(t2, viewDir, lightDirection, _SpecularPower2);

		secondarySpecular = smoothstep(-0.5 * _SecondarySpecularSmoothness + 0.5, _SecondarySpecularSmoothness * 0.5 + 0.5, secondarySpecular);

		float2_t hairStrandUV = TRANSFORM_TEX(IN.uv, _HairStrandTex);
		fixed hairStrandSample = tex2D(_HairStrandTex, hairStrandUV).r * _StrandTexIntensity;

		fixed secondarySpecularShadowAffection = lerp(1, totalLightRamp, _SecondarySpecularShadowAffection);
		specular += hairStrandSample * (secondarySpecular * specMaskSample) * (secondarySpecularShadowAffection * _SecondarySpecularIntensity) * _SpecularColor2;

		diffuse += specular;
		outColor = fixed4(diffuse, 1.0);

	#ifdef KAMAKURA_RIM_ON
		outColor.rgb = ApplyRim(outColor.rgb, IN.uv, dot(normalDir, viewDir), sampledCubeColor);
	#endif

		outColor.rgb *= texSample.a;
		outColor.a = texSample.a * _DiffuseColor.a;
		KAMAKURA_EXT_HairPixelFwdBase(IN, outColor)

		return outColor;
	}


	fixed4 PixelShaderForwardAdd(v2f_hair_fwd_add IN): SV_TARGET
	{
		fixed3 binormalDir = normalize(IN.binormal);
		fixed3 normalDir = normalize(IN.normal);
		fixed3 viewDir = normalize(IN.viewDir);

		float_t lightAtten = LIGHT_ATTENUATION(IN);

		fixed3 lightDirection = normalize(IN.lightDir);
		float_t adjuster = _LightRampUseGVertexColor * (2 * IN.color.g - 1) ;
		float_t lighting = Lambert(normalDir, lightDirection);
		fixed2 lightUV = fixed2(lighting + _LightRampOffset + adjuster, _LightRampPresetsOffset);
		fixed3 lightRampTexSample = tex2D(_LightRampTex, lightUV).xyz;

		fixed3 diffuse = lightAtten * _GlobalLightIntensity * (_LightColor0.xyz * lightRampTexSample);

		fixed4 outColor;
		float2_t diffuseUV = TRANSFORM_TEX(IN.uv, _MainTex);
		fixed4 texSample = tex2D(_MainTex, diffuseUV);
		texSample.rgb = AdjustColor(texSample.rgb);

		diffuse *= texSample.xyz * _DiffuseColor;

	#ifdef KAMAKURA_HATCH_ON
		float_t hatchDiffuseParam = _GlobalLightIntensity * lighting;
		diffuse = HatchDiffuse(IN.pos, IN.uv, hatchDiffuseParam * 2 - 1, lightAtten, diffuse);
	#endif

		float2_t hairSpecUV = TRANSFORM_TEX(IN.uv, _HairSpecShift);
		fixed hairSpecShift = (tex2D(_HairSpecShift, hairSpecUV).r - 0.5) * _SpecularShiftIntensity;

		float3_t t1 = ShiftBinormal(binormalDir, normalDir, _PrimaryShift + hairSpecShift);
		float3_t t2 = ShiftBinormal(binormalDir, normalDir, _SecondaryShift + hairSpecShift);

		float2_t specMaskUV = TRANSFORM_TEX(IN.uv, _SpecularMap);
		fixed specMaskSample = tex2D(_SpecularMap, specMaskUV).r;

		fixed primarySpecular = StrandSpecular(t1, viewDir, lightDirection, _SpecularPower1);
		primarySpecular = smoothstep(- 0.5 * _PrimarySpecularSmoothness + 0.5, _PrimarySpecularSmoothness * 0.5 + 0.5, primarySpecular);

		fixed3 specular = (primarySpecular * specMaskSample) * _SpecularColor1;
		fixed totalLightRamp = (_GlobalLightIntensity * lightRampTexSample);

		fixed primarySpecularShadowAffection = lerp(1, totalLightRamp, _PrimarySpecularShadowAffection);
		specular = specular * primarySpecularShadowAffection * _PrimarySpecularIntensity;

		fixed secondarySpecular = StrandSpecular(t2, viewDir, lightDirection, _SpecularPower2);
		secondarySpecular = smoothstep(-0.5 * _SecondarySpecularSmoothness + 0.5, _SecondarySpecularSmoothness * 0.5 + 0.5, secondarySpecular);

		float2_t hairStrandUV = TRANSFORM_TEX(IN.uv, _HairStrandTex);
		fixed hairStrandSample = tex2D(_HairStrandTex, hairStrandUV).r * _StrandTexIntensity;

		fixed secondarySpecularShadowAffection = lerp(1, totalLightRamp, _SecondarySpecularShadowAffection);
		specular += hairStrandSample * (secondarySpecular * specMaskSample) * (secondarySpecularShadowAffection * _SecondarySpecularIntensity) * _SpecularColor2;

		diffuse += specular * lightAtten;
		outColor = fixed4(diffuse, 1.0);

		outColor.rgb *= texSample.a;
		outColor.a = texSample.a * _DiffuseColor.a;
		KAMAKURA_EXT_HairPixelFwdAdd(IN, outColor)
		return outColor;
	}

#endif // __KAMAKURA_HAIR_CGINC