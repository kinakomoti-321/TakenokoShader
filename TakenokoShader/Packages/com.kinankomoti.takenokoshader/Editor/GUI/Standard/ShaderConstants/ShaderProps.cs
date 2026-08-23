using System.Collections.Generic;
using System.ComponentModel;
namespace Takenoko.Standard
{
public enum ShaderProps
{
    [Description("_ErrorMessageJPN")]
    ErrorMessageJPN,
    [Description("_ErrorMessageENG")]
    ErrorMessageENG,
    [Description("_AlphaMode")]
    AlphaMode,
    [Description("_TransparentZWrite")]
    TransparentZWrite,
    [Description("_RenderQueueOffset")]
    RenderQueueOffset,
    [Description("_Cutoff")]
    Cutoff,
    [Description("_UnityZWrite")]
    UnityZWrite,
    [Description("_UnitySrcBlend")]
    UnitySrcBlend,
    [Description("_UnityDstBlend")]
    UnityDstBlend,
    [Description("_UnityCullMode")]
    UnityCullMode,
    [Description("_UnityAlphaToMask")]
    UnityAlphaToMask,
    [Description("_MainTexTilingMode")]
    MainTexTilingMode,
    [Description("_MainTexcoord")]
    MainTexcoord,
    [Description("_MainTexOffset")]
    MainTexOffset,
    [Description("_MainTexScale")]
    MainTexScale,
    [Description("_HexRotationStrength")]
    HexRotationStrength,
    [Description("_HexFallOff")]
    HexFallOff,
    [Description("_HexExponent")]
    HexExponent,
    [Description("_HexEdgeSmoothness")]
    HexEdgeSmoothness,
    [Description("_Color")]
    Color,
    [Description("_MainTex")]
    MainTex,
    [Description("_RoughnessModel")]
    RoughnessModel,
    [Description("_RoughnessChannel")]
    RoughnessChannel,
    [Description("_Roughness")]
    Roughness,
    [Description("_RoughnessTex")]
    RoughnessTex,
    [Description("_MetallicChannel")]
    MetallicChannel,
    [Description("_Metallic")]
    Metallic,
    [Description("_MetallicTex")]
    MetallicTex,
    [Description("_OcclusionChannel")]
    OcclusionChannel,
    [Description("_Occlusion")]
    Occlusion,
    [Description("_OcclusionTex")]
    OcclusionTex,
    [Description("_OcclusionPower")]
    OcclusionPower,
    [Description("_BumpScale")]
    BumpScale,
    [Description("_BumpMap")]
    BumpMap,
    [Description("_Height")]
    Height,
    [Description("_HeightChannel")]
    HeightChannel,
    [Description("_HeightTex")]
    HeightTex,
    [Description("_HeightStrength")]
    HeightStrength,
    [Description("_HeightOffset")]
    HeightOffset,
    [Description("_HeightStep")]
    HeightStep,
    [Description("_HeightSubStep")]
    HeightSubStep,
    [Description("_HeightShadow")]
    HeightShadow,
    [Description("_Emission")]
    Emission,
    [Description("_EmissionColor")]
    EmissionColor,
    [Description("_EmissionMap")]
    EmissionMap,
    [Description("_ThinFilm")]
    ThinFilm,
    [Description("_ThinFilmThickness")]
    ThinFilmThickness,
    [Description("_ThinFilmThicknessTex")]
    ThinFilmThicknessTex,
    [Description("_ThinFilmChannel")]
    ThinFilmChannel,
    [Description("_ThinFilmThicknessMin")]
    ThinFilmThicknessMin,
    [Description("_ThinFilmThicknessMax")]
    ThinFilmThicknessMax,
    [Description("_ThinFilmIor")]
    ThinFilmIor,
    [Description("_VRCAreaLight")]
    VRCAreaLight,
    [Description("_AreaLightMask")]
    AreaLightMask,
    [Description("_AreaLightMask1")]
    AreaLightMask1,
    [Description("_AreaLightMask2")]
    AreaLightMask2,
    [Description("_AreaLightIntensity1")]
    AreaLightIntensity1,
    [Description("_AreaLightIntensity2")]
    AreaLightIntensity2,
    [Description("_AreaLightIntensity3")]
    AreaLightIntensity3,
    [Description("_AreaLightIntensity4")]
    AreaLightIntensity4,
    [Description("_AreaLightIntensity5")]
    AreaLightIntensity5,
    [Description("_LightVolume")]
    LightVolume,
    [Description("_LightVolumeMode")]
    LightVolumeMode,
    [Description("_LightVolumeStrength")]
    LightVolumeStrength,
    [Description("_LightVolumeColorModulation")]
    LightVolumeColorModulation,
    [Description("_LightVolumeIntensityMultiplier")]
    LightVolumeIntensityMultiplier,
    [Description("_SpecularOcclusion")]
    SpecularOcclusion,
    [Description("_SpecularOcclusionLightmap")]
    SpecularOcclusionLightmap,
    [Description("_SpecularOcclusionStrength")]
    SpecularOcclusionStrength,
    [Description("_SpecularOcclusionPower")]
    SpecularOcclusionPower,
    [Description("_MaskLightProbe")]
    MaskLightProbe,
    [Description("_MaskLightmap")]
    MaskLightmap,
    [Description("_FilterMainTexHueShift")]
    FilterMainTexHueShift,
    [Description("_FilterMainTexSaturateShift")]
    FilterMainTexSaturateShift,
    [Description("_FilterMainTexValueShift")]
    FilterMainTexValueShift,
    [Description("_FilterResultHueShift")]
    FilterResultHueShift,
    [Description("_FilterResultSaturateShift")]
    FilterResultSaturateShift,
    [Description("_FilterResultValueShift")]
    FilterResultValueShift,
    [Description("_Wetness")]
    Wetness,
    [Description("_WetnessColor")]
    WetnessColor,
    [Description("_SkyboxFog")]
    SkyboxFog,
    [Description("_SkyboxFogStrength")]
    SkyboxFogStrength,
    [Description("_SkyboxFogMode")]
    SkyboxFogMode,
    [Description("_SkyboxFogDistance")]
    SkyboxFogDistance,
    [Description("_SkyboxFogBoxSizeMin")]
    SkyboxFogBoxSizeMin,
    [Description("_SkyboxFogBoxSizeMax")]
    SkyboxFogBoxSizeMax,
    [Description("_SkyboxFogSphereCenter")]
    SkyboxFogSphereCenter,
    [Description("_SkyboxFogSphereRadius")]
    SkyboxFogSphereRadius,
}

public static class ShaderPropsName
{
    public static readonly Dictionary<ShaderProps, string> NameTable = new Dictionary<ShaderProps, string>
    {
        { ShaderProps.ErrorMessageJPN, "_ErrorMessageJPN" },
        { ShaderProps.ErrorMessageENG, "_ErrorMessageENG" },
        { ShaderProps.AlphaMode, "_AlphaMode" },
        { ShaderProps.TransparentZWrite, "_TransparentZWrite" },
        { ShaderProps.RenderQueueOffset, "_RenderQueueOffset" },
        { ShaderProps.Cutoff, "_Cutoff" },
        { ShaderProps.UnityZWrite, "_UnityZWrite" },
        { ShaderProps.UnitySrcBlend, "_UnitySrcBlend" },
        { ShaderProps.UnityDstBlend, "_UnityDstBlend" },
        { ShaderProps.UnityCullMode, "_UnityCullMode" },
        { ShaderProps.UnityAlphaToMask, "_UnityAlphaToMask" },
        { ShaderProps.MainTexTilingMode, "_MainTexTilingMode" },
        { ShaderProps.MainTexcoord, "_MainTexcoord" },
        { ShaderProps.MainTexOffset, "_MainTexOffset" },
        { ShaderProps.MainTexScale, "_MainTexScale" },
        { ShaderProps.HexRotationStrength, "_HexRotationStrength" },
        { ShaderProps.HexFallOff, "_HexFallOff" },
        { ShaderProps.HexExponent, "_HexExponent" },
        { ShaderProps.HexEdgeSmoothness, "_HexEdgeSmoothness" },
        { ShaderProps.Color, "_Color" },
        { ShaderProps.MainTex, "_MainTex" },
        { ShaderProps.RoughnessModel, "_RoughnessModel" },
        { ShaderProps.RoughnessChannel, "_RoughnessChannel" },
        { ShaderProps.Roughness, "_Roughness" },
        { ShaderProps.RoughnessTex, "_RoughnessTex" },
        { ShaderProps.MetallicChannel, "_MetallicChannel" },
        { ShaderProps.Metallic, "_Metallic" },
        { ShaderProps.MetallicTex, "_MetallicTex" },
        { ShaderProps.OcclusionChannel, "_OcclusionChannel" },
        { ShaderProps.Occlusion, "_Occlusion" },
        { ShaderProps.OcclusionTex, "_OcclusionTex" },
        { ShaderProps.OcclusionPower, "_OcclusionPower" },
        { ShaderProps.BumpScale, "_BumpScale" },
        { ShaderProps.BumpMap, "_BumpMap" },
        { ShaderProps.Height, "_Height" },
        { ShaderProps.HeightChannel, "_HeightChannel" },
        { ShaderProps.HeightTex, "_HeightTex" },
        { ShaderProps.HeightStrength, "_HeightStrength" },
        { ShaderProps.HeightOffset, "_HeightOffset" },
        { ShaderProps.HeightStep, "_HeightStep" },
        { ShaderProps.HeightSubStep, "_HeightSubStep" },
        { ShaderProps.HeightShadow, "_HeightShadow" },
        { ShaderProps.Emission, "_Emission" },
        { ShaderProps.EmissionColor, "_EmissionColor" },
        { ShaderProps.EmissionMap, "_EmissionMap" },
        { ShaderProps.ThinFilm, "_ThinFilm" },
        { ShaderProps.ThinFilmThickness, "_ThinFilmThickness" },
        { ShaderProps.ThinFilmThicknessTex, "_ThinFilmThicknessTex" },
        { ShaderProps.ThinFilmChannel, "_ThinFilmChannel" },
        { ShaderProps.ThinFilmThicknessMin, "_ThinFilmThicknessMin" },
        { ShaderProps.ThinFilmThicknessMax, "_ThinFilmThicknessMax" },
        { ShaderProps.ThinFilmIor, "_ThinFilmIor" },
        { ShaderProps.VRCAreaLight, "_VRCAreaLight" },
        { ShaderProps.AreaLightMask, "_AreaLightMask" },
        { ShaderProps.AreaLightMask1, "_AreaLightMask1" },
        { ShaderProps.AreaLightMask2, "_AreaLightMask2" },
        { ShaderProps.AreaLightIntensity1, "_AreaLightIntensity1" },
        { ShaderProps.AreaLightIntensity2, "_AreaLightIntensity2" },
        { ShaderProps.AreaLightIntensity3, "_AreaLightIntensity3" },
        { ShaderProps.AreaLightIntensity4, "_AreaLightIntensity4" },
        { ShaderProps.AreaLightIntensity5, "_AreaLightIntensity5" },
        { ShaderProps.LightVolume, "_LightVolume" },
        { ShaderProps.LightVolumeMode, "_LightVolumeMode" },
        { ShaderProps.LightVolumeStrength, "_LightVolumeStrength" },
        { ShaderProps.LightVolumeColorModulation, "_LightVolumeColorModulation" },
        { ShaderProps.LightVolumeIntensityMultiplier, "_LightVolumeIntensityMultiplier" },
        { ShaderProps.SpecularOcclusion, "_SpecularOcclusion" },
        { ShaderProps.SpecularOcclusionLightmap, "_SpecularOcclusionLightmap" },
        { ShaderProps.SpecularOcclusionStrength, "_SpecularOcclusionStrength" },
        { ShaderProps.SpecularOcclusionPower, "_SpecularOcclusionPower" },
        { ShaderProps.MaskLightProbe, "_MaskLightProbe" },
        { ShaderProps.MaskLightmap, "_MaskLightmap" },
        { ShaderProps.FilterMainTexHueShift, "_FilterMainTexHueShift" },
        { ShaderProps.FilterMainTexSaturateShift, "_FilterMainTexSaturateShift" },
        { ShaderProps.FilterMainTexValueShift, "_FilterMainTexValueShift" },
        { ShaderProps.FilterResultHueShift, "_FilterResultHueShift" },
        { ShaderProps.FilterResultSaturateShift, "_FilterResultSaturateShift" },
        { ShaderProps.FilterResultValueShift, "_FilterResultValueShift" },
        { ShaderProps.Wetness, "_Wetness" },
        { ShaderProps.WetnessColor, "_WetnessColor" },
        { ShaderProps.SkyboxFog, "_SkyboxFog" },
        { ShaderProps.SkyboxFogStrength, "_SkyboxFogStrength" },
        { ShaderProps.SkyboxFogMode, "_SkyboxFogMode" },
        { ShaderProps.SkyboxFogDistance, "_SkyboxFogDistance" },
        { ShaderProps.SkyboxFogBoxSizeMin, "_SkyboxFogBoxSizeMin" },
        { ShaderProps.SkyboxFogBoxSizeMax, "_SkyboxFogBoxSizeMax" },
        { ShaderProps.SkyboxFogSphereCenter, "_SkyboxFogSphereCenter" },
        { ShaderProps.SkyboxFogSphereRadius, "_SkyboxFogSphereRadius" },
    };
public static IReadOnlyDictionary<ShaderProps, string> NameTableReadonly => NameTable;
}
}
