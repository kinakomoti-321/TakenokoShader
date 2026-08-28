#ifndef TAKENOKO_STANDARD_FRAGMENT_HLSL
#define TAKENOKO_STANDARD_FRAGMENT_HLSL

#include "../Core/Takenoko_Lightmap.hlsl"
#include "../Core/Takenoko_Hash.hlsl"
#include "../Core/Takenoko_Noise.hlsl"
#include "../Core/Takenoko_Lighting.hlsl"

#include "Takenoko_StandardDefinition.hlsl"
#include "Takenoko_StandardAttribute.hlsl"
#include "Takenoko_StandardMapping.hlsl"
#include "Takenoko_StandardAreaLight.hlsl"

struct MaterialData
{
    float3 basecolor;
    float roughness;
    float metallic;
    float occlusion;
    float3 emission;
    float alpha;

    float3 normalTS;
};

inline MaterialData GetMaterialData(float2 texcoord, float3 positionWS, float3 normalWS)
{
    MaterialData materialData;
    float4 mainTex = Standard_BaseColor(texcoord);
    materialData.basecolor = mainTex.rgb;
    materialData.alpha = mainTex.a;
    materialData.roughness = Standard_Roughness(texcoord);
    materialData.metallic = Standard_Metallic(texcoord);
    materialData.occlusion = Standard_Occlusion(texcoord);
    materialData.emission = Standard_Emission(texcoord);
    materialData.normalTS = Standard_Normal(texcoord);

    return materialData;
}

#define Dielectric_F0 0.04

// VRC Light Volume evaluation
float3 EvaluateLightVolume(in LightingData lighting)
{
    float3 lightVolumeDiffuse = 0.0;
    
    #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON)
        float3 L0, L1r, L1g, L1b;
        float4 occlusion;
        
        // Sample normal light volumes
        LV_LightVolumeSH(lighting.positionWS, L0, L1r, L1g, L1b, occlusion);
        
        // Sample point light volumes
        LV_PointLightVolumeSH(lighting.positionWS, occlusion, L0, L1r, L1g, L1b);
        
        // Evaluate spherical harmonics for current normal
        lightVolumeDiffuse = LV_EvaluateSH(L0, L1r, L1g, L1b, lighting.sn);
        
        // Apply occlusion from light volumes
        lightVolumeDiffuse *= occlusion.rgb;
        
        // Apply user-configurable modulation
        lightVolumeDiffuse *= _LightVolumeColorModulation.rgb * _LightVolumeIntensityMultiplier;
    #endif
    
    return lightVolumeDiffuse;
}

float3 EvaluateSss(in LightingData lighting)
{
    float3 sss = 0.0;
    float backDotNL = saturate(dot(-lighting.sn, lighting.l));
    float3 diffuse = (backDotNL * lighting.lightColor) * lighting.basecolor;
    float thickness = lighting.sssThickness;
    sss = diffuse * smoothstep(0.0, 1.0, 1.0 - thickness) * 0.5;
    return saturate(sss);
}

float3 EvaluateLighting(in LightingData lighting)
{
    float metallic = lighting.metallic;
    float3 F0 = lerp(Dielectric_F0, lighting.basecolor, metallic);

    #if defined(_THINFILM_ON)
        float thickness = lerp(
            _ThinFilmThicknessMin,
            _ThinFilmThicknessMax,
            _ThinFilmThickness);
        lighting.thinFilmIor = _ThinFilmIor;
        lighting.thinFilmThickness = thickness;
    #endif

    // Light
    float3 lightDiffuse = DiffuseBSDF(lighting) * lighting.dotNL * lighting.lightColor;
    float3 lightSpecular = SpecularBSDF(lighting, F0) * lighting.dotNL * lighting.lightColor;

    // GI
    float3 environmentSpecular = 0.0;
    float3 environmentDiffuse = 0.0;

    #if defined(_TAKENOKO_FOWARD_BASE)
        environmentSpecular += SpecularEnvironment(lighting, F0);
        
        #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON) && defined(_LIGHTVOLUME_MODE_REPLACE)
            // Use VRC Light Volume instead of lightmap/SH
            environmentDiffuse += EvaluateLightVolume(lighting) * lighting.basecolor;
        #elif defined(LIGHTMAP_ON)
            environmentDiffuse += EvaluateLightmap(lighting.lightmapUV, lighting.sn, lighting.v) * lighting.basecolor * _MaskLightmap;
        #else
            environmentDiffuse += EvaluateSH(lighting.sn, lighting.positionWS) * lighting.basecolor * _MaskLightProbe;
        #endif
        
        #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON) && defined(_LIGHTVOLUME_MODE_ADDITIVE)
            // Additive Light Volume support (blend with existing lighting)
            environmentDiffuse += EvaluateLightVolume(lighting) * lighting.basecolor * _LightVolumeStrength * _MaskLightProbe;
        #endif
    #endif

    float3 sss = 0.0;
    #if defined(_SSS_ON)
        sss = EvaluateSss(lighting);
    #endif

    // Result
    float3 diffuse = (lightDiffuse + environmentDiffuse) * lighting.occlusion;
    float3 specular = lightSpecular + environmentSpecular;
    float3 emission = lighting.emission;

    #if defined(_SPECULAR_OCCLUSION_ON)
        specular *= lerp(1.0, pow(lighting.occlusion, _SpecularOcclusionPower), _SpecularOcclusionStrength);
        #if defined(_SPECULAR_OCCLUSION_LIGHTMAP_ON)
            specular *= lerp(1.0, pow(saturate(Luminance(environmentDiffuse)), _SpecularOcclusionPower), _SpecularOcclusionStrength);
        #endif
    #endif

    float3 result = specular + (1.0 - metallic) * (diffuse + sss) + emission;
    // result = sss;

    return result;
}

