using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Tempura
{
    public static partial class TempuraGui
    {
        public static void BoxBound(string title, MaterialProperty min, MaterialProperty max, MaterialEditor editor)
        {
            editor.ShaderProperty(min, new GUIContent($"{title} min"));
            editor.ShaderProperty(max, new GUIContent($"{title} max"));
            BoundHandle.DrawEditButton(editor.targets.OfType<Material>().ToArray(), BoundHandle.Shape.Box, min.name, max.name);
        }

        public static void SphereBound(string title, MaterialProperty center, MaterialProperty radius, MaterialEditor editor)
        {
            editor.ShaderProperty(center, new GUIContent($"{title} center"));
            editor.ShaderProperty(radius, new GUIContent($"{title} radius"));
            BoundHandle.DrawEditButton(editor.targets.OfType<Material>().ToArray(), BoundHandle.Shape.Sphere, center.name, radius.name);
        }
    }

    public static class BoundHandle
    {
        public enum Shape
        {
            Box,
            Sphere
        }

        private static readonly BoxBoundsHandle boxHandle = new BoxBoundsHandle();
        private static Material[] targets;
        private static Shape shape;
        private static string positionProperty;
        private static string sizeProperty;

        private static readonly Color handleColor = new Color(0.3f, 0.8f, 1.0f, 1.0f);

        public static bool IsEditing(Material material)
        {
            return material != null && targets != null && targets.Contains(material);
        }

        public static void DrawEditButton(Material[] materials, Shape boundShape, string positionPropertyName, string sizePropertyName)
        {
            bool editing = materials.Length > 0 && targets != null && targets.SequenceEqual(materials) &&
                shape == boundShape && positionProperty == positionPropertyName && sizeProperty == sizePropertyName;
            bool next = GUILayout.Toggle(editing, "Editing", GUI.skin.button);
            if (next == editing) return;

            if (next)
            {
                Begin(materials, boundShape, positionPropertyName, sizePropertyName);
            }
            else
            {
                End();
            }
        }

        public static void Begin(Material[] materials, Shape boundShape, string positionPropertyName, string sizePropertyName)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged -= End;
            Selection.selectionChanged += End;
            targets = materials.ToArray();
            shape = boundShape;
            positionProperty = positionPropertyName;
            sizeProperty = sizePropertyName;
            SceneView.RepaintAll();
        }

        public static void End()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= End;
            targets = null;
            positionProperty = null;
            sizeProperty = null;
            SceneView.RepaintAll();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (targets == null || targets.Length == 0 || targets.Any(m => m == null || !m.HasProperty(positionProperty) || !m.HasProperty(sizeProperty)))
            {
                End();
                return;
            }

            var material = targets[0];
            switch (shape)
            {
                case Shape.Box:
                    DrawBoxHandle(material);
                    break;
                case Shape.Sphere:
                    DrawSphereHandle(material);
                    break;
                default:
                    End();
                    break;
            }
        }

        private static void DrawBoxHandle(Material material)
        {
            Vector3 min = material.GetVector(positionProperty);
            Vector3 max = material.GetVector(sizeProperty);

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

                Undo.RecordObjects(targets, "Edit box bounds");
                foreach (var target in targets)
                {
                    target.SetVector(positionProperty, newMin);
                    target.SetVector(sizeProperty, newMax);
                }
            }
        }

        private static void DrawSphereHandle(Material material)
        {
            Vector3 center = material.GetVector(positionProperty);
            float radius = material.GetFloat(sizeProperty);

            using (new Handles.DrawingScope(handleColor))
            {
                EditorGUI.BeginChangeCheck();
                center = Handles.PositionHandle(center, Quaternion.identity);
                radius = Handles.RadiusHandle(Quaternion.identity, center, radius);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(targets, "Edit sphere bounds");
                    foreach (var target in targets)
                    {
                        target.SetVector(positionProperty, center);
                        target.SetFloat(sizeProperty, Mathf.Max(radius, 0.0f));
                    }
                }
            }
        }
    }
}
