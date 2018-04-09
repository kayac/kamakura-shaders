Shader "Hidden/Kayac/KamakuraPassesStandard"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Offset 1, 1

			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON

			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
			};

			uniform float4 _MainTex_ST;

			v2f vert( appdata_base v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				TRANSFER_SHADOW_CASTER(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _EnableCutout;
			uniform fixed _AlphaCutoutValue;

			float4 frag( v2f i ) : COLOR
			{
				fixed4 texcol = tex2D( _MainTex, i.uv );
			#ifdef _ENABLECUTOUT_ON
				clip( texcol.a - _AlphaCutoutValue );
			#endif
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

		}

		Pass
		{
			Name "Outline"
			Cull Front
			ZTest Less
			ZWrite [_OutlineWriteZ]

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON
			#pragma multi_compile_instancing

			#pragma vertex OutlineVert
			#pragma fragment OutlineFrag

			#include "UnityCG.cginc"
			#include "Kamakura-CGINC-Outline.cginc"
			ENDCG
		}

		Pass
		{
			Name "Forward"
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			ZWrite On
			ZTest LEqual
			Cull [_CullMode]
			Blend [_SrcBlend] [_DstBlend]

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON
			#pragma shader_feature _ KAMAKURA_HATCH_ON
			#pragma shader_feature _ KAMAKURA_RIM_ON
			#pragma shader_feature _ KAMAKURA_NORMALMAP_ON
			#pragma shader_feature _ KAMAKURA_SHADOWMOD_ON
			#pragma shader_feature _ KAMAKURA_EMISSION_ON
			#pragma shader_feature _ KAMAKURA_LOCALLIGHT_ON

			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex VertexShaderForwardBase
			#pragma fragment PixelShaderForwardBase

			#include "Kamakura-CGINC-StandardShading.cginc"
			ENDCG
		}


		Pass
		{
			Name "ForwardAdd"
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			ZWrite On
			ZTest LEqual
			Cull [_CullMode]
			Blend One One

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON
			#pragma shader_feature _ KAMAKURA_HATCH_ON
			#pragma shader_feature _ KAMAKURA_RIM_ON
			#pragma shader_feature _ KAMAKURA_NORMALMAP_ON
			#pragma shader_feature _ KAMAKURA_SHADOWMOD_ON
			#pragma shader_feature _ KAMAKURA_EMISSION_ON
			#pragma shader_feature _ KAMAKURA_LOCALLIGHT_ON

			#pragma multi_compile_fwdadd
			#pragma multi_compile_instancing

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex VertexShaderForwardAdd
			#pragma fragment PixelShaderForwardAdd

			#include "Kamakura-CGINC-StandardShading.cginc"
			ENDCG
		}

		Pass
		{
			Name "HairForward"
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			ZWrite On
			ZTest LEqual
			Cull [_CullMode]
			Blend [_SrcBlend] [_DstBlend]

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON
			#pragma shader_feature _ KAMAKURA_HATCH_ON
			#pragma shader_feature _ KAMAKURA_RIM_ON
			#pragma shader_feature _ KAMAKURA_SHADOWMOD_ON
			#pragma shader_feature _ KAMAKURA_LOCALLIGHT_ON

			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex VertexShaderForwardBase
			#pragma fragment PixelShaderForwardBase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Kamakura-CGINC-HairShading.cginc"

			ENDCG
		}

		Pass
		{
			Name "HairForwardAdd"
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			ZWrite On
			ZTest LEqual
			Cull [_CullMode]
			Blend One One

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			CGPROGRAM
			#pragma shader_feature _ _ENABLECUTOUT_ON
			#pragma shader_feature _ KAMAKURA_HATCH_ON
			#pragma shader_feature _ KAMAKURA_RIM_ON
			#pragma shader_feature _ KAMAKURA_SHADOWMOD_ON
			#pragma shader_feature _ KAMAKURA_LOCALLIGHT_ON

			#pragma multi_compile_fwdadd
			#pragma multi_compile_instancing

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex VertexShaderForwardAdd
			#pragma fragment PixelShaderForwardAdd

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Kamakura-CGINC-HairShading.cginc"

			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"
}
