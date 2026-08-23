using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Takenoko
{
    public class IridescenceLutMakerWindow : EditorWindow
    {
        private enum OutputFormat
        {
            Png8,
            ExrFloat
        }

        [Serializable]
        private sealed class LutPoint
        {
            [Range(0.0f, 1.0f)]
            public float position;

            public Color color;

            public LutPoint(float position, Color color)
            {
                this.position = position;
                this.color = color;
            }
        }

        private readonly List<LutPoint> _points = new List<LutPoint>();
        private readonly Dictionary<int, float[]> _spectrumCache = new Dictionary<int, float[]>();

        private int _resolution = 256;
        private OutputFormat _outputFormat = OutputFormat.Png8;
        private Vector2 _scrollPosition;
        private Texture2D _previewTexture;

        [MenuItem("Takenoko/Iridescence LUT Maker")]
        public static void ShowWindow()
        {
            IridescenceLutMakerWindow window = GetWindow<IridescenceLutMakerWindow>("Iridescence LUT");
            window.minSize = new Vector2(420.0f, 460.0f);
        }

        private void OnEnable()
        {
            if (_points.Count == 0)
            {
                _points.Add(new LutPoint(0.0f, Color.red));
                _points.Add(new LutPoint(0.33f, Color.green));
                _points.Add(new LutPoint(0.66f, Color.blue));
                _points.Add(new LutPoint(1.0f, Color.white));
            }

            RebuildPreviewTexture();
        }

        private void OnDisable()
        {
            DestroyPreviewTexture();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Iridescence LUT Maker", EditorStyles.boldLabel);
            EditorGUILayout.Space(4.0f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _resolution = EditorGUILayout.IntSlider("Resolution", _resolution, 2, 4096);
                _outputFormat = (OutputFormat)EditorGUILayout.EnumPopup("Format", _outputFormat);

                if (GUILayout.Button("Refresh Preview"))
                {
                    RebuildPreviewTexture();
                }
            }

            EditorGUILayout.Space(8.0f);
            DrawPointsGui();

            EditorGUILayout.Space(8.0f);
            DrawPreviewGui();

            EditorGUILayout.Space(12.0f);
            if (GUILayout.Button("Export LUT"))
            {
                ExportLut();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPointsGui()
        {
            EditorGUILayout.LabelField("Ramp Points", EditorStyles.boldLabel);

            for (int i = 0; i < _points.Count; i++)
            {
                LutPoint point = _points[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Point {i + 1}", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledScope(_points.Count <= 2))
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(72.0f)))
                            {
                                _points.RemoveAt(i);
                                RebuildPreviewTexture();
                                GUIUtility.ExitGUI();
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    point.position = EditorGUILayout.Slider("Position", point.position, 0.0f, 1.0f);
                    point.color = EditorGUILayout.ColorField(new GUIContent("Color"), point.color, true, true, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RebuildPreviewTexture();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Point"))
                {
                    AddPoint();
                }

                if (GUILayout.Button("Sort Points"))
                {
                    SortPoints();
                    RebuildPreviewTexture();
                }
            }
        }

        private void DrawPreviewGui()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            if (_previewTexture == null)
            {
                RebuildPreviewTexture();
            }

            Rect rect = GUILayoutUtility.GetRect(1.0f, 48.0f, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1.0f));
            }

            if (_previewTexture != null)
            {
                GUI.DrawTexture(rect, _previewTexture, ScaleMode.StretchToFill, false);
            }
        }

        private void AddPoint()
        {
            SortPoints();

            if (_points.Count == 0)
            {
                _points.Add(new LutPoint(0.0f, Color.white));
            }
            else if (_points.Count == 1)
            {
                _points.Add(new LutPoint(1.0f, _points[0].color));
            }
            else
            {
                int middleIndex = Mathf.Max(0, _points.Count / 2 - 1);
                LutPoint left = _points[middleIndex];
                LutPoint right = _points[Mathf.Min(middleIndex + 1, _points.Count - 1)];
                float position = (left.position + right.position) * 0.5f;
                Color color = SpectralLerp(left.color, right.color, 0.5f);
                _points.Add(new LutPoint(position, color));
            }

            SortPoints();
            RebuildPreviewTexture();
        }

        private void SortPoints()
        {
            _points.Sort((a, b) => a.position.CompareTo(b.position));
        }

        private void RebuildPreviewTexture()
        {
            DestroyPreviewTexture();
            _previewTexture = BuildRampTexture(Mathf.Clamp(_resolution, 2, 4096), _outputFormat);
            _previewTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        private Texture2D BuildRampTexture(int resolution, OutputFormat format)
        {
            SortPoints();
            float[][] pointSpectra = BuildPointSpectra();

            TextureFormat textureFormat = format == OutputFormat.ExrFloat
                ? TextureFormat.RGBAFloat
                : TextureFormat.RGBA32;
            bool linear = true;

            Texture2D texture = new Texture2D(resolution, 1, textureFormat, false, linear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color[] pixels = new Color[resolution];
            for (int x = 0; x < resolution; x++)
            {
                float t = resolution == 1 ? 0.0f : (float)x / (resolution - 1);
                pixels[x] = EvaluateRamp(t, pointSpectra);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private Color EvaluateRamp(float t, float[][] pointSpectra)
        {
            if (_points.Count == 0)
            {
                return Color.black;
            }

            if (_points.Count == 1)
            {
                return _points[0].color;
            }

            SortPoints();
            t = Mathf.Clamp01(t);

            if (t <= _points[0].position)
            {
                return _points[0].color;
            }

            for (int i = 0; i < _points.Count - 1; i++)
            {
                LutPoint left = _points[i];
                LutPoint right = _points[i + 1];
                if (t > right.position)
                {
                    continue;
                }

                float span = Mathf.Max(right.position - left.position, 1e-6f);
                float segmentT = Mathf.Clamp01((t - left.position) / span);
                return SpectralLerp(pointSpectra[i], pointSpectra[i + 1], segmentT);
            }

            return _points[_points.Count - 1].color;
        }

        private static Color SpectralLerp(Color left, Color right, float t)
        {
            float[] leftSpectrum = RGBToSpectrum.FromLinearSRGB(left);
            float[] rightSpectrum = RGBToSpectrum.FromLinearSRGB(right);
            return SpectralLerp(leftSpectrum, rightSpectrum, t);
        }

        private static Color SpectralLerp(float[] leftSpectrum, float[] rightSpectrum, float t)
        {
            float[] spectrum = RGBToSpectrum.SpectralLerp(leftSpectrum, rightSpectrum, t);
            Color color = RGBToSpectrum.ToLinearSRGB(spectrum);
            color.a = 1.0f;
            return color;
        }

        private float[][] BuildPointSpectra()
        {
            float[][] spectra = new float[_points.Count][];
            for (int i = 0; i < _points.Count; i++)
            {
                spectra[i] = GetCachedSpectrum(_points[i].color);
            }

            return spectra;
        }

        private float[] GetCachedSpectrum(Color color)
        {
            int key = HashCode.Combine(color.r, color.g, color.b, color.a);
            if (_spectrumCache.TryGetValue(key, out float[] cachedSpectrum))
            {
                return cachedSpectrum;
            }

            float[] spectrum = RGBToSpectrum.FromLinearSRGB(color);
            _spectrumCache[key] = spectrum;
            return spectrum;
        }

        private void ExportLut()
        {
            Texture2D texture = BuildRampTexture(Mathf.Clamp(_resolution, 2, 4096), _outputFormat);
            try
            {
                string extension = _outputFormat == OutputFormat.ExrFloat ? "exr" : "png";
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Iridescence LUT",
                    "IridescenceRamp",
                    extension,
                    "Select a location for the generated iridescence LUT.");

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                byte[] bytes = _outputFormat == OutputFormat.ExrFloat
                    ? texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat | Texture2D.EXRFlags.CompressZIP)
                    : texture.EncodeToPNG();

                string absolutePath = Path.GetFullPath(path);
                string directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(absolutePath, bytes);
                AssetDatabase.ImportAsset(path);
                ConfigureImportedTexture(path);
                AssetDatabase.Refresh();

                Debug.Log($"[Iridescence LUT Maker] Exported LUT to {path}");
            }
            finally
            {
                DestroyImmediate(texture);
            }
        }

        private static void ConfigureImportedTexture(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private void DestroyPreviewTexture()
        {
            if (_previewTexture == null)
            {
                return;
            }

            DestroyImmediate(_previewTexture);
            _previewTexture = null;
        }
    }
}
