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
#include "../ThirdParty/Bakery.hlsl"

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

//---------------------------------------------------
// Area Light
//---------------------------------------------------
// #region Area Light
#if defined(_AREALIGHT_ON)
    void Takenoko_AreaLight(float3 N, float3 V, float3 basecolor, float3 F, float roughness, float dotNV, float3 positionWS, float2 lightmapUV, out float3 areaDiffuse, out float3 areaSpecular)
    {
        areaDiffuse = 0.0;
        areaSpecular = 0.0;

        roughness = clamp(roughness, 0.001, 0.95);
        const float2 lutUV = GetLtcTexcoord(dotNV, roughness);
        const float3x3 Minv = SampleLtcInverseMatrix(lutUV, _UdonLtcLut, sampler__UdonLtcLut);
        
        if (_UdonEnableLtcSystem != 1.0)
        {
            return;
        }

        float2 fresnelUV = lutUV;
        float3 fresnel = F;
        float3 specular = 1.0;
        float3 diffuse = 1.0;
        float3 L[4];
        Texture2D lightTexture = _UdonLightTexture1;

        #if defined(_AREA_LIGHT_MASK_ON)
            float4 mask1 = saturate(TAKENOKO_SAMPLE(_AreaLightMask1, lightmapUV) * 10.0);
            float4 mask2 = saturate(TAKENOKO_SAMPLE(_AreaLightMask2, lightmapUV) * 10.0);
        #else
            float4 mask1 = 1.0;
            float4 mask2 = 1.0;
        #endif

        if (mask1.x >= 0.01)
        {
            L[0] = float3(_UdonLightVertex1[0], _UdonLightVertex1[1], _UdonLightVertex1[2]);
            L[1] = float3(_UdonLightVertex1[3], _UdonLightVertex1[4], _UdonLightVertex1[5]);
            L[2] = float3(_UdonLightVertex1[6], _UdonLightVertex1[7], _UdonLightVertex1[8]);
            L[3] = float3(_UdonLightVertex1[9], _UdonLightVertex1[10], _UdonLightVertex1[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission1.xyz * mask1.x * _AreaLightIntensity1;
            areaDiffuse += diffuse * _UdonLightEmission1.xyz * mask1.x * _AreaLightIntensity1;
        }

        lightTexture = _UdonLightTexture2;
        if (mask1.y >= 0.01)
        {
            L[0] = float3(_UdonLightVertex2[0], _UdonLightVertex2[1], _UdonLightVertex2[2]);
            L[1] = float3(_UdonLightVertex2[3], _UdonLightVertex2[4], _UdonLightVertex2[5]);
            L[2] = float3(_UdonLightVertex2[6], _UdonLightVertex2[7], _UdonLightVertex2[8]);
            L[3] = float3(_UdonLightVertex2[9], _UdonLightVertex2[10], _UdonLightVertex2[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission2.xyz * mask1.y * _AreaLightIntensity2;
            areaDiffuse += diffuse * _UdonLightEmission2.xyz * mask1.y * _AreaLightIntensity2;
        }

        lightTexture = _UdonLightTexture3;
        if (mask1.z >= 0.01)
        {
            L[0] = float3(_UdonLightVertex3[0], _UdonLightVertex3[1], _UdonLightVertex3[2]);
            L[1] = float3(_UdonLightVertex3[3], _UdonLightVertex3[4], _UdonLightVertex3[5]);
            L[2] = float3(_UdonLightVertex3[6], _UdonLightVertex3[7], _UdonLightVertex3[8]);
            L[3] = float3(_UdonLightVertex3[9], _UdonLightVertex3[10], _UdonLightVertex3[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission3.xyz * mask1.z * _AreaLightIntensity3;
            areaDiffuse += diffuse * _UdonLightEmission3.xyz * mask1.z * _AreaLightIntensity3;
        }

        lightTexture = _UdonLightTexture4;
        if (mask1.w >= 0.01)
        {
            L[0] = float3(_UdonLightVertex4[0], _UdonLightVertex4[1], _UdonLightVertex4[2]);
            L[1] = float3(_UdonLightVertex4[3], _UdonLightVertex4[4], _UdonLightVertex4[5]);
            L[2] = float3(_UdonLightVertex4[6], _UdonLightVertex4[7], _UdonLightVertex4[8]);
            L[3] = float3(_UdonLightVertex4[9], _UdonLightVertex4[10], _UdonLightVertex4[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission4.xyz * mask1.w * _AreaLightIntensity4;
            areaDiffuse += diffuse * _UdonLightEmission4.xyz * mask1.w * _AreaLightIntensity4;
        }

        lightTexture = _UdonLightTexture5;
        if (mask2.x >= 0.01)
        {
            L[0] = float3(_UdonLightVertex5[0], _UdonLightVertex5[1], _UdonLightVertex5[2]);
            L[1] = float3(_UdonLightVertex5[3], _UdonLightVertex5[4], _UdonLightVertex5[5]);
            L[2] = float3(_UdonLightVertex5[6], _UdonLightVertex5[7], _UdonLightVertex5[8]);
            L[3] = float3(_UdonLightVertex5[9], _UdonLightVertex5[10], _UdonLightVertex5[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission5.xyz * mask2.x * _AreaLightIntensity5;
            areaDiffuse += diffuse * _UdonLightEmission5.xyz * mask2.x * _AreaLightIntensity5;
        }

        lightTexture = _UdonLightTexture6;
        if (mask2.y >= 0.01)
        {
            L[0] = float3(_UdonLightVertex6[0], _UdonLightVertex6[1], _UdonLightVertex6[2]);
            L[1] = float3(_UdonLightVertex6[3], _UdonLightVertex6[4], _UdonLightVertex6[5]);
            L[2] = float3(_UdonLightVertex6[6], _UdonLightVertex6[7], _UdonLightVertex6[8]);
            L[3] = float3(_UdonLightVertex6[9], _UdonLightVertex6[10], _UdonLightVertex6[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission6.xyz * mask2.y * _AreaLightIntensity6;
            areaDiffuse += diffuse * _UdonLightEmission6.xyz * mask2.y * _AreaLightIntensity6;
        }

        lightTexture = _UdonLightTexture7;
        if (mask2.z >= 0.01)
        {
            L[0] = float3(_UdonLightVertex7[0], _UdonLightVertex7[1], _UdonLightVertex7[2]);
            L[1] = float3(_UdonLightVertex7[3], _UdonLightVertex7[4], _UdonLightVertex7[5]);
            L[2] = float3(_UdonLightVertex7[6], _UdonLightVertex7[7], _UdonLightVertex7[8]);
            L[3] = float3(_UdonLightVertex7[9], _UdonLightVertex7[10], _UdonLightVertex7[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission7.xyz * mask2.z * _AreaLightIntensity7;
            areaDiffuse += diffuse * _UdonLightEmission7.xyz * mask2.z * _AreaLightIntensity7;
        }

        lightTexture = _UdonLightTexture8;
        if (mask2.w >= 0.01)
        {
            L[0] = float3(_UdonLightVertex8[0], _UdonLightVertex8[1], _UdonLightVertex8[2]);
            L[1] = float3(_UdonLightVertex8[3], _UdonLightVertex8[4], _UdonLightVertex8[5]);
            L[2] = float3(_UdonLightVertex8[6], _UdonLightVertex8[7], _UdonLightVertex8[8]);
            L[3] = float3(_UdonLightVertex8[9], _UdonLightVertex8[10], _UdonLightVertex8[11]);
            EvaluateAreaLight(N, V, positionWS, L, Minv, lightTexture, sampler__UdonLtcLut, diffuse, specular);
            specular *= fresnel;
            diffuse *= basecolor;
            areaSpecular += specular * _UdonLightEmission8.xyz * mask2.w * _AreaLightIntensity8;
            areaDiffuse += diffuse * _UdonLightEmission8.xyz * mask2.w * _AreaLightIntensity8;
        }
    }
#endif
// #endregion

//---------------------------------------------------
// Fragment
//---------------------------------------------------
// #region Fragment
float4 Takenoko_FragmentStandard(VertexOutput i, bool isFrontFace : SV_ISFRONTFACE) : SV_Target
{
    float2 texcoord = MainTexcoord(i);
    float3 positionWS = i.positionWS;

    float3 n = normalize(i.normalWS);
    float3 t = normalize(i.tangentWS.xyz);
    float3x3 tbn = TBNMatrix(t, i.tangentWS.w, n);

    float3 viewWS = normalize(_WorldSpaceCameraPos - positionWS);
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

    float3 lightDirection = lightData.direction;
    float3 viewDirection = normalize(_WorldSpaceCameraPos - i.positionWS);
    float3 halfVector = normalize(lightDirection + viewDirection);
    float dotNL = saturate(dot(shadingNormal, lightDirection));
    float dotNV = saturate(dot(shadingNormal, viewDirection));
    float dotNH = saturate(dot(shadingNormal, halfVector));
    float dotLH = saturate(dot(lightDirection, halfVector));
    float attenuation = lightData.attenuation;
    float3 lightColor = lightData.color * attenuation;

    float3 basecolor = ShiftHSV(materialData.basecolor, float3(_FilterMainTexHueShift, _FilterMainTexSaturateShift, _FilterMainTexValueShift));
    float roughness = materialData.roughness;
    float metallic = materialData.metallic;
    float occlusion = materialData.occlusion;

    float3 F0 = lerp(0.04, basecolor, metallic);
    float3 F = ShlickFresnel(F0, dotNV);
    #if defined(_IRIDESCENCE_ON)
        float thinFilmIor = _ThinFilmIor;
        float thinFilmThickness = lerp(_ThinFilmThicknessMin, _ThinFilmThicknessMax, _ThinFilmThickness * Standard_ThinFilmThickness(i.texcoord0));
        if (thinFilmThickness > 1.0)
        {
            float3 bottomIor;
            float3 bottomKappa;
            ColorToComplexIor(clamp(F0, 0.0, 0.95), clamp(ShlickFresnel(F0, 0.01), 0.0, 0.95), bottomIor, bottomKappa);
            bottomIor = lerp(1.0, bottomIor, metallic);
            bottomKappa = lerp(0.01, max(bottomKappa, 0.01), metallic);

            float3 IridescenceF = IridescenceFresnel(dotLH, thinFilmThickness, 1.0, thinFilmIor, bottomIor, bottomKappa);
            F = lerp(F, saturate(IridescenceF), smoothstep(0.0, 1.0, saturate(thinFilmThickness / 50.0)));
        }
        else
        {
            F = ShlickFresnel(F0, dotNV);
        }
        // F = IridescenceF;
    #endif

    float3 lightDiffuse = DiffuseBRDF(basecolor, roughness, dotNH, dotNV, dotNL) * dotNL * lightColor;
    float3 lightSpecular = SpecularBRDF(F, roughness, dotNH, dotNV, dotNL) * dotNL * lightColor;
    float3 lightSss = 0.0;
    #if defined(_SSS_ON)
        float sssThickness = saturate((_SssThickness * Standard_SssThickness(texcoord)) / max(dot(shadingNormal, viewDirection), 0.01));
        float backDotNL = saturate(dot(-shadingNormal, lightDirection));
        float3 sssDiffuse = (backDotNL * lightColor) * basecolor;
        lightSss = saturate(sssDiffuse * smoothstep(0.0, 1.0, 1.0 - sssThickness) * 0.5);
    #endif
    float3 lightCoat = 0.0;
    #if defined(_CLEARCOAT_ON)
        float clearcoat = saturate(_ClearCoatStrength * Standard_Clearcoat(texcoord));
        lightCoat += SpecularBRDF(ShlickFresnel(0.04, dotNV), 0.25, dotNH, dotNV, dotNL) * dotNL * lightColor * clearcoat;
    #endif

    float3 environmentSpecular = 0.0;
    float3 environmentDiffuse = 0.0;
    float3 environmentCoat = 0.0;
    float3 reflUVW = reflect(-viewDirection, shadingNormal);

    #if defined(_TAKENOKO_FOWARD_BASE)
        environmentSpecular += SpecularEnvironment(F, roughness, reflUVW, lightData.probe, positionWS, dotNV);

        #if defined(_CLEARCOAT_ON)
            environmentCoat += SpecularEnvironment(ShlickFresnel(0.04, dotNV), 0.25, reflUVW, lightData.probe, positionWS, dotNV) * clearcoat;
        #endif

        #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON)
            float3 lightVolumeDiffuse = 0.0;
            float3 L0, L1r, L1g, L1b;
            float4 lightVolumeOcclusion;

            LV_LightVolumeSH(positionWS, L0, L1r, L1g, L1b, lightVolumeOcclusion);
            LV_PointLightVolumeSH(positionWS, lightVolumeOcclusion, L0, L1r, L1g, L1b);

            lightVolumeDiffuse = LV_EvaluateSH(L0, L1r, L1g, L1b, shadingNormal);
            lightVolumeDiffuse *= lightVolumeOcclusion.rgb;
            lightVolumeDiffuse *= _LightVolumeColorModulation.rgb * _LightVolumeIntensityMultiplier;
        #endif

        #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON) && defined(_LIGHTVOLUME_MODE_REPLACE)
            // Use VRC Light Volume instead of lightmap/SH
            environmentDiffuse += lightVolumeDiffuse * basecolor;
        #elif defined(LIGHTMAP_ON)
            float3 lightmapDiffuse = 0.0;
            float3 lightmapDirection = 0.0;

            EvaluateLightmap(i.lightmapUV, shadingNormal, materialData.normalTS, tbn, lightmapDiffuse, lightmapDirection);
            environmentDiffuse += EvaluateLightmapDiffuse(_AdditionalLightmap1, i.lightmapUV) * _AdditionalLightmapStrength1;
            environmentDiffuse += EvaluateLightmapDiffuse(_AdditionalLightmap2, i.lightmapUV) * _AdditionalLightmapStrength2;
            environmentDiffuse += EvaluateLightmapDiffuse(_AdditionalLightmap3, i.lightmapUV) * _AdditionalLightmapStrength3;
            environmentDiffuse += EvaluateLightmapDiffuse(_AdditionalLightmap4, i.lightmapUV) * _AdditionalLightmapStrength4;
            environmentDiffuse += lightmapDiffuse * basecolor * _MaskLightmap;

        #else
            environmentDiffuse += EvaluateSH(shadingNormal, positionWS) * basecolor * _MaskLightProbe;
        #endif

        #if defined(LIGHTVOLUME_SUPPORT) && defined(_LIGHTVOLUME_ON) && defined(_LIGHTVOLUME_MODE_ADDITIVE)
            // Additive Light Volume support (blend with existing lighting)
            environmentDiffuse += lightVolumeDiffuse * basecolor * _LightVolumeStrength * _MaskLightProbe;
        #endif
    #endif

    //---------------------------------------------------
    // Area Light
    //---------------------------------------------------
    float3 areaDiffuse = 0.0;
    float3 areaSpecular = 0.0;
    #if defined(_AREALIGHT_ON)
        Takenoko_AreaLight(shadingNormal, viewDirection, basecolor, F, roughness, dotNV, positionWS, i.lightmapUV, areaDiffuse, areaSpecular);
    #endif

    //---------------------------------------------------
    // Main Lighting
    //---------------------------------------------------
    float3 diffuse = (lightDiffuse + environmentDiffuse + areaDiffuse) * occlusion;
    float3 specular = lightSpecular + environmentSpecular + areaSpecular;
    float3 coat = lightCoat + environmentCoat;
    float3 sss = lightSss;
    float3 emission = materialData.emission;

    #if defined(_SPECULAR_OCCLUSION_ON)
        specular *= lerp(1.0, pow(occlusion, _SpecularOcclusionPower), _SpecularOcclusionStrength);
        #if defined(_SPECULAR_OCCLUSION_LIGHTMAP_ON)
            specular *= lerp(1.0, pow(saturate(Luminance(environmentDiffuse)), _SpecularOcclusionPower), _SpecularOcclusionStrength);
        #endif
    #endif

    float3 result = ((diffuse + sss) * (1.0 - metallic) + specular) + coat + emission;


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
