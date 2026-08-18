using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;
using UnityTexture2D = UnityEngine.Texture2D;
using Tempura;

namespace Takenoko
{
    public class TexturePackerWindow : EditorWindow
    {
        private const string ConfigPrefix = "TexturePacker/";

        private enum OutputFormat
        {
            Bit8,
            Float
        }

        private static readonly string[] OutputFormatLabels = { "8 bit", "Float" };

        [Serializable]
        private sealed class ChannelInput
        {
            public string label;
            public UnityTexture texture;
            public TextureChannel textureChannel = TextureChannel.R;
            public float constantValue;

            public ChannelInput(string label, float constantValue)
            {
                this.label = label;
                this.constantValue = constantValue;
            }
        }

        private readonly ChannelInput[] channels =
        {
            new ChannelInput("R", 0.0f),
            new ChannelInput("G", 0.0f),
            new ChannelInput("B", 0.0f),
            new ChannelInput("A", 1.0f)
        };

        private bool autoResolution = true;
        private Vector2Int manualResolution = new Vector2Int(1024, 1024);
        private OutputFormat outputFormat = OutputFormat.Bit8;
        private string outputPath = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem("Takenoko/Texture Packer")]
        public static void ShowWindow()
        {
            TexturePackerWindow window = GetWindow<TexturePackerWindow>("Texture Packer");
            window.minSize = new Vector2(420.0f, 420.0f);
        }

        private void OnEnable()
        {
            autoResolution = LoadBool("autoResolution", true);
            manualResolution.x = LoadInt("manualWidth", manualResolution.x);
            manualResolution.y = LoadInt("manualHeight", manualResolution.y);
            outputPath = EditorUserSettings.GetConfigValue(ConfigPrefix + "outputPath") ?? string.Empty;

            string savedFormatValue = EditorUserSettings.GetConfigValue(ConfigPrefix + "outputFormat");
            if (Enum.TryParse(savedFormatValue, out OutputFormat savedFormat))
            {
                outputFormat = savedFormat;
            }
            else
            {
                string savedPrecisionValue = EditorUserSettings.GetConfigValue(ConfigPrefix + "outputPrecision");
                if (savedPrecisionValue == "Byte8")
                {
                    outputFormat = OutputFormat.Bit8;
                }
                else if (Enum.TryParse(savedPrecisionValue, out OutputFormat savedPrecision))
                {
                    outputFormat = savedPrecision;
                }
            }
        }

        private void OnDisable()
        {
            SaveBool("autoResolution", autoResolution);
            SaveInt("manualWidth", manualResolution.x);
            SaveInt("manualHeight", manualResolution.y);
            EditorUserSettings.SetConfigValue(ConfigPrefix + "outputPath", outputPath ?? string.Empty);
            EditorUserSettings.SetConfigValue(ConfigPrefix + "outputFormat", outputFormat.ToString());
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Channels", EditorStyles.boldLabel);
            EditorGUILayout.Space(2.0f);

            foreach (ChannelInput channel in channels)
            {
                DrawChannelInput(channel);
                EditorGUILayout.Space(4.0f);
            }

            EditorGUILayout.Space(8.0f);
            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);

            autoResolution = EditorGUILayout.ToggleLeft("Auto", autoResolution);
            using (new EditorGUI.DisabledScope(autoResolution))
            {
                manualResolution.x = Mathf.Max(1, EditorGUILayout.IntField("Width", manualResolution.x));
                manualResolution.y = Mathf.Max(1, EditorGUILayout.IntField("Height", manualResolution.y));
            }

            if (autoResolution)
            {
                EditorGUILayout.HelpBox("Auto uses the maximum width and height found in the assigned textures.", MessageType.Info);
            }

            EditorGUILayout.Space(8.0f);
            EditorGUILayout.LabelField("Format", EditorStyles.boldLabel);
            outputFormat = (OutputFormat)EditorGUILayout.Popup(
                "Format",
                (int)outputFormat,
                OutputFormatLabels);

            EditorGUILayout.Space(12.0f);

