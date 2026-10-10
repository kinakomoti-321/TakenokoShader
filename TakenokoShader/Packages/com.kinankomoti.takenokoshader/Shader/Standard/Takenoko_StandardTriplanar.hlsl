#ifndef TAKENOKO_STANDARD_TRIPLANAR_HLSL
#define TAKENOKO_STANDARD_TRIPLANAR_HLSL

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

#endif
