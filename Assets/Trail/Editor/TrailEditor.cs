using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Trail
{
    internal class TrailEditor
    {
        #region Variables

        private static string projectPath = "";
        private static string localTrailPath = "";
        private static string buildTrailPath = "";
        private static string buildInfoTrailPath = "";
        private static string trailYaml = "";
        private static string cliInfoPath = "";

        #endregion

        #region Properties

        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    Initialize();
                }
                return projectPath;
            }
        }

        public static string LocalTrailPath
        {
            get
            {
                if (string.IsNullOrEmpty(localTrailPath))
                {
                    Initialize();
                }
                return localTrailPath;
            }
        }

        public static string BuildTrailPath
        {
            get
            {
                if (string.IsNullOrEmpty(buildTrailPath))
                {
                    Initialize();
                }
                return buildTrailPath;
            }
        }

        public static string BuildInfoTrailPath
        {
            get
            {
                if (string.IsNullOrEmpty(buildInfoTrailPath))
                {
                    Initialize();
                }
                return buildInfoTrailPath;
            }
        }

        public static string TrailYamlPath
        {
            get
            {
                if (string.IsNullOrEmpty(trailYaml))
                {
                    Initialize();
                }
                return trailYaml;
            }
        }

        public static string CLIStoragePath
        {
            get
            {
                if (string.IsNullOrEmpty(cliInfoPath))
                {
                    Initialize();
                }
                return cliInfoPath;
            }
        }

        #endregion

        #region Setup

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            //TODO change from "/Local" path to library and have user select build path.
            projectPath = Application.dataPath.Replace("/Assets", "");
            localTrailPath = projectPath + "/Local/Trail";
            buildTrailPath = localTrailPath + "/Build";
            buildInfoTrailPath = localTrailPath + "/buildinfo.txt";
            trailYaml = projectPath + "/trail.yaml";
            cliInfoPath = projectPath + "/Local/Trail/.cli";
        }

        [InitializeOnLoadMethod]
        private static void SetupScriptingDefineSymbol()
        {
            //TODO: Add this to either Report tools or make a first time popup and ask user to add.
            //      Also add support for Perforce checkout.
            var currentScriptDefineSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
            if (!currentScriptDefineSymbol.Contains("TRAIL"))
            {
                if (string.IsNullOrEmpty(currentScriptDefineSymbol))
                {
                    currentScriptDefineSymbol = "TRAIL";
                }
                else
                {
                    currentScriptDefineSymbol += ";TRAIL";
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, currentScriptDefineSymbol);
            }
        }

        #endregion

        #region Helper Methods

        public static string GetTrailDirectory()
        {
            string[] asmdefGuids = AssetDatabase.FindAssets("t:asmdef", new string[] { "Assets" });
            string asmdefGuid = asmdefGuids.FirstOrDefault((guid) =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                System.IO.FileInfo file = new System.IO.FileInfo(path);
                if (!file.Exists)
                {
                    return false;
                }
                return file.Name == "Trail.asmdef";
            });
            if (string.IsNullOrEmpty(asmdefGuid))
            {
                SDK.Log(LogLevel.Error, "Trail Editor", "Failed to find Trail directory. Have Trail.asmdef been deleted from the project?");
                return "";
            }
            System.IO.FileInfo asmdefFile = new System.IO.FileInfo(AssetDatabase.GUIDToAssetPath(asmdefGuid));
            return asmdefFile.DirectoryName.Replace("\\", "/");
        }

        public static string GetCLIName()
        {
            return
#if UNITY_EDITOR_WIN
                    "Trail CLI Windows.exe";
#elif UNITY_EDITOR_OSX
                    "Trail CLI MacOS";
#elif UNITY_EDITOR_LINUX
                    "Trail CLI Linux";
#else
#error Unsupported platform
                    "";
#endif
        }

        public static string GetCLIPath() 
        {
            return string.Format("{0}/Editor/CLI/{1}", GetTrailDirectory(), GetCLIName());
        }

        public static string GetProjectRelativePath(string path)
        {
            System.Uri pathUri = new System.Uri(path, System.UriKind.Absolute);
            System.Uri dataPathUri = new System.Uri(Application.dataPath, System.UriKind.Absolute);
            return dataPathUri.MakeRelativeUri(pathUri).ToString();
        }

        #endregion
    }
}
