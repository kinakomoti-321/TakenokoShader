#ifndef _TAKENOKO_STANDARD_LIGHTING_HLSL
#define _TAKENOKO_STANDARD_LIGHTING_HLSL

#include "Takenoko_Lightmap.hlsl"

//--------------------------------------------
// Unity Lighting
//--------------------------------------------
struct RefProbeData
{
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION) || defined(UNITY_ENABLE_REFLECTION_BUFFERS)
        float4 boxMin[2];
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        float4 boxMax[2];
        float4 probePosition[2];
    #endif
    float4 probeHDR[2];
};

struct UnityLightData
{
    float3 direction;
    float3 color;
    float attenuation;

    RefProbeData probe;
};

inline float LightAttenuation(in VertexOutput v)
{
    UNITY_LIGHT_ATTENUATION(attenuation, v, v.positionWS);
    return attenuation;
}

inline float3 LightDirection(float3 positionWS)
{
    return Unity_SafeNormalize(UnityWorldSpaceLightDir(positionWS));
}

UnityLightData GetUnityLightData(in VertexOutput v)
{
    UnityLightData lightData;
    lightData.direction = LightDirection(v.positionWS);
    lightData.attenuation = LightAttenuation(v);
    lightData.color = _LightColor0.rgb;

    lightData.probe.probeHDR[0] = unity_SpecCube0_HDR;
    lightData.probe.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
        lightData.probe.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        lightData.probe.boxMin[1] = unity_SpecCube1_BoxMin;
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        lightData.probe.boxMax[0] = unity_SpecCube0_BoxMax;
        lightData.probe.probePosition[0] = unity_SpecCube0_ProbePosition;
        lightData.probe.boxMax[1] = unity_SpecCube1_BoxMax;
        lightData.probe.boxMin[1] = unity_SpecCube1_BoxMin;
        lightData.probe.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    return lightData;
}

//--------------------------------------------
// Lighting Evaluation
//--------------------------------------------
inline float3 Mix(float3 a, float3 b, float f)
{
    return lerp(a, b, f);
}

inline float3 Layer(float3 a, float3 b)
{
    return a + b;
}

float3 EvaluateSH(float3 normalWS, float3 positionWS)
{
    float3 sh;
    sh = ShadeSH9(half4(normalWS, 1.0));

    return sh;
}


inline float3 DiffuseBRDF(float3 basecolor, float roughness, float dotLH, float dotNL, float dotNV)
{
    // Unity Standard uses DisneyDiffuse
    float fd90 = 0.5 + 2.0 * dotLH * dotLH * roughness;
    float lightScatter = 1.0 + (fd90 - 1.0) * pow(1.0 - dotNL, 5.0);
    float viewScatter = 1.0 + (fd90 - 1.0) * pow(1.0 - dotNV, 5.0);
    return basecolor * lightScatter * viewScatter;
}

// Shlick Fresnel Approximation
float3 ShlickFresnel(float3 F0, float cosine)
{
    float x = saturate(1.0 - cosine);
    return F0 + (1.0 - F0) * x * x * x * x * x;
}

float3 ShlickFresnel(float3 F0, float3 F90, float cosine)
{
    float x = saturate(1.0 - cosine);
    return lerp(F0, F90, x * x * x * x * x);
}

// Filament
// https://google.github.io/filament/Filament.html#materialsystem/specularbrdf
float DIsoGGX(float dotNH, float alpha)
{
    float a = dotNH * alpha;
    float k = alpha / (1.0 - dotNH * dotNH + a * a);
    return k * k * (1.0 / TAKE_PI);
}

// Visibility Function V
// V = G / 4(<n,v> <n,l>)
float VFastIsoGGX(float dotNV, float dotNL, float alpha)
{
    float a2 = alpha * alpha;
    float GGXV = dotNL * sqrt(dotNV * dotNV * (1.0 - a2) + a2);
    float GGXL = dotNV * sqrt(dotNL * dotNL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

// Microfacet BRDF
// f_{microfacet}(l, v) =  D G F / 4(<n,v> <n,l>) = D V F
inline float3 SpecularBRDF(float3 F, float roughness, float dotNH, float dotNV, float dotNL)
{
    float alpha = clamp(roughness * roughness, 0.0001, 1.0);

    float D = DIsoGGX(dotNH, alpha);
    float V = VFastIsoGGX(clamp(dotNV, 0.01, 1.0), clamp(dotNL, 0.01, 1.0), alpha); // avoid nan

    float3 specular = (D * V * TAKE_PI) * F;

    return specular;

    // TODO : Geometry AA
    // TODO : Energy Conservation

}

inline float3 GlossyEnvironment(UNITY_ARGS_TEXCUBE(tex), half4 hdr, float roughness, float3 reflUVW)
{
    half perceptualRoughness = roughness;

    perceptualRoughness = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness);

    half mip = perceptualRoughness * UNITY_SPECCUBE_LOD_STEPS;
    half3 R = reflUVW;
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, R, mip);

    return DecodeHDR(rgbm, hdr);
}

