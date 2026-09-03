using System.IO;
using UnityEditor;
using UnityEngine;

namespace Takenoko
{
    public static class BakeTextureWriter
    {
        /// <summary>
        /// Pushes baked values outward into empty texels.
        ///
        /// Without this, bilinear filtering and mip generation pull the background
        /// colour in along every UV island border and the seams show up as dark
        /// fringes on the model.
        /// </summary>
        public static void Dilate(float[] values, bool[] valid, int width, int height, int iterations)
        {
            if (iterations <= 0)
            {
                return;
            }

            bool[] current = valid;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                bool[] next = (bool[])current.Clone();
                bool changed = false;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * width + x;
                        if (current[index])
                        {
                            continue;
                        }

                        float sum = 0.0f;
                        int count = 0;

                        for (int oy = -1; oy <= 1; oy++)
                        {
                            int ny = y + oy;
                            if (ny < 0 || ny >= height)
                            {
                                continue;
                            }

                            for (int ox = -1; ox <= 1; ox++)
                            {
                                int nx = x + ox;
                                if (nx < 0 || nx >= width || (ox == 0 && oy == 0))
                                {
                                    continue;
                                }

                                int neighbour = ny * width + nx;
                                if (current[neighbour])
                                {
                                    sum += values[neighbour];
                                    count++;
                                }
                            }
                        }

                        if (count > 0)
                        {
                            values[index] = sum / count;
                            next[index] = true;
                            changed = true;
                        }
                    }
                }

                current = next;
                if (!changed)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Writes the single channel result as a PNG, replicated across RGB so any
        /// channel selection in the shader or the texture packer reads the same value.
        /// </summary>
        public static void WritePng(string absolutePath, float[] values, int width, int height)
        {
            Color32[] pixels = new Color32[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                byte value = ToByte(values[i]);
                pixels[i] = new Color32(value, value, value, 255);
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// Imports the written file as linear data. A thickness map feeding a shader
        /// must not carry an sRGB curve.
        /// </summary>
        public static void ImportAsLinearData(string absolutePath)
        {
            string relativePath = ToProjectRelativePath(absolutePath);
            if (relativePath == null)
            {
                return;
            }

            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            string normalized = absolutePath.Replace('\\', '/');
            string projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');

            if (!normalized.StartsWith(projectRoot + "/"))
            {
                return null;
            }

            string relative = normalized.Substring(projectRoot.Length + 1);
            return relative.StartsWith("Assets/") || relative.StartsWith("Packages/") ? relative : null;
        }

        private static byte ToByte(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0;
            }

            return (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(value) * 255.0f), 0, 255);
        }
    }
}
