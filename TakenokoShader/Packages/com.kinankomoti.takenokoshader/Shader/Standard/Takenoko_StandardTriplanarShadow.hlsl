#ifndef TAKENOKO_STANDARD_TRIPLANAR_SHADOW_HLSL
#define TAKENOKO_STANDARD_TRIPLANAR_SHADOW_HLSL

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "Packages/com.kinankomoti.takenokoshader/Shader/Standard/Takenoko_StandardDefinition.hlsl"
#include "Packages/com.kinankomoti.takenokoshader/Shader/Standard/Takenoko_StandardAttribute.hlsl"
#include "Packages/com.kinankomoti.takenokoshader/Shader/Standard/Takenoko_StandardMapping.hlsl"

struct TriplanarShadowOutput
{
    V2F_SHADOW_CASTER;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

TriplanarShadowOutput vertShadowCaster(Attribute v)
{
    TriplanarShadowOutput o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(TriplanarShadowOutput, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.normalWS = UnityObjectToWorldNormal(v.normal);
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    return o;
}

float4 fragShadowCaster(TriplanarShadowOutput i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    #if defined(_ALPHATEST_ON)
        MainTextureMapping mapping = MainMapping(0.0, i.positionWS, i.normalWS);
        clip(Standard_BaseColor(mapping).a - _Cutoff);
    #endif
    SHADOW_CASTER_FRAGMENT(i)
}

#endif
