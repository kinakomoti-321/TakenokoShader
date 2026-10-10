using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace ShaderBindingExporter
{
    static public class CodeGenerator
    {
        public struct OutputInfo
        {
            public string nameSpace;
            public string filePath;

            public string propsEnumName;
            public string propsDicName;

            public string keywordsEnumName;
            public string keywordsDicName;

            public bool enableExtension;
            public bool enableProperty;
            public bool enableKeyword;
        }

        public static void Generate(ShaderInfo shaderInfo, OutputInfo outputInfo)
        {
            string enumName = outputInfo.propsEnumName;
            string dicName = outputInfo.propsDicName;

            if (shaderInfo == null)
            {
                Debug.LogError("[CodeGenerator] ShaderInfo is null");
                return;
            }

            if (shaderInfo.propertyNames.Count == 0)
            {
                Debug.LogWarning("[CodeGenerator] No properties found in shader");
                return;
            }

            string code = CodeProperty(shaderInfo.propertyNames, outputInfo.nameSpace, enumName, dicName);
            string path = Path.Combine(outputInfo.filePath, $"{enumName}.cs");

            if (outputInfo.enableProperty)
            {
                File.WriteAllText(path, code);
                Debug.Log($"[CodeGenerator] Generated {enumName} and {dicName} at: {code}");
            }

            code = CodeEnumExtension(outputInfo.nameSpace);
            path = Path.Combine(outputInfo.filePath, "EnumExtension.cs");
            if (outputInfo.enableExtension)
            {
                File.WriteAllText(path, code);
                Debug.Log($"[CodeGenerator] Generated EnumExtension at: {path}");
            }

            code = CodeKeyward(shaderInfo.keywordNames, outputInfo, "ShaderKeywords", "ShaderKeywordsNames");
            path = Path.Combine(outputInfo.filePath, "ShaderKeywords.cs");
            if (outputInfo.enableKeyword)
            {
                File.WriteAllText(path, code);
                Debug.Log($"[CodeGenerator] Generated ShaderKeywords at: {path}");
            }

            AssetDatabase.Refresh();

        }

        private static string CodeEnumExtension(String nameSpace)
        {
            var enumExtensionBuilder = new StringBuilder();
            enumExtensionBuilder.AppendLine("using System;");
            enumExtensionBuilder.AppendLine("using System.ComponentModel;");
            enumExtensionBuilder.AppendLine();
            enumExtensionBuilder.AppendLine("namespace " + nameSpace);
            enumExtensionBuilder.AppendLine("{");
            enumExtensionBuilder.AppendLine("    public static class EnumExtension");
            enumExtensionBuilder.AppendLine("    {");
            enumExtensionBuilder.AppendLine("        public static string Name(this Enum value)");
            enumExtensionBuilder.AppendLine("        {");
            enumExtensionBuilder.AppendLine("            var field = value.GetType().GetField(value.ToString());");
            enumExtensionBuilder.AppendLine("            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;");
            enumExtensionBuilder.AppendLine("            return attr?.Description ?? value.ToString();");
            enumExtensionBuilder.AppendLine("        }");
            enumExtensionBuilder.AppendLine("    }");
            enumExtensionBuilder.AppendLine("}");
            return enumExtensionBuilder.ToString();
        }


        private static string CodeProperty(List<string> propertyNames, string nameSpace = "", string enumName = "ShaderProps", string dicName = "ShaderPropsNames")
        {
            var propBuilder = new StringBuilder();
            propBuilder.AppendLine("using System.Collections.Generic;");
            propBuilder.AppendLine("using System.ComponentModel;");

            if (!string.IsNullOrEmpty(nameSpace))
            {
                propBuilder.AppendLine($"namespace {nameSpace}");
                propBuilder.AppendLine("{");
            }

            propBuilder.AppendLine($"public enum {enumName}");
            propBuilder.AppendLine("{");

            var nameMap = new Dictionary<string, string>();
            foreach (string prop in propertyNames)
            {
                string enumEntry = prop.TrimStart('_');
                string safeName = enumEntry;
                int index = 1;
                while (nameMap.ContainsKey(safeName))
                    safeName = enumEntry + "_" + index++;
                nameMap[safeName] = prop;
                propBuilder.AppendLine($"    [Description(\"{prop}\")]");
                propBuilder.AppendLine($"    {safeName},");
            }

            propBuilder.AppendLine("}");

            propBuilder.AppendLine();
            propBuilder.AppendLine($"public static class {dicName}");
            propBuilder.AppendLine("{");
            propBuilder.AppendLine($"    public static readonly Dictionary<{enumName}, string> NameTable = new Dictionary<{enumName}, string>");
            propBuilder.AppendLine("    {");

            foreach (var pair in nameMap)
            {
                propBuilder.AppendLine($"        {{ {enumName}.{pair.Key}, \"{pair.Value}\" }},");
            }

            propBuilder.AppendLine("    };");
            propBuilder.AppendLine($"public static IReadOnlyDictionary<{enumName}, string> NameTableReadonly => NameTable;");
            propBuilder.AppendLine("}");

            if (!string.IsNullOrEmpty(nameSpace))
            {
                propBuilder.AppendLine("}");
            }

            return propBuilder.ToString();
        }
        private static String CodeKeyward(List<string> keywordNames, OutputInfo outputInfo, string enumName = "ShaderKeywords", string dicName = "ShaderKeywordsNames")
        {
            var keywordBuilder = new StringBuilder();
            keywordBuilder.AppendLine("using System.Collections.Generic;");
            keywordBuilder.AppendLine("using System.ComponentModel;");
            keywordBuilder.AppendLine();

            if (!string.IsNullOrEmpty(outputInfo.nameSpace))
            {
                keywordBuilder.AppendLine($"namespace {outputInfo.nameSpace}");
                keywordBuilder.AppendLine("{");
            }

            keywordBuilder.AppendLine($"public enum {enumName}");
            keywordBuilder.AppendLine("{");

            var nameMap = new Dictionary<string, string>();
            foreach (var keyword in keywordNames)
            {
                if (keyword.All(c => c == '_')) break;

                string enumEntry = UPPER_SNAKE_ToPascalCase(keyword.TrimStart('_'));
                string safeName = enumEntry;
                int index = 1;
                while (nameMap.ContainsKey(safeName))
                    safeName = enumEntry + "_" + index++;
                nameMap[safeName] = keyword;
                keywordBuilder.AppendLine($"    [Description(\"{keyword}\")]");
                keywordBuilder.AppendLine($"    {safeName},");
            }

            keywordBuilder.AppendLine("}");

            keywordBuilder.AppendLine();
            keywordBuilder.AppendLine($"public static class {dicName}");
            keywordBuilder.AppendLine("{");
            keywordBuilder.AppendLine($"    public static readonly Dictionary<{enumName}, string> NameTable = new Dictionary<{enumName}, string>");
            keywordBuilder.AppendLine("    {");

            foreach (var pair in nameMap)
            {
                keywordBuilder.AppendLine($"        {{ {enumName}.{pair.Key}, \"{pair.Value}\" }},");
            }

            keywordBuilder.AppendLine("    };");
            keywordBuilder.AppendLine($"public static IReadOnlyDictionary<{enumName}, string> NameTableReadonly => NameTable;");
            keywordBuilder.AppendLine("}");

            if (!string.IsNullOrEmpty(outputInfo.nameSpace))
            {
                keywordBuilder.AppendLine("}");
            }

            return keywordBuilder.ToString();
        }

        public static string UPPER_SNAKE_ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var parts = input.ToLower().Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }

            return string.Join("", parts);
        }

    }
}