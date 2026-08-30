Shader "Takenoko/Standard"
{
    Properties
    {
        //------------
        // Error Massage
        //------------
        _ErrorMessageJPN ("これはエラーメッセージです。これが表示されている場合、何かしらのEditor拡張が壊れている可能性があります。", Int) = 0
        _ErrorMessageENG ("This is an error message. If you see this, it is likely that some editor extension is broken.", Int) = 0

        //------------
        // Pipeline
        //------------
        _AlphaMode ("Alpha Mode", Int) = 0
        _TransparentZWrite ("TransparentZWrite", Int) = 0
        _RenderQueueOffset ("Render Queue Offset", Int) = 0
        _Cutoff ("Cut Off", Range(0, 1)) = 0.5

        _UnityZWrite ("Z write", Float) = 0.0
        _UnitySrcBlend ("Alpha Src", Float) = 0.0
        _UnityDstBlend ("Alpha Dst", Float) = 0.0
        _UnityCullMode ("Cull Mode", Float) = 2.0
        _UnityAlphaToMask ("AlphaToMask", Float) = 0.0

        //------------
        // Material
        //------------
        [Enum(Simple, 0, HexTiling, 1)] _MainTexTilingMode ("Tiling Mode", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MainTexcoord ("Tiling Channel", Int) = 0
        _MainTexOffset ("Tilling Offset", Vector) = (0, 0, 1, 1)
        _MainTexScale ("Tilling Offset", Vector) = (1, 1, 1, 1)

        // hex tiling
        _HexRotationStrength ("Hex Tiling Rotation Strength", Range(0, 10)) = 0.5
        _HexFallOff ("Hex Tiling Fall Off", Range(0, 1)) = 0.5
        _HexExponent ("Hex Tiling Exponent", Range(0, 10)) = 1.0
        _HexEdgeSmoothness ("Hex Tiling Edge Smoothness", Range(0, 1)) = 0.5

        _Color ("Color", Color) = (1, 1, 1, 1) // name Unity expectes
        _MainTex ("Texture", 2D) = "white" { }// name Unity expectes
        
        [Enum(Roughness, 0, Smoothness, 1)] _RoughnessModel ("Material Mode", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _RoughnessChannel ("Roughness Channel", Int) = 0
        _Roughness ("Roughness", Range(0, 1)) = 1.0
        _RoughnessTex ("Roughness Textrue", 2D) = "white" { }

        [Enum(R, 0, G, 1, B, 2, A, 3)] _MetallicChannel ("Metaliic Channel", Int) = 1
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _MetallicTex ("Metallic Texture", 2D) = "white" { }

        [Enum(R, 0, G, 1, B, 2, A, 3)] _OcclusionChannel ("Occlusion Channel", Int) = 2
        _Occlusion ("Occlusion Scale", Range(0, 1)) = 1
        _OcclusionTex ("Occlusion Texture", 2D) = "white" { }
        _OcclusionPower ("Occlusion Power", Range(1, 20)) = 1.0

        _BumpScale ("Bump Scale", Range(0, 2)) = 1.0
        [Normal] _BumpMap ("Normal Texture", 2D) = "bump" { }

        [Toggle] _Height ("Height", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _HeightChannel ("Height Channel", Int) = 1
        _HeightTex ("Height Texture", 2D) = "white" { }
        _HeightStrength ("Height Strength", Range(0.0, 0.1)) = 0.01
        _HeightOffset ("Height Offset", Range(0.0, 1.0)) = 0.0
        _HeightStep ("Height Step", Range(1.0, 50.0)) = 10.0
        _HeightSubStep ("Height Sub-Step", Range(1.0, 10.0)) = 3.0
        [Toggle] _HeightShadow ("Height Shaddow [Experimental]", Int) = 0

        [Toggle] _Emission ("Use Emission", Int) = 0
        [HDR] _EmissionColor ("Emission color", Color) = (0, 0, 0, 0) // name Unity expectes
        _EmissionMap ("Emission texture", 2D) = "white" { }// name Unity expectes

        // Iridescence
        [Toggle] _Iridescence ("Thin Film", Int) = 0
        _ThinFilmThickness ("Film thickness", Range(0, 1)) = 0.5
        _ThinFilmThicknessTex ("Film thickness texture", 2D) = "white" { }
        [Enum(R, 0, G, 1, B, 2, A, 3)] _ThinFilmChannel ("Thin Film Channel", Int) = 1
        _ThinFilmThicknessMin ("Thickness min", Range(0, 1000)) = 100
        _ThinFilmThicknessMax ("Thickness max", Range(0, 1000)) = 200
        _ThinFilmIor ("Ior", Range(1, 3)) = 1.5

        [Toggle] _Sss ("Subsurface scattering", Int) = 0
        [Enum(Volume, 0, ThinWall, 1)] _SssMode ("SSS Mode", Int) = 0
        _SssThickness ("Thickness", Range(0, 1)) = 0.5
        _SssThicknessTex ("Thickness", 2D) = "white" { }

        [Toggle] _ClearCoat ("Clear Coat", Int) = 0
        _ClearCoatMask ("Clear Coat Mask", 2D) = "white" { }

        [Toggle] _Fuzz ("Fuzz", Int) = 0
        _FuzzMask ("Fuzz Mask", 2D) = "white" { }

        // -------------
        // Effect
        // -------------
        [Toggle] _SpecularOcclusion ("Specular Occlusion [Experimental]", Int) = 0
        [Toggle] _SpecularOcclusionLightmap ("Lightmap Occlusion[Experimental]", Int) = 1
        _SpecularOcclusionStrength ("Power", Range(0, 1)) = 1.0
        _SpecularOcclusionPower ("Power", Range(1, 20)) = 5.0

        _MaskLightProbe ("Light Probe Mask", Range(0, 2)) = 1.0
        _MaskLightmap ("Lightmap Mask", Range(0, 2)) = 1.0

        _FilterMainTexHueShift ("Hue Shift", Range(-1, 1)) = 0.0
        _FilterMainTexSaturateShift ("Saturate", Range(-1, 1)) = 0.0
        _FilterMainTexValueShift ("Value", Range(-1, 1)) = 0.0

        _FilterResultHueShift ("Hue Shift", Range(-1, 1)) = 0.0
        _FilterResultSaturateShift ("Saturate", Range(-1, 1)) = 0.0
        _FilterResultValueShift ("Value", Range(-1, 1)) = 0.0

        [Toggle] _Wetness ("Wetness", Int) = 0
        _WetnessColor ("Wetness Color", Color) = (1, 1, 1, 0)

        [Toggle] _SkyboxFog ("Skybox Fog", Int) = 0
        _SkyboxFogStrength ("Skybox Strength", Range(0, 1)) = 0.5
        [Enum(Distance, 0, Box, 1, Sphere, 2)] _SkyboxFogMode ("Skybox Fog Mode", Int) = 0
        _SkyboxFogDistance ("Skybox Fog Distance", Range(0, 1000)) = 100.0
        _SkyboxFogBoxSizeMin ("Skybox Fog Box Size Min", Vector) = (-1, -1, -1, 0)
        _SkyboxFogBoxSizeMax ("Skybox Fog Box Size Max", Vector) = (1, 1, 1, 0)
        _SkyboxFogSphereCenter ("Skybox Fog Sphere Center", Vector) = (0, 0, 0, 0)
        _SkyboxFogSphereRadius ("Skybox Fog Sphere Radius", Range(0, 1000)) = 100.0

        // -------------
        // Area Light
        // -------------
        [Toggle] _VRCAreaLight ("VRC Area Light", Int) = 0
        [Toggle] _AreaLightMask ("Area Light Mask", Int) = 0
        _AreaLightMask1 ("Area Light Mask 1", 2D) = "white" { }
        _AreaLightMask2 ("Area Light Mask 2", 2D) = "white" { }
        _AreaLightIntensity1 ("Area Light Intensity", Range(0, 2)) = 1.0
        _AreaLightIntensity2 ("Area Light Intensity", Range(0, 2)) = 1.0
        _AreaLightIntensity3 ("Area Light Intensity", Range(0, 2)) = 1.0
        _AreaLightIntensity4 ("Area Light Intensity", Range(0, 2)) = 1.0
        _AreaLightIntensity5 ("Area Light Intensity", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        //-------------------------------------------------
        // Foward Pass
        //-------------------------------------------------
        Pass
        {
            Name "FowardBase"
            Tags { "LightMode" = "ForwardBase" }

            Cull [_UnityCullMode]
            Blend [_UnitySrcBlend] [_UnityDstBlend]
            ZWrite [_UnityZWrite]
            ZTest LEqual
            BlendOp Add, Max
            AlphaToMask [_UnityAlphaToMask]

            CGPROGRAM
            #pragma vertex Takenoko_VertexStandard
            #pragma fragment Takenoko_FragmentStandard

            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            
            #pragma shader_feature _EMISSION

            // Main Texture
            #pragma shader_feature_local _MAINTEX_TILING_SIMPLE _MAINTEX_TILING_HEX
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _MAINTEX_UV0 _MAINTEX_UV1 _MAINTEX_UV2 _MAINTEX_UV3
            #pragma shader_feature_local _ROUGHNESS_MODEL_ROUGHNESS _ROUGHNESS_MODEL_SMOOTHNESS
            #pragma shader_feature_local _ROUGHNESS_CHANNEL_R _ROUGHNESS_CHANNEL_G _ROUGHNESS_CHANNEL_B _ROUGHNESS_CHANNEL_A
            #pragma shader_feature_local _METALLIC_CHANNEL_R _METALLIC_CHANNEL_G _METALLIC_CHANNEL_B _METALLIC_CHANNEL_A
            #pragma shader_feature_local _OCCLUSION_CHANNEL_R _OCCLUSION_CHANNEL_G _OCCLUSION_CHANNEL_B _OCCLUSION_CHANNEL_A
            #pragma shader_feature_local _THINFILM_ON

            #pragma shader_feature_local _HEIGHT_ON
            #pragma shader_feature_local _HEIGHT_CHANNEL_R _HEIGHT_CHANNEL_G _HEIGHT_CHANNEL_B _HEIGHT_CHANNEL_A

            #pragma shader_feature _EMISSION_ON

            #pragma shader_feature_local _SPECULAR_OCCLUSION_ON
            #pragma shader_feature_local _SPECULAR_OCCLUSION_LIGHTMAP_ON
            #pragma shader_feature_local _SKYBOXFOG_ON
            #pragma shader_feature_local _SKYBOXFOG_DISTANCE _SKYBOXFOG_BOX _SKYBOXFOG_SPHERE

            #pragma shader_feature_local _IRIDESCENCE_ON
            #pragma shader_feature_local _SSS_ON
            #pragma shader_feature_local _SSS_MODE_VOLUME _SSS_MODE_THINWALL

            #pragma shader_feature_local _LIGHTMAP_DEFAULT _LIGHTMAP_SH _LIGHTMAP_MONOSH
            
            // VRC Light Volume
            #pragma shader_feature_local _LIGHTVOLUME_OFF _LIGHTVOLUME_ON
            #pragma shader_feature_local _LIGHTVOLUME_MODE_REPLACE _LIGHTVOLUME_MODE_ADDITIVE
            #pragma multi_compile _ LIGHTVOLUME_SUPPORT
            
            // VRC Area Light
            #pragma shader_feature_local _VRC_AREALIGHT_ON
            #pragma shader_feature_local _AREA_LIGHT_MASK_ON

            #define _TAKENOKO_FOWARD_BASE

            #include "Standard/Takenoko_StandardVertex.hlsl"
            #include "Standard/Takenoko_StandardFragment.hlsl"

            ENDCG
        }

        //-------------------------------------------------
        // FowardAdd Pass
        //-------------------------------------------------

        Pass
        {
            Name "FowardAdd"
            Tags { "LightMode" = "ForwardAdd" }

            Cull [_UnityCullMode]
            Blend One One
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex Takenoko_VertexStandard
            #pragma fragment Takenoko_FragmentStandard

            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_instancing

            // Main Texture
            #pragma shader_feature_local _MAINTEX_TILING_SIMPLE _MAINTEX_TILING_HEX
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _MAINTEX_UV0 _MAINTEX_UV1 _MAINTEX_UV2 _MAINTEX_UV3
            #pragma shader_feature_local _ROUGHNESS_MODEL_ROUGHNESS _ROUGHNESS_MODEL_SMOOTHNESS
            #pragma shader_feature_local _ROUGHNESS_CHANNEL_R _ROUGHNESS_CHANNEL_G _ROUGHNESS_CHANNEL_B _ROUGHNESS_CHANNEL_A
            #pragma shader_feature_local _METALLIC_CHANNEL_R _METALLIC_CHANNEL_G _METALLIC_CHANNEL_B _METALLIC_CHANNEL_A
            #pragma shader_feature_local _OCCLUSION_CHANNEL_R _OCCLUSION_CHANNEL_G _OCCLUSION_CHANNEL_B _OCCLUSION_CHANNEL_A
            
            #pragma shader_feature_local _IRIDESCENCE_ON
            #pragma shader_feature_local _SSS_ON
            #pragma shader_feature_local _SSS_MODE_VOLUME _SSS_MODE_THINWALL

            #pragma shader_feature_local _HEIGHT_ON
            #pragma shader_feature_local _HEIGHT_CHANNEL_R _HEIGHT_CHANNEL_G _HEIGHT_CHANNEL_B _HEIGHT_CHANNEL_A

            #define _TAKENOKO_FOWARD_ADD

            #include "Standard/Takenoko_StandardVertex.hlsl"
            #include "Standard/Takenoko_StandardFragment.hlsl"

            ENDCG
        }

        //-------------------------------------------------
        // Shadow Caster Pass
        //-------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_UnityCullMode]
            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }

        //-------------------------------------------------
        // Meta Pass
        //-------------------------------------------------
        Pass
        {
            // TODO : Original Meta Pass for Metalness Workflow

            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma multi_compile_instancing
            
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    CustomEditor "Takenoko.Standard.StandardGUI"
}
