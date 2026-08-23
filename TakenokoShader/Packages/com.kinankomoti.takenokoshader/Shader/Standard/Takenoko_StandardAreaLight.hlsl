#ifndef TAKENOKO_STANDARD_AREA_LIGHT_HLSL
#define TAKENOKO_STANDARD_AREA_LIGHT_HLSL

#if defined(_VRC_AREALIGHT_ON)
    float _UdonEnableLtcSystem;

    TAKENOKO_TEXTURE2D(_UdonLtcLut);
    Texture2D _UdonFresnelLut;

    float _UdonLightVertex1[12]; // 4 vertices * 3 components
    Texture2D _UdonLightTexture1;
    float4 _UdonLightEmission1;

    float _UdonLightVertex2[12]; // 4 vertices * 3 components
    Texture2D _UdonLightTexture2;
    float4 _UdonLightEmission2;

    float _UdonLightVertex3[12]; // 4 vertices * 3 components
    Texture2D _UdonLightTexture3;
    float4 _UdonLightEmission3;

    float _UdonLightVertex4[12]; // 4 vertices * 3 components
    Texture2D _UdonLightTexture4;
    float4 _UdonLightEmission4;

    float _UdonLightVertex5[12]; // 4 vertices * 3 components
    Texture2D _UdonLightTexture5;
    float4 _UdonLightEmission5;

    static const float LUT_SIZE = 64;
    static const float LUT_SCALE = (LUT_SIZE - 1.0) / LUT_SIZE;
    static const float LUT_BIAS = 0.5 / LUT_SIZE;

    float2 GetLtcTexcoord(float cosine, float roughness)
    {
        float2 uv;
        uv.x = roughness;
        uv.y = sqrt(1.0 - cosine);
        uv = uv * LUT_SCALE + LUT_BIAS;
        return uv;
    }

    float3x3 SampleLtcInverseMatrix(float2 uv, Texture2D lutTexture, SamplerState lutSampler)
    {
        float4 lut = lutTexture.SampleLevel(lutSampler, uv, 0);
        return float3x3(
            float3(lut.x, 0, lut.y),
            float3(0, 1, 0),
            float3(lut.z, 0, lut.w)
        );
    }

    // Z-clip
    void ClipQuadToHorizon(inout float3 L[5], out int n)
    {
        // detect clipping config
        int config = 0;
        if (L[0].z > 0.0) config += 1;
        if (L[1].z > 0.0) config += 2;
        if (L[2].z > 0.0) config += 4;
        if (L[3].z > 0.0) config += 8;

        // clip
        n = 0;

        if (config == 0)
        {
            // clip all

        }
        else if (config == 1) // V1 clip V2 V3 V4

        {
            n = 3;
            L[1] = -L[1].z * L[0] + L[0].z * L[1];
            L[2] = -L[3].z * L[0] + L[0].z * L[3];
        }
        else if (config == 2) // V2 clip V1 V3 V4

        {
            n = 3;
            L[0] = -L[0].z * L[1] + L[1].z * L[0];
            L[2] = -L[2].z * L[1] + L[1].z * L[2];
        }
        else if (config == 3) // V1 V2 clip V3 V4

        {
            n = 4;
            L[2] = -L[2].z * L[1] + L[1].z * L[2];
            L[3] = -L[3].z * L[0] + L[0].z * L[3];
        }
        else if (config == 4) // V3 clip V1 V2 V4

        {
            n = 3;
            L[0] = -L[3].z * L[2] + L[2].z * L[3];
            L[1] = -L[1].z * L[2] + L[2].z * L[1];
        }
        else if (config == 5) // V1 V3 clip V2 V4) impossible

        {
            n = 0;
        }
        else if (config == 6) // V2 V3 clip V1 V4

        {
            n = 4;
            L[0] = -L[0].z * L[1] + L[1].z * L[0];
            L[3] = -L[3].z * L[2] + L[2].z * L[3];
        }
        else if (config == 7) // V1 V2 V3 clip V4

        {
            n = 5;
            L[4] = -L[3].z * L[0] + L[0].z * L[3];
            L[3] = -L[3].z * L[2] + L[2].z * L[3];
        }
        else if (config == 8) // V4 clip V1 V2 V3

        {
            n = 3;
            L[0] = -L[0].z * L[3] + L[3].z * L[0];
            L[1] = -L[2].z * L[3] + L[3].z * L[2];
            L[2] = L[3];
        }
        else if (config == 9) // V1 V4 clip V2 V3

        {
            n = 4;
            L[1] = -L[1].z * L[0] + L[0].z * L[1];
            L[2] = -L[2].z * L[3] + L[3].z * L[2];
        }
        else if (config == 10) // V2 V4 clip V1 V3) impossible

        {
            n = 0;
        }
        else if (config == 11) // V1 V2 V4 clip V3

        {
            n = 5;
            L[4] = L[3];
            L[3] = -L[2].z * L[3] + L[3].z * L[2];
            L[2] = -L[2].z * L[1] + L[1].z * L[2];
        }
        else if (config == 12) // V3 V4 clip V1 V2

        {
            n = 4;
            L[1] = -L[1].z * L[2] + L[2].z * L[1];
            L[0] = -L[0].z * L[3] + L[3].z * L[0];
        }
        else if (config == 13) // V1 V3 V4 clip V2

        {
            n = 5;
            L[4] = L[3];
            L[3] = L[2];
            L[2] = -L[1].z * L[2] + L[2].z * L[1];
            L[1] = -L[1].z * L[0] + L[0].z * L[1];
        }
        else if (config == 14) // V2 V3 V4 clip V1

        {
            n = 5;
            L[4] = -L[0].z * L[3] + L[3].z * L[0];
            L[0] = -L[0].z * L[1] + L[1].z * L[0];
        }
        else if (config == 15) // V1 V2 V3 V4

        {
            n = 4;
        }

        if (n == 3)
            L[3] = L[0];
        if (n == 4)
            L[4] = L[0];
    }

    float IntegrateEdge(float3 v1, float3 v2)
    {
        float x = dot(v1, v2);
        float y = abs(x);

        float a = 0.8543985 + (0.4965155 + 0.0145206 * y) * y;
        float b = 3.4175940 + (4.1616724 + y) * y;
        float v = a / b;

        float theta_sintheta = (x > 0.0) ? v : 0.5 * rsqrt(max(1.0 - x * x, 1e-7)) - v;

        return (cross(v1, v2) * theta_sintheta).z;
    }

    float GaussianKernel(in float x, in float sigma)
    {
        float s = 1 / sigma;
        // 1/sqrt(2 * PI) = 0.39894
        return 0.39894 * exp(-0.5 * x * x * s * s) * s;
    }

    float GaussianInv(float y, float sigma)
    {
        // sqrt(2 * PI) = 2.50662
        return sigma * sqrt(-2 * log(2.50662 * sigma * y));
    }

    float SquareSDF(float2 uv)
    {
        uv -= 0.5;
        float2 st = abs(uv) - 0.5;
        return max(st.x, st.y);
    }

    float3 FilterTexture(Texture2D tex, SamplerState texSampler, float3 v0, float3 v1, float3 v2)
    {
        // Reference : Kanikama shader by shivaduke28
        // https://github.com/shivaduke28/kanikama

        // Orthogonal projection
        float3 V1 = v0 - v1;
        float3 V2 = v2 - v1;
        float Area = length(cross(V1, V2));
        float3 N = normalize(cross(V1, V2));
        
        float r = dot(v1, N);
        float3 P = r * N - v1;

        // Skew coordinates
        float dotP1V1 = dot(P, V1);
        float dotP1V2 = dot(P, V2);
        float dotV1V1 = dot(V1, V1);
        float dotV2V2 = dot(V2, V2);
        float dotV1V2 = dot(V1, V2);
        float delta = dotV1V1 * dotV2V2 - dotV1V2 * dotV1V2;

        float2 uv;
        uv.y = (dotV2V2 * dotP1V1 - dotV1V2 * dotP1V2) / delta;
        uv.x = (-dotV1V2 * dotP1V1 + dotV1V1 * dotP1V2) / delta;
        uv.y = 1.0 - uv.y;

        // Blur sigma
        float sigma = abs(r) / sqrt(Area);
        float add = max(0, SquareSDF(uv));
        sigma += add;

        // Approximate gaussian function by step functions.
        // Texture's filter mode should be Trilinear.
        float y0 = GaussianKernel(0, sigma);
        float y1 = y0 * 0.75;
        float x1 = GaussianInv(y1, sigma);
        float y2 = y0 * 0.5;
        float x2 = GaussianInv(y2, sigma);
        float y3 = y0 * 0.25;
        float x3 = GaussianInv(y3, sigma);

        half4 col = 0;

        float2 dx = float2(0.5, 0);
        float2 dy = float2(0, 0.5);

        col += tex.SampleGrad(texSampler, uv, dx * x3, dy * x3) * 0.333;
        col += tex.SampleGrad(texSampler, uv, dx * x2, dy * x2) * 0.333;
        col += tex.SampleGrad(texSampler, uv, dx * x1, dy * x1) * 0.333;

        return col;
    }

    float3 EvaluateAreaLightContribution(float3 v[4], Texture2D lightTexture, SamplerState lightSampler)
    {
        float3 p[5];
        p[0] = v[0];
        p[1] = v[1];
        p[2] = v[2];
        p[3] = v[3];
        p[4] = 0.0;

        int numPoint;
        ClipQuadToHorizon(p, numPoint);

        if (numPoint == 0)
            return float3(0, 0, 0);

        p[0] = normalize(p[0]);
        p[1] = normalize(p[1]);
        p[2] = normalize(p[2]);
        p[3] = normalize(p[3]);
        p[4] = normalize(p[4]);

        float integration = 0.0;
        integration += IntegrateEdge(p[0], p[1]);
        integration += IntegrateEdge(p[1], p[2]);
        integration += IntegrateEdge(p[2], p[3]);
        if (numPoint >= 4)
            integration += IntegrateEdge(p[3], p[4]);
        if (numPoint == 5)
            integration += IntegrateEdge(p[4], p[0]);

        // integration = abs(integration);
        integration = max(0.0, -integration);

        float3 Lo_i = integration;

        Lo_i *= FilterTexture(lightTexture, lightSampler, v[0], v[1], v[2]);

        return Lo_i;
    }

    void EvaluateAreaLight(float3 N, float3 V, float3 P, float3 points[4], float3x3 Minv, Texture2D lightTexture, SamplerState lightSampler, out float3 diffuse, out float3 specular)
    {
        float3 tangent = normalize(V - N * dot(V, N));
        float3 binormal = cross(N, tangent);
        float3x3 worldToTangent = transpose(float3x3(tangent, binormal, N));

        float3 v[4];
        v[0] = mul(points[0] - P, worldToTangent);
        v[1] = mul(points[1] - P, worldToTangent);
        v[2] = mul(points[2] - P, worldToTangent);
        v[3] = mul(points[3] - P, worldToTangent);

        diffuse = EvaluateAreaLightContribution(v, lightTexture, lightSampler);

        v[0] = mul(v[0], Minv);
        v[1] = mul(v[1], Minv);
        v[2] = mul(v[2], Minv);
        v[3] = mul(v[3], Minv);

        specular = EvaluateAreaLightContribution(v, lightTexture, lightSampler);
    }

#endif
#endif
