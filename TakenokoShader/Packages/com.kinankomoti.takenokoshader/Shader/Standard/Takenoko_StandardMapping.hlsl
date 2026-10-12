#ifndef TAKENOKO_STANDARD_MAPPING_HLSL
#define TAKENOKO_STANDARD_MAPPING_HLSL

#include "Packages/com.kinankomoti.takenokoshader/Shader/Core/Takenoko_Utils.hlsl"

#if defined(_SECONDTEXTURE_ON)
float2 SecondTexcoord(VertexOutput i)
{
    #if defined(_SECOND_UV1)
        return i.texcoord1;
    #elif defined(_SECOND_UV2)
        return i.texcoord2;
    #elif defined(_SECOND_UV3)
        return i.texcoord3;
    #else
        return i.texcoord0;
    #endif
}

void Standard_SecondTexture(float2 texcoord, float2 maskUV, inout float3 albedo, inout float3 normalTS)
{
    float2 secondUV = texcoord * _SecondAlbedo_ST.xy + _SecondAlbedo_ST.zw;
    // Share the linear clamp sampler for secondary textures.
    float4 maskSample = saturate(_SecondMask.Sample(sampler_LinearClamp, maskUV));
    #if defined(_SECOND_MASK_CHANNEL_G)
        float mask = maskSample.g;
    #elif defined(_SECOND_MASK_CHANNEL_B)
        float mask = maskSample.b;
    #elif defined(_SECOND_MASK_CHANNEL_A)
        float mask = maskSample.a;
    #else
        float mask = maskSample.r;
    #endif
    float3 second = _SecondAlbedo.Sample(sampler_LinearClamp, secondUV).rgb * _SecondColor.rgb;
    float3 blended;
    #if defined(_SECOND_BLEND_REPLACE)
        blended = second;
    #elif defined(_SECOND_BLEND_ADD)
        blended = albedo + second;
    #elif defined(_SECOND_BLEND_OVERLAY)
        blended = lerp(2.0 * albedo * second,
            1.0 - 2.0 * (1.0 - albedo) * (1.0 - second), step(0.5, albedo));
    #else
        blended = albedo * second;
    #endif
    albedo = lerp(albedo, blended, mask);
    float3 detailNormal = UnpackScaleNormal(_SecondNormal.Sample(sampler_LinearClamp, secondUV), _SecondNormalScale);
    detailNormal = normalize(lerp(float3(0.0, 0.0, 1.0), detailNormal, mask));
    normalTS = BlendNormals(normalTS, detailNormal);
}
#endif

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

#if defined(_IRIDESCENCE_ON)
    float Standard_ThinFilmThickness(float2 texcoord)
    {
        float4 thickness = TAKENOKO_SAMPLE_SAMPLER(_ThinFilmThicknessTex, sampler_LinearClamp, texcoord);
        #if defined(_THINFILM_CHANNEL_R)
            return thickness.r;
        #elif defined(_THINFILM_CHANNEL_G)
            return thickness.g;
        #elif defined(_THINFILM_CHANNEL_B)
            return thickness.b;
        #elif defined(_THINFILM_CHANNEL_A)
            return thickness.a;
        #else
            return thickness.r;
        #endif
    }
#endif

#if defined(_SSS_ON)
    float3 Standard_SssColor(float2 texcoord)
    {
        return _SssColor.rgb * TAKENOKO_SAMPLE_SAMPLER(_SssColorTex, sampler_LinearClamp, texcoord).rgb;
    }

    float Standard_SssThickness(float2 texcoord)
    {
        float4 thickness = TAKENOKO_SAMPLE_SAMPLER(_SssThicknessTex, sampler_LinearClamp, texcoord);
        #if defined(_SSS_THICKNESS_CHANNEL_R)
            return thickness.r;
        #elif defined(_SSS_THICKNESS_CHANNEL_G)
            return thickness.g;
        #elif defined(_SSS_THICKNESS_CHANNEL_B)
            return thickness.b;
        #elif defined(_SSS_THICKNESS_CHANNEL_A)
            return thickness.a;
        #else
            return thickness.r;
        #endif
    }
#endif

#if defined(_CLEARCOAT_ON)
    float Standard_Clearcoat(float2 texcoord)
    {
        float4 clearcoat = TAKENOKO_SAMPLE_SAMPLER(_ClearCoatMask, sampler_LinearClamp, texcoord);
        #if defined(_CLEARCOAT_MASK_CHANNEL_R)
            return clearcoat.r;
        #elif defined(_CLEARCOAT_MASK_CHANNEL_G)
            return clearcoat.g;
        #elif defined(_CLEARCOAT_MASK_CHANNEL_B)
            return clearcoat.b;
        #elif defined(_CLEARCOAT_MASK_CHANNEL_A)
            return clearcoat.a;
        #else
            return clearcoat.r;
        #endif
    }
