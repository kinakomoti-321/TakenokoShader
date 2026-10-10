using System;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Takenoko.Standard
{
    using Tempura;
    using static Tempura.TempuraGui;

    public class StandardGUI : ShaderGUI
    {
        private bool _initialized = false;
        public class Settings : ScriptableSingleton<Settings>
        {
            public bool foldRenderMenu = false;
            public bool foldMaterialMenu = false;
            public bool foldLightingMenu = false;
            public bool foldEffectMenu = false;
            public bool foldAreaLightMenu = false;
            public bool foldDebugMenu = false;
        }

        public static Settings settings { get { return Settings.instance; } }

        // VRC Light Volumes defines VRC_LIGHT_VOLUMES itself from an
        // [InitializeOnLoad] hook, so nothing here has to go looking for it:
        // installing or removing the package recompiles this file with the other
        // branch, and the shader keyword follows on the next material inspection.
        private static bool IsLightVolumeAvailable()
        {
#if VRC_LIGHT_VOLUMES
            return true;
#else
            return false;
#endif
        }

        private static bool IsSrgbTexture(Texture texture)
        {
            if (texture == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            return importer != null && importer.sRGBTexture;
        }

        private static void DrawLinearTextureWarning(Material[] materials, string propertyName, string label)
        {
            var importers = materials
                .Where(material => material != null)
                .Select(material => material.GetTexture(propertyName))
                .Where(IsSrgbTexture)
                .Select(texture => AssetDatabase.GetAssetPath(texture))
                .Distinct()
                .Select(AssetImporter.GetAtPath)
                .OfType<TextureImporter>()
                .ToArray();

            if (importers.Length == 0)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                $"{label} はリニアデータ用テクスチャです。Texture Import Settings の sRGB (Color Texture) を無効にしてください。",
                MessageType.Warning);

            if (GUILayout.Button("Fix now", EditorStyles.miniButton))
            {
                foreach (TextureImporter importer in importers)
                {
                    importer.sRGBTexture = false;
                    importer.SaveAndReimport();
                }

                GUIUtility.ExitGUI();
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var props = ShaderPropsName.NameTable.ToDictionary(x => x.Key, x => FindProperty(x.Value, properties));

            var material = materialEditor.target as Material;
            var materials = materialEditor.targets.Select(m => m as Material).ToArray();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Takenoko Shader", EditorStyles.boldLabel);
            TempuraGui.Space4();

            //-----------------------------------------------
            // Pipeline
            //-----------------------------------------------
            #region Pipeline
            settings.foldRenderMenu = TempuraGui.FoldOut("Pipeline", "", settings.foldRenderMenu);
            if (settings.foldRenderMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {

                    TempuraGui.Space2();
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        TempuraGui.Popup<AlphaMode>("Alpha mode", props[ShaderProps.AlphaMode], materialEditor);
                        var alphaMode = (AlphaMode)(int)props[ShaderProps.AlphaMode].floatValue;
                        if (alphaMode == AlphaMode.Cutout)
                        {
                            using (new VerticalScope(TempuraGui.smallBox))
                            {
                                materialEditor.ShaderProperty(props[ShaderProps.Cutoff], new GUIContent("Cutout"));
                            }
                        }
                        TempuraGui.Popup<CullMode>("Cull mode", props[ShaderProps.UnityCullMode], materialEditor);

                        materialEditor.EnableInstancingField();
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        EditorGUILayout.LabelField("Render queue", material.renderQueue.ToString());
                        TempuraGui.IntField("Render queue offset", props[ShaderProps.RenderQueueOffset], materialEditor);
                    }
                }
                TempuraGui.Space2();
            }
            #endregion


            //-----------------------------------------------
            // Material Parameter
            //-----------------------------------------------
            #region MaterialParameter
            settings.foldMaterialMenu = TempuraGui.FoldOut("Material parameter", "", settings.foldMaterialMenu);
            if (settings.foldMaterialMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        EditorGUILayout.LabelField("Main texture", EditorStyles.boldLabel);
                        using (new VerticalScope(TempuraGui.borderBox))
                        {
                            // MainTex
                            materialEditor.ShaderProperty(props[ShaderProps.MainTexTilingMode], new GUIContent("Tiling mode"));

                            if (props[ShaderProps.MainTexTilingMode].hasMixedValue ||
                                (TilingMode)(int)props[ShaderProps.MainTexTilingMode].floatValue != TilingMode.Triplanar)
                                TempuraGui.Popup<Texcoord>("MainTex texcoord", props[ShaderProps.MainTexcoord], materialEditor);
                            materialEditor.ShaderProperty(props[ShaderProps.MainTexOffset], new GUIContent("MainTex offset"));
                            materialEditor.ShaderProperty(props[ShaderProps.MainTexScale], new GUIContent("MainTex scale"));
                        }

                        using (new VerticalScope(TempuraGui.borderBox))
                        {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Basecolor"), props[ShaderProps.MainTex], props[ShaderProps.Color]);
                            TempuraGui.Popup<RoughnessModel>("Workflow", props[ShaderProps.RoughnessModel], materialEditor);
                            RoughnessModel roughnessModel = Cast<RoughnessModel>(material, ShaderProps.RoughnessModel.Name());
                            if (roughnessModel == RoughnessModel.Roughness)
                            {
                                materialEditor.TexturePropertyTwoLines(new GUIContent("Roughness"), props[ShaderProps.RoughnessTex], props[ShaderProps.Roughness], new GUIContent("Channel"), props[ShaderProps.RoughnessChannel]);
                            }
                            else
                            {
                                materialEditor.TexturePropertyTwoLines(new GUIContent("Smoothness"), props[ShaderProps.RoughnessTex], props[ShaderProps.Roughness], new GUIContent("Channel"), props[ShaderProps.RoughnessChannel]);
                            }
                            DrawLinearTextureWarning(materials, ShaderProps.RoughnessTex.Name(), "Roughness");

                            materialEditor.TexturePropertyTwoLines(new GUIContent("Metallic"), props[ShaderProps.MetallicTex], props[ShaderProps.Metallic], new GUIContent("Channel"), props[ShaderProps.MetallicChannel]);
                            DrawLinearTextureWarning(materials, ShaderProps.MetallicTex.Name(), "Metallic");
                            materialEditor.TexturePropertyTwoLines(new GUIContent("Occlusion"), props[ShaderProps.OcclusionTex], props[ShaderProps.Occlusion], new GUIContent("Channel"), props[ShaderProps.OcclusionChannel]);
                            materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), props[ShaderProps.BumpMap], props[ShaderProps.BumpScale]);
                        }

                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.SecondTexture], new GUIContent("Second texture"));
                        if (props[ShaderProps.SecondTexture].hasMixedValue || Cast(props[ShaderProps.SecondTexture].floatValue))
                        {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Mask"), props[ShaderProps.SecondMask]);
                            TempuraGui.Popup<TextureChannel>("Channel", props[ShaderProps.SecondMaskChannel], materialEditor);
                            TempuraGui.Popup<Texcoord>("UV", props[ShaderProps.SecondTexcoord], materialEditor);
                            materialEditor.TextureScaleOffsetProperty(props[ShaderProps.SecondAlbedo]);
                            DrawLinearTextureWarning(materials, ShaderProps.SecondMask.Name(), "Mask");
                            materialEditor.ShaderProperty(props[ShaderProps.SecondBlendMode], new GUIContent("Blend mode"));
                            materialEditor.TexturePropertySingleLine(new GUIContent("Basecolor"), props[ShaderProps.SecondAlbedo], props[ShaderProps.SecondColor]);
                            materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), props[ShaderProps.SecondNormal], props[ShaderProps.SecondNormalScale]);
                        }
                    }

                    // TODO : Height
                    // using (new VerticalScope(TempuraGui.borderBox))
                    // {
                    //     materialEditor.ShaderProperty(props[ShaderProps.Height], new GUIContent("Height"));
                    //     bool useHeight = Cast(props[ShaderProps.Height].floatValue);
                    //     if (useHeight)
                    //     {
                    //         TempuraGui.Space2();
                    //         materialEditor.TexturePropertySingleLine(new GUIContent("Height"), props[ShaderProps.HeightTex], props[ShaderProps.HeightStrength]);
                    //         DrawLinearTextureWarning(materials, ShaderProps.HeightTex.Name(), "Height");
                    //         TempuraGui.Popup<TextureChannel>("Height channel", props[ShaderProps.HeightChannel], materialEditor);
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightOffset], new GUIContent("Height offset"));
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightStep], new GUIContent("Height step"));
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightSubStep], new GUIContent("Height sub step"));
                    //     }
                    // }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.Emission], new GUIContent("Emission"));
                        bool useEmission = Cast(props[ShaderProps.Emission].floatValue);
                        if (useEmission)
                        {
                            TempuraGui.Space2();
                            materialEditor.TexturePropertySingleLine(new GUIContent("Emission"), props[ShaderProps.EmissionMap], props[ShaderProps.EmissionColor]);
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.Iridescence], new GUIContent("Iridescence"));
                        bool useIridescence = Cast(props[ShaderProps.Iridescence].floatValue);
                        if (useIridescence)
                        {
                            TempuraGui.Space2();
                            materialEditor.TexturePropertyTwoLines(new GUIContent("Film thickness"), props[ShaderProps.ThinFilmThicknessTex], props[ShaderProps.ThinFilmThickness], new GUIContent("Channel"), props[ShaderProps.ThinFilmChannel]);
                            DrawLinearTextureWarning(materials, ShaderProps.ThinFilmThicknessTex.Name(), "Thin film thickness");
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmThicknessMin], new GUIContent("Thickness min"));
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmThicknessMax], new GUIContent("Thickness max"));
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmIor], new GUIContent("Ior"));
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.Sss], new GUIContent("Subsurface scattering"));
                        bool useSss = Cast(props[ShaderProps.Sss].floatValue);
                        if (useSss)
                        {
                            TempuraGui.Space2();

                            DrawLinearTextureWarning(materials, ShaderProps.SssThicknessTex.Name(), "SSS thickness");
                            materialEditor.TexturePropertyTwoLines(new GUIContent("Thickness"), props[ShaderProps.SssThicknessTex], props[ShaderProps.SssThickness], new GUIContent("Channel"), props[ShaderProps.SssThicknessChannel]);
                            materialEditor.TexturePropertySingleLine(new GUIContent("SSS color"), props[ShaderProps.SssColorTex], props[ShaderProps.SssColor]);
                            materialEditor.ShaderProperty(props[ShaderProps.SssFrontPower], new GUIContent("Front intensity"));
                            materialEditor.ShaderProperty(props[ShaderProps.SssBackPower], new GUIContent("Back intensity"));
                            materialEditor.ShaderProperty(props[ShaderProps.SssAnisotropy], new GUIContent("Anisotropy"));
                            materialEditor.ShaderProperty(props[ShaderProps.SssShadowStrength], new GUIContent("Shadow intensity"));
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.ClearCoat], new GUIContent("ClearCoat"));
                        bool useClearCoat = Cast(props[ShaderProps.ClearCoat].floatValue);
                        if (useClearCoat)
                        {
                            TempuraGui.Space2();
                            materialEditor.ShaderProperty(props[ShaderProps.ClearCoatStrength], new GUIContent("Intensity"));
                            // materialEditor.TexturePropertyTwoLines(new GUIContent("Mask"), props[ShaderProps.ClearCoatMask], null, new GUIContent("Channel"), props[ShaderProps.ClearCoatMaskChannel]);
                        }
                    }
                }
                TempuraGui.Space2();
            }
            #endregion

            //-----------------------------------------------
            // Lighting
            //-----------------------------------------------
            #region Lighting
            settings.foldLightingMenu = TempuraGui.FoldOut("Lighting", "", settings.foldLightingMenu);
            if (settings.foldLightingMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusion], new GUIContent("Specular occlusion"));
                        bool useSpecularOcclusion = Cast(props[ShaderProps.SpecularOcclusion].floatValue);
                        if (useSpecularOcclusion)
                        {
                            materialEditor.ShaderProperty(props[ShaderProps.OcclusionIntesity], new GUIContent("Intensity"));
                            materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusionLightmap], new GUIContent("Use light map"));
                        }
                    }


                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.EstimateSHDirection], new GUIContent("Estimate SH direction"));
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.MaskLightProbe], new GUIContent("Light probe intensity"));
                        materialEditor.ShaderProperty(props[ShaderProps.MaskLightmap], new GUIContent("Lightmap intensity"));
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        EditorGUILayout.LabelField("Lightmap", EditorStyles.boldLabel);
                        TempuraGui.Popup<LightmapMode>("Format", props[ShaderProps.LightmapMode], materialEditor);

                        var lightmapMode = Cast<LightmapMode>(material, ShaderProps.LightmapMode.Name());
                        if (lightmapMode == LightmapMode.SH || lightmapMode == LightmapMode.MonoSH)
                        {
                            materialEditor.ShaderProperty(props[ShaderProps.LightmapNonLinearSh], new GUIContent("Non-linear SH"));
                        }

                        TempuraGui.Space2();

                        EditorGUILayout.LabelField("Additional lightmap", EditorStyles.boldLabel);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Lightmap 1"), props[ShaderProps.AdditionalLightmap1], props[ShaderProps.AdditionalLightmapStrength1]);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Lightmap 2"), props[ShaderProps.AdditionalLightmap2], props[ShaderProps.AdditionalLightmapStrength2]);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Lightmap 3"), props[ShaderProps.AdditionalLightmap3], props[ShaderProps.AdditionalLightmapStrength3]);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Lightmap 4"), props[ShaderProps.AdditionalLightmap4], props[ShaderProps.AdditionalLightmapStrength4]);
                    }
                }
            }
            #endregion


            //-----------------------------------------------
            // Effect
            //-----------------------------------------------
            #region Effect
            settings.foldEffectMenu = TempuraGui.FoldOut("Effect", "", settings.foldEffectMenu);
            if (settings.foldEffectMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        EditorGUILayout.LabelField("Filter", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("Basecolor", EditorStyles.boldLabel);
                        materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexHueShift], new GUIContent("Hue shift"));
                        materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexSaturateShift], new GUIContent("Saturate"));
                        materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexValueShift], new GUIContent("Value"));

                        EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
                        materialEditor.ShaderProperty(props[ShaderProps.FilterResultHueShift], new GUIContent("Hue shift"));
                        materialEditor.ShaderProperty(props[ShaderProps.FilterResultSaturateShift], new GUIContent("Saturate"));
                        materialEditor.ShaderProperty(props[ShaderProps.FilterResultValueShift], new GUIContent("Value"));
                    }


                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.SkyboxFog], new GUIContent("Skybox fog"));
                        bool useSkyboxFog = Cast(props[ShaderProps.SkyboxFog].floatValue);

                        if (useSkyboxFog)
                        {
                            using (new VerticalScope(TempuraGui.borderBox))
                            {
                                TempuraGui.Popup<SkyboxFogMode>("Skybox fog mode", props[ShaderProps.SkyboxFogMode], materialEditor);
                                var skyboxFogMode = (SkyboxFogMode)(int)props[ShaderProps.SkyboxFogMode].floatValue;

                                materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogStrength], new GUIContent("Skybox intensity"));
                                if (skyboxFogMode == SkyboxFogMode.Distance)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogDistance], new GUIContent("Skybox distance"));
                                }
                                else if (skyboxFogMode == SkyboxFogMode.Box)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogBoxSizeMin], new GUIContent("Skybox box size min"));
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogBoxSizeMax], new GUIContent("Skybox box size max"));
                                    SkyboxFogSceneHandle.DrawEditButton(materials);
                                }
                                else if (skyboxFogMode == SkyboxFogMode.Sphere)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogSphereCenter], new GUIContent("Skybox sphere center"));
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogSphereRadius], new GUIContent("Skybox sphere radius"));
                                    SkyboxFogSceneHandle.DrawEditButton(materials);
                                }
                            }
                        }

                    }
                }

                TempuraGui.Space2();
            }
            #endregion
            //-----------------------------------------------
            // Area Light
            //-----------------------------------------------
            #region Area Light
            settings.foldAreaLightMenu = TempuraGui.FoldOut("Area light", "", settings.foldAreaLightMenu);
            if (settings.foldAreaLightMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.AreaLight], new GUIContent("AreaLight"));
                        bool useAreaLight = Cast(props[ShaderProps.AreaLight].floatValue);
                        if (useAreaLight)
                        {
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightMask], new GUIContent("Area light mask"));
                            if (Cast(props[ShaderProps.AreaLightMask].floatValue))
                            {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Area light mask 1"), props[ShaderProps.AreaLightMask1]);
                                materialEditor.TexturePropertySingleLine(new GUIContent("Area light mask 2"), props[ShaderProps.AreaLightMask2]);
                            }
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity1], new GUIContent("Area light intensity 1"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity2], new GUIContent("Area light intensity 2"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity3], new GUIContent("Area light intensity 3"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity4], new GUIContent("Area light intensity 4"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity5], new GUIContent("Area light intensity 5"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity6], new GUIContent("Area light intensity 6"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity7], new GUIContent("Area light intensity 7"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity8], new GUIContent("Area light intensity 8"));
                        }
                    }
                }
                TempuraGui.Space2();
            }
            #endregion

            //-----------------------------------------------
            // Debug
            //-----------------------------------------------
            #region Debug
            settings.foldDebugMenu = TempuraGui.FoldOut("Debug", "", settings.foldDebugMenu);
            if (settings.foldDebugMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.CheckParameter], new GUIContent("CheckParameter"));
                        if (props[ShaderProps.CheckParameter].hasMixedValue || Cast(props[ShaderProps.CheckParameter].floatValue))
                        {
                            materialEditor.ShaderProperty(props[ShaderProps.CheckParameterMode], new GUIContent("Parameter"));
                        }
                        EditorGUILayout.LabelField("RenderQueue", material.renderQueue.ToString());
                        EditorGUILayout.LabelField("Cull", ((CullMode)props[ShaderProps.UnityCullMode].floatValue).ToString());
                        EditorGUILayout.LabelField("SrcBlend", ((BlendMode)props[ShaderProps.UnitySrcBlend].floatValue).ToString());
                        EditorGUILayout.LabelField("DstBlend", ((BlendMode)props[ShaderProps.UnityDstBlend].floatValue).ToString());
                        EditorGUILayout.LabelField("ZWrite", ((UnityZWriteMode)props[ShaderProps.UnityZWrite].floatValue).ToString());
                        EditorGUILayout.LabelField("AlphaToMask", ((UnityAlphaToMaskMode)props[ShaderProps.UnityAlphaToMask].floatValue).ToString());
                        EditorGUILayout.LabelField("Enabled keywords", string.Join("\n", material.shaderKeywords), EditorStyles.textArea);
                    }
                }
                TempuraGui.Space2();
            }
            #endregion

            if (EditorGUI.EndChangeCheck())
            {
                // Invoked on every slider change.
                foreach (Material mat in materials)
                {
                    Apply(mat);
                }

                if (SkyboxFogSceneHandle.IsEditing(material))
                {
                    SceneView.RepaintAll();
                }
            }

            if (!_initialized)
            {
                _initialized = true;
                Debug.Log("Initalized");
                foreach (Material mat in materials)
                {
                    Apply(mat);
                }
            }

        }

        public override void OnClosed(Material material)
        {
            if (SkyboxFogSceneHandle.IsEditing(material))
            {
                SkyboxFogSceneHandle.End();
            }
            base.OnClosed(material);
        }

        private void Apply(Material material)
        {
            var alphaMode = Cast<AlphaMode>(material, ShaderProps.AlphaMode.Name());
            var renderQueueOffset = material.GetInt(ShaderProps.RenderQueueOffset.Name());
            var transparentZwrite = material.GetInt(ShaderProps.TransparentZWrite.Name());

            switch (alphaMode)
            {
                case AlphaMode.Opaque:
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetInt(ShaderProps.UnitySrcBlend.Name(), (int)BlendMode.One);
                    material.SetInt(ShaderProps.UnityDstBlend.Name(), (int)BlendMode.Zero);
                    material.SetInt(ShaderProps.UnityZWrite.Name(), (int)UnityZWriteMode.On);
                    material.SetInt(ShaderProps.UnityAlphaToMask.Name(), (int)UnityAlphaToMaskMode.Off);
                    material.DisableKeyword(ShaderKeywords.AlphatestOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphablendOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphapremultiplyOn.Name());

                    material.renderQueue = (int)RenderQueue.Geometry + renderQueueOffset;
                    break;
                case AlphaMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt(ShaderProps.UnitySrcBlend.Name(), (int)BlendMode.One);
                    material.SetInt(ShaderProps.UnityDstBlend.Name(), (int)BlendMode.Zero);
                    material.SetInt(ShaderProps.UnityZWrite.Name(), (int)UnityZWriteMode.On);
                    material.SetInt(ShaderProps.UnityAlphaToMask.Name(), (int)UnityAlphaToMaskMode.On);
                    material.EnableKeyword(ShaderKeywords.AlphatestOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphablendOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphapremultiplyOn.Name());

                    material.renderQueue = (int)RenderQueue.AlphaTest + renderQueueOffset;
                    break;
                case AlphaMode.Transparent when transparentZwrite == 0:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt(ShaderProps.UnitySrcBlend.Name(), (int)BlendMode.SrcAlpha);
                    material.SetInt(ShaderProps.UnityDstBlend.Name(), (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderProps.UnityZWrite.Name(), (int)UnityZWriteMode.Off);
                    material.SetInt(ShaderProps.UnityAlphaToMask.Name(), (int)UnityAlphaToMaskMode.Off);
                    material.DisableKeyword(ShaderKeywords.AlphatestOn.Name());
                    material.EnableKeyword(ShaderKeywords.AlphablendOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphapremultiplyOn.Name());

                    material.renderQueue = (int)RenderQueue.Transparent + renderQueueOffset;
                    break;

                case AlphaMode.Transparent when transparentZwrite == 1:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt(ShaderProps.UnitySrcBlend.Name(), (int)BlendMode.SrcAlpha);
                    material.SetInt(ShaderProps.UnityDstBlend.Name(), (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ShaderProps.UnityZWrite.Name(), (int)UnityZWriteMode.On);
                    material.SetInt(ShaderProps.UnityAlphaToMask.Name(), (int)UnityAlphaToMaskMode.On);
                    material.DisableKeyword(ShaderKeywords.AlphatestOn.Name());
                    material.EnableKeyword(ShaderKeywords.AlphablendOn.Name());
                    material.DisableKeyword(ShaderKeywords.AlphapremultiplyOn.Name());

                    material.renderQueue = (int)RenderQueue.Transparent + renderQueueOffset;
                    break;
            }

            TempuraGui.ToggleKeyward(material, "_CHECKPARAMETER_ON",
                Cast(material.GetFloat(ShaderProps.CheckParameter.Name())));

            // Shader Keyward
            TempuraGui.ToggleKeyward(material, "_ESTIMATE_SH_DIRECTION_OFF",
                !Cast(material.GetFloat(ShaderProps.EstimateSHDirection.Name())));
            if (Cast(material.GetFloat(ShaderProps.SecondTexture.Name())))
                material.EnableKeyword("_SECONDTEXTURE_ON");
            else
                material.DisableKeyword("_SECONDTEXTURE_ON");
            TempuraGui.ExcusiveKeyward<SecondTextureBlendMode>(material, ShaderProps.SecondBlendMode.Name(), new string[]
            {
                "_SECOND_BLEND_ADD", "_SECOND_BLEND_MULTIPLY", "_SECOND_BLEND_OVERLAY", "_SECOND_BLEND_REPLACE"
            });
            TempuraGui.ExcusiveKeyward<Texcoord>(material, ShaderProps.SecondTexcoord.Name(), new string[]
            {
                "_SECOND_UV0", "_SECOND_UV1", "_SECOND_UV2", "_SECOND_UV3"
            });
            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.SecondMaskChannel.Name(), new string[]
            {
                "_SECOND_MASK_CHANNEL_R", "_SECOND_MASK_CHANNEL_G", "_SECOND_MASK_CHANNEL_B", "_SECOND_MASK_CHANNEL_A"
            });

            TempuraGui.ExcusiveKeyward<RoughnessModel>(material, ShaderProps.RoughnessModel.Name(), new string[]
            {
                ShaderKeywords.RoughnessModelRoughness.Name(),
                ShaderKeywords.RoughnessModelSmoothness.Name()
            });

            // LIGHTVOLUME_SUPPORT gates the include of the package's own cginc, so it
            // tracks whether the package is actually in the project rather than being
            // something the user toggles.
            TempuraGui.ToggleKeyward(material, ShaderKeywords.LightvolumeSupport.Name(), IsLightVolumeAvailable());

            TempuraGui.ExcusiveKeyward<LightmapMode>(material, ShaderProps.LightmapMode.Name(), new string[]
            {
                ShaderKeywords.LightmapDefault.Name(),
                ShaderKeywords.LightmapRnm.Name(),
                ShaderKeywords.LightmapSh.Name(),
                ShaderKeywords.LightmapMonosh.Name()
            });

            TempuraGui.ToggleKeyward(material, ShaderKeywords.LightmapNonlinearShOn.Name(),
                Cast(material.GetFloat(ShaderProps.LightmapNonLinearSh.Name())));

            bool triplanar = (TilingMode)material.GetInt(ShaderProps.MainTexTilingMode.Name()) == TilingMode.Triplanar;
            material.SetInt(ShaderProps.MainTexTilingMode.Name(), (int)(triplanar ? TilingMode.Triplanar : TilingMode.Simple));
            TempuraGui.ToggleKeyward(material, ShaderKeywords.MaintexTilingSimple.Name(), !triplanar);
            TempuraGui.ToggleKeyward(material, ShaderKeywords.MaintexTilingTriplanar.Name(), triplanar);
            material.DisableKeyword(ShaderKeywords.MaintexTilingHex.Name());

            TempuraGui.ExcusiveKeyward<Texcoord>(material, ShaderProps.MainTexcoord.Name(), new string[]{
                ShaderKeywords.MaintexUv0.Name(),
                ShaderKeywords.MaintexUv1.Name(),
                ShaderKeywords.MaintexUv2.Name(),
                ShaderKeywords.MaintexUv3.Name(),
            });

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.RoughnessChannel.Name(), new string[]{
                ShaderKeywords.RoughnessChannelR.Name(),
                ShaderKeywords.RoughnessChannelG.Name(),
                ShaderKeywords.RoughnessChannelB.Name(),
                ShaderKeywords.RoughnessChannelA.Name(),
            });

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.MetallicChannel.Name(), new string[]{
                ShaderKeywords.MetallicChannelR.Name(),
                ShaderKeywords.MetallicChannelG.Name(),
                ShaderKeywords.MetallicChannelB.Name(),
                ShaderKeywords.MetallicChannelA.Name(),
            });

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.OcclusionChannel.Name(), new string[]{
                ShaderKeywords.OcclusionChannelR.Name(),
                ShaderKeywords.OcclusionChannelG.Name(),
                ShaderKeywords.OcclusionChannelB.Name(),
                ShaderKeywords.OcclusionChannelA.Name(),
            });

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.ThinFilmChannel.Name(), new string[]{
                ShaderKeywords.ThinfilmChannelR.Name(),
                ShaderKeywords.ThinfilmChannelG.Name(),
                ShaderKeywords.ThinfilmChannelB.Name(),
                ShaderKeywords.ThinfilmChannelA.Name(),
            });

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.SssThicknessChannel.Name(), new string[]{
                ShaderKeywords.SssThicknessChannelR.Name(),
                ShaderKeywords.SssThicknessChannelG.Name(),
                ShaderKeywords.SssThicknessChannelB.Name(),
                ShaderKeywords.SssThicknessChannelA.Name(),
            });

            if (Cast(material.GetFloat(ShaderProps.ClearCoat.Name())))
            {
                material.EnableKeyword(ShaderKeywords.ClearcoatOn.Name());
            }
            else
            {
                material.DisableKeyword(ShaderKeywords.ClearcoatOn.Name());
            }

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.ClearCoatMaskChannel.Name(), new string[]{
                ShaderKeywords.ClearcoatMaskChannelR.Name(),
                ShaderKeywords.ClearcoatMaskChannelG.Name(),
                ShaderKeywords.ClearcoatMaskChannelB.Name(),
                ShaderKeywords.ClearcoatMaskChannelA.Name(),
            });

            if (Cast(material.GetFloat(ShaderProps.Height.Name())))
            {
                material.EnableKeyword(ShaderKeywords.HeightOn.Name());
            }
            else
            {
                material.DisableKeyword(ShaderKeywords.HeightOn.Name());
            }

            TempuraGui.ExcusiveKeyward<TextureChannel>(material, ShaderProps.HeightChannel.Name(), new string[]{
                ShaderKeywords.HeightChannelR.Name(),
                ShaderKeywords.HeightChannelG.Name(),
                ShaderKeywords.HeightChannelB.Name(),
                ShaderKeywords.HeightChannelA.Name(),
            });

            TempuraGui.ExcusiveKeyward<SkyboxFogMode>(material, ShaderProps.SkyboxFogMode.Name(), new string[]{
                ShaderKeywords.SkyboxfogDistance.Name(),
                ShaderKeywords.SkyboxfogBox.Name(),
                ShaderKeywords.SkyboxfogSphere.Name(),
            });


            bool useEmission = Cast(material.GetFloat(ShaderProps.Emission.Name()));
            if (useEmission)
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                material.EnableKeyword("_EMISSION_ON");
            }
            else
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                material.DisableKeyword("_EMISSION_ON");
            }

        }
    }
}
