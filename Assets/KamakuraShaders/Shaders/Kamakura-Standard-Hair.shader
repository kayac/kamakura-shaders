Shader "Kayac/Kamakura-Hair"
{
	Properties
	{
//@ <About>
		[Header(About)] [KSFlags(NonSelectable)] [KamakuraShaderVersion]_ShaderVersion ("_ShaderVersion", Vector) = (1, 0, 8, -1)
		[HideInInspector] _PatchData ("_PatchData", Vector) = (0, 0, 0, 0)
//@ </About>

//@ <Basic_Hair>
		[Header(Basic)]
		_DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
		[KamakuraBlendMode] _BlendMode ("Blend Mode", Float) = 0.0
		[KSFlags(AutoRenderQueue)] [MaterialToggle] _AutoAdjustRenderQueue ("Auto Adjust Render Queue", Float) = 1.0
//@ </Basic_Hair>

//@ <Filter>
		[Header(Filter)] [KSFlags(EnableFlag)] [Toggle]
		_EnableFilter ("Enable", Float) = 0.0
		_FilterHue("Hue", Range(-0.5,0.5)) = 0.0
		_FilterSaturation("Saturation", Float) = 1.0
		_FilterBrightness("Brightness", Float) = 1.0
		_FilterContrast("Contrast Level", Float) = 1.0
		_FilterContrastMidPoint("Contrast Mid Point", Float) = 0.5
//@ </Filter>

//@ <Specular_Hair>
		[Header(Specular)]
		_HairSpecShift ("Local Shift", 2D) = "gray" {}
		_SpecularShiftIntensity ("Local Shift Strength", Float) = 1.0
		[NoScaleOffset] _SpecularMap ("Mask", 2D) = "white" {}
		[RotationParam(_BinormalRotSin, _BinormalRotCos)] _BinormalRotation ("Rotation", Range(0.0, 360.0)) = 0.0
		[HideInInspector] _BinormalRotSin ("-", Float) = 0.0
		[HideInInspector] _BinormalRotCos ("-", Float) = 1.0

		[Header(Primary Specular)]
		_SpecularColor1 ("Color", Color) = (1, 1, 1, 1)
		_SpecularPower1 ("Power", Float) = 10
		_PrimarySpecularIntensity ("Intensity", Range(0.0, 1.0)) = 1.0
		_PrimarySpecularSmoothness ("Smoothness", Range(0.0, 1.0)) = 1.0
		_PrimaryShift ("Shift", Range(-2, 2)) = 0
		_PrimarySpecularShadowAffection ("Shadow Affection", Range(0, 1)) = 0.5

		[Header(Secondary Specular)]
		_SpecularColor2 ("Color", Color) = (1, 1, 1, 1)
		_SpecularPower2 ("Power", Float) = 10
		_SecondarySpecularIntensity ("Intensity", Range(0.0, 1.0)) = 1.0
		_SecondarySpecularSmoothness ("Smoothness", Range(0.0, 1.0)) = 1.0
		_SecondaryShift ("Shift", Range(-2, 2)) = 0
		_HairStrandTex ("Strand Texture", 2D) = "gray" {}
		_StrandTexIntensity ("Strand Texture Intensity", Range(0, 1.0)) = 1.0
		_SecondarySpecularShadowAffection ("Shadow Affection", Range(0, 1)) = 0.5
//@ </Specular_Hair>

//@ <Light_Ramp>
		[Header(Light Ramp)]
		[NoScaleOffset] _LightRampTex ("Ramp", 2D) = "white" {}
		_LightRampPresetsOffset ("Preset", Range(0.0, 1.0)) = 0.0
		_LightRampOffset ("Offset", Range(-1.0, 1.0)) = 0.0
		[MaterialToggle] _LightRampUseGVertexColor ("Offset Using VertexColor (green)", Float) = 0.0
//@ </Light_Ramp>

//@ <Local_Light>
		[Header(Local Light)] [KSFlags(EnableFlag)] [Toggle(KAMAKURA_LOCALLIGHT_ON)]
		_EnableLocalLight ("Enable", Float) = 0.0
		[PerRendererData] _LocalLightVec ("Local Light Vector", Vector) = (1.0, 0.0, 0.0, 0.0)
		[PerRendererData] _LocalLightColor ("Local Light Color", Color) = (0.0, 0.0, 0.0, 1.0)
		[PerRendererData] _GlobalLightIntensity ("Global Light Intensity", Float) = 1.0
		[PerRendererData] _LocalLightIntensity ("Local Light Intensity", Float) = 0.0
		[PerRendererData] _LocalLightVecAsMain ("Use Local Light Vector As Main Light Vector", Float) = 0.0
//@ </Local_Light>

//@ <Shadow_Mod>
		[Header(Shadow)] [KSFlags(EnableFlag)] [Toggle(KAMAKURA_SHADOWMOD_ON)]
		_EnableShadowMod ("Enable", Float) = 0.0
		[NoScaleOffset] _ShadowModTex ("Texture", 2D) = "white" {}
		[KeywordEnum(Normal, Multiply)] _ShadowModBlendMode ("Blend Mode", Float) = 0.0
		_ShadowModColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowModIntensity ("Intensity", Float) = 1
		[MaterialToggle] _ShadowModUseFilter ("Filter Texture", Float) = 0.0
//@ </Shadow_Mod>

//@ <Ambient>
		[Header(Ambient)]
		_AmbientColor ("Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_AmbientIntensity ("Intensity", Float) = 0.5
		_AmbientUnitySHIntensity ("GI Intensity", Float) = 0.0
//@ </Ambient>

//@ <Outline>
		[Header(Outline)] [KSFlags(EnableFlag)] [MaterialToggle]
		_EnableOutline ("Enable", Float) = 0.0
		_OutlineColor ("Color", Color) = (0.1, 0.1, 0.1, 1)
		_OutlineBlendColorTexture ("Blend Diffuse Texture", Range(0.0, 1.0)) = 1.0
		[ScaledRangeParam(0.0, 1.0)] _OutlineSize ("Thickness", Range(0.0, 0.05)) = 0.01
		_OutlineCameraDistanceAdaptRate ("Adaptive Thickness", Range(0.0, 1.0)) = 1.0
		[ScaledRangeParam(0.0, 1.0)] _OutlineZOffset ("Z Offset", Range(0.0, 0.00015)) = 0.0
		[MaterialToggle] _OutlineUseRVertexColor ("Thickness Using VertexColor (red)", Float) = 0.0
//@ </Outline>

//@ <Hatch>
		[Header(Hatch)] [KSFlags(EnableFlag)] [Toggle(KAMAKURA_HATCH_ON)]
		_EnableHatch ("Enable", Float) = 0.0
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
//@ </Hatch>

//@ <Rim>
		[Header(Rim)] [KSFlags(EnableFlag)] [Toggle(KAMAKURA_RIM_ON)]
		_EnableRim ("Enable", Float) = 0.0
		[KeywordEnum(Additive, Normal)] _RimBlendingMode ("Blend Mode", Float) = 0.0
		_RimColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimUnitySHIntensity ("GI Intensity", Float) = 0.0
		_RimSize ("Size", Range(0, 1)) = 0.3
		_RimIntensity ("Intensity", Range(0.01, 1)) = 0.3
		_RimSoftness ("Softness", Range(0.01, 1)) = 0.3
		_RimNoiseTex ("Noise", 2D) = "white" {}
//@ </Rim>

//@ <Cube_Color>
		[Header(Cube Color)] [KSFlags(EnableFlag)] [Toggle(KAMAKURA_CUBECOLOR_ON)]
		_EnableCubeColor ("Enable", Float) = 0.0
		[MaterialToggle] _RimUseCubeColor ("Use For Rim", Float) = 0
		[MaterialToggle] _AmbientUseCubeColor ("Use For Ambient", Float) = 0.0
		_CubeColor3 ("Left", Color) = (1, 1, 1, 1)
		_CubeColor1 ("Right", Color) = (1, 1, 1, 1)
		_CubeColor2 ("Front", Color) = (1, 1, 1, 1)
		_CubeColor4 ("Back", Color) = (1, 1, 1, 1)
		_CubeColor0 ("Top", Color) = (1, 1, 1, 1)
		_CubeColor5 ("Down", Color) = (1, 1, 1, 1)
		[PerRendererData] _CubeColorUseLocalSpace ("Use In Local Space", Float) = 0
		[PerRendererData] _CubeColorLocalSpaceMatrixRow0  ("CLSMR0", Vector) = (1, 0, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow1  ("CLSMR1", Vector) = (0, 1, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow2  ("CLSMR2", Vector) = (0, 0, 1, 0)
//@ </Cube_Color>

//@ <Stencil>
		[Header(Stencil)]
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Compare Function", Float) = 0
		_StencilRef ("Stencil Value", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Pass Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Fail Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail Operation", Float) = 0
//@ </Stencil>

//@ <Etc_Hair>
		[Header(ETC)]
		[Toggle] _UsingRightMirroredMesh ("Is Mesh Mirrored Right-to-Left", Float) = 0.0
//@ </Etc_Hair>

//@ <Blend_Mode>
		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
//@ </Blend_Mode>
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		UsePass "Hidden/Kayac/KamakuraPassesStandard/HAIRFORWARD"
		UsePass "Hidden/Kayac/KamakuraPassesStandard/HAIRFORWARDADD"
		UsePass "Hidden/Kayac/KamakuraPassesStandard/OUTLINE"
	}
	FallBack "Diffuse"
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"
}
