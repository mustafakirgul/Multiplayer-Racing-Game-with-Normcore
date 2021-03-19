using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Trail
{
    internal class ShaderFixes
    {
        #region Shader Level

        public const int SHADER_LEVEL = 35;
        public static string ShaderLevel { get { return string.Format("{0:0.0}", SHADER_LEVEL / 10f); } }
        public static float ShaderLevelFloat { get { return SHADER_LEVEL / 10f; } }
        public static int ToShaderLevel(string val) { return (int)(float.Parse(val) * 10); }

        #endregion


        private static Cache[] caches;


        [InitializeOnLoadMethod]
        private static void LoadShaderReports()
        {
            caches = AssetDatabase.FindAssets("t:Shader", new string[] { "Assets" })
                .Select(x => new Cache(AssetDatabase.GUIDToAssetPath(x)))
                .ToArray();
            for (int i = 0, length = caches.Length; i < length; i++)
            {
                var cache = caches[i];
                var report = Report.Create(
                    string.Format("'{0}' pragma target", cache.name),
                    "This is to limit the shader level to the maximum OpenGLES3 supports, at " + ShaderLevel,
                    ReportCategory.Shaders,
                    @"https://docs.unity3d.com/Manual/SL-ShaderCompileTargets.html",
                    () => cache.HasRecommendedFixes ? ReportState.Recommended : ReportState.Hidden,
                    new ReportAction(new GUIContent("Fix", "Set pragma target to 3.5"), cache.FixAll)
                    );
            }
        }

        private class Cache
        {
            public string name;
            public string path;
            public string[] text;
            public float[] pragmaTarget;
            public int[] pragmaTargetLines;
            public bool isDirty = false;

            public bool HasRecommendedFixes
            {
                get
                {
                    LoadFile();
                    for (int i = 0; i < pragmaTarget.Length; i++)
                    {
                        if (pragmaTarget[i] > ShaderLevelFloat)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public Cache(string path)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path);
                this.path = path;
                // LoadFile(path);
            }

            private void LoadFile()
            {
                if (!System.IO.File.Exists(path))
                    return;
                List<int> lineNumber = new List<int>();
                List<float> pragmaValue = new List<float>();
                text = System.IO.File.ReadAllLines(path);
                for (int i = 0, length = text.Length; i < length; i++)
                {
                    string line = text[i];
                    if (line.Trim().StartsWith("#pragma target"))
                    {
                        lineNumber.Add(i);
                        float result = 0;
                        float.TryParse(line.Trim().Remove(0, 14), out result);
                        pragmaValue.Add(result);
                    }
                }
                pragmaTargetLines = lineNumber.ToArray();
                pragmaTarget = pragmaValue.ToArray();
            }

            public void FixAll()
            {
                for (int i = 0; i < pragmaTarget.Length; i++)
                {
                    if (pragmaTarget[i] > ShaderLevelFloat)
                    {
                        Fix(i);
                    }
                }
                if (isDirty)
                {
                    System.IO.File.WriteAllLines(path, text);
                }
            }

            public void Fix(int index)
            {
                pragmaTarget[index] = Mathf.Min(ShaderLevelFloat, pragmaTarget[index]);
                text[pragmaTargetLines[index]] = "#pragma target " + pragmaTarget[index];
                isDirty = true;
            }
        }

    }
}
