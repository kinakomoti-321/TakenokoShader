#ifndef _TAKENOKO_STANDARD_LIGHTMAP_HLSL
#define _TAKENOKO_STANDARD_LIGHTMAP_HLSL

#include "Takenoko_Utils.hlsl"

float3 EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 viewWS)
{
    // Normal
    float3 diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;
    return diffuse;
}

#endif