//---------------------------------------------------
// Area Light
//---------------------------------------------------
// #region Area Light
float3 Takenoko_AreaLight(in LightingData lightingData)
{
    #if !defined(_VRC_AREALIGHT_ON)
        return 0.0;
    #else
        const float3 N = lightingData.sn;
        const float3 V = lightingData.v;
        const float3 basecolor = lightingData.basecolor;
        const float3 metallic = lightingData.metallic;
        const float3 F0 = lerp(0.04, lightingData.basecolor, lightingData.metallic);
        const float cosine = lightingData.dotNV;
        const float2 lutUV = GetLtcTexcoord(cosine, lightingData.roughness);
        const float3x3 Minv = SampleLtcInverseMatrix(lutUV, _UdonLtcLut, sampler__UdonLtcLut);
        
        const float3 worldPos = lightingData.positionWS;
        if (_UdonEnableLtcSystem != 1.0)
        {
            return 0.0;
        }

        float3 color = 0.0;
        float2 fresnelUV = lutUV;
        float4 fresnelLut = _UdonFresnelLut.SampleLevel(sampler__UdonLtcLut, fresnelUV, 0);
        float3 fresnel = F0 * fresnelLut.r + (1.0 - F0) * fresnelLut.g;
        float3 specular = 1.0;
        float3 diffuse = 1.0;
        float3 L[4];
        Texture2D lightTexture = _UdonLightTexture1;

        #if defined(_AREA_LIGHT_MASK_ON)
            float3 mask1 = saturate(TAKENOKO_SAMPLE(_AreaLightMask1, lightingData.lightmapUV).xyz * 10.0);
            float2 mask2 = saturate(TAKENOKO_SAMPLE(_AreaLightMask2, lightingData.lightmapUV).xy * 10.0);
        #else
            float3 mask1 = 1.0;
            float2 mask2 = 1.0;
        #endif

        if (mask1.x >= 0.01)
        {
            L[0] = float3(_UdonLightVertex1[0], _UdonLightVertex1[1], _UdonLightVertex1[2]);
            L[1] = float3(_UdonLightVertex1[3], _UdonLightVertex1[4], _UdonLightVertex1[5]);
            L[2] = float3(_UdonLightVertex1[6], _UdonLightVertex1[7], _UdonLightVertex1[8]);
            L[3] = float3(_UdonLightVertex1[9], _UdonLightVertex1[10], _UdonLightVertex1[11]);
            EvaluateAreaLight(N, V, worldPos, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            color += (specular + diffuse * (1.0 - metallic)) * _UdonLightEmission1.xyz * mask1.x * _AreaLightIntensity1;
        }

        lightTexture = _UdonLightTexture2;
        if (mask1.y >= 0.01)
        {
            L[0] = float3(_UdonLightVertex2[0], _UdonLightVertex2[1], _UdonLightVertex2[2]);
            L[1] = float3(_UdonLightVertex2[3], _UdonLightVertex2[4], _UdonLightVertex2[5]);
            L[2] = float3(_UdonLightVertex2[6], _UdonLightVertex2[7], _UdonLightVertex2[8]);
            L[3] = float3(_UdonLightVertex2[9], _UdonLightVertex2[10], _UdonLightVertex2[11]);
            EvaluateAreaLight(N, V, worldPos, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            color += (specular + diffuse * (1.0 - metallic)) * _UdonLightEmission2.xyz * mask1.y * _AreaLightIntensity2;
        }

        lightTexture = _UdonLightTexture3;
        if (mask1.z >= 0.01)
        {
            L[0] = float3(_UdonLightVertex3[0], _UdonLightVertex3[1], _UdonLightVertex3[2]);
            L[1] = float3(_UdonLightVertex3[3], _UdonLightVertex3[4], _UdonLightVertex3[5]);
            L[2] = float3(_UdonLightVertex3[6], _UdonLightVertex3[7], _UdonLightVertex3[8]);
            L[3] = float3(_UdonLightVertex3[9], _UdonLightVertex3[10], _UdonLightVertex3[11]);
            EvaluateAreaLight(N, V, worldPos, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            color += (specular + diffuse * (1.0 - metallic)) * _UdonLightEmission3.xyz * mask1.z * _AreaLightIntensity3;
        }

        lightTexture = _UdonLightTexture4;
        if (mask2.x >= 0.01)
        {
            L[0] = float3(_UdonLightVertex4[0], _UdonLightVertex4[1], _UdonLightVertex4[2]);
            L[1] = float3(_UdonLightVertex4[3], _UdonLightVertex4[4], _UdonLightVertex4[5]);
            L[2] = float3(_UdonLightVertex4[6], _UdonLightVertex4[7], _UdonLightVertex4[8]);
            L[3] = float3(_UdonLightVertex4[9], _UdonLightVertex4[10], _UdonLightVertex4[11]);
            EvaluateAreaLight(N, V, worldPos, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            color += (specular + diffuse * (1.0 - metallic)) * _UdonLightEmission4.xyz * mask2.x * _AreaLightIntensity4;
        }

        lightTexture = _UdonLightTexture5;
        if (mask2.y >= 0.01)
        {
            L[0] = float3(_UdonLightVertex5[0], _UdonLightVertex5[1], _UdonLightVertex5[2]);
            L[1] = float3(_UdonLightVertex5[3], _UdonLightVertex5[4], _UdonLightVertex5[5]);
            L[2] = float3(_UdonLightVertex5[6], _UdonLightVertex5[7], _UdonLightVertex5[8]);
            L[3] = float3(_UdonLightVertex5[9], _UdonLightVertex5[10], _UdonLightVertex5[11]);
            EvaluateAreaLight(N, V, worldPos, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            color += (specular + diffuse * (1.0 - metallic)) * _UdonLightEmission5.xyz * mask2.y * _AreaLightIntensity5;
        }

        return color;

    #endif
}
// #endregion

//---------------------------------------------------
// Fragment
//---------------------------------------------------
// #region Fragment
float4 Takenoko_FragmentStandard(VertexOutput i, bool isFrontFace : SV_ISFRONTFACE) : SV_Target
{
    float2 texcoord = MainTexcoord(i);
    float3 positionWS = i.positionWS;
    float depthWS = length(positionWS - _WorldSpaceCameraPos);

    float3 n = normalize(i.normalWS);
    float3 t = normalize(i.tangentWS.xyz);
    float3x3 tbn = TBNMatrix(t, i.tangentWS.w, n);

    float3 viewWS = normalize(_WorldSpaceCameraPos - positionWS);
    float3 viewTS = mul(tbn, viewWS);
    float2 parallaxOffset = 0.0;
    texcoord += parallaxOffset;

    float wetness = 0.0;
    float3 wetnessNoise = 0.0;

    if (_Wetness > 0.5)
    {
        if (i.positionWS.y < _WetnessHeight)
        {
            wetnessNoise = Cyclic(float3(i.positionWS.xz * 5.0, _Time.y), 1.0, 1.0);
            wetness = saturate((-i.positionWS.y + _WetnessHeight) * 10.0);
            texcoord += wetnessNoise.xy * 0.005 * wetness;
        }
    }

    MaterialData materialData = GetMaterialData(texcoord, i.positionWS, n);

    #if defined(_ALPHATEST_ON)
        clip(materialData.alpha - _Cutoff);
    #endif

    float3 shadingNormal = normalize(mul(materialData.normalTS, tbn));
    if (!isFrontFace)
    {
        shadingNormal = -shadingNormal;
    }
    //---------------------------------------------------
    // Pre-Effect
    //---------------------------------------------------
    if (wetness > 0.0)
    {
        if (i.positionWS.y < _WetnessHeight)
        {
            materialData.basecolor = lerp(materialData.basecolor, materialData.basecolor * _WetnessColor, wetness);
            materialData.roughness = lerp(materialData.roughness, 0.1, wetness);
            shadingNormal = normalize(float3(0, 1, 0) + (2.0 * wetnessNoise - 1.0) * 0.01);
        }
    }

    //---------------------------------------------------
    // Lighting
    //---------------------------------------------------
    UnityLightData lightData = GetUnityLightData(i);

    LightingData lightingData;
    lightingData.positionWS = i.positionWS;
    lightingData.positionOS = i.positionOS;
    lightingData.positionSS = i.positionSS.xy / i.positionSS.w;
    lightingData.sn = shadingNormal;
    lightingData.st = i.tangentWS;
    lightingData.gn = i.normalWS;

    lightingData.l = lightData.direction;
    lightingData.v = normalize(_WorldSpaceCameraPos - i.positionWS);
    lightingData.h = normalize(lightingData.l + lightingData.v);
    lightingData.dotNL = saturate(dot(lightingData.sn, lightingData.l));
    lightingData.dotNV = saturate(dot(lightingData.sn, lightingData.v));
    lightingData.dotHV = saturate(dot(lightingData.h, lightingData.v));
    lightingData.dotNH = saturate(dot(lightingData.sn, lightingData.h));
    lightingData.dotLH = saturate(dot(lightingData.l, lightingData.h));
    lightingData.attenuation = lightData.attenuation;
    lightingData.rawLightColor = lightData.color;
    lightingData.lightColor = lightData.color * lightingData.attenuation;

    lightingData.basecolor = ShiftHSV(materialData.basecolor, float3(_FilterMainTexHueShift, _FilterMainTexSaturateShift, _FilterMainTexValueShift));
    lightingData.roughness = materialData.roughness;
    lightingData.metallic = materialData.metallic;
    lightingData.emission = materialData.emission;
    lightingData.occlusion = materialData.occlusion;
    #if defined(_IRIDESCENCE_ON)
        lightingData.thinFilmIor = _ThinFilmIor;
        lightingData.thinFilmThickness = lerp(_ThinFilmThicknessMin, _ThinFilmThicknessMax, _ThinFilmThickness);
    #endif
    #if defined(_SSS_ON)
        lightingData.sssThickness = saturate(_SssThickness / max(dot(lightingData.sn, lightingData.v), 0.01));
    #endif

    lightingData.texcoord0 = i.texcoord0;
    lightingData.texcoord1 = i.texcoord1;
    lightingData.texcoord2 = i.texcoord2;
    lightingData.texcoord3 = i.texcoord3;

    lightingData.reflUVW = reflect(-lightingData.v, lightingData.sn);
    lightingData.probe = lightData.probe;

    lightingData.lightmapUV = i.lightmapUV;

    //---------------------------------------------------
    // Main Lighting
    //---------------------------------------------------
    float3 result = EvaluateLighting(lightingData);

    //---------------------------------------------------
    // Area Light
    //---------------------------------------------------
    #if defined(_VRC_AREALIGHT_ON) && defined(_TAKENOKO_FOWARD_BASE)
        result += Takenoko_AreaLight(lightingData);
    #endif

    //---------------------------------------------------
    // Post-Effect
    //---------------------------------------------------
    // Color Filter

    // Skybox Filter
    #if defined(_SKYBOXFOG_ON)
        float3 skybox = ReflectionProbe(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0), unity_SpecCube0_HDR, -viewWS, 0);
        float fogFactor = 0.0;
        #if defined(_SKYBOXFOG_DISTANCE)
            float fogDistance = length(_WorldSpaceCameraPos - positionWS);
            fogFactor = smoothstep(100 * _SkyboxFogStrength, _SkyboxFogDistance, fogDistance);
        #elif defined(_SKYBOXFOG_BOX)
            float3 boxMin = _SkyboxFogBoxSizeMin.xyz;
            float3 boxMax = _SkyboxFogBoxSizeMax.xyz;
            float3 boxCenter = (boxMin + boxMax) * 0.5;

            float d = sdBox3D(positionWS - boxCenter, (boxMax - boxMin) * 0.5);
            
            fogFactor = smoothstep(-100 * _SkyboxFogStrength, 0.0, d);

        #elif defined(_SKYBOXFOG_SPHERE)
            float3 sphereCenter = _SkyboxFogSphereCenter.xyz;
            float sphereRadius = _SkyboxFogSphereRadius;
            float d = length(positionWS - sphereCenter) - sphereRadius;

            fogFactor = smoothstep(-100 * _SkyboxFogStrength, 0.0, d);
        #endif

        result = lerp(result, skybox, fogFactor);
    #endif

    
    float4 col = float4(result, materialData.alpha);

    #if defined(_TAKENOKO_FOWARD_ADD)
        UNITY_APPLY_FOG_COLOR(i.fogCoord, col, half4(0, 0, 0, 0));
    #else
        UNITY_APPLY_FOG(i.fogCoord, col);
    #endif


    return col;
}
// #endregion

#endif
