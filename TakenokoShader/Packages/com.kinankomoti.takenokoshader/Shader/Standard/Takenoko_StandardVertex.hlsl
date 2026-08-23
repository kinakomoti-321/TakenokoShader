#ifndef TAKENOKO_STANDARD_VERTEX_HLSL
#define TAKENOKO_STANDARD_VERTEX_HLSL

#include "Takenoko_StandardDefinition.hlsl"
#include "Takenoko_StandardAttribute.hlsl"

VertexOutput Takenoko_VertexStandard(Attribute v)
{
    VertexOutput o;
    UNITY_SETUP_INSTANCE_ID(v);
    
    UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.positionOS = v.vertex.xyz;
    o.positionSS = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
    o.normalWS.xyz = UnityObjectToWorldNormal(v.normal);
    o.tangentWS.xyz = UnityObjectToWorldDir(v.tangent.xyz);
    o.tangentWS.w = v.tangent.w;

    o.texcoord0 = v.texcoord0;
    o.texcoord1 = v.texcoord1;
    o.texcoord2 = v.texcoord2;
    o.texcoord3 = v.texcoord3;

    // Lightmap UV
    #ifdef DYNAMICLIGHTMAP_ON
        o.lightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    #ifdef LIGHTMAP_ON
        o.lightmapUV = GetLightmapUV(v.texcoord1);
    #endif
    
    UNITY_TRANSFER_SHADOW(o, o.pos);
    UNITY_TRANSFER_FOG(o, o.pos);

    return o;
}


#endif