#ifndef _TAKENOKO_NOISE_HLSL
#define _TAKENOKO_NOISE_HLSL


// 2Dノイズ（簡易）
float noise(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

// 2D スムーズノイズ
float smoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = noise(i);
    float b = noise(i + float2(1.0, 0.0));
    float c = noise(i + float2(0.0, 1.0));
    float d = noise(i + float2(1.0, 1.0));

    float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Fractal Brownian Motion
float fbm(float2 p)
{
    float total = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < 4; ++i)
    {
        total += amplitude * smoothNoise(p * frequency);
        frequency *= 2.0;
        amplitude *= 0.5;
    }

    return total;
}

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