            if (GUILayout.Button("Pack"))
            {
                Export();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawChannelInput(ChannelInput channel)
        {
            EditorGUILayout.BeginVertical(TempuraGui.borderBox);
            EditorGUILayout.LabelField(channel.label, EditorStyles.boldLabel);

            channel.texture = (UnityTexture)EditorGUILayout.ObjectField("Texture", channel.texture, typeof(UnityTexture), false);
            if (channel.texture == null)
            {
                channel.constantValue = EditorGUILayout.FloatField("Value", channel.constantValue);
            }
            else
            {
                channel.textureChannel = (TextureChannel)EditorGUILayout.EnumPopup("Channel", channel.textureChannel);
                EditorGUILayout.LabelField("Resolution", $"{channel.texture.width} x {channel.texture.height}");
            }

            EditorGUILayout.EndVertical();
        }

        private void Export()
        {
            Vector2Int outputResolution = ResolveOutputResolution();
            if (outputResolution.x <= 0 || outputResolution.y <= 0)
            {
                Debug.LogError("[Texture Packer] Could not resolve output resolution.");
                return;
            }

            string selectedPath = EditorUtility.SaveFilePanelInProject(
                "Save Packed Texture",
                GetDefaultFileName(),
                GetExtension(outputFormat),
                "Select a location for the packed texture.");

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            outputPath = selectedPath;
            string normalizedPath = NormalizeOutputPath(outputPath, outputFormat);
            string absolutePath = Path.GetFullPath(normalizedPath);

            Dictionary<UnityTexture, TakenokoTexture> textures = new Dictionary<UnityTexture, TakenokoTexture>();
            try
            {
                foreach (ChannelInput channel in channels)
                {
                    if (channel.texture == null || textures.ContainsKey(channel.texture))
                    {
                        continue;
                    }

                    if (!TakenokoTexture.TryCreate(channel.texture, out TakenokoTexture packedTexture, out string error))
                    {
                        Debug.LogError($"[Texture Packer] Failed to read texture '{channel.texture.name}': {error}");
                        return;
                    }

                    textures.Add(channel.texture, packedTexture);
                }

                Color[] packedPixels = new Color[outputResolution.x * outputResolution.y];
                for (int y = 0; y < outputResolution.y; y++)
                {
                    for (int x = 0; x < outputResolution.x; x++)
                    {
                        Vector2Int index = new Vector2Int(x, y);
                        Vector2 uv = new Vector2(
                            (x + 0.5f) / outputResolution.x,
                            (y + 0.5f) / outputResolution.y);

                        packedPixels[y * outputResolution.x + x] = new Color(
                            ResolveChannelValue(channels[0], textures, outputResolution, index, uv),
                            ResolveChannelValue(channels[1], textures, outputResolution, index, uv),
                            ResolveChannelValue(channels[2], textures, outputResolution, index, uv),
                            ResolveChannelValue(channels[3], textures, outputResolution, index, uv));
                    }
                }

                WriteTextureAsset(absolutePath, outputResolution, packedPixels);
                outputPath = normalizedPath;

                AssetDatabase.Refresh();
                Debug.Log($"[Texture Packer] Exported packed texture to {normalizedPath}");
            }
            finally
            {
                foreach (TakenokoTexture texture in textures.Values)
                {
                    texture.Dispose();
                }
            }
        }

        private Vector2Int ResolveOutputResolution()
        {
            if (!autoResolution)
            {
                return new Vector2Int(Mathf.Max(1, manualResolution.x), Mathf.Max(1, manualResolution.y));
            }

            int width = 0;
            int height = 0;
            foreach (ChannelInput channel in channels)
            {
                if (channel.texture == null)
                {
                    continue;
                }

                width = Mathf.Max(width, channel.texture.width);
                height = Mathf.Max(height, channel.texture.height);
            }

            return new Vector2Int(width, height);
        }

        private float ResolveChannelValue(
            ChannelInput channel,
            IReadOnlyDictionary<UnityTexture, TakenokoTexture> textures,
            Vector2Int outputResolution,
            Vector2Int index,
            Vector2 uv)
        {
            if (channel.texture == null)
            {
                return channel.constantValue;
            }

            TakenokoTexture source = textures[channel.texture];
            Color sampled = source.Resolution == outputResolution ? source.Get(index) : source.Sample(uv);
            return GetChannel(sampled, channel.textureChannel);
        }

        private static float GetChannel(Color color, TextureChannel channel)
        {
            switch (channel)
            {
                case TextureChannel.R:
                    return color.r;
                case TextureChannel.G:
                    return color.g;
                case TextureChannel.B:
                    return color.b;
                case TextureChannel.A:
                    return color.a;
                default:
                    return color.r;
            }
        }

        private void WriteTextureAsset(string absolutePath, Vector2Int resolution, Color[] pixels)
        {
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            switch (outputFormat)
            {
                case OutputFormat.Bit8:
                    WritePng(absolutePath, resolution, pixels);
                    break;
                case OutputFormat.Float:
                    WriteHdr(absolutePath, resolution, pixels);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WritePng(string absolutePath, Vector2Int resolution, Color[] pixels)
        {
            Color32[] bytePixels = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                bytePixels[i] = new Color32(
                    ToByte(pixel.r),
                    ToByte(pixel.g),
                    ToByte(pixel.b),
                    ToByte(pixel.a));
            }

            UnityTexture2D texture = new UnityTexture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false, false);
            try
            {
                texture.SetPixels32(bytePixels);
                texture.Apply(false, false);
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static byte ToByte(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0;
            }

            return (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(value) * 255.0f), 0, 255);
        }

        private static void WriteHdr(string absolutePath, Vector2Int resolution, Color[] pixels)
        {
            UnityTexture2D texture = new UnityTexture2D(resolution.x, resolution.y, TextureFormat.RGBAFloat, false, true);
            try
            {
                texture.SetPixels(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(
                    absolutePath,
                    texture.EncodeToEXR(UnityEngine.Texture2D.EXRFlags.OutputAsFloat | UnityEngine.Texture2D.EXRFlags.CompressZIP));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static string GetExtension(OutputFormat format)
        {
            switch (format)
            {
                case OutputFormat.Bit8:
                    return "png";
                case OutputFormat.Float:
                    return "exr";
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static string NormalizeOutputPath(string path, OutputFormat format)
        {
            string extension = "." + GetExtension(format);
            return Path.ChangeExtension(path, extension);
        }

        private string GetDefaultFileName()
        {
            if (!string.IsNullOrEmpty(outputPath))
            {
                return Path.GetFileNameWithoutExtension(outputPath);
            }

            return "PackedTexture";
        }

        private static bool LoadBool(string key, bool defaultValue)
        {
            string value = EditorUserSettings.GetConfigValue(ConfigPrefix + key);
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        private static int LoadInt(string key, int defaultValue)
        {
            string value = EditorUserSettings.GetConfigValue(ConfigPrefix + key);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        private static void SaveBool(string key, bool value)
        {
            EditorUserSettings.SetConfigValue(ConfigPrefix + key, value.ToString());
        }

        private static void SaveInt(string key, int value)
        {
            EditorUserSettings.SetConfigValue(ConfigPrefix + key, value.ToString());
        }
    }
}
