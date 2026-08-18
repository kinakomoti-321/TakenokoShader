using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace ShaderBindingExporter
{
    public class ShaderInfo
    {
        public List<string> propertyNames = new();
        public List<string> keywordNames = new();
    };

    static public class Parser
    {
        public static ShaderInfo Parse(Shader shader)
        {
            ShaderInfo info = new ShaderInfo();

            int propCount = shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                String propertyName = shader.GetPropertyName(i);
                info.propertyNames.Add(propertyName);
            }

            var keywords = shader.keywordSpace.keywords;
            foreach (var keyword in keywords)
            {
                String keywordName = keyword.name;
                info.keywordNames.Add(keywordName);
            }

            return info;
        }
    };

}