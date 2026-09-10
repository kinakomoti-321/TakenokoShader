#ifndef TAKENOKO_VRCLIGHTVOLUME_HLSL
#define TAKENOKO_VRCLIGHTVOLUME_HLSL

// Bridges the VRC Light Volumes package into the same shape the lightmap and light
// probe paths use, so the caller does not have to know which source it got.
#if defined(LIGHTVOLUME_SUPPORT)
    #include "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc"
#endif

// Diffuse for the shading normal, plus the optimal linear direction of the sampled
// L1 SH.
//
// LightVolumeSH is the package's own top level entry rather than the LV_ internals:
// it checks whether volumes are enabled, falls back to dering'd light probes when
// they are not, and folds point light volumes and their occlusion in for us.
void VRCLightVolume_EvaluateSH(float3 positionWS, float3 normalWS, out float3 diffuse, out ShDirectionalLight directionalLight)
{
    #if defined(LIGHTVOLUME_SUPPORT)
        float3 L0;
        float3 L1r;
        float3 L1g;
        float3 L1b;
        LightVolumeSH(positionWS, L0, L1r, L1g, L1b);

        // An L1 reconstruction can dip below zero, and the volumes are authored data
        // rather than a guaranteed non-negative signal.
        diffuse = max(LightVolumeEvaluate(normalWS, L0, L1r, L1g, L1b), 0.0);

        // The package groups L1 by channel - L1r holds x, y and z for red - while
        // EvaluateShDirectionalLight wants one axis per argument. This is the
        // transpose between the two conventions, the same one EvaluateSH does for
        // unity_SHAr/g/b.
        directionalLight = EvaluateShDirectionalLight(
            L0,
            float3(L1r.x, L1g.x, L1b.x),
            float3(L1r.y, L1g.y, L1b.y),
            float3(L1r.z, L1g.z, L1b.z));
    #else
        diffuse = 0.0;
        directionalLight = NoShDirectionalLight();
    #endif
}

#endif
