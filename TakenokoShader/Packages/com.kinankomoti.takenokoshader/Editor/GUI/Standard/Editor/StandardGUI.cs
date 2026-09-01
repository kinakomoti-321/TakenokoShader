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
            public bool foldMainMenu = false;
            public bool foldLightingMenu = false;
            public bool foldEffectMenu = false;
            public bool foldAreaLightMenu = false;
            public bool foldDebugMenu = false;
        }

        public static Settings settings { get { return Settings.instance; } }

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

            if (GUILayout.Button("Fix Now", EditorStyles.miniButton))
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
                        TempuraGui.Popup<AlphaMode>("Alpha Mode", props[ShaderProps.AlphaMode], materialEditor);
                        var alphaMode = (AlphaMode)(int)props[ShaderProps.AlphaMode].floatValue;
                        if (alphaMode == AlphaMode.Cutout)
                        {
                            using (new VerticalScope(TempuraGui.smallBox))
                            {
                                materialEditor.ShaderProperty(props[ShaderProps.Cutoff], new GUIContent("Cutout"));
                            }
                        }
                        TempuraGui.Popup<CullMode>("Cull Mode", props[ShaderProps.UnityCullMode], materialEditor);

                        materialEditor.EnableInstancingField();
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        EditorGUILayout.LabelField("Render Queue", material.renderQueue.ToString());
                        TempuraGui.IntField("Render Queue Offset", props[ShaderProps.RenderQueueOffset], materialEditor);
                    }
                }
                TempuraGui.Space2();
            }
            #endregion


            //-----------------------------------------------
            // Main Texture
            //-----------------------------------------------
            #region Main Texture
            settings.foldMainMenu = TempuraGui.FoldOut("Main Texture", "", settings.foldMainMenu);
            if (settings.foldMainMenu)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        TempuraGui.Popup<TilingMode>("Tiling Mode", props[ShaderProps.MainTexTilingMode], materialEditor);

                        var tilingMode = Cast<TilingMode>(material, ShaderProps.MainTexTilingMode.Name());
                        if (tilingMode == TilingMode.HexTiling)
                        {
                            using (new VerticalScope(TempuraGui.borderBox))
                            {
                                materialEditor.ShaderProperty(props[ShaderProps.HexRotationStrength], new GUIContent("Rotation Strength"));
                                materialEditor.ShaderProperty(props[ShaderProps.HexFallOff], new GUIContent("Fall Off"));
                                materialEditor.ShaderProperty(props[ShaderProps.HexExponent], new GUIContent("Exponent"));
                                materialEditor.ShaderProperty(props[ShaderProps.HexEdgeSmoothness], new GUIContent("Edge Smoothness"));
                            }
                        }

                        TempuraGui.Popup<Texcoord>("MainTex Texcoord", props[ShaderProps.MainTexcoord], materialEditor);
                        materialEditor.ShaderProperty(props[ShaderProps.MainTexOffset], new GUIContent("MainTex Offset"));
                        materialEditor.ShaderProperty(props[ShaderProps.MainTexScale], new GUIContent("MainTex Scale"));
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Base Color"), props[ShaderProps.MainTex], props[ShaderProps.Color]);
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
                        materialEditor.ShaderProperty(props[ShaderProps.OcclusionPower], new GUIContent("Occlusion Power"));
                        materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), props[ShaderProps.BumpMap], props[ShaderProps.BumpScale]);
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
                    //         TempuraGui.Popup<TextureChannel>("Height Channel", props[ShaderProps.HeightChannel], materialEditor);
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightOffset], new GUIContent("Height Offset"));
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightStep], new GUIContent("Height Step"));
                    //         materialEditor.ShaderProperty(props[ShaderProps.HeightSubStep], new GUIContent("Height Sub Step"));
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
                            DrawLinearTextureWarning(materials, ShaderProps.ThinFilmThicknessTex.Name(), "Thin Film Thickness");
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmThicknessMin], new GUIContent("Thickness Min"));
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmThicknessMax], new GUIContent("Thickness Max"));
                            materialEditor.ShaderProperty(props[ShaderProps.ThinFilmIor], new GUIContent("Ior"));
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.Sss], new GUIContent("Subsurface Scattering"));
                        bool useSss = Cast(props[ShaderProps.Sss].floatValue);
                        if (useSss)
                        {
                            TempuraGui.Space2();
                            TempuraGui.Popup<SssMode>("Mode", props[ShaderProps.SssMode], materialEditor);
                            materialEditor.TexturePropertyTwoLines(new GUIContent("Thickness"), props[ShaderProps.SssThicknessTex], props[ShaderProps.SssThickness], new GUIContent("Channel"), props[ShaderProps.SssThicknessChannel]);
                            DrawLinearTextureWarning(materials, ShaderProps.SssThicknessTex.Name(), "SSS Thickness");
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.ClearCoat], new GUIContent("ClearCoat"));
                        bool useClearCoat = Cast(props[ShaderProps.ClearCoat].floatValue);
                        if (useClearCoat)
                        {
                            TempuraGui.Space2();
                            materialEditor.ShaderProperty(props[ShaderProps.ClearCoatStrength], new GUIContent("Strength"));
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
                        materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusion], new GUIContent("Specular Occlusion"));
                        bool useSpecularOcclusion = Cast(props[ShaderProps.SpecularOcclusion].floatValue);
                        if (useSpecularOcclusion)
                        {
                            using (new VerticalScope(TempuraGui.borderBox))
                            {
                                materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusionStrength], new GUIContent("Strength"));
                                materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusionPower], new GUIContent("Power"));
                                materialEditor.ShaderProperty(props[ShaderProps.SpecularOcclusionLightmap], new GUIContent("Light map"));
                            }
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.MaskLightProbe], new GUIContent("Mask Light Probe"));
                        materialEditor.ShaderProperty(props[ShaderProps.MaskLightmap], new GUIContent("Mask Lightmap"));
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
                        using (new VerticalScope(TempuraGui.borderBox))
                        {
                            EditorGUILayout.LabelField("Main Texture", EditorStyles.boldLabel);
                            materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexHueShift], new GUIContent("Hue Shift"));
                            materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexSaturateShift], new GUIContent("Saturate"));
                            materialEditor.ShaderProperty(props[ShaderProps.FilterMainTexValueShift], new GUIContent("Value"));
                        }
                        using (new VerticalScope(TempuraGui.borderBox))
                        {
                            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
                            materialEditor.ShaderProperty(props[ShaderProps.FilterResultHueShift], new GUIContent("Hue Shift"));
                            materialEditor.ShaderProperty(props[ShaderProps.FilterResultSaturateShift], new GUIContent("Saturate"));
                            materialEditor.ShaderProperty(props[ShaderProps.FilterResultValueShift], new GUIContent("Value"));
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.Wetness], new GUIContent("Wetness"));
                        bool useWetness = Cast(props[ShaderProps.Wetness].floatValue);
                        if (useWetness)
                        {
                            using (new VerticalScope(TempuraGui.borderBox))
                            {
                                materialEditor.ShaderProperty(props[ShaderProps.WetnessColor], new GUIContent("Wetness Color"));
                            }
                        }
                    }

                    using (new VerticalScope(TempuraGui.borderBox))
                    {
                        materialEditor.ShaderProperty(props[ShaderProps.SkyboxFog], new GUIContent("Skybox Fog"));
                        bool useSkyboxFog = Cast(props[ShaderProps.SkyboxFog].floatValue);

                        if (useSkyboxFog)
                        {
                            using (new VerticalScope(TempuraGui.borderBox))
                            {
                                TempuraGui.Popup<SkyboxFogMode>("Skybox Fog Mode", props[ShaderProps.SkyboxFogMode], materialEditor);
                                var skyboxFogMode = (SkyboxFogMode)(int)props[ShaderProps.SkyboxFogMode].floatValue;

                                materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogStrength], new GUIContent("Skybox Strength"));
                                if (skyboxFogMode == SkyboxFogMode.Distance)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogDistance], new GUIContent("Skybox Distance"));
                                }
                                else if (skyboxFogMode == SkyboxFogMode.Box)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogBoxSizeMin], new GUIContent("Skybox Box Size Min"));
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogBoxSizeMax], new GUIContent("Skybox Box Size Max"));
                                    SkyboxFogSceneHandle.DrawEditButton(materials);
                                }
                                else if (skyboxFogMode == SkyboxFogMode.Sphere)
                                {
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogSphereCenter], new GUIContent("Skybox Sphere Center"));
                                    materialEditor.ShaderProperty(props[ShaderProps.SkyboxFogSphereRadius], new GUIContent("Skybox Sphere Radius"));
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
            settings.foldAreaLightMenu = TempuraGui.FoldOut("Area Light", "", settings.foldAreaLightMenu);
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
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightMask], new GUIContent("Area Light Mask"));
                            if (Cast(props[ShaderProps.AreaLightMask].floatValue))
                            {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Area Light Mask 1"), props[ShaderProps.AreaLightMask1]);
                                materialEditor.TexturePropertySingleLine(new GUIContent("Area Light Mask 2"), props[ShaderProps.AreaLightMask2]);
                            }
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity1], new GUIContent("Area Light Intensity 1"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity2], new GUIContent("Area Light Intensity 2"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity3], new GUIContent("Area Light Intensity 3"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity4], new GUIContent("Area Light Intensity 4"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity5], new GUIContent("Area Light Intensity 5"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity6], new GUIContent("Area Light Intensity 6"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity7], new GUIContent("Area Light Intensity 7"));
                            materialEditor.ShaderProperty(props[ShaderProps.AreaLightIntensity8], new GUIContent("Area Light Intensity 8"));
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
                        EditorGUILayout.LabelField("RenderQueue", material.renderQueue.ToString());
                        EditorGUILayout.LabelField("Cull", ((CullMode)props[ShaderProps.UnityCullMode].floatValue).ToString());
                        EditorGUILayout.LabelField("SrcBlend", ((BlendMode)props[ShaderProps.UnitySrcBlend].floatValue).ToString());
                        EditorGUILayout.LabelField("DstBlend", ((BlendMode)props[ShaderProps.UnityDstBlend].floatValue).ToString());
                        EditorGUILayout.LabelField("ZWrite", ((UnityZWriteMode)props[ShaderProps.UnityZWrite].floatValue).ToString());
                        EditorGUILayout.LabelField("AlphaToMask", ((UnityAlphaToMaskMode)props[ShaderProps.UnityAlphaToMask].floatValue).ToString());
                        EditorGUILayout.LabelField("Enabled Keywords", string.Join("\n", material.shaderKeywords), EditorStyles.textArea);
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

            // Shader Keyward
            TempuraGui.ExcusiveKeyward<RoughnessModel>(material, ShaderProps.RoughnessModel.Name(), new string[]
            {
                ShaderKeywords.RoughnessModelRoughness.Name(),
                ShaderKeywords.RoughnessModelSmoothness.Name()
            });

            TempuraGui.ExcusiveKeyward<TilingMode>(material, ShaderProps.MainTexTilingMode.Name(), new string[]
            {
                ShaderKeywords.MaintexTilingSimple.Name(),
                ShaderKeywords.MaintexTilingHex.Name()
            });

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
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.AnyEmissive;
                // material.EnableKeyword(ShaderKeywords.Emission.Name());
            }
            else
            {
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                // material.DisableKeyword(ShaderKeywords.Emission.Name());
            }

        }
    }
}
