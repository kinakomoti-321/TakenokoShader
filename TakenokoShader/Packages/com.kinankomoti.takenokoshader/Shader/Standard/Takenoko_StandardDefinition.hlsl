#ifndef TAKENOKO_STANDARD_DEFINE_HLSL
#define TAKENOKO_STANDARD_DEFINE_HLSL

#include "../Core/Takenoko_Utils.hlsl"

//--------------------------------------------
// Shader Properties
//--------------------------------------------
float _Cutoff;

float4 _Color;
TAKENOKO_TEXTURE2D(_MainTex)

float _Roughness;
TAKENOKO_TEXTURE2D(_RoughnessTex);

float _Metallic;
TAKENOKO_TEXTURE2D(_MetallicTex);

float _BumpScale;
TAKENOKO_TEXTURE2D(_BumpMap);

float4 _EmissionColor; // name Unity expectes
TAKENOKO_TEXTURE2D(_EmissionMap); // name Unity expectes

float _Occlusion;
TAKENOKO_TEXTURE2D(_OcclusionTex);
float _OcclusionPower;

float4 _MainTexOffset;
float4 _MainTexScale;

float _HexRotationStrength;
float _HexFallOff;
float _HexExponent;
float _HexEdgeSmoothness;

TAKENOKO_TEXTURE2D(_HeightTex);
float _HeightStrength;
float _HeightOffset;
float _HeightStep;
float _HeightSubStep;

#if defined(_THINFILM_ON)
    float _ThinFilmThickness;
    TAKENOKO_TEXTURE2D(_ThinFilmThicknessTex);
    float _ThinFilmThicknessMin;
    float _ThinFilmThicknessMax;
    float _ThinFilmIor;
#endif

float _SpecularOcclusionStrength;
float _SpecularOcclusionPower;

float _MaskLightProbe;
float _MaskLightmap;

float _FilterMainTexHueShift;
float _FilterMainTexSaturateShift;
float _FilterMainTexValueShift;

float _FilterResultHueShift;
float _FilterResultSaturateShift;
float _FilterResultValueShift;

float _Wetness;
float3 _WetnessColor;
float _WetnessHeight;

float _SkyboxFog;
float _SkyboxFogStrength;
float _SkyboxFogDistance;
float4 _SkyboxFogBoxSizeMin;
float4 _SkyboxFogBoxSizeMax;
float4 _SkyboxFogSphereCenter;
float _SkyboxFogSphereRadius;

#if defined(_VRC_AREALIGHT_ON)
    #if defined(_AREA_LIGHT_MASK_ON)
        TAKENOKO_TEXTURE2D(_AreaLightMask1);
        TAKENOKO_TEXTURE2D(_AreaLightMask2);
    #endif

    float _AreaLightIntensity1;
    float _AreaLightIntensity2;
    float _AreaLightIntensity3;
    float _AreaLightIntensity4;
    float _AreaLightIntensity5;
#endif

//--------------------------------------------
// VRC Light Volume Properties
//--------------------------------------------

#if defined(LIGHTVOLUME_SUPPORT)
    // Light Volume strength for additive blending
    float _LightVolumeStrength;
    
    // Light Volume color modulation
    float4 _LightVolumeColorModulation;
    
    // Light Volume intensity multiplier
    float _LightVolumeIntensityMultiplier;
#endif

#endif
