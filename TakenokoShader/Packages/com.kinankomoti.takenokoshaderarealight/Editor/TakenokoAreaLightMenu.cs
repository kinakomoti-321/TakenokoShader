using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kinankomoti.TakenokoShader.Editor
{
    internal static class TakenokoAreaLightMenu
    {
        private const string PackagePath = "Packages/com.kinankomoti.takenokoshaderarealight/";
        private const string AssetPath = "Assets/TakenokoShaderAreaLight/";

        [MenuItem("GameObject/TakenokoShader/AreaLight", false, 10)]
        private static void CreateAreaLight(MenuCommand command)
        {
            CreatePrefab(command, "TakenokoAreaLight.prefab", "TakenokoAreaLight");
        }

        [MenuItem("GameObject/TakenokoShader/LightSystem", false, 10)]
        private static void CreateLightSystem(MenuCommand command)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                TakenokoLightSystem[] systems = TakenokoLightSystemValidation.FindLightSystems();
                if (systems.Length > 0)
                {
                    Selection.activeGameObject = systems[0].gameObject;
                    EditorGUIUtility.PingObject(systems[0].gameObject);
                    return;
                }
            }
            CreatePrefab(command, "TakenokoLightSystem.prefab", "LightSystem");
        }

        private static void CreatePrefab(MenuCommand command, string prefabName, string objectName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PackagePath + prefabName);
            if (prefab == null)
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath + prefabName);
            if (prefab == null)
            {
                Debug.LogError($"TakenokoShader: {objectName} の Prefab が見つかりません。AreaLight を再インポートしてください。");
                return;
            }

            GameObject parent = command.context as GameObject;
            var scene = parent != null ? parent.scene : SceneManager.GetActiveScene();
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            StageUtility.PlaceGameObjectInCurrentStage(instance);
            GameObjectUtility.SetParentAndAlign(instance, parent);
            instance.name = objectName;
            GameObjectUtility.EnsureUniqueNameForSibling(instance);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {objectName}");
            Selection.activeGameObject = instance;
        }
    }
}
