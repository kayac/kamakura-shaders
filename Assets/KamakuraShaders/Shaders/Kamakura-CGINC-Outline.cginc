
#ifndef __KAMAKURA_OUTLINE_CGINC
#define __KAMAKURA_OUTLINE_CGINC

#include "Kamakura-CGINC-Shared.cginc"

struct appdata_outline
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	fixed4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_outline
{
	float4 pos : SV_POSITION;
	float2 UV  : TEXCOORD0;
};

uniform fixed _EnableOutline;
uniform fixed4 _OutlineColor;
uniform fixed _OutlineCameraDistanceAdaptRate;
uniform fixed _OutlineBlendColorTexture;
uniform fixed _OutlineUseRVertexColor;
uniform float_t _OutlineSize;
uniform float_t _OutlineZOffset;

v2f_outline OutlineVert(appdata_outline v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_BRANCH
	if (_EnableOutline > 0.5)
	{
		float4 vertex = v.vertex;
		float3 normal = v.normal;

		float4 worldPos = mul(unity_ObjectToWorld, vertex);
		float thickness = _OutlineSize * (v.color.r * _OutlineUseRVertexColor + (1 - _OutlineUseRVertexColor));
		float cameraDistance =  lerp(1, distance(_WorldSpaceCameraPos, worldPos.xyz), (1 - UNITY_MATRIX_P[3][3]) * _OutlineCameraDistanceAdaptRate);
		float3 clipNormal = normalize(mul(UNITY_MATRIX_IT_MV, float4(normal, 1)).xyz);

		clipNormal.x *= _ScreenParams.y / _ScreenParams.x;

		#if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(SHADER_API_D3D9)
			clipNormal.y *= -1;
		#endif

		v2f_outline o;
		UNITY_INITIALIZE_OUTPUT(v2f_outline, o)

		o.pos = UnityObjectToClipPos(vertex);
		float3 clipPos = float3(o.pos.xyz + thickness * clipNormal * cameraDistance);
		o.pos.xy = clipPos.xy;

		#ifdef UNITY_REVERSED_Z
			o.pos.z -= _OutlineZOffset;
		#else
			o.pos.z += _OutlineZOffset;
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

fixed4 OutlineFrag(v2f_outline i) : COLOR
{
	fixed4 mainMapColor = tex2D(_MainTex, i.UV);
#ifdef _ENABLECUTOUT_ON
	clip(mainMapColor.a - _AlphaCutoutValue);
#endif
	return lerp(_OutlineColor, _OutlineColor * mainMapColor, _OutlineBlendColorTexture);
}

#endif // __KAMAKURA_OUTLINE_CGINC