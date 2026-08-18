using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using Tempura;

namespace ShaderBindingExporter
{
    public class ShaderBindingExporter : EditorWindow
    {
        private Shader shader;
        private Shader preShader = null;

        private ShaderInfo shaderInfo = new ShaderInfo();

        // GUI
        private bool enableExtension = false;
        private bool enableProperty = true;
        private bool enableKeyword = true;
        private bool enableAutoExport = false;

        private String nameSpace = "";
        private string outputPath = "";

        private bool showProperties = false;

        private Vector2 scroll;
        private Vector2 propScroll;
        private Vector2 keywordScroll;

        private string selectedShaderPath;
        private DateTime lastShaderWriteTime;

        bool updated = false;

        [MenuItem("Takenoko/Shader Binding Exporte")]
        public static void ShowWindow()
        {
            GetWindow<ShaderBindingExporter>("Shader Binding Exporter");
        }

        private static void LoadConfig(string name, ref bool value)
        {
            var v = EditorUserSettings.GetConfigValue(name);
            if (string.IsNullOrEmpty(v))
            {
                return;
            }

            value = v == true.ToString();
        }

        void OnEnable()
        {
            shader = null;
            string guid = EditorUserSettings.GetConfigValue("ShaderBindingExporter/shaderGUID");
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                preShader = shader;
            }

            outputPath = EditorUserSettings.GetConfigValue("ShaderBindingExporter/outputPath") ?? "";
            nameSpace = EditorUserSettings.GetConfigValue("ShaderBindingExporter/nameSpace") ?? "";

            LoadConfig("ShaderBindingExporter/enableExtension", ref enableExtension);
            LoadConfig("ShaderBindingExporter/enableProperty", ref enableProperty);
            LoadConfig("ShaderBindingExporter/enableKeyword", ref enableKeyword);
            LoadConfig("ShaderBindingExporter/enableAutoExport", ref enableAutoExport);
        }

        void OnDisable()
        {
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            string guid = AssetDatabase.AssetPathToGUID(shaderPath);
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/shaderGUID", guid);
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/outputPath", outputPath);
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/nameSpace", nameSpace);
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/enableExtension", enableExtension.ToString());
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/enableProperty", enableProperty.ToString());
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/enableKeyword", enableKeyword.ToString());
            EditorUserSettings.SetConfigValue("ShaderBindingExporter/enableAutoExport", enableAutoExport.ToString());

            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            shader = (Shader)EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);
            EditorGUILayout.Space();

            if (preShader != shader)
            {
                preShader = shader;
                if (shader != null)
                {
                    var shaderName = shader.name;
                    nameSpace = shaderName.Replace(" ", "").Replace("/", ".");
                }
            }

            if (shader != null)
            {
                // Check Shader Update
                string path = AssetDatabase.GetAssetPath(shader);
                string fullPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), path);
                DateTime currentWriteTime = File.GetLastWriteTimeUtc(fullPath);
                if (currentWriteTime > lastShaderWriteTime || selectedShaderPath != fullPath)
                {
                    selectedShaderPath = fullPath;
                    lastShaderWriteTime = File.GetLastWriteTimeUtc(fullPath);
                    shaderInfo = Parser.Parse(shader);
                    updated = true;

                    Debug.Log("[Shader Binding Exporter] Detected updating shader");
                }
            }
            else
            {
                shaderInfo = new ShaderInfo();
                selectedShaderPath = string.Empty;
            }

            showProperties = EditorGUILayout.Foldout(showProperties, "bindings");
            if (showProperties)
            {
                using (new VerticalScope(TempuraGui.largeBox))
                {
                    EditorGUILayout.LabelField("Property", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Property Number : " + shaderInfo.propertyNames.Count);

                    using (new BoxScope(TempuraGui.noMarginBoxB, 0.8f))
                    {
                        propScroll = EditorGUILayout.BeginScrollView(propScroll, GUILayout.Height(150));
                        foreach (var prop in shaderInfo.propertyNames)
                            EditorGUILayout.LabelField("- " + prop);
                        EditorGUILayout.EndScrollView();
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Keyword", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Keyword Number : " + shaderInfo.keywordNames.Count);
                    using (new BoxScope(TempuraGui.noMarginBoxB, 0.8f))
                    {
                        keywordScroll = EditorGUILayout.BeginScrollView(keywordScroll, GUILayout.Height(200));
                        foreach (var keyword in shaderInfo.keywordNames)
                        {
                            EditorGUILayout.LabelField("- " + keyword);
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
            }


            // Config
            EditorGUILayout.Space();
            TempuraGui.DrawHorizontalLine(2.0f);
            EditorGUILayout.LabelField("Output Config", EditorStyles.boldLabel);

            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);

            enableProperty = EditorGUILayout.Toggle("Shader property", enableProperty);
            enableKeyword = EditorGUILayout.Toggle("Shader keyword", enableKeyword);
            enableExtension = EditorGUILayout.Toggle("Enum extension", enableExtension);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output path");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("...", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("Select output path", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path;
                }
            }

            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(outputPath) ? "..." : outputPath, GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            enableAutoExport = EditorGUILayout.ToggleLeft("Enable auto export", enableAutoExport);

            if (GUILayout.Button("Export"))
            {
                if (shader == null)
                {
                    Debug.LogError("[Shader Binding Exporter] Select shader");
                }
                else if (outputPath == "")
                {
                    Debug.LogError("[Shader Binding Exporter] Select output path");
                }
                else if (!enableExtension && !enableProperty && !enableKeyword)
                {
                    Debug.LogError("[Shader Binding Exporter] Select at least one: enum extension, property, keyword");
                }
                else
                {
                    CodeGenerator.Generate(shaderInfo, new CodeGenerator.OutputInfo
                    {
                        nameSpace = nameSpace,
                        filePath = outputPath,
                        propsEnumName = "ShaderProps",
                        propsDicName = "ShaderPropsName",
                        keywordsEnumName = "ShaderKeywords",
                        keywordsDicName = "ShaderKeywordsName",
                        enableExtension = enableExtension,
                        enableProperty = enableProperty,
                        enableKeyword = enableKeyword
                    });
                }

            }

            if (updated)
            {
                if (enableAutoExport)
                {
                    if (shader == null)
                    {
                        Debug.LogError("[Shader Binding Exporter] Select shader");
                    }
                    else if (outputPath == "")
                    {
                        Debug.LogError("[Shader Binding Exporter] Select output path");
                    }
                    else
                    {
                        CodeGenerator.Generate(shaderInfo, new CodeGenerator.OutputInfo
                        {
                            nameSpace = nameSpace,
                            filePath = outputPath,
                            propsEnumName = "ShaderProps",
                            propsDicName = "ShaderPropsName",
                            keywordsEnumName = "ShaderKeywords",
                            keywordsDicName = "ShaderKeywordsName",
                            enableExtension = enableExtension,
                            enableProperty = enableProperty,
                            enableKeyword = enableKeyword
                        });
                    }
                }
                updated = false;
                Repaint();
            }

            EditorGUILayout.EndScrollView();
        }

    }
}
