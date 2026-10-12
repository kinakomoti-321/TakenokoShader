#ifndef TAKENOKO_VRCLIGHTVOLUME_HLSL
#define TAKENOKO_VRCLIGHTVOLUME_HLSL

#if defined(LIGHTVOLUME_SUPPORT)
    #include "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc"

    void VRCLightVolume_EvaluateSH(float3 positionWS, float3 normalWS, out float3 diffuse, out ShDirectionalLight directionalLight)
    {
        float3 L0;
        float3 L1r;
        float3 L1g;
        float3 L1b;
        LightVolumeSH(positionWS, L0, L1r, L1g, L1b);

        diffuse = max(LightVolumeEvaluate(normalWS, L0, L1r, L1g, L1b), 0.0);

        directionalLight = EvaluateShDirectionalLight(
            L0,
            float3(L1r.x, L1g.x, L1b.x),
            float3(L1r.y, L1g.y, L1b.y),
            float3(L1r.z, L1g.z, L1b.z));
    }
#endif

#endif
