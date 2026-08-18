using System;
using UnityEditor;
using UnityEngine;

namespace Tempura
{
    public readonly struct LabelScope : IDisposable
    {
        public LabelScope(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
        }
        public void Dispose()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    public readonly struct IndentScope : IDisposable
    {
        private readonly int _indentOffset;

        public IndentScope(int indentOffset = 1)
        {
            _indentOffset = indentOffset;
            EditorGUI.indentLevel += _indentOffset;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel -= _indentOffset;
        }
    }

    public readonly struct BoxScope : IDisposable
    {
        private readonly bool _ended;

        public BoxScope(GUIStyle style, float widthRatio = 1.0f)
        {
            float totalWidth = EditorGUIUtility.currentViewWidth;
            float boxWidth = totalWidth * Mathf.Clamp01(widthRatio);

            EditorGUILayout.BeginVertical(style, GUILayout.Width(boxWidth));
            _ended = true;
        }

        public void Dispose()
        {
            if (_ended)
                EditorGUILayout.EndVertical();
        }
    }

    public readonly struct HorizontalScope : IDisposable
    {
        public HorizontalScope(int n = 0)
        {
            EditorGUILayout.BeginHorizontal();
        }
        public HorizontalScope(GUIStyle style)
        {
            EditorGUILayout.BeginHorizontal(style);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    public readonly struct VerticalScope : IDisposable
    {
        public VerticalScope(int n = 0)
        {
            EditorGUILayout.BeginVertical();
        }

        public VerticalScope(GUIStyle style)
        {
            EditorGUILayout.BeginVertical(style);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

    public static class TempuraGui
    {
        public static GUIStyle noMarginBox = InitializeBox(1, 0, 0, false);
        public static GUIStyle noMarginBoxB = InitializeBox(1, 0, 0, true);
        public static GUIStyle smallBox = InitializeBox(1, 2, 2, false);
        public static GUIStyle smallBoxB = InitializeBox(1, 2, 2, true);
        public static GUIStyle largeBox = InitializeBox(1, 5, 5, false);
        public static GUIStyle largeBoxB = InitializeBox(1, 5, 5, true);
        public static GUIStyle shrikenFoldOut = new GUIStyle("ShurikenModuleTitle");
        public static GUIStyle foldoutStyle = new GUIStyle(EditorStyles.label)
        {
            font = EditorStyles.label.font,
            fontSize = EditorStyles.label.fontSize + 2,
            fontStyle = EditorStyles.label.fontStyle,
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(24, 7, 4, 4),
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 28
        };

        public static GUIStyle borderBox = CreateBorderBoxStyle(
            new Color(0.27f, 0.27f, 0.27f, 0.5f),
            new Color(0.4f, 0.4f, 0.4f, 1.0f));


        public static GUIStyle InitializeBox(int border, int margin, int padding, bool background = false)
        {
            GUIStyle style = new GUIStyle();
            if (background) style = GUI.skin.box;

            style.border = new RectOffset(border, border, border, border);
            style.margin = new RectOffset(margin, margin, margin, margin);
            style.padding = new RectOffset(padding, padding, padding, padding);
            style.overflow = new RectOffset(0, 0, 0, 0);

            return style;
        }

        public static GUIStyle CreateBorderBoxStyle(Color backgroundColor, Color borderColor, int margin = 2, int padding = 4)
        {
            var texture = CreateBorderTexture(backgroundColor, borderColor);
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(margin, margin, margin, margin),
                padding = new RectOffset(padding, padding, padding, padding),
                overflow = new RectOffset(0, 0, 0, 0)
            };

            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onActive.background = texture;
            style.onFocused.background = texture;

            return style;
        }

        private static Texture2D CreateBorderTexture(Color backgroundColor, Color borderColor)
        {
            var texture = new Texture2D(3, 3, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    bool isBorder = x == 0 || x == 2 || y == 0 || y == 2;
                    texture.SetPixel(x, y, isBorder ? borderColor : backgroundColor);
                }
            }

            texture.Apply();
            return texture;
        }

        public static bool FoldOut(string title, string help, bool fold)
        {
            var rect = GUILayoutUtility.GetRect(0f, foldoutStyle.fixedHeight, foldoutStyle);
            if (Event.current.type == EventType.Repaint)
            {
                var backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                var borderColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

                EditorGUI.DrawRect(rect, backgroundColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), borderColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), borderColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), borderColor);
                EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), borderColor);
            }

            var labelRect = new Rect(
                rect.x + foldoutStyle.padding.left,
                rect.y + -2f,
                rect.width - foldoutStyle.padding.horizontal,
                rect.height);
            GUI.Label(labelRect, new GUIContent(title, help), foldoutStyle);

            var event_ = Event.current;

            var toggleRect = new Rect(rect.x + 10f, rect.y + 7f, 13f, 13f);
            if (event_.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, fold, false);
            }

            if (event_.type == EventType.MouseDown && rect.Contains(event_.mousePosition))
            {
                fold = !fold;
                event_.Use();
            }

            EditorGUILayout.Space(-2);

            return fold;
        }

        public static void DrawHorizontalLine(float thickness = 1f, float padding = 6f)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness + padding);
            rect.height = thickness;
            rect.y += padding / 2f;
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
        }

        public static void IntField(string title, MaterialProperty prop, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            int value = (int)prop.floatValue; // intValue is not work
            value = EditorGUILayout.IntField(title, value);
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                editor.RegisterPropertyChangeUndo($"Change {title}");
                prop.floatValue = value;
            }
            EditorGUI.showMixedValue = false;
        }

        public static bool Popup<T>(string title, MaterialProperty prop, MaterialEditor editor) where T : struct
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            var ret = EditorGUILayout.Popup(title, (int)prop.floatValue, Enum.GetNames(typeof(T)));
            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                editor.RegisterPropertyChangeUndo($"Change {title}");
                prop.floatValue = ret;
            }

            EditorGUI.showMixedValue = false;
            return changed;
        }

        public static void Space2()
        {
            GUILayout.Space(2);
        }

        public static void Space4()
        {
            GUILayout.Space(4);
        }
        public static T Cast<T>(int value) where T : Enum
        {
            return (T)(object)value;
        }
        public static T Cast<T>(MaterialProperty matProperty) where T : Enum
        {
            int value = matProperty.intValue;
            return Cast<T>(value);
        }
        public static T Cast<T>(Material material, String title) where T : Enum
        {
            int value = material.GetInt(title);
            return Cast<T>(value);
        }

        public static bool Cast(float value)
        {
            return value > 0;
        }

        // Shader Keyward
        public static void ToggleKeyward(Material material, String keyward, bool enable)
        {
            if (enable)
            {
                material.EnableKeyword(keyward);
            }
            else
            {
                material.DisableKeyword(keyward);
            }

        }

        // The order of the keywords to be given should match the enum
        public static void ExcusiveKeyward<T>(Material material, string propName, string[] keywords) where T : Enum
        {
            T select = Cast<T>(material, propName);
            ExcusiveKeyward(material, select, keywords);
        }

        public static void ExcusiveKeyward<T>(Material material, T select, string[] keywords) where T : Enum
        {
            int enumIdx = Convert.ToInt32(select);
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i == enumIdx)
                {
                    material.EnableKeyword(keywords[i]);
                }
                else
                {
                    material.DisableKeyword(keywords[i]);
                }
            }
        }
    }
}
