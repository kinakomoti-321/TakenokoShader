#ifndef _TAKENOKO_STANDARD_LIGHTING_HLSL
#define _TAKENOKO_STANDARD_LIGHTING_HLSL

#include "Takenoko_Lightmap.hlsl"

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

struct LightingData
{
    float3 l; // Light Direction
    float3 v; // View Direction
    float3 h; // Half Vector
    float3 sn; // Shading Normal
    float3 st; // Shading Tangent
    float3 gn; // Geometry Normal

    float dotNL; // dot(n, l)
    float dotNV; // dot(n, v)
    float dotHV; // dot(h, v)
    float dotNH; // dot(n, h)
    float dotLH; // dot(l, h)

    // Reflection Probe
    float3 reflUVW;
    RefProbeData probe;

    // Lighting Parameters
    float attenuation;
    float3 rawLightColor;
    float3 lightColor; // lightColor = rawLightColor * attenuation;

    float3 basecolor;
    float3 roughness; // smoothness is 1.0 - roughness
    float3 metallic;
    float3 emission;
    #if defined(_THINFILM_ON)
        float thinFilmThickness;
        float thinFilmIor;
    #endif

    float occlusion;


    // LightMap
    float2 texcoord0;
    float2 texcoord1;
    float2 texcoord2;
    float2 texcoord3;
    float2 lightmapUV;


    // Geometry
    float3 positionWS;
    float2 positionSS;
    float3 positionOS;
};

//--------------------------------------------
// Unity Lighting
//--------------------------------------------
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
float3 EvaluateSH(float3 normalWS, float3 positionWS)
{
    // TODO : VRC Volume Light
    float3 sh;
    sh = ShadeSH9(half4(normalWS, 1.0));

    return sh;
}

inline float3 DiffuseBSDF(in LightingData lighting)
{
    // Unity Standard uses DisneyDiffuse
    float fd90 = 0.5 + 2.0 * lighting.dotLH * lighting.dotLH * lighting.roughness;
    float lightScatter = 1.0 + (fd90 - 1.0) * pow(1.0 - lighting.dotNL, 5.0);
    float viewScatter = 1.0 + (fd90 - 1.0) * pow(1.0 - lighting.dotNV, 5.0);
    return lighting.basecolor * lightScatter * viewScatter;
}


// Shlick Fresnel Approximation
float3 SlickFresnel(float3 F0, float cosine)
{
    float x = saturate(1.0 - cosine);
    return F0 + (1.0 - F0) * x * x * x * x * x;
}

float3 SlickFresnel(float3 F0, float3 F90, float cosine)
{
    float x = saturate(1.0 - cosine);
    return lerp(F0, F90, x * x * x * x * x);
}

inline float3 ThinFilmSensitivity(float opd, float phase)
{
    const float3 gaussianValue = float3(5.4856e-13, 4.4201e-13, 5.2481e-13);
    const float3 gaussianPosition = float3(1.6810e06, 1.7953e06, 2.2084e06);
    const float3 gaussianVariance = float3(4.3278e09, 9.3046e09, 6.6121e09);

    const float spectralPhase = 2.0 * UNITY_PI * opd * 1.0e-9;
    float3 xyz = gaussianValue * sqrt(2.0 * UNITY_PI * gaussianVariance) *
    cos(gaussianPosition * spectralPhase + phase) *
    exp(-gaussianVariance * spectralPhase * spectralPhase);

    xyz.x += 9.7470e-14 * sqrt(2.0 * UNITY_PI * 4.5282e09) *
    cos(2.2399e06 * spectralPhase + phase) *
    exp(-4.5282e09 * spectralPhase * spectralPhase);

    const float3 rgb = float3(
        2.3706743 * xyz.x - 0.9000405 * xyz.y - 0.4706338 * xyz.z,
        - 0.5138850 * xyz.x + 1.4253036 * xyz.y + 0.0885814 * xyz.z,
        0.0052982 * xyz.x - 0.0146949 * xyz.y + 1.0093968 * xyz.z);

    return rgb / 1.0685e-7;
}

