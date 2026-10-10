#ifndef _TAKENOKO_Utils_HLSL
#define _TAKENOKO_Utils_HLSL

//--------------------------------------------
// Texture Sample
//--------------------------------------------

SamplerState sampler_LinearClamp;

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

inline float square(float x)
{
    return x * x;
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
    float cmax = max(rgb.r, max(rgb.g, rgb.b));
    float cmin = min(rgb.r, min(rgb.g, rgb.b));
    float chroma = cmax - cmin;

    // Hue is undefined for achromatic colors, including black.
    if (chroma == 0.0)
        return float3(0.0, 0.0, cmax);

    float hue = rgb.r == cmax ? (rgb.g - rgb.b) / chroma
        : rgb.g == cmax ? 2.0 + (rgb.b - rgb.r) / chroma
        : 4.0 + (rgb.r - rgb.g) / chroma;

    return float3(frac(hue / 6.0), chroma / cmax, cmax);
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

//--------------------------------------------
// SH Directional Light
//--------------------------------------------

// A single pseudo directional light standing in for an L0/L1 spherical harmonic.
// For L1 data the optimal linear direction is the dominant direction: the L1 band
// is the first moment of the sphere, so it already points at the mean incoming
// light. Only L2 would make the two differ.
struct ShDirectionalLight
{
    float3 direction;
    float3 color;
};

ShDirectionalLight NoShDirectionalLight()
{
    ShDirectionalLight light;
    light.direction = float3(0.0, 1.0, 0.0);
    light.color = 0.0;
    return light;
}

// Optimal linear direction
ShDirectionalLight EvaluateShDirectionalLight(float3 L0, float3 L1x, float3 L1y, float3 L1z)
{
    const float3 luma = float3(0.2125, 0.7154, 0.0721);

    float3 R1 = float3(dot(L1x, luma), dot(L1y, luma), dot(L1z, luma));
    float R0 = max(dot(L0, luma), 1.0e-5);
    float lengthR1 = length(R1);

    ShDirectionalLight light;
    light.direction = lengthR1 > 1.0e-5 ? R1 / lengthR1 : float3(0.0, 1.0, 0.0);

    float directionality = saturate(lengthR1 / R0);
    float3 alongDirection = max(
        L0 + light.direction.x * L1x + light.direction.y * L1y + light.direction.z * L1z, 0.0);
    light.color = alongDirection * directionality;

    return light;
}

#endif
