
#ifndef __KAMAKURA_OUTLINE_CGINC
#define __KAMAKURA_OUTLINE_CGINC

#include "Kamakura-CGINC-Shared.cginc"

struct appdata_outline
{
	float4_t vertex : POSITION;
	float2_t texcoord : TEXCOORD0;
	float2_t texcoord2 : TEXCOORD1;
	float3_t normal : NORMAL;
	fixed4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_outline
{
	float4_t pos : SV_POSITION;
	float2_t UV  : TEXCOORD0;
};

uniform fixed _EnableOutline;
uniform fixed4 _OutlineColor;
uniform fixed _OutlineCameraDistanceAdaptRate;
uniform fixed _OutlineBlendColorTexture;
uniform fixed _OutlineUseRVertexColor;

v2f_outline OutlineVert(appdata_outline v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_BRANCH
	if (_EnableOutline > 0.5)
	{
		float4_t vertex = v.vertex;
		float3_t normal = v.normal;

	#ifdef KAMAKURA_VERTEXANIM_ON
		float2_t uvSample = GetSamplingUV(v.texcoord2);
		vertex += GetVertexAnimationDisplacement(uvSample);
		normal = GetVertexAnimationNormal(uvSample);
	#endif

		float4_t worldPos = mul(unity_ObjectToWorld, vertex);
		float3_t viewDir = normalize(WorldSpaceViewDir(vertex));
		float3_t worldNormal = mul(float4_t(normal.xyz, 0), unity_WorldToObject);
		float_t thickness = v.color.r * _OutlineUseRVertexColor + (1 - _OutlineUseRVertexColor);
		float_t normalScale = _OutlineSize * thickness;
		float_t cameraDistance = lerp(length(_WorldSpaceCameraPos-worldPos.xyz), 1, 1 - _OutlineCameraDistanceAdaptRate);

		worldPos.xyz += normalScale * worldNormal * cameraDistance;
		worldPos.xyz = mul(unity_WorldToObject, float4_t(worldPos.xyz, 1)).xyz;

		v2f_outline o;
		UNITY_INITIALIZE_OUTPUT(v2f_outline, o)

		o.pos = UnityObjectToClipPos(worldPos);

		#ifdef UNITY_REVERSED_Z
			o.pos.z -= _DepthBias;
		#else
			o.pos.z += _DepthBias;
		#endif

		o.UV = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
		return o;
	}
	else
	{
		v2f_outline o;
		o.pos = 0;
		o.UV = 0;
		return o;
	}
}

float4_t OutlineFrag(v2f_outline i) : COLOR
{
	float4_t mainMapColor = tex2D(_MainTex, i.UV);
#ifdef _ENABLECUTOUT_ON
	clip(mainMapColor.a - _AlphaCutoutValue);
#endif
	return lerp(_OutlineColor, _OutlineColor * mainMapColor, _OutlineBlendColorTexture);
}

#endif // __KAMAKURA_OUTLINE_CGINC