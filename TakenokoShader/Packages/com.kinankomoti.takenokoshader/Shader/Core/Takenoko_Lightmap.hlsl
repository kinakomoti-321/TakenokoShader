#ifndef _TAKENOKO_STANDARD_LIGHTMAP_HLSL
#define _TAKENOKO_STANDARD_LIGHTMAP_HLSL

#include "Takenoko_Utils.hlsl"
#include "Takenoko_Lighting.hlsl"
#include "../ThirdParty/Bakery.hlsl"

SamplerState sampler_LinearClamp;

void EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 normalTS, float3x3 tbn, out float3 diffuse, out float3 direction)
{
    diffuse = 0.0;
    direction = 0.0;

    float3 lightmapDiffuse;
    float3 directionTS;
    Bakery_EvaluateLightmap(lightmapUV, normalWS, normalTS, lightmapDiffuse, directionTS);
    diffuse = lightmapDiffuse;
    direction = normalize(mul(directionTS, tbn));
}

float3 EvaluateLightmapDiffuse(Texture2D lightmap, float2 lightmapUV)
{
    return DecodeLightmap(lightmap.Sample(sampler_LinearClamp, lightmapUV)).xyz;
}

#endif
