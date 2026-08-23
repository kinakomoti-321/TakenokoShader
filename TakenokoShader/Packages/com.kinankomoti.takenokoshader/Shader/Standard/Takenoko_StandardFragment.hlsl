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

inline MaterialData HexTiling(float2 texcoord)
{
    float w1 = 0.0, w2 = 0.0, w3 = 0.0;
    int2 vert1, vert2, vert3;
    TriangleGrid(texcoord, w1, w2, w3, vert1, vert2, vert3);

    float rotStrength = _HexRotationStrength;
    float2x2 rot1 = LoadRot2x2(vert1, rotStrength);
    float2x2 rot2 = LoadRot2x2(vert2, rotStrength);
    float2x2 rot3 = LoadRot2x2(vert3, rotStrength);

    float2 cen1 = MakeCenST(vert1);
    float2 cen2 = MakeCenST(vert2);
    float2 cen3 = MakeCenST(vert3);

    float hashScale = 1.0;
    float2 uv1 = mul(texcoord - cen1, rot1) + cen1 + hash22(float2(vert1)) * hashScale;
    float2 uv2 = mul(texcoord - cen2, rot2) + cen2 + hash22(float2(vert2)) * hashScale;
    float2 uv3 = mul(texcoord - cen3, rot3) + cen3 + hash22(float2(vert3)) * hashScale;

    float2 dx = ddx(texcoord);
    float2 dy = ddy(texcoord);

    float3 c1 = Standard_BaseColor(uv1, mul(dx, rot1), mul(dy, rot1));
    float3 c2 = Standard_BaseColor(uv2, mul(dx, rot2), mul(dy, rot2));
    float3 c3 = Standard_BaseColor(uv3, mul(dx, rot3), mul(dy, rot3));

    float fallOff = _HexFallOff;
    float ex = _HexExponent;
    float edgeSmoothness = _HexEdgeSmoothness;
    float3 hexWeight = float3(w1, w2, w3);
    
    float3 W = LuminanceWeight(hexWeight, c1, c2, c3, fallOff, ex, edgeSmoothness);
    
    float3 basecolor = W.x * c1 + W.y * c2 + W.z * c3;

    float o1 = Standard_Occlusion(uv1, mul(rot1, dx), mul(rot1, dy));
    float o2 = Standard_Occlusion(uv2, mul(rot2, dx), mul(rot2, dy));
    float o3 = Standard_Occlusion(uv3, mul(rot3, dx), mul(rot3, dy));

    float occlusion = W.x * o1 + W.y * o2 + W.z * o3;

    float r1 = Standard_Roughness(uv1, mul(rot1, dx), mul(rot1, dy));
    float r2 = Standard_Roughness(uv2, mul(rot2, dx), mul(rot2, dy));
    float r3 = Standard_Roughness(uv3, mul(rot3, dx), mul(rot3, dy));

    float roughness = W.x * r1 + W.y * r2 + W.z * r3;

    float m1 = Standard_Metallic(uv1, mul(rot1, dx), mul(rot1, dy));
    float m2 = Standard_Metallic(uv2, mul(rot2, dx), mul(rot2, dy));
    float m3 = Standard_Metallic(uv3, mul(rot3, dx), mul(rot3, dy));

    float metallic = W.x * m1 + W.y * m2 + W.z * m3;

    float3 e1 = Standard_Emission(uv1, mul(dx, rot1), mul(dy, rot1));
    float3 e2 = Standard_Emission(uv2, mul(dx, rot2), mul(dy, rot2));
    float3 e3 = Standard_Emission(uv3, mul(dx, rot3), mul(dy, rot3));

    float3 emission = W.x * e1 + W.y * e2 + W.z * e3;

    // Hex Artifact Umm...
    float2 n1 = TspaceNormalToDerivative(Standard_Normal(uv1, mul(dx, rot1), mul(dy, rot1)));
    float2 n2 = TspaceNormalToDerivative(Standard_Normal(uv2, mul(dx, rot2), mul(dy, rot2)));
    float2 n3 = TspaceNormalToDerivative(Standard_Normal(uv3, mul(dx, rot3), mul(dy, rot3)));

    float2 d1 = mul(transpose(rot1), n1);
    float2 d2 = mul(transpose(rot2), n2);
    float2 d3 = mul(transpose(rot3), n3);

    float3 nW = HeightSlopeWeight(hexWeight, d1, d2, d3, fallOff, ex, edgeSmoothness);
    float2 hdiv = nW.x * d1 + nW.y * d2 + nW.z * d3;

    float3 normal = normalize(float3(-hdiv, 1.0));

    MaterialData materialData;
    materialData.basecolor = basecolor;
    materialData.roughness = roughness;
    materialData.metallic = metallic;
    materialData.occlusion = occlusion;
    materialData.emission = emission;
    materialData.normalTS = normal;

    return materialData;
}

inline MaterialData GetMaterialData(float2 texcoord, float3 positionWS, float3 normalWS)
{
    #if defined(_MAINTEX_TILING_HEX)
        return HexTiling(texcoord);
    #else
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
    #endif
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

    float3 result = specular + (1.0 - metallic) * diffuse + emission;

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
    float2 parallaxOffset = ParallaxOffset(texcoord, viewTS, 1.0);
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
    lightingData.dotNL = max(0.0, dot(lightingData.sn, lightingData.l));
    lightingData.dotNV = max(0.0, dot(lightingData.sn, lightingData.v));
    lightingData.dotHV = max(0.0, dot(lightingData.h, lightingData.v));
    lightingData.dotNH = max(0.0, dot(lightingData.sn, lightingData.h));
    lightingData.dotLH = max(0.0, dot(lightingData.l, lightingData.h));
    lightingData.attenuation = lightData.attenuation;
    lightingData.rawLightColor = lightData.color;
    lightingData.lightColor = lightData.color * lightingData.attenuation;

    lightingData.basecolor = ShiftHSV(materialData.basecolor, float3(_FilterMainTexHueShift, _FilterMainTexSaturateShift, _FilterMainTexValueShift));
    lightingData.roughness = materialData.roughness;
    lightingData.metallic = materialData.metallic;
    lightingData.emission = materialData.emission;
    lightingData.occlusion = materialData.occlusion;

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
