using System.Collections.Generic;
using System.ComponentModel;

namespace Takenoko.Standard
{
public enum ShaderKeywords
{
    [Description("STEREO_INSTANCING_ON")]
    StereoInstancingOn,
    [Description("UNITY_SINGLE_PASS_STEREO")]
    UnitySinglePassStereo,
    [Description("STEREO_MULTIVIEW_ON")]
    StereoMultiviewOn,
    [Description("STEREO_CUBEMAP_RENDER_ON")]
    StereoCubemapRenderOn,
    [Description("_EMISSION")]
    Emission,
    [Description("_EMISSION_ON")]
    EmissionOn,
    [Description("LIGHTVOLUME_SUPPORT")]
    LightvolumeSupport,
    [Description("FOG_LINEAR")]
    FogLinear,
    [Description("FOG_EXP")]
    FogExp,
    [Description("FOG_EXP2")]
    FogExp2,
    [Description("INSTANCING_ON")]
    InstancingOn,
    [Description("_MAINTEX_TILING_SIMPLE")]
    MaintexTilingSimple,
    [Description("_MAINTEX_TILING_HEX")]
    MaintexTilingHex,
    [Description("_ALPHATEST_ON")]
    AlphatestOn,
    [Description("_ALPHABLEND_ON")]
    AlphablendOn,
    [Description("_ALPHAPREMULTIPLY_ON")]
    AlphapremultiplyOn,
    [Description("_MAINTEX_UV0")]
    MaintexUv0,
    [Description("_MAINTEX_UV1")]
    MaintexUv1,
    [Description("_MAINTEX_UV2")]
    MaintexUv2,
    [Description("_MAINTEX_UV3")]
    MaintexUv3,
    [Description("_ROUGHNESS_MODEL_ROUGHNESS")]
    RoughnessModelRoughness,
    [Description("_ROUGHNESS_MODEL_SMOOTHNESS")]
    RoughnessModelSmoothness,
    [Description("_ROUGHNESS_CHANNEL_R")]
    RoughnessChannelR,
    [Description("_ROUGHNESS_CHANNEL_G")]
    RoughnessChannelG,
    [Description("_ROUGHNESS_CHANNEL_B")]
    RoughnessChannelB,
    [Description("_ROUGHNESS_CHANNEL_A")]
    RoughnessChannelA,
    [Description("_METALLIC_CHANNEL_R")]
    MetallicChannelR,
    [Description("_METALLIC_CHANNEL_G")]
    MetallicChannelG,
    [Description("_METALLIC_CHANNEL_B")]
    MetallicChannelB,
    [Description("_METALLIC_CHANNEL_A")]
    MetallicChannelA,
    [Description("_OCCLUSION_CHANNEL_R")]
    OcclusionChannelR,
    [Description("_OCCLUSION_CHANNEL_G")]
    OcclusionChannelG,
    [Description("_OCCLUSION_CHANNEL_B")]
    OcclusionChannelB,
    [Description("_OCCLUSION_CHANNEL_A")]
    OcclusionChannelA,
    [Description("_THINFILM_ON")]
    ThinfilmOn,
    [Description("_HEIGHT_ON")]
    HeightOn,
    [Description("_HEIGHT_CHANNEL_R")]
    HeightChannelR,
    [Description("_HEIGHT_CHANNEL_G")]
    HeightChannelG,
    [Description("_HEIGHT_CHANNEL_B")]
    HeightChannelB,
    [Description("_HEIGHT_CHANNEL_A")]
    HeightChannelA,
    [Description("_SPECULAR_OCCLUSION_ON")]
    SpecularOcclusionOn,
    [Description("_SPECULAR_OCCLUSION_LIGHTMAP_ON")]
    SpecularOcclusionLightmapOn,
    [Description("_SKYBOXFOG_ON")]
    SkyboxfogOn,
    [Description("_SKYBOXFOG_DISTANCE")]
    SkyboxfogDistance,
    [Description("_SKYBOXFOG_BOX")]
    SkyboxfogBox,
    [Description("_SKYBOXFOG_SPHERE")]
    SkyboxfogSphere,
    [Description("_IRIDESCENCE_ON")]
    IridescenceOn,
    [Description("_THINFILM_CHANNEL_R")]
    ThinfilmChannelR,
    [Description("_THINFILM_CHANNEL_G")]
    ThinfilmChannelG,
    [Description("_THINFILM_CHANNEL_B")]
    ThinfilmChannelB,
    [Description("_THINFILM_CHANNEL_A")]
    ThinfilmChannelA,
    [Description("_SSS_ON")]
    SssOn,
    [Description("_SSS_MODE_VOLUME")]
    SssModeVolume,
    [Description("_SSS_MODE_THINWALL")]
    SssModeThinwall,
    [Description("_SSS_THICKNESS_CHANNEL_R")]
    SssThicknessChannelR,
    [Description("_SSS_THICKNESS_CHANNEL_G")]
    SssThicknessChannelG,
    [Description("_SSS_THICKNESS_CHANNEL_B")]
    SssThicknessChannelB,
    [Description("_SSS_THICKNESS_CHANNEL_A")]
    SssThicknessChannelA,
    [Description("_CLEARCOAT_ON")]
    ClearcoatOn,
    [Description("_CLEARCOAT_MASK_CHANNEL_R")]
    ClearcoatMaskChannelR,
    [Description("_CLEARCOAT_MASK_CHANNEL_G")]
    ClearcoatMaskChannelG,
    [Description("_CLEARCOAT_MASK_CHANNEL_B")]
    ClearcoatMaskChannelB,
    [Description("_CLEARCOAT_MASK_CHANNEL_A")]
    ClearcoatMaskChannelA,
    [Description("_LIGHTMAP_DEFAULT")]
    LightmapDefault,
    [Description("_LIGHTMAP_SH")]
    LightmapSh,
    [Description("_LIGHTMAP_MONOSH")]
    LightmapMonosh,
    [Description("_LIGHTVOLUME_OFF")]
    LightvolumeOff,
    [Description("_LIGHTVOLUME_ON")]
    LightvolumeOn,
    [Description("_LIGHTVOLUME_MODE_REPLACE")]
    LightvolumeModeReplace,
    [Description("_LIGHTVOLUME_MODE_ADDITIVE")]
    LightvolumeModeAdditive,
    [Description("_VRC_AREALIGHT_ON")]
    VrcArealightOn,
    [Description("_AREA_LIGHT_MASK_ON")]
    AreaLightMaskOn,
    [Description("DIRECTIONAL")]
    Directional,
    [Description("LIGHTPROBE_SH")]
    LightprobeSh,
    [Description("SHADOWS_SHADOWMASK")]
    ShadowsShadowmask,
    [Description("DYNAMICLIGHTMAP_ON")]
    DynamiclightmapOn,
    [Description("LIGHTMAP_ON")]
    LightmapOn,
    [Description("LIGHTMAP_SHADOW_MIXING")]
    LightmapShadowMixing,
    [Description("DIRLIGHTMAP_COMBINED")]
    DirlightmapCombined,
    [Description("SHADOWS_SCREEN")]
    ShadowsScreen,
    [Description("VERTEXLIGHT_ON")]
    VertexlightOn,
    [Description("POINT")]
    Point,
    [Description("SPOT")]
    Spot,
    [Description("POINT_COOKIE")]
    PointCookie,
    [Description("DIRECTIONAL_COOKIE")]
    DirectionalCookie,
    [Description("SHADOWS_DEPTH")]
    ShadowsDepth,
    [Description("SHADOWS_SOFT")]
    ShadowsSoft,
    [Description("SHADOWS_CUBE")]
    ShadowsCube,
    [Description("_METALLICGLOSSMAP")]
    Metallicglossmap,
    [Description("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A")]
    SmoothnessTextureAlbedoChannelA,
    [Description("_DETAIL_MULX2")]
    DetailMulx2,
    [Description("EDITOR_VISUALIZATION")]
    EditorVisualization,
}

public static class ShaderKeywordsNames
{
    public static readonly Dictionary<ShaderKeywords, string> NameTable = new Dictionary<ShaderKeywords, string>
    {
        { ShaderKeywords.StereoInstancingOn, "STEREO_INSTANCING_ON" },
        { ShaderKeywords.UnitySinglePassStereo, "UNITY_SINGLE_PASS_STEREO" },
        { ShaderKeywords.StereoMultiviewOn, "STEREO_MULTIVIEW_ON" },
        { ShaderKeywords.StereoCubemapRenderOn, "STEREO_CUBEMAP_RENDER_ON" },
        { ShaderKeywords.Emission, "_EMISSION" },
        { ShaderKeywords.EmissionOn, "_EMISSION_ON" },
        { ShaderKeywords.LightvolumeSupport, "LIGHTVOLUME_SUPPORT" },
        { ShaderKeywords.FogLinear, "FOG_LINEAR" },
        { ShaderKeywords.FogExp, "FOG_EXP" },
        { ShaderKeywords.FogExp2, "FOG_EXP2" },
        { ShaderKeywords.InstancingOn, "INSTANCING_ON" },
        { ShaderKeywords.MaintexTilingSimple, "_MAINTEX_TILING_SIMPLE" },
        { ShaderKeywords.MaintexTilingHex, "_MAINTEX_TILING_HEX" },
        { ShaderKeywords.AlphatestOn, "_ALPHATEST_ON" },
        { ShaderKeywords.AlphablendOn, "_ALPHABLEND_ON" },
        { ShaderKeywords.AlphapremultiplyOn, "_ALPHAPREMULTIPLY_ON" },
        { ShaderKeywords.MaintexUv0, "_MAINTEX_UV0" },
        { ShaderKeywords.MaintexUv1, "_MAINTEX_UV1" },
        { ShaderKeywords.MaintexUv2, "_MAINTEX_UV2" },
        { ShaderKeywords.MaintexUv3, "_MAINTEX_UV3" },
        { ShaderKeywords.RoughnessModelRoughness, "_ROUGHNESS_MODEL_ROUGHNESS" },
        { ShaderKeywords.RoughnessModelSmoothness, "_ROUGHNESS_MODEL_SMOOTHNESS" },
        { ShaderKeywords.RoughnessChannelR, "_ROUGHNESS_CHANNEL_R" },
        { ShaderKeywords.RoughnessChannelG, "_ROUGHNESS_CHANNEL_G" },
        { ShaderKeywords.RoughnessChannelB, "_ROUGHNESS_CHANNEL_B" },
        { ShaderKeywords.RoughnessChannelA, "_ROUGHNESS_CHANNEL_A" },
        { ShaderKeywords.MetallicChannelR, "_METALLIC_CHANNEL_R" },
        { ShaderKeywords.MetallicChannelG, "_METALLIC_CHANNEL_G" },
        { ShaderKeywords.MetallicChannelB, "_METALLIC_CHANNEL_B" },
        { ShaderKeywords.MetallicChannelA, "_METALLIC_CHANNEL_A" },
        { ShaderKeywords.OcclusionChannelR, "_OCCLUSION_CHANNEL_R" },
        { ShaderKeywords.OcclusionChannelG, "_OCCLUSION_CHANNEL_G" },
        { ShaderKeywords.OcclusionChannelB, "_OCCLUSION_CHANNEL_B" },
        { ShaderKeywords.OcclusionChannelA, "_OCCLUSION_CHANNEL_A" },
        { ShaderKeywords.ThinfilmOn, "_THINFILM_ON" },
        { ShaderKeywords.HeightOn, "_HEIGHT_ON" },
        { ShaderKeywords.HeightChannelR, "_HEIGHT_CHANNEL_R" },
        { ShaderKeywords.HeightChannelG, "_HEIGHT_CHANNEL_G" },
        { ShaderKeywords.HeightChannelB, "_HEIGHT_CHANNEL_B" },
        { ShaderKeywords.HeightChannelA, "_HEIGHT_CHANNEL_A" },
        { ShaderKeywords.SpecularOcclusionOn, "_SPECULAR_OCCLUSION_ON" },
        { ShaderKeywords.SpecularOcclusionLightmapOn, "_SPECULAR_OCCLUSION_LIGHTMAP_ON" },
        { ShaderKeywords.SkyboxfogOn, "_SKYBOXFOG_ON" },
        { ShaderKeywords.SkyboxfogDistance, "_SKYBOXFOG_DISTANCE" },
        { ShaderKeywords.SkyboxfogBox, "_SKYBOXFOG_BOX" },
        { ShaderKeywords.SkyboxfogSphere, "_SKYBOXFOG_SPHERE" },
        { ShaderKeywords.IridescenceOn, "_IRIDESCENCE_ON" },
        { ShaderKeywords.ThinfilmChannelR, "_THINFILM_CHANNEL_R" },
        { ShaderKeywords.ThinfilmChannelG, "_THINFILM_CHANNEL_G" },
        { ShaderKeywords.ThinfilmChannelB, "_THINFILM_CHANNEL_B" },
        { ShaderKeywords.ThinfilmChannelA, "_THINFILM_CHANNEL_A" },
        { ShaderKeywords.SssOn, "_SSS_ON" },
        { ShaderKeywords.SssModeVolume, "_SSS_MODE_VOLUME" },
        { ShaderKeywords.SssModeThinwall, "_SSS_MODE_THINWALL" },
        { ShaderKeywords.SssThicknessChannelR, "_SSS_THICKNESS_CHANNEL_R" },
        { ShaderKeywords.SssThicknessChannelG, "_SSS_THICKNESS_CHANNEL_G" },
        { ShaderKeywords.SssThicknessChannelB, "_SSS_THICKNESS_CHANNEL_B" },
        { ShaderKeywords.SssThicknessChannelA, "_SSS_THICKNESS_CHANNEL_A" },
        { ShaderKeywords.ClearcoatOn, "_CLEARCOAT_ON" },
        { ShaderKeywords.ClearcoatMaskChannelR, "_CLEARCOAT_MASK_CHANNEL_R" },
        { ShaderKeywords.ClearcoatMaskChannelG, "_CLEARCOAT_MASK_CHANNEL_G" },
        { ShaderKeywords.ClearcoatMaskChannelB, "_CLEARCOAT_MASK_CHANNEL_B" },
        { ShaderKeywords.ClearcoatMaskChannelA, "_CLEARCOAT_MASK_CHANNEL_A" },
        { ShaderKeywords.LightmapDefault, "_LIGHTMAP_DEFAULT" },
        { ShaderKeywords.LightmapSh, "_LIGHTMAP_SH" },
        { ShaderKeywords.LightmapMonosh, "_LIGHTMAP_MONOSH" },
        { ShaderKeywords.LightvolumeOff, "_LIGHTVOLUME_OFF" },
        { ShaderKeywords.LightvolumeOn, "_LIGHTVOLUME_ON" },
        { ShaderKeywords.LightvolumeModeReplace, "_LIGHTVOLUME_MODE_REPLACE" },
        { ShaderKeywords.LightvolumeModeAdditive, "_LIGHTVOLUME_MODE_ADDITIVE" },
        { ShaderKeywords.VrcArealightOn, "_VRC_AREALIGHT_ON" },
        { ShaderKeywords.AreaLightMaskOn, "_AREA_LIGHT_MASK_ON" },
        { ShaderKeywords.Directional, "DIRECTIONAL" },
        { ShaderKeywords.LightprobeSh, "LIGHTPROBE_SH" },
        { ShaderKeywords.ShadowsShadowmask, "SHADOWS_SHADOWMASK" },
        { ShaderKeywords.DynamiclightmapOn, "DYNAMICLIGHTMAP_ON" },
        { ShaderKeywords.LightmapOn, "LIGHTMAP_ON" },
        { ShaderKeywords.LightmapShadowMixing, "LIGHTMAP_SHADOW_MIXING" },
        { ShaderKeywords.DirlightmapCombined, "DIRLIGHTMAP_COMBINED" },
        { ShaderKeywords.ShadowsScreen, "SHADOWS_SCREEN" },
        { ShaderKeywords.VertexlightOn, "VERTEXLIGHT_ON" },
        { ShaderKeywords.Point, "POINT" },
        { ShaderKeywords.Spot, "SPOT" },
        { ShaderKeywords.PointCookie, "POINT_COOKIE" },
        { ShaderKeywords.DirectionalCookie, "DIRECTIONAL_COOKIE" },
        { ShaderKeywords.ShadowsDepth, "SHADOWS_DEPTH" },
        { ShaderKeywords.ShadowsSoft, "SHADOWS_SOFT" },
        { ShaderKeywords.ShadowsCube, "SHADOWS_CUBE" },
        { ShaderKeywords.Metallicglossmap, "_METALLICGLOSSMAP" },
        { ShaderKeywords.SmoothnessTextureAlbedoChannelA, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A" },
        { ShaderKeywords.DetailMulx2, "_DETAIL_MULX2" },
        { ShaderKeywords.EditorVisualization, "EDITOR_VISUALIZATION" },
    };
public static IReadOnlyDictionary<ShaderKeywords, string> NameTableReadonly => NameTable;
}
}
