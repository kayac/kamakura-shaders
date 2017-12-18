Shader "Kayac/Kamakura-Hair"
{
	Properties
	{
		[Header(About)]
		[KamakuraShaderVersion] _ShaderVersion ("_ShaderVersion", Vector) = (1, 0, 0, -1)

		[Header(Basic)]
		_DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		[KamakuraBlendMode] _BlendMode ("Blend Mode", Float) = 0.0

		[Header(Filter)]
		[Toggle] _EnableFilter ("Enable Filter", Float) = 0.0
		_FilterHue("Hue", Range(-0.5,0.5)) = 0.0
		_FilterSaturation("Saturation", Float) = 1.0
		_FilterBrightness("Brightness", Float) = 1.0
		_FilterContrast("Contrast", Float) = 1.0
		_FilterContrastMidPoint("Contrast Mid Point", Float) = 0.5

		[Header(Specular)]
		_HairSpecShift ("Local Shift", 2D) = "gray" {}
		_SpecularShiftIntensity ("Local Shift Strength", Float) = 1.0
		[NoScaleOffset] _SpecularMap ("Mask", 2D) = "white" {}

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

		[Header(Light Ramp)]
		[NoScaleOffset] _LightRampTex("Light Ramp", 2D) = "white" {}
		_LightRampPresetsOffset("Light Ramp Preset", Range(0.0, 1.0)) = 0.5
		_LightRampOffset ("Light Ramp Offset", Range(-1.0, 1.0)) = 0.0
		[MaterialToggle] _LightRampUseGVertexColor ("Shift Light Ramp Using VertexColor (G)", Float) = 0.0

		// Shadow parameters
		[Header(Shadow)]
		[Toggle(KAMAKURA_SHADOWMOD_ON)] _EnableShadowMod ("Enable Shadow", Float) = 0.0
		[NoScaleOffset] _ShadowModTex ("Shadow Texture", 2D) = "white" {}
		_ShadowModColor ("Shadow Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowModIntensity ("Shadow Intensity", Float) = 1
		[MaterialToggle] _ShadowModUseFilter ("Use Filter for shadow texture", Float) = 0.0


		[Header(Ambient)]
		_AmbientColor ("Ambient Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_AmbientIntensity ("Ambient Intensity", Float) = 0.5
		_AmbientUnitySHIntensity ("Unity's Ambient Intensity", Float) = 0.0

		[Header(Outline)]
		[Toggle(KAMAKURA_OUTLINE_ON)] _EnableOutline ("Enable Outline", Float) = 0.0
		_OutlineColor ("Outline Color", Color) = (0.1, 0.1, 0.1, 1)
		_OutlineBlendColorTexture ("Blend Color-Texture Value", Range(0.0, 1.0)) = 1.0
		[ScaledRangeParam(0.0, 1.0)]_OutlineSize ("Outline Thickness", Range(0.0, 0.05)) = 0.01
		_OutlineCameraDistanceAdaptRate ("Adapt to Camera Distance Value", Range(0.0, 1.0)) = 1.0
		[MaterialToggle] _OutlineWriteZ ("Enable Inner-side Outline", Float) = 1.0
		[MaterialToggle] _OutlineUseRVertexColor ("Adjust Outline Using VertexColor (R)", Float) = 0

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
		[KeywordEnum(Additive, Blend)]_RimBlendingMode ("Blending Mode", Float) = 0.0
		_RimColor ("Rim Color", Color) = (1, 1, 1, 1)
		_RimSize ("Rim Size", Range(0, 1)) = 0.3
		_RimIntensity ("Rim Intensity", Range(0.01, 1)) = 0.3
		_RimSoftness ("Rim Softness", Range(0.01, 1)) = 0.3
		_RimNoiseTex ("Rim Noise", 2D) = "white" {}

		[Header(Cube Color)]
		[Toggle] _EnableCubeColor ("Enable Cube Color", Float) = 0.0
		[MaterialToggle] _RimUseCubeColor ("Use For Rim", Float) = 0
		[MaterialToggle] _AmbientUseCubeColor ("Use For Ambient", Float) = 0.0
		// Cube color parameters
		_CubeColor3 ("Left", Color) = (1, 1, 1, 1)
		_CubeColor1 ("Right", Color) = (1, 1, 1, 1)
		_CubeColor2 ("Front", Color) = (1, 1, 1, 1)
		_CubeColor4 ("Back", Color) = (1, 1, 1, 1)
		_CubeColor0 ("Top", Color) = (1, 1, 1, 1)
		_CubeColor5 ("Down", Color) = (1, 1, 1, 1)

		// Cube color local space matrix
		[PerRendererData] _CubeColorUseLocalSpace ("Use In Local Space", Float) = 0
		[PerRendererData] _CubeColorLocalSpaceMatrixRow0  ("CLSMR0", Vector) = (1, 0, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow1  ("CLSMR1", Vector) = (0, 1, 0, 0)
		[PerRendererData] _CubeColorLocalSpaceMatrixRow2  ("CLSMR2", Vector) = (0, 0, 1, 0)

		[Header(Stencil)]
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Compare Function", Float) = 0
		_StencilRef ("Stencil Value", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Pass Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Fail Operation", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail Operation", Float) = 0

		[Header(ETC)]
		[Toggle] _UsingRightMirroredMesh ("Is Mesh Mirrored Right-to-Left", Float) = 0.0


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

		UsePass "Hidden/Kayac/KamakuraPasses/OUTLINE"
		UsePass "Hidden/Kayac/KamakuraPasses/HAIRFORWARD"
		UsePass "Hidden/Kayac/KamakuraPasses/HAIRFORWARDADD"
	}
	FallBack "Diffuse"
	CustomEditor "Kayac.VisualArts.KamakuraShaderGUI"
}
