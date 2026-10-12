using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Kinankomoti.TakenokoShader.Editor
{
    [InitializeOnLoad]
    public sealed class TakenokoLightSystemValidation : IVRCSDKBuildRequestedCallback
    {
        private static bool reportedDuplicate;

        static TakenokoLightSystemValidation()
        {
            EditorApplication.hierarchyChanged += QueueValidation;
            QueueValidation();
        }

        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return requestedBuildType != VRCSDKRequestedBuildType.Scene || ValidateScenes(true);
        }

        public static TakenokoLightSystem[] FindLightSystems()
        {
            var systems = new List<TakenokoLightSystem>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || EditorSceneManager.IsPreviewScene(scene)) continue;
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && prefabStage.scene == scene) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    systems.AddRange(root.GetComponentsInChildren<TakenokoLightSystem>(true));
            }
            return systems.ToArray();
        }

        public static bool ValidateScenes(bool logErrors)
        {
            TakenokoLightSystem[] systems = FindLightSystems();
            if (systems.Length <= 1) return true;
            if (logErrors)
                Debug.LogError("TakenokoShader: Only one AreaLightSystem is allowed across loaded scenes. Remove duplicate systems, including inactive objects.", systems[1]);
            return false;
        }

        private static void QueueValidation()
        {
            EditorApplication.update -= ValidateHierarchy;
            EditorApplication.update += ValidateHierarchy;
        }

        private static void ValidateHierarchy()
        {
            EditorApplication.update -= ValidateHierarchy;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            bool valid = ValidateScenes(false);
            if (!valid && !reportedDuplicate) ValidateScenes(true);
            reportedDuplicate = !valid;
        }
    }
}
