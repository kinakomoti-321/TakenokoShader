#ifndef _TAKENOKO_STANDARD_LIGHTMAP_HLSL
#define _TAKENOKO_STANDARD_LIGHTMAP_HLSL

#include "Packages/com.kinankomoti.takenokoshader/Shader/Core/Takenoko_Utils.hlsl"
#include "Packages/com.kinankomoti.takenokoshader/Shader/Core/Takenoko_Lighting.hlsl"
#include "Packages/com.kinankomoti.takenokoshader/Shader/ThirdParty/Bakery.hlsl"

void EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 normalTS, float3x3 tbn, out float3 diffuse, out ShDirectionalLight directionalLight)
{
    Bakery_EvaluateLightmap(lightmapUV, normalWS, normalTS, tbn, diffuse, directionalLight);
}

float3 EvaluateLightmapDiffuse(Texture2D lightmap, float2 lightmapUV)
{
    return DecodeLightmap(lightmap.Sample(sampler_LinearClamp, lightmapUV)).xyz;
}

#endif
