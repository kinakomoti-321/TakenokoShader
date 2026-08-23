#ifndef TAKENOKO_STANDARD_COMMON_HLSL
#define TAKENOKO_STANDARD_COMMON_HLSL

#include "../Core/Takenoko_Utils.hlsl"

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "UnityShaderVariables.cginc"

struct Attribute
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1; // Lightmap UV
    float2 texcoord2 : TEXCOORD2;
    float2 texcoord3 : TEXCOORD3;
    float4 color : COLOR0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 pos : SV_POSITION;

    float3 positionWS : TEXCOORD0;
    float3 positionOS : TEXCOORD1;
    float4 positionSS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
    float4 tangentWS : TEXCOORD4;

    float2 texcoord0 : TEXCOORD5;
    float2 texcoord1 : TEXCOORD6;
    float2 texcoord2 : TEXCOORD7;
    float2 texcoord3 : TEXCOORD8;

    UNITY_FOG_COORDS(9)
    UNITY_SHADOW_COORDS(10)

    float2 lightmapUV : TEXCOORD11;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


#endif