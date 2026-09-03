#ifndef TAKENOKO_BAKERY_HLSL
#define TAKENOKO_BAKERY_HLSL

sampler2D _RNM0;
sampler2D _RNM1;
sampler2D _RNM2;
float4 _RNM0_TexelSize;

float3 Bakery_EvaluateSHDiffuse(float3 L0, float3 L1x, float3 L1y, float3 L1z, float3 normalWS)
{
    float3 linearSH = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;

    #if defined(_LIGHTMAP_SH) || defined(_LIGHTMAP_MONOSH)
        float3 R1 = 0.5 * float3(length(L1x), length(L1y), length(L1z));
        float directionality = saturate(length(R1) / max(dot(L0, 1.0), 1.0e-4));
        float3 dominant = float3(dot(L1x, float3(0.2125, 0.7154, 0.0721)),
        dot(L1y, float3(0.2125, 0.7154, 0.0721)),
        dot(L1z, float3(0.2125, 0.7154, 0.0721)));
        float q = dot(normalize(dominant + 1.0e-5), normalWS) * 0.5 + 0.5;
        float p = 1.0 + 2.0 * directionality;
        float a = (1.0 - directionality) / (1.0 + directionality);
        float nonlinearLuma = dot(L0, 1.0) * (a + (1.0 - a) * (p + 1.0) * pow(q, p));
        float linearLuma = max(dot(linearSH, 1.0), 1.0e-4);
        linearSH *= nonlinearLuma / linearLuma;
    #endif

    return max(linearSH, 0.0);
}

void Bakery_EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 normalTS,
out float3 diffuse, out float3 specularDirectionTS)
{
    diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;
    specularDirectionTS = 0.0;

    #if defined(_LIGHTMAP_RNM)
    {
        const float3 basis0 = float3(0.81649658, 0.0, 0.57735027);
        const float3 basis1 = float3(-0.40824829, 0.70710678, 0.57735027);
        const float3 basis2 = float3(-0.40824829, -0.70710678, 0.57735027);

        float3 rnm0 = DecodeLightmap(tex2D(_RNM0, lightmapUV));
        float3 rnm1 = DecodeLightmap(tex2D(_RNM1, lightmapUV));
        float3 rnm2 = DecodeLightmap(tex2D(_RNM2, lightmapUV));
        diffuse = saturate(dot(basis0, normalTS)) * rnm0
        + saturate(dot(basis1, normalTS)) * rnm1
        + saturate(dot(basis2, normalTS)) * rnm2;

        float3 directionTS = basis0 * dot(rnm0, float3(0.2125, 0.7154, 0.0721))
        + basis1 * dot(rnm1, float3(0.2125, 0.7154, 0.0721))
        + basis2 * dot(rnm2, float3(0.2125, 0.7154, 0.0721));
        specularDirectionTS = directionTS;
    #elif defined(_LIGHTMAP_SH) || defined(_LIGHTMAP_MONOSH)

        float3 directionData;
        #if defined(_LIGHTMAP_MONOSH)
            directionData = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lightmapUV).xyz * 2.0 - 1.0;
            float3 L0 = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;
            float3 L1x = directionData.x * L0 * 2.0;
            float3 L1y = directionData.y * L0 * 2.0;
            float3 L1z = directionData.z * L0 * 2.0;
        #else
            float3 nL1x = tex2D(_RNM0, lightmapUV).xyz * 2.0 - 1.0;
            float3 nL1y = tex2D(_RNM1, lightmapUV).xyz * 2.0 - 1.0;
            float3 nL1z = tex2D(_RNM2, lightmapUV).xyz * 2.0 - 1.0;
            float3 L0 = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;
            float3 L1x = nL1x * L0 * 2.0;
            float3 L1y = nL1y * L0 * 2.0;
            float3 L1z = nL1z * L0 * 2.0;
            directionData = float3(dot(nL1x, float3(0.2125, 0.7154, 0.0721)),
            dot(nL1y, float3(0.2125, 0.7154, 0.0721)),
            dot(nL1z, float3(0.2125, 0.7154, 0.0721)));
        #endif

        diffuse = Bakery_EvaluateSHDiffuse(L0, L1x, L1y, L1z, normalWS);
        specularDirectionTS = directionData;
    #endif
}

#endif