#endif

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
    #if (defined(_TAKENOKO_FOWARD_BASE) || defined(_TAKENOKO_META)) && defined(_EMISSION_ON)
        emission = _EmissionColor * TAKENOKO_SAMPLE(_EmissionMap, texcoord);
    #endif
    return emission;
}

inline float4 Standard_Emission(float2 texcoord, float2 dx, float2 dy)
{
    float4 emission = 0;
    #if (defined(_TAKENOKO_FOWARD_BASE) || defined(_TAKENOKO_META)) && defined(_EMISSION_ON)
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

struct MainTextureMapping
{
    float2 uv;
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        float2 uvX;
        float2 uvY;
        float2 uvZ;
        float3 weights;
        float3 normalWS;
        float3 axisSign;
    #endif
};

MainTextureMapping MainMapping(float2 uv, float3 positionWS, float3 normalWS)
{
    MainTextureMapping m;
    m.uv = uv;
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        m.normalWS = normalize(normalWS);
        m.axisSign = step(0.0, m.normalWS) * 2.0 - 1.0;
        m.weights = pow(abs(m.normalWS), 4.0);
        m.weights /= max(dot(m.weights, 1.0), 1.0e-5);
        m.uvX = float2(-m.axisSign.x * positionWS.z, positionWS.y) * _MainTexScale.xy + _MainTexOffset.xy;
        m.uvY = float2(positionWS.x, -m.axisSign.y * positionWS.z) * _MainTexScale.xy + _MainTexOffset.xy;
        m.uvZ = float2(m.axisSign.z * positionWS.x, positionWS.y) * _MainTexScale.xy + _MainTexOffset.xy;
    #endif
    return m;
}

// Reuse channel selection and texture samplers in the existing UV evaluators.
#if defined(_MAINTEX_TILING_TRIPLANAR)
    #define TAKENOKO_MAPPED_EVALUATOR(type, name) \
        type name(MainTextureMapping m) { return name(m.uvX) * m.weights.x + name(m.uvY) * m.weights.y + name(m.uvZ) * m.weights.z; }
#else
    #define TAKENOKO_MAPPED_EVALUATOR(type, name) \
        type name(MainTextureMapping m) { return name(m.uv); }
#endif

TAKENOKO_MAPPED_EVALUATOR(float4, Standard_BaseColor)
TAKENOKO_MAPPED_EVALUATOR(float, Standard_Roughness)
TAKENOKO_MAPPED_EVALUATOR(float, Standard_Metallic)
TAKENOKO_MAPPED_EVALUATOR(float, Standard_Occlusion)
TAKENOKO_MAPPED_EVALUATOR(float, Standard_Height)
TAKENOKO_MAPPED_EVALUATOR(float4, Standard_Emission)
#if defined(_SSS_ON)
    TAKENOKO_MAPPED_EVALUATOR(float3, Standard_SssColor)
    TAKENOKO_MAPPED_EVALUATOR(float, Standard_SssThickness)
#endif
#if defined(_IRIDESCENCE_ON)
    TAKENOKO_MAPPED_EVALUATOR(float, Standard_ThinFilmThickness)
#endif
#if defined(_CLEARCOAT_ON)
    TAKENOKO_MAPPED_EVALUATOR(float, Standard_Clearcoat)
#endif
#undef TAKENOKO_MAPPED_EVALUATOR

float3 Standard_Normal(MainTextureMapping m, float3x3 tbn)
{
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        float3 nx = Standard_Normal(m.uvX);
        float3 ny = Standard_Normal(m.uvY);
        float3 nz = Standard_Normal(m.uvZ);
        float2 scaleSign = step(0.0, _MainTexScale.xy) * 2.0 - 1.0;
        nx.xy *= scaleSign;
        ny.xy *= scaleSign;
        nz.xy *= scaleSign;
        float3 n = m.normalWS;
        // Whiteout blending preserves the geometric normal for a flat normal map.
        float3 worldX = float3(n.x * nx.z, n.y + nx.y, n.z - m.axisSign.x * nx.x);
        float3 worldY = float3(n.x + ny.x, n.y * ny.z, n.z - m.axisSign.y * ny.y);
        float3 worldZ = float3(n.x + m.axisSign.z * nz.x, n.y + nz.y, n.z * nz.z);
        float3 normalWS = normalize(worldX * m.weights.x + worldY * m.weights.y + worldZ * m.weights.z);
        return normalize(mul(tbn, normalWS));
    #else
        return Standard_Normal(m.uv);
    #endif
}

#endif // _TAKENOKO_STANDARD_TEXTURE_HLSL
