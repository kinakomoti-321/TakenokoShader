#ifndef _TAKENOKO_Utils_HLSL
#define _TAKENOKO_Utils_HLSL

//--------------------------------------------
// Texture Sample
//--------------------------------------------

#define TAKENOKO_TEXTURE2D(name) Texture2D name; SamplerState sampler_##name;
#define TAKENOKO_SAMPLE(tex, uv) tex.Sample(sampler_##tex, uv)
#define TAKENOKO_SAMPLE_LOD(tex, uv, lod) tex.SampleLevel(sampler_##tex, uv, lod)
#define TAKENOKO_SAMPLE_GRAD(tex, uv, ddx, ddy) tex.SampleGrad(sampler_##tex, uv, ddx, ddy)
#define TAKENOKO_SAMPLE_SAMPLER(tex, samp, uv) tex.Sample(samp, uv)
#define TAKENOKO_SAMPLE_SAMPLER_LOD(tex, samp, uv, lod) tex.SampleLevel(samp, uv, lod)
#define TAKENOKO_SAMPLE_SAMPLER_GRAD(tex, samp, uv, ddx, ddy) tex.SampleGrad(samp, uv, ddx, ddy)

//--------------------------------------------
// Pipeline
//--------------------------------------------

// Texcoord1 is used for lightmap in Unity
inline float2 GetLightmapUV(float2 texcoord1)
{
    return texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;
}

//--------------------------------------------
// SDF
//--------------------------------------------

inline float sdBox3D(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, max(d.y, d.z)), 0.0);
}

//--------------------------------------------
// Math
//--------------------------------------------

// TAKENOKO_ -> TAKE_
// PI is often conflicted...

#define TAKE_PI 3.14159264
#define TAKE_PI2 6.28318531

inline float3x3 TBNMatrix(float3 t, float tw, float3 n)
{
    float crossSign = (tw > 0.0 ? 1.0 : - 1.0) * unity_WorldTransformParams.w;
    float3 b = cross(n, t) * crossSign;
    return float3x3(t, b, n);
}

inline float3 Binormal(float3 t, float tw, float3 n)
{
    float crossSign = (tw > 0.0 ? 1.0 : - 1.0) * unity_WorldTransformParams.w;
    return cross(n, t) * crossSign;
}

#define MOD_GLSL(a) \
inline a modGLSL(a x, a y)\
{\
    return x - y * floor(x / y); \
}\

MOD_GLSL(float)
MOD_GLSL(float2)
MOD_GLSL(float3)
MOD_GLSL(float4)

//--------------------------------------------
// Color
//--------------------------------------------

float Luminance(float3 color)
{
    // sRGB->CIE XYZ
    return 0.212639 * color.x + 0.715169 * color.y + 0.072192 * color.z;
}

float3 RGB2HSV(float3 rgb)
{
    float3 hsv;
    float cmax = max(rgb.r, max(rgb.g, rgb.b));
    float cmin = min(rgb.r, min(rgb.g, rgb.b));
    
    hsv.z = cmax; // value

    float chroma = cmax - cmin;
    hsv.y = chroma / cmax; // saturation

    if (rgb.r > rgb.g && rgb.r > rgb.b)
    {
        hsv.x = (0.0 + (rgb.g - rgb.b) / chroma) / 6.0; // hue

    }
    else if (rgb.g > rgb.b)
    {
        hsv.x = (2.0 + (rgb.b - rgb.r) / chroma) / 6.0; // hue

    }
    else
    {
        hsv.x = (4.0 + (rgb.r - rgb.g) / chroma) / 6.0; // hue

    }
    hsv.x = frac(hsv.x);
    return hsv;
}

// smooth hsv: https://www.shadertoy.com/view/MsS3Wc
float3 HSV2RGB(float3 hsv)
{
    float3 rgb = clamp(abs(modGLSL(hsv.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    rgb = rgb * rgb * (3.0 - 2.0 * rgb); // cubic smoothing

    return hsv.z * lerp(1.0, rgb, hsv.y);
}

float3 ShiftHSV(float3 rgb, float3 shift)
{
    float3 hsv = RGB2HSV(rgb);
    hsv += shift;
    hsv.yz = saturate(hsv.yz);
    hsv.x = frac(hsv.x);
    return HSV2RGB(hsv);
}

#endif