inline float3 ThinFilmFresnel(
    in LightingData lighting,
    float3 baseF0)
{
    #if defined(_THINFILM_ON)
        const float etaFilm = max(lighting.thinFilmIor, 1.0001);
        const float cosTheta1 = saturate(lighting.dotHV);
        const float sinTheta1Sqr = 1.0 - cosTheta1 * cosTheta1;
        const float cosTheta2Sqr = 1.0 - sinTheta1Sqr / (etaFilm * etaFilm);

        if (cosTheta2Sqr <= 0.0)
        {
            return 1.0;
        }

        const float cosTheta2 = sqrt(cosTheta2Sqr);
        const float filmF0 = ((1.0 - etaFilm) / (1.0 + etaFilm)) *
        ((1.0 - etaFilm) / (1.0 + etaFilm));

        const float3 R12 = SlickFresnel(filmF0.xxx, cosTheta1);
        const float3 T12 = 1.0 - R12;
        const float3 R23 = SlickFresnel(saturate(baseF0), cosTheta2);
        const float3 r123 = sqrt(max(R12 * R23, 0.0));
        const float3 Rs = T12 * T12 * R23 / max(1.0 - R12 * R23, 1.0e-4);

        // baseF0 does not contain the lower-interface phase information.
        const float phase = UNITY_PI;
        const float opd = 2.0 * etaFilm * lighting.thinFilmThickness * cosTheta2;

        const float3 sensitivity0 = ThinFilmSensitivity(0.0, 0.0);
        float3 result = R12 + Rs;
        float3 interference = Rs - T12;

        for (int m = 1; m <= 2; ++m)
        {
            interference *= r123;
            const float3 sensitivity = ThinFilmSensitivity(opd * m, phase * m);
            result += 2.0 * interference * sensitivity / max(sensitivity0, 1.0e-4);
        }

        return saturate(result);
    #else
        return saturate(baseF0);
    #endif
}

float3 Fresnel(in LightingData lighting, float cosine, float3 F0)
{
    #if defined(_THINFILM_ON)
        return ThinFilmFresnel(lighting, F0);
    #else
        return SlickFresnel(F0, cosine);
    #endif
}

// Filament
// https://google.github.io/filament/Filament.html#materialsystem/specularbrdf
float D_Iso_GGX(float dotNH, float alpha)
{
    float a = dotNH * alpha;
    float k = alpha / (1.0 - dotNH * dotNH + a * a);
    return k * k * (1.0 / TAKE_PI);
}

// Visibility Function V
// V = G / 4(<n,v> <n,l>)
float V_Fast_Iso_GGX(float dotNV, float dotNL, float alpha)
{
    float a2 = alpha * alpha;
    float GGXV = dotNL * sqrt(dotNV * dotNV * (1.0 - a2) + a2);
    float GGXL = dotNV * sqrt(dotNL * dotNL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

// Microfacet BRDF
// f_{microfacet}(l, v) =  D G F / 4(<n,v> <n,l>) = D V F
inline float3 SpecularBSDF(in LightingData lighting, float3 F0)
{
    float alpha = clamp(lighting.roughness * lighting.roughness, 0.0001, 1.0);

    float D = D_Iso_GGX(lighting.dotNH, alpha);
    float V = V_Fast_Iso_GGX(clamp(lighting.dotNV, 0.01, 1.0), clamp(lighting.dotNL, 0.01, 1.0), alpha); // avoid nan
    #if defined(_THINFILM_ON)
        float3 F = ThinFilmFresnel(lighting, F0);
    #else
        float3 F = SlickFresnel(F0, lighting.dotHV);
    #endif

    float3 specular = (D * V * UNITY_PI) * F;

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

inline float3 IndirectSpecular(in LightingData lighting)
{
    float3 specular;

    float3 reflUVW = lighting.reflUVW;
    
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        half3 originalReflUVW = lighting.reflUVW;
        reflUVW = BoxProjectedCubemapDirection(originalReflUVW, lighting.positionWS, lighting.probe.probePosition[0], lighting.probe.boxMin[0], lighting.probe.boxMax[0]);
    #endif
    
    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        half3 env0 = GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), lighting.probe.probeHDR[0], lighting.roughness, reflUVW);
        #ifdef UNITY_SPECCUBE_BLENDING
            const float kBlendFactor = 0.99999;
            float blendLerp = lighting.probe.boxMin[0].w;
            UNITY_BRANCH
            if (blendLerp < kBlendFactor)
            {
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    reflUVW = BoxProjectedCubemapDirection(originalReflUVW, lighting.positionWS, lighting.probe.probePosition[1], lighting.probe.boxMin[1], lighting.probe.boxMax[1]);
                #endif
                
                half3 env1 = GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), lighting.probe.probeHDR[1], lighting.roughness, reflUVW);
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
    
    return specular * lighting.occlusion;
}

inline float3 SpecularEnvironment(in LightingData lighting, float3 F0)
{
    // Unity BSDF
    float roughness = lighting.roughness * lighting.roughness;
    float perceptualRoughness = lighting.roughness;
    float smoothness = 1.0 - perceptualRoughness;

    half surfaceReduction;
    #ifdef UNITY_COLORSPACE_GAMMA
        surfaceReduction = 1.0 - 0.28 * roughness * perceptualRoughness;
    #else
        surfaceReduction = 1.0 / (roughness * roughness + 1.0);
    #endif

    float3 grazingTerm = saturate(smoothness + F0);

    //float3 fresnel = SlickFresnel(F0, lighting.dotNV);
    float3 fresnel = SlickFresnel(F0, grazingTerm, lighting.dotNV);
    return IndirectSpecular(lighting) * surfaceReduction * fresnel;
}

#endif
