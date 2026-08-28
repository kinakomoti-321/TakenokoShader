#ifndef TAKENOKO_STANDARD_MAPPING_HLSL
#define TAKENOKO_STANDARD_MAPPING_HLSL

#include "../Core/Takenoko_Utils.hlsl"

inline float4 Standard_BaseColor(float2 texcoord)
{
    return TAKENOKO_SAMPLE(_MainTex, texcoord) * _Color;
}

inline float4 Standard_BaseColor(float2 texcoord, float2 dx, float2 dy)
{
    return TAKENOKO_SAMPLE_GRAD(_MainTex, texcoord, dx, dy) * _Color;
}

inline float Standard_Roughness(float2 texcoord)
{
    float roughness = 0;
    #if defined(_ROUGHNESS_CHANNEL_R)
        roughness = (TAKENOKO_SAMPLE(_RoughnessTex, texcoord) * _Roughness).r;
    #elif defined(_ROUGHNESS_CHANNEL_G)
        roughness = (TAKENOKO_SAMPLE(_RoughnessTex, texcoord) * _Roughness).g;
    #elif defined(_ROUGHNESS_CHANNEL_B)
        roughness = (TAKENOKO_SAMPLE(_RoughnessTex, texcoord) * _Roughness).b;
    #elif defined(_ROUGHNESS_CHANNEL_A)
        roughness = (TAKENOKO_SAMPLE(_RoughnessTex, texcoord) * _Roughness).a;
    #endif

    #if defined(_ROUGHNESS_MODEL_SMOOTHNESS)
        roughness = 1.0 - roughness;
    #endif

    return roughness;
}

inline float Standard_Roughness(float2 texcoord, float2 dx, float2 dy)
{
    float roughness = 0;
    #if defined(_ROUGHNESS_CHANNEL_R)
        roughness = (TAKENOKO_SAMPLE_GRAD(_RoughnessTex, texcoord, dx, dy) * _Roughness).r;
    #elif defined(_ROUGHNESS_CHANNEL_G)
        roughness = (TAKENOKO_SAMPLE_GRAD(_RoughnessTex, texcoord, dx, dy) * _Roughness).g;
    #elif defined(_ROUGHNESS_CHANNEL_B)
        roughness = (TAKENOKO_SAMPLE_GRAD(_RoughnessTex, texcoord, dx, dy) * _Roughness).b;
    #elif defined(_ROUGHNESS_CHANNEL_A)
        roughness = (TAKENOKO_SAMPLE_GRAD(_RoughnessTex, texcoord, dx, dy) * _Roughness).a;
    #endif

    #if defined(_ROUGHNESS_MODEL_SMOOTHNESS)
        roughness = 1.0 - roughness;
    #endif

    return roughness;
}

float Standard_Metallic(float2 texcoord)
{
    float metallic = 0;
    #if defined(_METALLIC_CHANNEL_R)
        metallic = (TAKENOKO_SAMPLE(_MetallicTex, texcoord) * _Metallic).r;
    #elif defined(_METALLIC_CHANNEL_G)
        metallic = (TAKENOKO_SAMPLE(_MetallicTex, texcoord) * _Metallic).g;
    #elif defined(_METALLIC_CHANNEL_B)
        metallic = (TAKENOKO_SAMPLE(_MetallicTex, texcoord) * _Metallic).b;
    #elif defined(_METALLIC_CHANNEL_A)
        metallic = (TAKENOKO_SAMPLE(_MetallicTex, texcoord) * _Metallic).a;
    #endif

    return metallic;
}

float Standard_Metallic(float2 texcoord, float2 dx, float2 dy)
{
    float metallic = 0;
    #if defined(_METALLIC_CHANNEL_R)
        metallic = (TAKENOKO_SAMPLE_GRAD(_MetallicTex, texcoord, dx, dy) * _Metallic).r;
    #elif defined(_METALLIC_CHANNEL_G)
        metallic = (TAKENOKO_SAMPLE_GRAD(_MetallicTex, texcoord, dx, dy) * _Metallic).g;
    #elif defined(_METALLIC_CHANNEL_B)
        metallic = (TAKENOKO_SAMPLE_GRAD(_MetallicTex, texcoord, dx, dy) * _Metallic).b;
    #elif defined(_METALLIC_CHANNEL_A)
        metallic = (TAKENOKO_SAMPLE_GRAD(_MetallicTex, texcoord, dx, dy) * _Metallic).a;
    #endif

    return metallic;
}

float Standard_Occlusion(float2 texcoord)
{

    float occlusion = 0;
    #if defined(_OCCLUSION_CHANNEL_R)
        occlusion = (TAKENOKO_SAMPLE(_OcclusionTex, texcoord)).r;
    #elif defined(_OCCLUSION_CHANNEL_G)
        occlusion = (TAKENOKO_SAMPLE(_OcclusionTex, texcoord)).g;
    #elif defined(_OCCLUSION_CHANNEL_B)
        occlusion = (TAKENOKO_SAMPLE(_OcclusionTex, texcoord)).b;
    #elif defined(_OCCLUSION_CHANNEL_A)
        occlusion = (TAKENOKO_SAMPLE(_OcclusionTex, texcoord)).a;
    #endif

    occlusion = pow(occlusion, _OcclusionPower);
    occlusion = lerp(1.0, occlusion, _Occlusion);

    return occlusion;
}

