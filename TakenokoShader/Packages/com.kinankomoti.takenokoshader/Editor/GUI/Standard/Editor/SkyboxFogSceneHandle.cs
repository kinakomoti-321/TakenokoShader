using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Takenoko.Standard
{
    using Tempura;

    // Scene view editing for the SkyboxFog Box / Sphere region.
    // Activated from StandardGUI while the material inspector is open.
    public static class SkyboxFogSceneHandle
    {
        private static readonly BoxBoundsHandle boxHandle = new BoxBoundsHandle();
        private static Material[] targets;

        private static readonly Color handleColor = new Color(0.3f, 0.8f, 1.0f, 1.0f);
        private static readonly Color fillColor = new Color(0.3f, 0.8f, 1.0f, 0.05f);

        public static bool IsEditing(Material material)
        {
            return targets != null && targets.Contains(material);
        }

        public static void DrawEditButton(Material[] materials)
        {
            bool editing = materials.Length > 0 && IsEditing(materials[0]);
            bool next = GUILayout.Toggle(editing, editing ? "Finish Editing" : "Edit in Scene", GUI.skin.button);
            if (next == editing) return;

            if (next)
            {
                Begin(materials);
            }
            else
            {
                End();
            }
        }

        public static void Begin(Material[] materials)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            targets = materials;
            SceneView.RepaintAll();
        }

        public static void End()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            targets = null;
            SceneView.RepaintAll();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (targets == null || targets.Length == 0 || targets.Any(m => m == null))
            {
                End();
                return;
            }

            var material = targets[0];
            if (!TempuraGui.Cast(material.GetFloat(ShaderProps.SkyboxFog.Name())))
            {
                End();
                return;
            }

            var mode = TempuraGui.Cast<SkyboxFogMode>(material, ShaderProps.SkyboxFogMode.Name());
            switch (mode)
            {
                case SkyboxFogMode.Box:
                    DrawBoxHandle(material);
                    break;
                case SkyboxFogMode.Sphere:
                    DrawSphereHandle(material);
                    break;
                default:
                    End();
                    break;
            }
        }

        private static void DrawBoxHandle(Material material)
        {
            Vector3 min = material.GetVector(ShaderProps.SkyboxFogBoxSizeMin.Name());
            Vector3 max = material.GetVector(ShaderProps.SkyboxFogBoxSizeMax.Name());

            boxHandle.center = (min + max) * 0.5f;
            boxHandle.size = max - min;
            boxHandle.SetColor(handleColor);

            EditorGUI.BeginChangeCheck();
            boxHandle.DrawHandle();
            Vector3 center = Handles.PositionHandle(boxHandle.center, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 half = boxHandle.size * 0.5f;
                Vector3 newMin = Vector3.Min(center - half, center + half);
                Vector3 newMax = Vector3.Max(center - half, center + half);

                Undo.RecordObjects(targets, "Edit Skybox Fog Box");
                foreach (var target in targets)
                {
                    target.SetVector(ShaderProps.SkyboxFogBoxSizeMin.Name(), newMin);
                    target.SetVector(ShaderProps.SkyboxFogBoxSizeMax.Name(), newMax);
                }
            }
        }

        private static void DrawSphereHandle(Material material)
        {
            Vector3 center = material.GetVector(ShaderProps.SkyboxFogSphereCenter.Name());
            float radius = material.GetFloat(ShaderProps.SkyboxFogSphereRadius.Name());

            Handles.color = handleColor;

            EditorGUI.BeginChangeCheck();
            center = Handles.PositionHandle(center, Quaternion.identity);
            radius = Handles.RadiusHandle(Quaternion.identity, center, radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Edit Skybox Fog Sphere");
                foreach (var target in targets)
                {
                    target.SetVector(ShaderProps.SkyboxFogSphereCenter.Name(), center);
                    target.SetFloat(ShaderProps.SkyboxFogSphereRadius.Name(), Mathf.Max(radius, 0.0f));
                }
            }
        }
    }
}
