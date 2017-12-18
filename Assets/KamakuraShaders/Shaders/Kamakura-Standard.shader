
Shader "Kayac/Kamakura"
{
	Properties
	{
		[Header(About)]
		[KamakuraShaderVersion] _ShaderVersion ("_ShaderVersion", Vector) = (1, 0, 0, -1)

		[Header(Basic)]
		_DiffuseColor ("Diffuse Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
		[KamakuraBlendMode] _BlendMode ("Blend Mode", Float) = 0.0
		[MaterialToggle] _EnableCutout ("Enable Cutout", Float ) = 0.0
		_AlphaCutoutValue ("Alpha Cutout Value", Range(0.0, 1.0)) = 0.0

		[Header(Filter)]
		[Toggle] _EnableFilter ("Enable Filter", Float) = 0.0
		_FilterHue("Hue", Range(-0.5,0.5)) = 0.0
		_FilterSaturation("Saturation", Float) = 1.0
		_FilterBrightness("Brightness", Float) = 1.0
		_FilterContrast("Contrast Level", Float) = 1.0
		_FilterContrastMidPoint("Contrast Mid Point", Float) = 0.5

		[Header(Normal)]
		[Toggle(KAMAKURA_NORMALMAP_ON)] _EnableNormal ("Enable Normal Map", Float) = 0.0
		[CheckNormalType][NoScaleOffset] _NormalTex ("Normal Map", 2D) = "bump" {}

		// Specular parameters
		[Header(Specular)]
		_SpecularColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[NoScaleOffset] _SpecularTex ("Specular Mask", 2D) = "white" {}
		[PowerSlider(10.0)] _SpecularPower ("Specular Power", Range(1.0, 500.0)) = 10.0
		_SpecularIntensity ("Specular Intensity", Range(0.0, 1.0)) = 1.0
		_SpecularSmoothness ("Specular Smoothness", Range(0.05, 1.0)) = 1.0

		// Light-ramp parameters
		[Header(Light Ramp)]
		[NoScaleOffset] _LightRampTex ("Light Ramp", 2D) = "white" {}
		_LightRampPresetsOffset ("Light Ramp Preset", Range(0.0, 1.0)) = 0.0
		_LightRampOffset ("Light Ramp Offset", Range(-1.0, 1.0)) = 0.0
		[MaterialToggle] _LightRampUseGVertexColor ("Shift Light Ramp Using VertexColor (G)", Float) = 0.0

		// Shadow parameters
		[Header(Shadow)]
		[Toggle(KAMAKURA_SHADOWMOD_ON)]
		_EnableShadowMod ("Enable Shadow", Float) = 0.0
		[NoScaleOffset] _ShadowModTex ("Shadow Texture", 2D) = "white" {}
		_ShadowModColor ("Shadow Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowModIntensity ("Shadow Intensity", Float) = 1
		[MaterialToggle] _ShadowModUseFilter ("Use Filter for shadow texture", Float) = 0.0

		[Header(Ambient)]
		_AmbientColor ("Ambient Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_AmbientIntensity ("Ambient Intensity", Float) = 0.5
		_AmbientUnitySHIntensity ("Unity's Ambient Intensity", Float) = 0.0

		// Outline parameters
		[Header(Outline)]
		[MaterialToggle] _EnableOutline ("Enable Outline", Float) = 0.0
		_OutlineColor ("Outline Color", Color) = (0.1, 0.1, 0.1, 1)
		_OutlineBlendColorTexture ("Blend Color-Texture Value", Range(0.0, 1.0)) = 1.0
		[ScaledRangeParam(0.0, 1.0)] _OutlineSize ("Outline Thickness", Range(0.0, 0.05)) = 0.01
		_OutlineCameraDistanceAdaptRate ("Adapt to Camera Distance Value", Range(0.0, 1.0)) = 1.0
		[MaterialToggle] _OutlineWriteZ ("Enable Inner-side Outline", Float) = 1.0
		[MaterialToggle] _OutlineUseRVertexColor ("Adjust Outline Using VertexColor (R)", Float) = 0.0

		// Hatch parameters
		[Header(Hatch)]
		[Toggle(KAMAKURA_HATCH_ON)] _EnableHatch ("Enable Hatch", Float) = 0.0
		_HatchTex ("Hatch Texture", 2D) = "white" {}
		[MaterialToggle] _HatchScreenSpace ("Hatch in Screen Space", Float) = 0.0
		_Hatch1Color ("Hatch Level 1 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Hatch2Color ("Hatch Level 2 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Hatch3Color ("Hatch Level 3 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		[NoScaleOffset] _HatchMask ("Hatch Mask", 2D) = "white" {}
		_HatchOffset ("Hatch Threshold", Range(-1.0, 1.0)) = 0.0
		_HatchIntensity ("Hatch Intensity", Range(0.0, 2.0)) = 0.0
		_HatchRangeStretch ("Hatch Range Stretch", Range(1.0, 8.0)) = 1.0
		[RotationParam(_HatchRotSin, _HatchRotCos)] _HatchRotation ("Hatch Rotation", Range(0.0, 360.0)) = 0.0
		[HideInInspector] _HatchRotSin ("-", Float) = 0.0
		[HideInInspector] _HatchRotCos ("-", Float) = 1.0

		// Rim-light parameters
		[Header(Rim)]
		[Toggle(KAMAKURA_RIM_ON)] _EnableRim ("Enable Rim", Float) = 0.0
		[KeywordEnum(Additive, Normal)]_RimBlendingMode ("Blending Mode", Float) = 0.0
		_RimColor ("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimSize ("Rim Size", Range(0, 1)) = 0.3
		_RimIntensity ("Rim Intensity", Range(0.01, 1)) = 0.3
		_RimSoftness ("Rim Softness", Range(0.01, 1)) = 0.3
		_RimNoiseTex ("Rim Noise", 2D) = "white" {}

		[Header(Emission)]
		[Toggle(KAMAKURA_EMISSION_ON)] _EnableEmission ("Enable Emission", Float) = 0.0
		[NoScaleOffset] _EmissionTex ("Emission Map", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
		_EmissionPower ("Emission Power", Float) = 0.5

		[Header(Cube Color)]
		[Toggle(KAMAKURA_CUBECOLOR_ON)] _EnableCubeColor ("Enable Cube Color", Float) = 0.0
		[MaterialToggle] _RimUseCubeColor ("Use For Rim", Float) = 0
		[MaterialToggle] _AmbientUseCubeColor ("Use For Ambient", Float) = 0.0
		// Cube color parameters
		_CubeColor3 ("Left", Color) = (1, 1, 1, 1)
		_CubeColor1 ("Right", Color) = (1, 1, 1, 1)
		_CubeColor2 ("Front", Color) = (1, 1, 1, 1)
		_CubeColor4 ("Back", Color) = (1, 1, 1, 1)
		_CubeColor0 ("Top", Color) = (1, 1, 1, 1)
		_CubeColor5 ("Down", Color) = (1, 1, 1, 1)

		[Header(Stencil)]
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Compare Function", Float) = 0
		_StencilRef ("Stencil Value", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Pass Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Fail Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail Operation", Float) = 0

		// Cube color local space matrix
		[PerRendererData] _CubeColorUseLocalSpace ("Use In Local Space", Float) = 0
		[PerRendererData] _CubeColorLocalSpaceMatrixRow0  ("CLSMR0", Vector) = (1, 0, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow1  ("CLSMR1", Vector) = (0, 1, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow2  ("CLSMR2", Vector) = (0, 0, 1, 0)

		// Local light parameters
		[HideInInspector] _LocalLightVec ("Local Light Vector", Vector) = (1.0, 0.0, 0.0, 0.0)
		[HideInInspector] _LocalLightColor ("Local Light Color", Color) = (0.0, 0.0, 0.0, 1.0)
		[HideInInspector] _GlobalLightIntensity ("Global Light Intensity", Float) = 1.0
		[HideInInspector] _LocalLightIntensity ("Local Light Intensity", Float) = 1.0
		[HideInInspector] _LocalLightVecAsMain ("Use Local Light Vector As Main Light Vector", Float) = 0.0

		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		UsePass "Hidden/Kayac/KamakuraPasses/SHADOWCASTER"
		UsePass "Hidden/Kayac/KamakuraPasses/OUTLINE"
		UsePass "Hidden/Kayac/KamakuraPasses/FORWARD"
		UsePass "Hidden/Kayac/KamakuraPasses/FORWARDADD"
	}
	FallBack "Diffuse"
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"
}