float Standard_Height(float2 texcoord)
{
    float height = 0;
    #if defined(_HEIGHT_CHANNEL_R)
        height = (TAKENOKO_SAMPLE(_HeightTex, texcoord)).r;
    #elif defined(_HEIGHT_CHANNEL_G)
        height = (TAKENOKO_SAMPLE(_HeightTex, texcoord)).g;
    #elif defined(_HEIGHT_CHANNEL_B)
        height = (TAKENOKO_SAMPLE(_HeightTex, texcoord)).b;
    #elif defined(_HEIGHT_CHANNEL_A)
        height = (TAKENOKO_SAMPLE(_HeightTex, texcoord)).a;
    #endif

    return height;
}

float Standard_Height(float2 texcoord, float2 dx, float2 dy)
{
    float height = 0;
    #if defined(_HEIGHT_CHANNEL_R)
        height = (TAKENOKO_SAMPLE_GRAD(_HeightTex, texcoord, dx, dy)).r;
    #elif defined(_HEIGHT_CHANNEL_G)
        height = (TAKENOKO_SAMPLE_GRAD(_HeightTex, texcoord, dx, dy)).g;
    #elif defined(_HEIGHT_CHANNEL_B)
        height = (TAKENOKO_SAMPLE_GRAD(_HeightTex, texcoord, dx, dy)).b;
    #elif defined(_HEIGHT_CHANNEL_A)
        height = (TAKENOKO_SAMPLE_GRAD(_HeightTex, texcoord, dx, dy)).a;
    #endif

    return height;
}

float Standard_Occlusion(float2 texcoord, float2 dx, float2 dy)
{
    float occlusion = 0;
    #if defined(_OCCLUSION_CHANNEL_R)
        occlusion = (TAKENOKO_SAMPLE_GRAD(_OcclusionTex, texcoord, dx, dy)).r;
    #elif defined(_OCCLUSION_CHANNEL_G)
        occlusion = (TAKENOKO_SAMPLE_GRAD(_OcclusionTex, texcoord, dx, dy)).g;
    #elif defined(_OCCLUSION_CHANNEL_B)
        occlusion = (TAKENOKO_SAMPLE_GRAD(_OcclusionTex, texcoord, dx, dy)).b;
    #elif defined(_OCCLUSION_CHANNEL_A)
        occlusion = (TAKENOKO_SAMPLE_GRAD(_OcclusionTex, texcoord, dx, dy)).a;
    #endif

    occlusion = lerp(1.0, occlusion, _Occlusion);

    return occlusion;
}

inline float4 Standard_Emission(float2 texcoord)
{
    float4 emission = 0;
    #if defined(_TAKENOKO_FOWARD_BASE) && defined(_EMISSION_ON)
        emission = _EmissionColor * TAKENOKO_SAMPLE(_EmissionMap, texcoord);
    #endif
    return emission;
}

inline float4 Standard_Emission(float2 texcoord, float2 dx, float2 dy)
{
    float4 emission = 0;
    #if defined(_TAKENOKO_FOWARD_BASE) && defined(_EMISSION_ON)
        emission = _EmissionColor * TAKENOKO_SAMPLE_GRAD(_EmissionMap, texcoord, dx, dy);
    #endif
    return emission;
}

inline float3 Standard_Normal(float2 texcoord)
{
    float3 normalTex = normalize(UnpackScaleNormal(TAKENOKO_SAMPLE(_BumpMap, texcoord), _BumpScale));
    return normalTex;
}

inline float3 Standard_Normal(float2 texcoord, float2 dx, float2 dy)
{
    float3 normalTex = normalize(UnpackScaleNormal(TAKENOKO_SAMPLE_GRAD(_BumpMap, texcoord, dx, dy), _BumpScale));
    return normalTex;
}

inline float2 MainTexcoord(VertexOutput i)
{
    #if defined(_MAINTEX_UV0)
        return i.texcoord0 * float2(_MainTexScale.x, _MainTexScale.y) + float2(_MainTexOffset.x, _MainTexOffset.y);
    #elif defined(_MAINTEX_UV1)
        return i.texcoord1 * float2(_MainTexScale.x, _MainTexScale.y) + float2(_MainTexOffset.x, _MainTexOffset.y);
    #elif defined(_MAINTEX_UV2)
        return i.texcoord2 * float2(_MainTexScale.x, _MainTexScale.y) + float2(_MainTexOffset.x, _MainTexOffset.y);
    #elif defined(_MAINTEX_UV3)
        return i.texcoord3 * float2(_MainTexScale.x, _MainTexScale.y) + float2(_MainTexOffset.x, _MainTexOffset.y);
    #else
        return i.texcoord0 * float2(_MainTexScale.x, _MainTexScale.y) + float2(_MainTexOffset.x, _MainTexOffset.y);
    #endif
}

#endif // _TAKENOKO_STANDARD_TEXTURE_HLSL