inline float3 ReflectionProbe(UNITY_ARGS_TEXCUBE(tex), half4 hdr, float3 direction, float miplevel)
{
    half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, direction, miplevel);
    return DecodeHDR(rgbm, hdr);
}

inline float3 IndirectSpecular(float roughness, float3 reflUVW, RefProbeData probe, float3 positionWS)
{
    float3 specular;

    float3 originalReflUVW = reflUVW;
    
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        reflUVW = BoxProjectedCubemapDirection(originalReflUVW, positionWS, probe.probePosition[0], probe.boxMin[0], probe.boxMax[0]);
    #endif
    
    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        half3 env0 = GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), probe.probeHDR[0], roughness, reflUVW);
        #ifdef UNITY_SPECCUBE_BLENDING
            const float kBlendFactor = 0.99999;
            float blendLerp = probe.boxMin[0].w;
            UNITY_BRANCH
            if (blendLerp < kBlendFactor)
            {
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    reflUVW = BoxProjectedCubemapDirection(originalReflUVW, positionWS, probe.probePosition[1], probe.boxMin[1], probe.boxMax[1]);
                #endif
                
                half3 env1 = GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), probe.probeHDR[1], roughness, reflUVW);
                specular = lerp(env1, env0, blendLerp);
            }
            else
            {
                specular = env0;
            }
        #else
            specular = env0;
        #endif
    #endif
    
    return specular;
}

void ColorToComplexIor(float3 r, float3 g, inout float3 n, inout float3 k)
{
    r = clamp(r, 0.0, 0.99);
    g = saturate(g);

    float3 sqrtR = sqrt(r);
    float3 nMin = (1.0 - r) / (1.0 + r);
    float3 nMax = (1.0 + sqrtR) / (1.0 - sqrtR);

    n = lerp(nMax, nMin, g);

    float3 k2 = (r * (n + 1.0) * (n + 1.0) - (n - 1.0) * (n - 1.0)) / (1.0 - r);
    k = sqrt(max(k2, 0.0));
}

inline float3 SpecularEnvironment(float3 F0, float roughness, float3 reflUVW, RefProbeData probe, float3 positionWS, float dotNV)
{
    // Unity BSDF
    float roughnessSq = roughness * roughness;
    float perceptualRoughness = roughness;
    float smoothness = 1.0 - perceptualRoughness;

    half surfaceReduction;
    #ifdef UNITY_COLORSPACE_GAMMA
        surfaceReduction = 1.0 - 0.28 * roughnessSq * perceptualRoughness;
    #else
        surfaceReduction = 1.0 / (roughnessSq + 1.0);
    #endif

    float3 grazingTerm = saturate(smoothness + F0);

    float3 fresnel = ShlickFresnel(F0, grazingTerm, dotNV);
    return IndirectSpecular(roughness, reflUVW, probe, positionWS) * surfaceReduction * fresnel;
}

// A Practical Extension to Microfacet Theory for the Modeling of Varying Iridescence
float2 FresnelPhase(float cosTheta, float eta1, float eta2, float kappa2)
{
    float sinThetaSq = 1.0 - cosTheta * cosTheta;
    float A = square(eta2) * (1.0 - square(kappa2)) - square(eta1) * sinThetaSq;
    float B = sqrt(square(A) + square(2 * square(eta2) * kappa2));
    float U = sqrt((A + B) / 2.0);
    float V = sqrt((B - A) / 2.0);

    float phiS = atan2(2 * eta1 * V * cosTheta, square(U) + square(V) - square(eta1 * cosTheta));
    float phiP = atan2(2 * eta1 * square(eta2) * cosTheta * (2 * kappa2 * U - (1.0 - square(kappa2)) * V),
    square(square(eta2) * (1.0 + square(kappa2)) * cosTheta) - square(eta1) * (square(U) + square(V)));

    return float2(phiS, phiP);
}

