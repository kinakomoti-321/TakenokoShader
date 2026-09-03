using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Takenoko
{
    public class TextureBakerWindow : EditorWindow
    {
        private const string ConfigPrefix = "TextureBaker/";

        private static readonly int[] ResolutionOptions = { 256, 512, 1024, 2048, 4096 };
        private static readonly string[] ResolutionLabels = { "256", "512", "1024", "2048", "4096" };
        private static readonly string[] UvSetLabels = { "UV0", "UV1", "UV2", "UV3" };

        private GameObject bakeRoot;
        private BakeTarget[] targets;
        private string[] targetLabels;
        private int targetIndex;

        private BakeUvSet uvSet = BakeUvSet.Uv0;
        private int resolutionIndex = 2;
        private int sampleCount = 64;
        private int padding = 8;
        private string outputPath = string.Empty;
        private Vector2 scrollPosition;

        [MenuItem("Takenoko/Texture Baker")]
        public static void ShowWindow()
        {
            TextureBakerWindow window = GetWindow<TextureBakerWindow>("Texture Baker");
            window.minSize = new Vector2(420.0f, 460.0f);
        }

        private void OnEnable()
        {
            targets = BakeTarget.CreateAll();
            targetLabels = new string[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                targetLabels[i] = targets[i].DisplayName;
                targets[i].LoadSettings(ConfigPrefix + targets[i].DisplayName + "/");
            }

            targetIndex = Mathf.Clamp(LoadInt("targetIndex", 0), 0, Mathf.Max(0, targets.Length - 1));
            uvSet = (BakeUvSet)Mathf.Clamp(LoadInt("uvSet", 0), 0, 3);
            resolutionIndex = Mathf.Clamp(LoadInt("resolutionIndex", 2), 0, ResolutionOptions.Length - 1);
            sampleCount = Mathf.Clamp(LoadInt("sampleCount", 64), 1, 4096);
            padding = Mathf.Clamp(LoadInt("padding", 8), 0, 64);
            outputPath = EditorUserSettings.GetConfigValue(ConfigPrefix + "outputPath") ?? string.Empty;
        }

        private void OnDisable()
        {
            SaveInt("targetIndex", targetIndex);
            SaveInt("uvSet", (int)uvSet);
            SaveInt("resolutionIndex", resolutionIndex);
            SaveInt("sampleCount", sampleCount);
            SaveInt("padding", padding);
            EditorUserSettings.SetConfigValue(ConfigPrefix + "outputPath", outputPath);

            if (targets != null)
            {
                foreach (BakeTarget target in targets)
                {
                    target.SaveSettings(ConfigPrefix + target.DisplayName + "/");
                }
            }
        }

        private void OnGUI()
        {
            if (targets == null || targets.Length == 0)
            {
                EditorGUILayout.HelpBox("No bake target implementation was found.", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Texture Baker", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
            bakeRoot = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Bake Target", "Every renderer under this object is baked into one map."),
                bakeRoot, typeof(GameObject), true);

            uvSet = (BakeUvSet)EditorGUILayout.Popup(
                new GUIContent("UV Set", "Which UV channel defines the texture layout."),
                (int)uvSet, UvSetLabels);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Bake", EditorStyles.boldLabel);
            targetIndex = EditorGUILayout.Popup(new GUIContent("Target"), targetIndex, targetLabels);

            using (new EditorGUI.IndentLevelScope())
            {
                targets[targetIndex].DrawSettings();
            }

            EditorGUILayout.Space();

            resolutionIndex = EditorGUILayout.Popup(new GUIContent("Resolution"), resolutionIndex, ResolutionLabels);
            sampleCount = EditorGUILayout.IntSlider(
                new GUIContent("Samples", "Rays per texel."), sampleCount, 1, 1024);
            padding = EditorGUILayout.IntSlider(
                new GUIContent("Padding", "Texels of dilation pushed outside each UV island."), padding, 0, 64);

            int resolution = ResolutionOptions[resolutionIndex];
            long texels = (long)resolution * resolution;
            EditorGUILayout.HelpBox(
                string.Format(
                    "{0} x {0} at {1} samples is up to {2:N0} rays.",
                    resolution, sampleCount, texels * sampleCount),
                MessageType.None);

            // The sample map keeps a world position and normal per texel, which is
            // where the memory goes rather than the output image itself.
            if (texels * 29L > 256L * 1024L * 1024L)
            {
                EditorGUILayout.HelpBox(
                    string.Format(
                        "This resolution needs roughly {0:N0} MB for the intermediate sample map.",
                        texels * 29L / (1024L * 1024L)),
                    MessageType.Warning);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Path"),
                    new GUIContent(string.IsNullOrEmpty(outputPath) ? "(not set)" : outputPath));

                if (GUILayout.Button("Browse", GUILayout.Width(70.0f)))
                {
                    BrowseOutputPath();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(bakeRoot == null))
            {
                if (GUILayout.Button("Bake", GUILayout.Height(30.0f)))
                {
                    Bake();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void BrowseOutputPath()
        {
            string directory = string.IsNullOrEmpty(outputPath) ? Application.dataPath : Path.GetDirectoryName(outputPath);
            string defaultName = bakeRoot != null
                ? bakeRoot.name + "_" + targets[targetIndex].FileSuffix
                : targets[targetIndex].FileSuffix;

            string selected = EditorUtility.SaveFilePanel("Save Baked Texture", directory, defaultName, "png");
            if (!string.IsNullOrEmpty(selected))
            {
                outputPath = selected;
                GUI.FocusControl(null);
            }
        }

        private void Bake()
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                BrowseOutputPath();
                if (string.IsNullOrEmpty(outputPath))
                {
                    return;
                }
            }

            BakeTarget target = targets[targetIndex];
            int resolution = ResolutionOptions[resolutionIndex];
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                EditorUtility.DisplayProgressBar("Texture Baker", "Collecting geometry...", 0.0f);

                string error;
                BakeScene scene = BakeScene.Collect(bakeRoot, uvSet, out error);
                if (scene == null)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Texture Baker", error, "OK");
                    return;
                }

                EditorUtility.DisplayProgressBar(
                    "Texture Baker",
                    string.Format("Building BVH over {0:N0} triangles...", scene.TriangleCount),
                    0.1f);
                Bvh bvh = new Bvh(scene.Positions, scene.TriangleCount);

                EditorUtility.DisplayProgressBar("Texture Baker", "Rasterizing UV space...", 0.2f);
                UvSampleMap sampleMap = UvRasterizer.Rasterize(scene, resolution, resolution);

                if (sampleMap.ValidCount == 0)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(
                        "Texture Baker",
                        string.Format("{0} covers no texels. Check that the mesh is unwrapped in that channel.", uvSet),
                        "OK");
                    return;
                }

                BakeContext context = new BakeContext
                {
                    Scene = scene,
                    Caster = null,
                    SampleCount = sampleCount,
                    SceneDiagonal = scene.Diagonal,
                    RayBias = Mathf.Max(1.0e-6f, scene.Diagonal * 1.0e-4f)
                };

                target.Prepare(context);

                float[] values = new float[resolution * resolution];
                if (!BakeTexels(target, context, bvh, sampleMap, values, resolution))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                EditorUtility.DisplayProgressBar("Texture Baker", "Dilating...", 0.95f);
                BakeTextureWriter.Dilate(values, sampleMap.Valid, resolution, resolution, padding);

                EditorUtility.DisplayProgressBar("Texture Baker", "Writing...", 0.98f);
                BakeTextureWriter.WritePng(outputPath, values, resolution, resolution);
                BakeTextureWriter.ImportAsLinearData(outputPath);

                stopwatch.Stop();
                Debug.Log(string.Format(
                    "[Texture Baker] Baked {0} from {1} renderer(s), {2:N0} triangles, {3:N0} texels in {4:F1}s -> {5}",
                    target.DisplayName,
                    scene.RendererCount,
                    scene.TriangleCount,
                    sampleMap.ValidCount,
                    stopwatch.Elapsed.TotalSeconds,
                    outputPath));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Rays are traced on worker threads, but the progress bar and cancellation
        /// have to be driven from the main thread, so rows are dispatched in blocks
        /// and the UI is updated between them.
        /// </summary>
        private bool BakeTexels(
            BakeTarget target,
            BakeContext context,
            Bvh bvh,
            UvSampleMap sampleMap,
            float[] values,
            int resolution)
        {
            const int RowsPerBlock = 16;

            for (int blockStart = 0; blockStart < resolution; blockStart += RowsPerBlock)
            {
                int blockEnd = Mathf.Min(resolution, blockStart + RowsPerBlock);
                float progress = 0.2f + 0.75f * blockStart / resolution;

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Texture Baker",
                        string.Format("Tracing rays... row {0} / {1}", blockStart, resolution),
                        progress))
                {
                    return false;
                }

                Parallel.For(
                    blockStart,
                    blockEnd,
                    () => new BvhRayCaster(bvh),
                    (y, loopState, caster) =>
                    {
                        BakeContext threadContext = context;
                        threadContext.Caster = caster;

                        int rowOffset = y * resolution;
                        for (int x = 0; x < resolution; x++)
                        {
                            int index = rowOffset + x;
                            if (!sampleMap.Valid[index])
                            {
                                continue;
                            }

                            values[index] = target.Evaluate(
                                threadContext,
                                sampleMap.Positions[index],
                                sampleMap.Normals[index],
                                (uint)index);
                        }

                        return caster;
                    },
                    caster => { });
            }

            return true;
        }

        private static int LoadInt(string key, int defaultValue)
        {
            string value = EditorUserSettings.GetConfigValue(ConfigPrefix + key);
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        private static void SaveInt(string key, int value)
        {
            EditorUserSettings.SetConfigValue(ConfigPrefix + key, value.ToString());
        }
    }
}
