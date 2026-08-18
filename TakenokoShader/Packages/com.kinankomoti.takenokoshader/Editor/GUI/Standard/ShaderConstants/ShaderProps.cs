using System.Collections.Generic;
using System.ComponentModel;
namespace Takenoko.Standard
{
    public enum ShaderProps
    {
        [Description("_ErrorMessage_JPN")]
        ErrorMessage_JPN,
        [Description("_ErrorMessage_ENG")]
        ErrorMessage_ENG,
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
        [Description("_MaintexOffset")]
        MaintexOffset,
        [Description("_MaintexScale")]
        MaintexScale,
        [Description("_Hex_RotationStrength")]
        Hex_RotationStrength,
        [Description("_Hex_FallOff")]
        Hex_FallOff,
        [Description("_Hex_Exponent")]
        Hex_Exponent,
        [Description("_Hex_EdgeSmoothness")]
        Hex_EdgeSmoothness,
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
        [Description("_VRC_AreaLight")]
        VRC_AreaLight,
        [Description("_Area_Light_Mask")]
        Area_Light_Mask,
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
        [Description("_Specular_Occlusion")]
        Specular_Occlusion,
        [Description("_Specular_Occlusion_Lightmap")]
        Specular_Occlusion_Lightmap,
        [Description("_SpecularOcclusionStrength")]
        SpecularOcclusionStrength,
        [Description("_SpecularOcclusionPower")]
        SpecularOcclusionPower,
        [Description("_MaskLightProbe")]
        MaskLightProbe,
        [Description("_MaskLightmap")]
        MaskLightmap,
        [Description("_Filter_MainTex_HueShift")]
        Filter_MainTex_HueShift,
        [Description("_Filter_MainTex_SaturateShift")]
        Filter_MainTex_SaturateShift,
        [Description("_Filter_MainTex_ValueShift")]
        Filter_MainTex_ValueShift,
        [Description("_Filter_Result_HueShift")]
        Filter_Result_HueShift,
        [Description("_Filter_Result_SaturateShift")]
        Filter_Result_SaturateShift,
        [Description("_Filter_Result_ValueShift")]
        Filter_Result_ValueShift,
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
        { ShaderProps.ErrorMessage_JPN, "_ErrorMessage_JPN" },
        { ShaderProps.ErrorMessage_ENG, "_ErrorMessage_ENG" },
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
        { ShaderProps.MaintexOffset, "_MaintexOffset" },
        { ShaderProps.MaintexScale, "_MaintexScale" },
        { ShaderProps.Hex_RotationStrength, "_Hex_RotationStrength" },
        { ShaderProps.Hex_FallOff, "_Hex_FallOff" },
        { ShaderProps.Hex_Exponent, "_Hex_Exponent" },
        { ShaderProps.Hex_EdgeSmoothness, "_Hex_EdgeSmoothness" },
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
        { ShaderProps.VRC_AreaLight, "_VRC_AreaLight" },
        { ShaderProps.Area_Light_Mask, "_Area_Light_Mask" },
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
        { ShaderProps.Specular_Occlusion, "_Specular_Occlusion" },
        { ShaderProps.Specular_Occlusion_Lightmap, "_Specular_Occlusion_Lightmap" },
        { ShaderProps.SpecularOcclusionStrength, "_SpecularOcclusionStrength" },
        { ShaderProps.SpecularOcclusionPower, "_SpecularOcclusionPower" },
        { ShaderProps.MaskLightProbe, "_MaskLightProbe" },
        { ShaderProps.MaskLightmap, "_MaskLightmap" },
        { ShaderProps.Filter_MainTex_HueShift, "_Filter_MainTex_HueShift" },
        { ShaderProps.Filter_MainTex_SaturateShift, "_Filter_MainTex_SaturateShift" },
        { ShaderProps.Filter_MainTex_ValueShift, "_Filter_MainTex_ValueShift" },
        { ShaderProps.Filter_Result_HueShift, "_Filter_Result_HueShift" },
        { ShaderProps.Filter_Result_SaturateShift, "_Filter_Result_SaturateShift" },
        { ShaderProps.Filter_Result_ValueShift, "_Filter_Result_ValueShift" },
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
