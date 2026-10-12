#ifndef _TAKENOKO_NOISE_HLSL
#define _TAKENOKO_NOISE_HLSL

// Cyclic Noise
// https://scrapbox.io/0b5vr/Cyclic_Noise

inline float3x3 GetOrthogonalBasis(float3 direction)
{
    direction = normalize(direction);
    float3 right = normalize(cross(float3(0, 1, 0), direction));
    float3 up = normalize(cross(direction, right));
    return float3x3(right, up, direction);
}

inline float3 Cyclic(float3 p, float pers, float lacu)
{
    float4 sum = 0;
    float3x3 rot = GetOrthogonalBasis(float3(2, -3, 1));

    for (int i = 0; i < 5; i++)
    {
        p = mul(p, rot);
        p += sin(p.zxy);
        sum += float4(cross(cos(p), sin(p.yzx)), 1);
        sum /= pers;
        p *= lacu;
    }
    
    return sum.xyz / sum.w;
}

#endif