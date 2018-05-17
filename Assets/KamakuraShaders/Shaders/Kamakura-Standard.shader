
Shader "Kayac/Kamakura"
{
	Properties
	{
		[Header(About)]
		[KamakuraShaderVersion] _ShaderVersion ("_ShaderVersion", Vector) = (1, 0, 7, -1)

		[Header(Basic)]
		_DiffuseColor ("Diffuse Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
		[KamakuraBlendMode] _BlendMode ("Blend Mode", Float) = 0.0
		[MaterialToggle] _EnableCutout ("Enable Cutout", Float ) = 0.0
		_AlphaCutoutValue ("Alpha Cutout Value", Range(0.0, 1.0)) = 0.0

		[Header(Filter)]
		[Toggle] _EnableFilter ("Enable", Float) = 0.0
		_FilterHue("Hue", Range(-0.5,0.5)) = 0.0
		_FilterSaturation("Saturation", Float) = 1.0
		_FilterBrightness("Brightness", Float) = 1.0
		_FilterContrast("Contrast Level", Float) = 1.0
		_FilterContrastMidPoint("Contrast Mid Point", Float) = 0.5

		[Header(Normal)]
		[Toggle(KAMAKURA_NORMALMAP_ON)] _EnableNormal ("Enable", Float) = 0.0
		[CheckNormalType][NoScaleOffset] _NormalTex ("Texture", 2D) = "bump" {}
		_NormalIntensity ("Intensity", Float) = 1.0

		// Specular parameters
		[Header(Specular)]
		_SpecularColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[NoScaleOffset] _SpecularTex ("Mask", 2D) = "white" {}
		[PowerSlider(10.0)] _SpecularPower ("Power", Range(1.0, 500.0)) = 10.0
		_SpecularIntensity ("Intensity", Range(0.0, 1.0)) = 1.0
		_SpecularSmoothness ("Softness", Range(0.05, 1.0)) = 1.0

		// Light-ramp parameters
		[Header(Light Ramp)]
		[NoScaleOffset] _LightRampTex ("Ramp", 2D) = "white" {}
		_LightRampPresetsOffset ("Preset", Range(0.0, 1.0)) = 0.0
		_LightRampOffset ("Offset", Range(-1.0, 1.0)) = 0.0
		[MaterialToggle] _LightRampUseGVertexColor ("Offset Using VertexColor (green)", Float) = 0.0

		// Shadow parameters
		[Header(Shadow)]
		[Toggle(KAMAKURA_SHADOWMOD_ON)]
		_EnableShadowMod ("Enable", Float) = 0.0
		[NoScaleOffset] _ShadowModTex ("Texture", 2D) = "white" {}
		[KeywordEnum(Normal, Multiply)] _ShadowModBlendMode ("Blend Mode", Float) = 0.0
		_ShadowModColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowModIntensity ("Intensity", Float) = 1
		[MaterialToggle] _ShadowModUseFilter ("Filter Texture", Float) = 0.0

		[Header(Ambient)]
		_AmbientColor ("Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_AmbientIntensity ("Intensity", Float) = 0.5
		_AmbientUnitySHIntensity ("Unity's Ambient Intensity", Float) = 0.0

		// Outline parameters
		[Header(Outline)]
		[MaterialToggle] _EnableOutline ("Enable", Float) = 0.0
		_OutlineColor ("Color", Color) = (0.1, 0.1, 0.1, 1)
		_OutlineBlendColorTexture ("Blend Diffuse Texture", Range(0.0, 1.0)) = 1.0
		[ScaledRangeParam(0.0, 1.0)] _OutlineSize ("Thickness", Range(0.0, 0.05)) = 0.01
		_OutlineCameraDistanceAdaptRate ("Adaptive Thickness", Range(0.0, 1.0)) = 1.0
		[MaterialToggle] _OutlineWriteZ ("Write Depth", Float) = 1.0
		[ScaledRangeParam(0.0, 1.0)] _OutlineZOffset ("Z Offset", Range(0.0, 0.00015)) = 0.0
		[MaterialToggle] _OutlineUseRVertexColor ("Thickness Using VertexColor (red)", Float) = 0.0

		// Hatch parameters
		[Header(Hatch)]
		[Toggle(KAMAKURA_HATCH_ON)] _EnableHatch ("Enable", Float) = 0.0
		_HatchTex ("Texture", 2D) = "white" {}
		[KeywordEnum(Normal, Multiply)] _HatchBlendMode ("Blend Mode", Float) = 0.0
		[MaterialToggle] _HatchScreenSpace ("Screen Space", Float) = 0.0
		_Hatch1Color ("Level 1 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Hatch2Color ("Level 2 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Hatch3Color ("Level 3 Color", Color) = (0.0, 0.0, 0.0, 1.0)
		[NoScaleOffset] _HatchMask ("Mask", 2D) = "white" {}
		_HatchOffset ("Threshold", Range(-1.0, 1.0)) = 0.0
		_HatchIntensity ("Intensity", Range(0.0, 2.0)) = 1.0
		_HatchRangeStretch ("Stretch", Range(1.0, 8.0)) = 1.0
		[RotationParam(_HatchRotSin, _HatchRotCos)] _HatchRotation ("Rotation", Range(0.0, 360.0)) = 0.0
		[HideInInspector] _HatchRotSin ("-", Float) = 0.0
		[HideInInspector] _HatchRotCos ("-", Float) = 1.0

		// Rim-light parameters
		[Header(Rim)]
		[Toggle(KAMAKURA_RIM_ON)] _EnableRim ("Enable", Float) = 0.0
		[KeywordEnum(Additive, Normal)]_RimBlendingMode ("Blend Mode", Float) = 0.0
		_RimColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimSize ("Size", Range(0, 1)) = 0.3
		_RimIntensity ("Intensity", Range(0.01, 1)) = 0.3
		_RimSoftness ("Softness", Range(0.01, 1)) = 0.3
		_RimNoiseTex ("Noise", 2D) = "white" {}

		[Header(Emission)]
		[Toggle(KAMAKURA_EMISSION_ON)] _EnableEmission ("Enable", Float) = 0.0
		[NoScaleOffset] _EmissionTex ("Texture", 2D) = "white" {}
		_EmissionColor ("Color", Color) = (1, 1, 1, 1)
		_EmissionPower ("Power", Float) = 0.5

		[Header(Cube Color)]
		[Toggle(KAMAKURA_CUBECOLOR_ON)] _EnableCubeColor ("Enable", Float) = 0.0
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

		UsePass "Hidden/Kayac/KamakuraPassesStandard/SHADOWCASTER"
		UsePass "Hidden/Kayac/KamakuraPassesStandard/OUTLINE"
		UsePass "Hidden/Kayac/KamakuraPassesStandard/FORWARD"
		UsePass "Hidden/Kayac/KamakuraPassesStandard/FORWARDADD"
	}
	FallBack "Diffuse"
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"
}
