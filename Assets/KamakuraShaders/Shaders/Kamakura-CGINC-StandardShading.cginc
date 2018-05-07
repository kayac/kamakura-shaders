#ifndef __KAMAKURA_SHADING_CGINC
#define __KAMAKURA_SHADING_CGINC

	#include "Kamakura-CGINC-Shared.cginc"


	uniform fixed4 _DiffuseColor;
	uniform fixed3 _LightColor0;

#ifdef KAMAKURA_LOCALLIGHT_ON
	uniform fixed _LocalLightVecAsMain;
	uniform float4_t _LocalLightVec;
	uniform fixed _LocalLightIntensity;
#endif
	uniform fixed3 _LocalLightColor;
	uniform fixed _GlobalLightIntensity;

	struct v2f_standard_fwd_base
	{
		KAMAKURA_V2F_POS
		fixed4 color 					:	COLOR0;
		float2_t uvs					:	TEXCOORD0;
		float3_t normal					:	TEXCOORD1;
		fixed3 ambient					:	TEXCOORD2;
		fixed3 sampledCubeColor			:	TEXCOORD3;
		float3_t viewDir				:	TEXCOORD4;
		float3_t lightDir 				:	TEXCOORD5;
		LIGHTING_COORDS(6, 7)
	#ifdef KAMAKURA_NORMALMAP_ON
		float4_t tangent				:	TEXCOORD8;
	#endif
	#ifdef KAMAKURA_LOCALLIGHT_ON
		float3_t localLightDir 			:	TEXCOORD9;
	#endif
	};

	struct v2f_standard_fwd_add
	{
		KAMAKURA_V2F_POS
		fixed4 color 					:	COLOR0;
		float2_t uvs					:	TEXCOORD0;
		float3_t normal					:	TEXCOORD1;
		fixed3 sampledCubeColor			:	TEXCOORD2;
		float3_t viewDir				:	TEXCOORD3;
		float3_t lightDir 				:	TEXCOORD4;
		LIGHTING_COORDS(5, 6)
	#ifdef KAMAKURA_NORMALMAP_ON
		float4_t tangent				:	TEXCOORD7;
	#endif
	#ifdef KAMAKURA_LOCALLIGHT_ON
		float3_t localLightDir 			:	TEXCOORD8;
	#endif
	};

	v2f_standard_fwd_base VertexShaderForwardBase(appdata_full v)
	{
		UNITY_SETUP_INSTANCE_ID(v)
		v2f_standard_fwd_base OUT;
		UNITY_INITIALIZE_OUTPUT(v2f_standard_fwd_base, OUT)

		float4_t vertex = v.vertex;
		float3_t normal = v.normal;

	#ifdef KAMAKURA_VERTEXANIM_ON
		float2_t uvSample = GetSamplingUV(v.texcoord1);
		vertex += GetVertexAnimationDisplacement(uvSample);
		normal = GetVertexAnimationNormal(uvSample);
	#endif // KAMAKURA_VERTEXANIM_ON

		OUT.pos = UnityObjectToClipPos(vertex);
		OUT.uvs = v.texcoord;
		OUT.normal = normal;
		OUT.color = v.color;
		OUT.ambient = 0;

	#ifdef KAMAKURA_NORMALMAP_ON
		OUT.tangent = v.tangent;
	#endif

		float3_t worldNormal = UnityObjectToWorldNormal(normal);
		OUT.ambient = ShadeSH9(float4(worldNormal, 1.0)) * _AmbientUnitySHIntensity;
	#ifdef KAMAKURA_LOCALLIGHT_ON
		OUT.localLightDir = normalize(mul(unity_WorldToObject, _LocalLightVec).xyz);
	#endif
		OUT.sampledCubeColor = GetCubeColor(normal);
		OUT.viewDir = normalize(GetViewDir(vertex));
		OUT.lightDir = normalize(ObjSpaceLightDir(vertex));

		TRANSFER_VERTEX_TO_FRAGMENT(OUT)
		KAMAKURA_EXT_VertexFwdBase(v, OUT)
		return OUT;
	}

	v2f_standard_fwd_add VertexShaderForwardAdd(appdata_full v)
	{
		UNITY_SETUP_INSTANCE_ID(v)
		v2f_standard_fwd_add OUT;
		UNITY_INITIALIZE_OUTPUT(v2f_standard_fwd_add, OUT)

		float4_t vertex = v.vertex;
		float3_t normal = v.normal;

	#ifdef KAMAKURA_VERTEXANIM_ON
		float2_t uvSample = GetSamplingUV(v.texcoord1);
		vertex += GetVertexAnimationDisplacement(uvSample);
		normal = GetVertexAnimationNormal(uvSample);
	#endif // KAMAKURA_VERTEXANIM_ON

		OUT.pos = UnityObjectToClipPos(vertex);
		OUT.uvs = v.texcoord;
		OUT.normal = normal;
		OUT.color = v.color;

	#ifdef KAMAKURA_NORMALMAP_ON
		OUT.tangent = v.tangent;
	#endif

	#ifdef KAMAKURA_LOCALLIGHT_ON
		OUT.localLightDir = normalize(mul(unity_WorldToObject, _LocalLightVec).xyz);
	#endif

		OUT.sampledCubeColor = GetCubeColor(normal);
		float3_t lightDir = normalize(ObjSpaceLightDir(vertex));
		OUT.viewDir = normalize(GetViewDir(vertex));
		OUT.lightDir = lightDir;
		TRANSFER_VERTEX_TO_FRAGMENT(OUT)
		KAMAKURA_EXT_VertexFwdAdd(v, OUT)
		return OUT;
	}

	fixed4 PixelShaderForwardBase(v2f_standard_fwd_base IN): SV_TARGET
	{
		float2_t diffuseUVs = TRANSFORM_TEX(IN.uvs, _MainTex);
		fixed4 texSample = tex2D(_MainTex, diffuseUVs);

		texSample.rgb = AdjustColor(texSample.rgb);

	#ifdef _ENABLECUTOUT_ON
		clip(texSample.a - _AlphaCutoutValue);
	#endif

		float3_t normalDir = normalize(IN.normal);
		float3_t viewDir = normalize(IN.viewDir);


	#ifdef KAMAKURA_NORMALMAP_ON
		fixed hasChirality = abs(sign(IN.tangent.w));
		fixed tangentChirality = hasChirality * IN.tangent.w + (1 - hasChirality) * 1;
		float4_t tangentDir = float4_t(normalize(IN.tangent.xyz), tangentChirality);
		normalDir = ApplyNormalMap(normalDir, tangentDir, IN.uvs);
	#endif


		fixed3 sampledCubeColor = IN.sampledCubeColor;
		fixed3 ambient = GetAmbient(sampledCubeColor, IN.ambient);

		float3_t lightDirection = normalize(IN.lightDir);
		float_t shadowAtten = SHADOW_ATTENUATION(IN);
		float_t lighting = Lambert(normalDir, lightDirection);
		fixed4 lightRampTexResult = LightRampTexSample(lighting, IN.color.g);
		fixed lightRampAdjuster = lightRampTexResult.w;
		fixed3 lightRampTexSample = lightRampTexResult.rgb * shadowAtten;

		fixed3 diffuse = _GlobalLightIntensity * (_LightColor0.xyz * lightRampTexSample) + ambient;

	#ifdef KAMAKURA_LOCALLIGHT_ON
		float3_t localLightDirection = normalize(IN.localLightDir);
		float_t localLighting = Lambert(normalDir, localLightDirection);
		fixed2 localLightUV = fixed2(localLighting + _LightRampOffset + lightRampAdjuster, _LightRampPresetsOffset);
		fixed3 localLightRampTexSample = tex2D(_LightRampTex, localLightUV).xyz;
		diffuse = localLightRampTexSample * _LocalLightColor.xyz + diffuse;
		lightDirection = _LocalLightVecAsMain > 0.5 ? localLightDirection : lightDirection;
		fixed3 localLightDiffuse = _LocalLightIntensity * localLightRampTexSample;
		fixed unmappedLocalLightDiffuse = _LocalLightIntensity * localLighting;
	#else
		fixed3 localLightDiffuse = fixed3(0, 0, 0);
		fixed unmappedLocalLightDiffuse = 0;
	#endif

	#ifdef KAMAKURA_SHADOWMOD_ON
		diffuse = ShadowMod(IN.uvs, texSample * _DiffuseColor, _GlobalLightIntensity * lightRampTexSample, _LightColor0.xyz, localLightDiffuse, _LocalLightColor.xyz, ambient);
	#else
		diffuse = diffuse * texSample.xyz * _DiffuseColor;
	#endif

	#ifdef KAMAKURA_HATCH_ON
		float_t hatchDiffuseParam = _GlobalLightIntensity * lighting + unmappedLocalLightDiffuse;
		diffuse = HatchDiffuse(IN.pos, IN.uvs, hatchDiffuseParam * 2 - 1, 1, diffuse);
	#endif

		diffuse += Specular(viewDir, normalDir, lightDirection, shadowAtten, IN.uvs);
		fixed4 outColor = fixed4(diffuse, 1.0);

	#ifdef KAMAKURA_RIM_ON
		outColor.rgb = ApplyRim(outColor.rgb, IN.uvs, dot(normalDir, viewDir), sampledCubeColor);
	#endif

	#ifdef KAMAKURA_EMISSION_ON
		outColor = ApplyEmission(outColor, IN.uvs);
	#endif

		outColor.rgb *= texSample.a;
		outColor.a = texSample.a * _DiffuseColor.a;

		KAMAKURA_EXT_PixelFwdBase(IN, outColor)
		return outColor;
	}

	fixed4 PixelShaderForwardAdd(v2f_standard_fwd_add IN): SV_TARGET
	{
		fixed4 outColor;

		float2_t diffuseUVs = TRANSFORM_TEX(IN.uvs, _MainTex);
		fixed4 texSample = tex2D(_MainTex, diffuseUVs);

	#ifdef _ENABLECUTOUT_ON
		clip(texSample.a - _AlphaCutoutValue);
	#endif

		texSample.rgb = AdjustColor(texSample.rgb);
		float3_t normalDir = normalize(IN.normal);

	#ifdef KAMAKURA_NORMALMAP_ON
		fixed hasChirality = abs(sign(IN.tangent.w));
		fixed tangentChirality = hasChirality * IN.tangent.w + (1 - hasChirality) * 1;
		float4_t tangentDir = float4_t(normalize(IN.tangent.xyz), tangentChirality);
		normalDir = ApplyNormalMap(normalDir, tangentDir, IN.uvs);
	#endif

		float_t lightRampAdjuster = _LightRampUseGVertexColor * (2 * IN.color.g - 1);
		float_t lightAtten = LIGHT_ATTENUATION(IN);
		float3_t lightDirection = normalize(IN.lightDir);
		float_t lighting = saturate(Lambert(normalDir, lightDirection));
		fixed2 lightUV = fixed2(lighting + _LightRampOffset + lightRampAdjuster, _LightRampPresetsOffset);
		fixed3 lightRampTexSample = tex2D(_LightRampTex, lightUV).xyz;

		fixed3 diffuse = (lightAtten * _GlobalLightIntensity) * _LightColor0 * lightRampTexSample * lightAtten;
		diffuse = diffuse * texSample.xyz * _DiffuseColor;

	#ifdef KAMAKURA_HATCH_ON
		float_t hatchDiffuseParam = _GlobalLightIntensity * lighting;
		diffuse = HatchDiffuse(IN.pos, IN.uvs, hatchDiffuseParam * 2 - 1, lightAtten, diffuse);
	#endif

		diffuse += Specular(IN.viewDir, normalDir, lightDirection, lightAtten, IN.uvs);

		outColor.rgb = diffuse * texSample.a;
		outColor.a = texSample.a * _DiffuseColor.a;

		KAMAKURA_EXT_PixelFwdAdd(IN, outColor)
		return outColor;
	}
#endif // __KAMAKURA_SHADING_CGINC