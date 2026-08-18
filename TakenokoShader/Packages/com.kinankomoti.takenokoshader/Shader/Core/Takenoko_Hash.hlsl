#ifndef _TAKENOKO_RANDOM_HLSL
#define _TAKENOKO_RANDOM_HLSL

//--------------------------------------------------------------------------------
// Hash
//--------------------------------------------------------------------------------
#define C_HASH 20242024U
#define V1_HASH(x) x = C_HASH * ((x >> 8U)^x); \
    x = C_HASH * ((x >> 8U)^x); \
    x = C_HASH * ((x >> 8U)^x);
#define V2_HASH(x) x = C_HASH * ((x >> 8U)^x.yx); \
    x = C_HASH * ((x >> 8U)^x.yx); \
    x = C_HASH * ((x >> 8U)^x.yx);
#define V3_HASH(x) x = C_HASH * ((x >> 8U)^x.yzx); \
    x = C_HASH * ((x >> 8U)^x.yzx); \
    x = C_HASH * ((x >> 8U)^x.yzx);
#define V4_HASH(x) x = C_HASH * ((x >> 8U)^x.yzwx); \
    x = C_HASH * ((x >> 8U)^x.yzwx); \
    x = C_HASH * ((x >> 8U)^x.yzwx);

inline float hash11(float p)
{
    uint x = asuint(p);
    V1_HASH(x);
    return float(x) / uint(-1);
}

inline float hash12(float2 p)
{
    uint2 x = asuint(p);
    V2_HASH(x);
    return float(x.x) / uint(-1);
}

inline float hash13(float3 p)
{
    uint3 x = asuint(p);
    V3_HASH(x);
    return float(x.x) / uint(-1);
}

inline float hash14(float4 p)
{
    uint4 x = asuint(p);
    V4_HASH(x);
    return float(x.x) / uint(-1);
}

inline float2 hash21(float p)
{
    uint2 x = asuint(float2(p, 1.0));
    V2_HASH(x);
    return float2(x) / uint(-1);
}

inline float2 hash22(float2 p)
{
    uint2 x = asuint(p);
    V2_HASH(x);
    return float2(x) / uint(-1);
}

inline float2 hash23(float3 p)
{
    uint3 x = asuint(p);
    V3_HASH(x);
    return float2(x.xy) / uint(-1);
}

inline float2 hash24(float4 p)
{
    uint4 x = asuint(p);
    V4_HASH(x);
    return float2(x.xy) / uint(-1);
}

inline float3 hash31(float p)
{
    uint3 x = asuint(float3(p, 1.0, 2.0));
    V3_HASH(x);
    return float3(x) / uint(-1);
}

inline float3 hash32(float2 p)
{
    uint3 x = asuint(float3(p, 1.0));
    V3_HASH(x);
    return float3(x) / uint(-1);
}

inline float3 hash33(float3 p)
{
    uint3 x = asuint(p);
    V3_HASH(x);
    return float3(x) / uint(-1);
}

inline float3 hash34(float4 p)
{
    uint4 x = asuint(p);
    V4_HASH(x);
    return float3(x.xyz) / uint(-1);
}

inline float4 hash41(float p)
{
    uint4 x = asuint(float4(p, 1.0, 2.0, 3.0));
    V4_HASH(x);
    return float4(x) / uint(-1);
}

inline float4 hash42(float2 p)
{
    uint4 x = asuint(float4(p, 1.0, 2.0));
    V4_HASH(x);
    return float4(x) / uint(-1);
}

inline float4 hash43(float3 p)
{
    uint4 x = asuint(float4(p, 1.0));
    V4_HASH(x);
    return float4(x) / uint(-1);
}

inline float4 hash44(float4 p)
{
    uint4 x = asuint(p);
    V4_HASH(x);
    return float4(x) / uint(-1);
}

uint rngSeed;
#define RNG_INIT(time, fragcoord) rngSeed = (time * 100.0 + 1.0) * (fragcoord.x + fragcoord.y * 1024.0)

float Rng1()
{
    V1_HASH(rngSeed);
    return float(rngSeed) / uint(-1);
}

float2 Rng2()
{
    return float2(Rng1(), Rng1());
}

float3 Rng3()
{
    return float3(Rng1(), Rng1(), Rng1());
}

#endif