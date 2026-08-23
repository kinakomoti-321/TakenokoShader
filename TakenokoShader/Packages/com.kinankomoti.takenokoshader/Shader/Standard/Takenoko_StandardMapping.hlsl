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

// Hex-Tiling
// https://github.com/mmikk/hextile-demo/tree/main

void TriangleGrid(float2 texcoord, inout float w1, inout float w2, inout float w3,
inout int2 vert1, inout int2 vert2, inout int2 vert3)
{
    float2 st = texcoord * 2.0 * sqrt(3.0);
    const float2x2 gridToSkewedGrid = float2x2(1.0, -0.57735027, 0.0, 1.15470054);
    float2 skewedCoord = mul(gridToSkewedGrid, st);

    int2 baseId = int2(floor(skewedCoord));
    float3 temp = float3(frac(skewedCoord), 0);
    temp.z = 1.0 - temp.x - temp.y;
    
    float s = step(0.0, -temp.z);
    float s2 = 2 * s - 1;

    w1 = -temp.z * s2;
    w2 = s - temp.y * s2;
    w3 = s - temp.x * s2;

    vert1 = baseId + int2(s, s);
    vert2 = baseId + int2(s, 1 - s);
    vert3 = baseId + int2(1 - s, s);
}

float2 MakeCenST(int2 vertex)
{
    float2x2 invSkewMat = float2x2(1.0, 0.5, 0.0, 1.0 / 1.15470054);

    return mul(invSkewMat, vertex) / (2 * sqrt(3));
}

float2x2 LoadRot2x2(int2 idx, float rotStrength)
{
    float angle = abs(idx.x * idx.y) + abs(idx.x + idx.y) + TAKE_PI;

    // remap to +/-pi
    angle = fmod(angle, 2 * TAKE_PI);
    if (angle < 0) angle += 2 * TAKE_PI;
    if (angle > TAKE_PI) angle -= 2 * TAKE_PI;

    angle *= rotStrength;

    float cs = cos(angle), si = sin(angle);

    return float2x2(cs, -si, si, cs);
}

float3 Gain3(float3 x, float r)
{
    // increase contrast when r>0.5 and
    // reduce contrast if less
    float k = log(1 - r) / log(0.5);

    float3 s = 2 * step(0.5, x);
    float3 m = 2 * (1 - s);

    float3 res = 0.5 * s + 0.25 * m * pow(max(0.0, s + x * m), k);
    
    return res.xyz / (res.x + res.y + res.z);
}

float2 TspaceNormalToDerivative(float3 vM)
{
    // const float scale = 1.0 / 128.0;
    const float scale = 1.0 / 1024.0;

    // Ensure vM delivers a positive third component using abs() and
    // constrain vM.z so the range of the derivative is [-128; 128].
    const float3 vMa = abs(vM);
    const float z_ma = max(vMa.z, scale * max(vMa.x, vMa.y));

    // Set to match positive vertical texture coordinate axis.
    const bool gFlipVertDeriv = false;
    const float s = gFlipVertDeriv ? - 1.0 : 1.0;
    return -float2(vM.x, s * vM.y) / z_ma;
}

inline float3 LuminanceWeight(float3 hexWeight, float3 c1, float3 c2, float3 c3, float fallOff, float expo, float edgeSmoothness)
{
    float3 Lw = float3(0.299, 0.587, 0.114);
    float3 Dw = float3(dot(c1.xyz, Lw), dot(c2.xyz, Lw), dot(c3.xyz, Lw));
    Dw = lerp(1.0, Dw, fallOff);

    float3 W = Dw * pow(hexWeight, expo);
    W /= (W.x + W.y + W.z);

    if (edgeSmoothness != 0.5) W = Gain3(W, edgeSmoothness);

    return W;
}

inline float3 HeightSlopeWeight(float3 hexWeight, float2 dh1, float2 dh2, float2 dh3, float fallOff, float expo, float edgeSmoothness)
{
    float3 nD = float3(dot(dh1, dh1), dot(dh2, dh2), dot(dh3, dh3));
    float3 nDw = sqrt(nD / (1.0 + nD));
    nDw = lerp(1.0, nDw, fallOff);

    float3 nW = nDw * pow(hexWeight, expo);
    nW /= (nW.x + nW.y + nW.z);

    if (edgeSmoothness != 0.5) nW = Gain3(nW, edgeSmoothness);
    
    return nW;
}

// Reference : https://github.com/MochiesCode/Mochies-Unity-Shaders/
inline float2 ParallaxOffset(float2 uv, float3 viewTS, float step)
{
    #if defined(_HEIGHT_ON)
        float strength = 1.0;
        float3 result = 0;
        int stepIndex = 0;
        int numSteps = int(_HeightStep);
        float layerHeight = 1.0 / numSteps;
        float2 plane = _HeightStrength * (viewTS.xy / viewTS.z);
        float2 deltaTex = -plane * layerHeight;

        float2 prevTexOffset = 0;
        float prevRayZ = 1.0f;
        float prevHeight = 0.0f;

        float2 currTexOffset = deltaTex;
        float currRayZ = 1.0f - layerHeight;
        float currHeight = 0.0f;

        for (int i = 0; i < numSteps; i++)
        {
            currHeight = Standard_Height(uv + currTexOffset) + _HeightOffset;
            if (currHeight > currRayZ)
            {
                stepIndex = numSteps + 1;
            }
            else
            {
                prevTexOffset = currTexOffset;
                prevRayZ = currRayZ;
                prevHeight = currHeight;
                currTexOffset += deltaTex;
                currRayZ -= layerHeight;
            }
        }

        int numSubStep = int(_HeightSubStep);

        // Binary Search
        float2 midTexOffset = 0;
        float rayStep = layerHeight;
        for (int i = 0; i < numSubStep; i++)
        {
            // it assumes linear interpolation height, and estimates a hit point.
            float estimation = (prevHeight - prevRayZ) / (prevHeight - currHeight + currRayZ - prevRayZ);
            midTexOffset = prevTexOffset +estimation * deltaTex;
            float midRayZ = prevRayZ - estimation * rayStep;
            float midHeight = Standard_Height(uv + midTexOffset) + _HeightOffset;
            if (midHeight > midRayZ)
            {
                // hit
                currTexOffset = midTexOffset;
                currRayZ = midRayZ;
                currHeight = midHeight;
                rayStep *= estimation;
                deltaTex *= estimation;
            }
            else
            {
                prevTexOffset = midTexOffset;
                prevRayZ = midRayZ;
                prevHeight = midHeight;
                rayStep *= (1.0 - estimation);
                deltaTex *= (1.0 - estimation);
            }
        }
        float2 dx = ddx(midTexOffset);
        float2 dy = ddy(midTexOffset);

        return midTexOffset;
    #else
        return 0.0;
    #endif
}


#endif // _TAKENOKO_STANDARD_TEXTURE_HLSL