float3 EvalSensitivity(float opd, float3 shift)
{
    float phase = 2 * TAKE_PI * opd * 1.0e-9;
    float3 val = float3(5.4856e-13, 4.4201e-13, 5.2481e-13);
    float3 pos = float3(1.6810e6, 1.7953e6, 2.2084e6);
    float3 var = float3(4.3278e9, 9.3046e9, 6.6121e9);

    float3 xyz = val * sqrt(2 * TAKE_PI * var) * cos(pos * phase + shift) * exp(-phase * phase * var);
    xyz.x += 9.7470e-14 * sqrt(2 * TAKE_PI * 4.5282e9) * cos(2.2399e6 * phase + shift.x) * exp(-4.5282e9 * phase * phase);
    return xyz / 1.0685e-7;
}

float2 FresnelConductorExact(float cosTheta, float eta, float kappa)
{
    cosTheta = saturate(cosTheta);

    float cosThetaSq = square(cosTheta);
    float sinThetaSq = 1.0 - cosThetaSq;
    float temp = square(eta) - square(kappa) - sinThetaSq;
    float a2PlusB2 = sqrt(square(temp) + 4.0 * square(eta) * square(kappa));
    float a = sqrt(max(0.5 * (a2PlusB2 + temp), 0.0));

    float term1 = a2PlusB2 + cosThetaSq;
    float term2 = 2.0 * a * cosTheta;
    float Rs = (term1 - term2) / max(term1 + term2, 1.0e-6);

    float term3 = a2PlusB2 * cosThetaSq + square(sinThetaSq);
    float term4 = term2 * sinThetaSq;
    float Rp = Rs * (term3 - term4) / max(term3 + term4, 1.0e-6);

    return saturate(float2(Rs, Rp));
}

inline float3 IridescenceFresnel(float ct1, float height, float eta1, float eta2, float3 eta3, float3 kappa3)
{
    ct1 = saturate(ct1);

    float scale = eta1 / eta2;
    float ct2Sq = 1.0 - (1.0 - square(ct1)) * square(scale);

    if (ct2Sq <= 0.0)
        return 1.0;

    float ct2 = sqrt(ct2Sq);

    float2 R12 = FresnelConductorExact(ct1, eta2 / eta1, 0.0);
    float2 T121 = 1.0 - R12;

    float2 R23R = FresnelConductorExact(ct2, eta3.r / eta2, kappa3.r / eta2);
    float2 R23G = FresnelConductorExact(ct2, eta3.g / eta2, kappa3.g / eta2);
    float2 R23B = FresnelConductorExact(ct2, eta3.b / eta2, kappa3.b / eta2);

    float3 R23s = float3(R23R.x, R23G.x, R23B.x);
    float3 R23p = float3(R23R.y, R23G.y, R23B.y);

    float2 phi12 = FresnelPhase(ct1, eta1, eta2, 0.0);
    float phi21s = TAKE_PI - phi12.x;
    float phi21p = TAKE_PI - phi12.y;

    float2 phi23R = FresnelPhase(ct2, eta2, eta3.r, kappa3.r);
    float2 phi23G = FresnelPhase(ct2, eta2, eta3.g, kappa3.g);
    float2 phi23B = FresnelPhase(ct2, eta2, eta3.b, kappa3.b);

    float3 phi23s = float3(phi23R.x, phi23G.x, phi23B.x);
    float3 phi23p = float3(phi23R.y, phi23G.y, phi23B.y);

    float D = 2.0 * eta2 * height * ct2;

    float3 r123s = sqrt(max(R12.x * R23s, 0.0));
    float3 r123p = sqrt(max(R12.y * R23p, 0.0));

    float3 I = 0.0;

    float3 RstarP = square(T121.y) * R23p / max(1.0 - R12.y * R23p, 1.0e-6);
    I += R12.y + RstarP;

    float3 Cm = RstarP - T121.y;

    [unroll]
    for (int m = 1; m <= 3; ++m)
    {
        Cm *= r123p;
        float3 Sm = 2.0 * EvalSensitivity((float)m * D, (float)m * (phi23p + phi21p));
        I += Cm * Sm;
    }

    float3 RstarS = square(T121.x) * R23s / max(1.0 - R12.x * R23s, 1.0e-6);
    I += R12.x + RstarS;

    Cm = RstarS - T121.x;

    [unroll]
    for (int m = 1; m <= 3; ++m)
    {
        Cm *= r123s;
        float3 Sm = 2.0 * EvalSensitivity((float)m * D, (float)m * (phi23s + phi21s));
        I += Cm * Sm;
    }

    I *= 0.5;

    float3 rgb;
    rgb.r = 2.3646381 * I.x - 0.8965361 * I.y - 0.4680737 * I.z;
    rgb.g = -0.5151664 * I.x + 1.4264000 * I.y + 0.0887608 * I.z;
    rgb.b = 0.0052037 * I.x - 0.0144081 * I.y + 1.0092106 * I.z;

    return max(rgb, 0.0);
}


#endif
