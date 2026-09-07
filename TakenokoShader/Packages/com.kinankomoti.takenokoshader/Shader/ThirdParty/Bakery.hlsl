#ifndef TAKENOKO_BAKERY_HLSL
#define TAKENOKO_BAKERY_HLSL

sampler2D _RNM0;
sampler2D _RNM1;
sampler2D _RNM2;
float4 _RNM0_TexelSize;

static const float3 BAKERY_LUMA_CONV = float3(0.2125, 0.7154, 0.0721);

static const float3 BAKERY_RNM_BASIS0 = float3(0.81649658, 0.0, 0.57735027);
static const float3 BAKERY_RNM_BASIS1 = float3(-0.40824829, 0.70710678, 0.57735027);
static const float3 BAKERY_RNM_BASIS2 = float3(-0.40824829, -0.70710678, 0.57735027);

float Bakery_EvaluateDiffuseL1Geomerics(float L0, float3 L1, float3 normalWS)
{
    float R0 = max(L0, 1.0e-5);
    float3 R1 = 0.5 * L1;
    float lenR1 = max(length(R1), 1.0e-5);
    float directionality = lenR1 / R0;

    float q = saturate(dot(R1 / lenR1, normalWS) * 0.5 + 0.5);
    float p = 1.0 + 2.0 * directionality;
    float a = (1.0 - directionality) / (1.0 + directionality);

    return R0 * (a + (1.0 - a) * (p + 1.0) * pow(q, p));
}

float3 Bakery_EvaluateSHDiffuse(float3 L0, float3 L1x, float3 L1y, float3 L1z, float3 normalWS)
{
    float3 sh = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;

    #if defined(_LIGHTMAP_NONLINEAR_SH_ON)
        float lumaSH = Bakery_EvaluateDiffuseL1Geomerics(dot(L0, 1.0),
        float3(dot(L1x, 1.0), dot(L1y, 1.0), dot(L1z, 1.0)), normalWS);
        float linearLumaSH = dot(sh, 1.0);

        sh *= lerp(1.0, lumaSH / max(linearLumaSH, 1.0e-5), saturate(linearLumaSH * 16.0));
    #endif

    return max(sh, 0.0);
}

void Bakery_EvaluateRNM(float2 lightmapUV, float3 normalTS, out float3 diffuse, out float3 directionTS, out float3 directionColor)
{
    float3 rnm0 = DecodeLightmap(tex2D(_RNM0, lightmapUV));
    float3 rnm1 = DecodeLightmap(tex2D(_RNM1, lightmapUV));
    float3 rnm2 = DecodeLightmap(tex2D(_RNM2, lightmapUV));

    diffuse = saturate(dot(BAKERY_RNM_BASIS0, normalTS)) * rnm0
    + saturate(dot(BAKERY_RNM_BASIS1, normalTS)) * rnm1
    + saturate(dot(BAKERY_RNM_BASIS2, normalTS)) * rnm2;

    float luma0 = dot(rnm0, BAKERY_LUMA_CONV);
    float luma1 = dot(rnm1, BAKERY_LUMA_CONV);
    float luma2 = dot(rnm2, BAKERY_LUMA_CONV);

    directionTS = BAKERY_RNM_BASIS0 * luma0
    + BAKERY_RNM_BASIS1 * luma1
    + BAKERY_RNM_BASIS2 * luma2;
    float mean = max((luma0 + luma1 + luma2) / 3.0, 1.0e-5);
    float3 varying = BAKERY_RNM_BASIS0 * (luma0 - mean)
    + BAKERY_RNM_BASIS1 * (luma1 - mean)
    + BAKERY_RNM_BASIS2 * (luma2 - mean);
    float directionality = saturate(length(varying) / mean);

    float3 normalizedDirection = normalize(directionTS);
    directionColor = directionality * (
        saturate(dot(BAKERY_RNM_BASIS0, normalizedDirection)) * rnm0
        + saturate(dot(BAKERY_RNM_BASIS1, normalizedDirection)) * rnm1
        + saturate(dot(BAKERY_RNM_BASIS2, normalizedDirection)) * rnm2);
}

void Bakery_EvaluateSH(float2 lightmapUV, float3 normalWS, out float3 diffuse, out ShDirectionalLight directionalLight)
{
    float3 L0 = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;

    #if defined(_LIGHTMAP_MONOSH)
        float3 nL1 = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lightmapUV).xyz * 2.0 - 1.0;
        float3 L1x = nL1.x * L0 * 2.0;
        float3 L1y = nL1.y * L0 * 2.0;
        float3 L1z = nL1.z * L0 * 2.0;
    #else
        float3 nL1x = tex2D(_RNM0, lightmapUV).xyz * 2.0 - 1.0;
        float3 nL1y = tex2D(_RNM1, lightmapUV).xyz * 2.0 - 1.0;
        float3 nL1z = tex2D(_RNM2, lightmapUV).xyz * 2.0 - 1.0;
        float3 L1x = nL1x * L0 * 2.0;
        float3 L1y = nL1y * L0 * 2.0;
        float3 L1z = nL1z * L0 * 2.0;
    #endif

    diffuse = Bakery_EvaluateSHDiffuse(L0, L1x, L1y, L1z, normalWS);
    directionalLight = EvaluateShDirectionalLight(L0, L1x, L1y, L1z);
}

void Bakery_EvaluateLightmap(float2 lightmapUV, float3 normalWS, float3 normalTS, float3x3 tbn,
out float3 diffuse, out ShDirectionalLight directionalLight)
{
    #if defined(_LIGHTMAP_RNM)
        float3 directionTS;
        float3 directionColor;
        Bakery_EvaluateRNM(lightmapUV, normalTS, diffuse, directionTS, directionColor);
        directionalLight.direction = normalize(mul(directionTS, tbn));
        directionalLight.color = directionColor;
    #elif defined(_LIGHTMAP_SH) || defined(_LIGHTMAP_MONOSH)
        Bakery_EvaluateSH(lightmapUV, normalWS, diffuse, directionalLight);
    #else
        diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)).xyz;
        directionalLight = NoShDirectionalLight();
    #endif
}

#endif
