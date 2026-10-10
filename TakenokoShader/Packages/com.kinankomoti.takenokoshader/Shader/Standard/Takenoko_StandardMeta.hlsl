#ifndef TAKENOKO_STANDARD_META_HLSL
#define TAKENOKO_STANDARD_META_HLSL

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityMetaPass.cginc"
#include "Takenoko_StandardDefinition.hlsl"
#include "Takenoko_StandardAttribute.hlsl"
#include "Takenoko_StandardMapping.hlsl"

struct MetaVertexOutput
{
    float4 pos : SV_POSITION;
    float4 uv01 : TEXCOORD0;
    float4 uv23 : TEXCOORD1;
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        float3 positionWS : TEXCOORD4;
        float3 normalWS : TEXCOORD5;
    #endif
    #ifdef EDITOR_VISUALIZATION
        float2 vizUV : TEXCOORD2;
        float4 lightCoord : TEXCOORD3;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

MetaVertexOutput Takenoko_VertexMeta(Attribute v)
{
    MetaVertexOutput o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(MetaVertexOutput, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1, v.texcoord2,
        unity_LightmapST, unity_DynamicLightmapST);
    o.uv01 = float4(v.texcoord0, v.texcoord1);
    o.uv23 = float4(v.texcoord2, v.texcoord3);
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.normalWS = UnityObjectToWorldNormal(v.normal);
    #endif
    #ifdef EDITOR_VISUALIZATION
        if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
            o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord0, v.texcoord1,
                v.texcoord2, unity_EditorViz_Texture_ST);
        else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
        {
            o.vizUV = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;
            o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, v.vertex));
        }
    #endif
    return o;
}

float4 Takenoko_FragmentMeta(MetaVertexOutput i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    // Reuse the forward pass UV selection and material texture mapping.
    VertexOutput mapping;
    UNITY_INITIALIZE_OUTPUT(VertexOutput, mapping);
    mapping.texcoord0 = i.uv01.xy;
    mapping.texcoord1 = i.uv01.zw;
    mapping.texcoord2 = i.uv23.xy;
    mapping.texcoord3 = i.uv23.zw;
    float2 uv = MainTexcoord(mapping);
    #if defined(_MAINTEX_TILING_TRIPLANAR)
        MainTextureMapping mainMapping = MainMapping(uv, i.positionWS, i.normalWS);
    #else
        MainTextureMapping mainMapping = MainMapping(uv, 0.0, float3(0, 0, 1));
    #endif
    float4 mainTex = Standard_BaseColor(mainMapping);
    #if defined(_ALPHATEST_ON)
        clip(mainTex.a - _Cutoff);
    #endif

    float3 basecolor = mainTex.rgb;
    #if defined(_SECONDTEXTURE_ON)
        float3 normalTS = float3(0.0, 0.0, 1.0);
        Standard_SecondTexture(SecondTexcoord(mapping), uv, basecolor, normalTS);
    #endif
    basecolor = ShiftHSV(basecolor, float3(_FilterMainTexHueShift,
        _FilterMainTexSaturateShift, _FilterMainTexValueShift));
    float metallic = Standard_Metallic(mainMapping);
    float roughness = Standard_Roughness(mainMapping);
    float3 diffuse = basecolor * (1.0 - metallic);
    float3 specular = lerp(0.04, basecolor, metallic);

    UnityMetaInput meta;
    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, meta);
    // Match Unity's rough-metal bounce approximation using Takenoko's diffuse model.
    meta.Albedo = diffuse + specular * roughness * roughness * 0.5;
    meta.SpecularColor = specular;
    meta.Emission = Standard_Emission(mainMapping).rgb;
    #ifdef EDITOR_VISUALIZATION
        meta.Albedo = diffuse;
        meta.VizUV = i.vizUV;
        meta.LightCoord = i.lightCoord;
    #endif
    return UnityMetaFragment(meta);
}

#endif
