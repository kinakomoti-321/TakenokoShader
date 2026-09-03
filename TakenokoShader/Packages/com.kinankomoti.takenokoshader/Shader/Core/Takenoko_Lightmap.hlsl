#ifndef _TAKENOKO_STANDARD_LIGHTMAP_HLSL
#define _TAKENOKO_STANDARD_LIGHTMAP_HLSL

#include "Takenoko_Utils.hlsl"
#include "Takenoko_Lighting.hlsl"
#include "../ThirdParty/Bakery.hlsl"

SamplerState sampler_LinearClamp;

void EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 normalTS, float3x3 tbn, out float3 diffuse, out float3 direction)
{
    float3 directionWS;
    Bakery_EvaluateLightmap(lightmapUV, normalWS, normalTS, tbn, diffuse, directionWS);

    // Unlit texels give a zero direction, so guard the divide instead of normalizing.
    float directionLength = length(directionWS);
    direction = directionLength > 1.0e-6 ? directionWS / directionLength : 0.0;
}

float3 EvaluateLightmapDiffuse(Texture2D lightmap, float2 lightmapUV)
{
    return DecodeLightmap(lightmap.Sample(sampler_LinearClamp, lightmapUV)).xyz;
}

#endif
