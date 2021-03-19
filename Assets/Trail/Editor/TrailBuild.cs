using System;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using UnityEngine;

namespace Trail
{
    public class TrailBuild
    {
        #region Delegates

        public delegate void PreBuildCallback();
        public delegate BuildReport CustomBuildCallback();

        #endregion

        #region Variables

        public static event PreBuildCallback OnPreBuild = null;
        public static event CustomBuildCallback CustomBuild = null;
        private static TrailBuildCache cache = null;

        #endregion

        #region Properties

        public static DateTime BuildTime
        {
            get
            {
                VerifyCache();
                return cache.BuildTime;
            }
        }

        public static DateTime UploadTime
        {
            get
            {
                VerifyCache();
                return cache.UploadTime;
            }
        }

        public static bool IsUploaded
        {
            get
            {
                VerifyCache();
                return cache.Uploaded;
            }
        }

        public static string BuildLocationPath
        {
            get
            {
                VerifyCache();
                if (HasBuild())
                {
                    return cache.BuildLocationPath;
                }
                return "";
            }
        }

        public static string BuildLocationRelativePath
        {
            get
            {
                VerifyCache();
                if (HasBuild())
                {
                    return cache.BuildLocationRelativePath;
                }
                return "";
            }
        }

        #endregion

        #region Loading

        [InitializeOnLoadMethod]
        private static void LoadCache()
        {
            VerifyCache();
        }

        private static void VerifyCache()
        {
            if (cache == null)
            {
                cache = new TrailBuildCache();
                cache.Load();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verifies if a build is tracked by build cache. If manually deleted any files, this might give false results.
        /// </summary>
        /// <returns></returns>
        public static bool HasBuild()
        {
            return System.IO.File.Exists(TrailEditor.BuildInfoTrailPath) && System.IO.Directory.Exists(cache.BuildLocationPath);
        }

        /// <summary>
        /// This will run a full report to verify how many required issues in need to fix before uploading to Trail.
        /// </summary>
        /// <returns></returns>
        public static int RunFullReport()
        {
            return ReportWindow.RunFullReport();
        }

        /// <summary>
        /// Used to update build cache to track build location and time.
        /// </summary>
        /// <param name="report"></param>
        public static void UpdateBuildCache(BuildReport report)
        {
            if (report == null)
            {
                return;
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                return;
            }
            cache = new TrailBuildCache(report);
            cache.Save();
        }

        /// <summary>
        /// The build pipeline used when pressing "Build" in Trail SDK Editor
        /// </summary>
        /// <returns>The build report given by Unity</returns>
        public static BuildReport Build()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Build Failed", "Can't do build while compling code!", "Ok");
                return null;
            }
            if (EditorApplication.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Build", "Can't do build while playing, do you want to stop playmode?", "Yes", "Cancel"))
                {
                    EditorApplication.isPlaying = false;
                }
                else
                {
                    return null;
                }
            }

            if (OnPreBuild != null)
            {
                OnPreBuild.Invoke();
            }

            BuildReport report = CustomBuild != null ? CustomBuild.Invoke() : DefaultBuild();
            UpdateBuildCache(report);
            return report;
        }

        /// <summary>
        /// Our build setup, a default WebGL setup without developer mode.
        /// </summary>
        /// <returns></returns>
        private static BuildReport DefaultBuild()
        {
            var reportCount = RunFullReport();
            if (reportCount > 0)
            {
                var res = EditorUtility.DisplayDialogComplex("Report",
                string.Format("Trail Report has identified {0} critical issue{1} with your build.", reportCount, reportCount > 1 ? "s" : ""),
                "Open report window", "Cancel", "Build anyway");
                if (res == 0)
                {
                    ReportWindow.Open();
                    return null;
                }
                if (res == 1)
                {
                    return null;
                }
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUtility.DisplayDialog("Build Failed", "Wrong platform selected! Select WebGL before attempting to build for Trail.", "Ok");
                return null;
            }

            BuildPlayerOptions options = new BuildPlayerOptions();

            options.scenes = EditorBuildSettings.scenes
                .Where(x => x.enabled)
                .Select(x => x.path)
                .ToArray();

            options.target = BuildTarget.WebGL;
            options.targetGroup = BuildTargetGroup.WebGL;
            options.options = BuildOptions.ShowBuiltPlayer;
            options.locationPathName = TrailEditor.BuildTrailPath;

            return UnityEditor.BuildPipeline.BuildPlayer(options);
        }

#if CSHARP_7_3_OR_NEWER

        /// <summary>
        /// Simplified way to upload a build to Trail.
        /// Make sure you already logged into Trail.
        /// </summary>
        /// <returns></returns>
        public static CLI.UploadBuildRequest Upload()
        {
            if (!HasBuild())
            {
                SDK.Log(LogLevel.Warning, "Trail Build", "Attempting to Upload a build, however no build found.");
                return null;
            }
            var request = CLI.UploadBuild(cache.BuildLocationPath);
            request.AddCallback((success) =>
            {
                if (success)
                {
                    UpdateBuildCacheUpload();
                }
            });
            return request;
        }

        /// <summary>
        /// Simplified way to upload a build at given path to Trail.
        /// Make sure you already logged into Trail.
        /// </summary>
        /// <param name="buildPath">Specific path the build is located.</param>
        /// <returns></returns>
        public static CLI.UploadBuildRequest Upload(string buildPath)
        {
            if (!System.IO.Directory.Exists(buildPath))
            {
                SDK.Log(LogLevel.Warning, "Trail Build", "Attempting to upload build, but no directory exists at: " + buildPath);
                return null;
            }
            var request = CLI.UploadBuild(buildPath);
            request.AddCallback((success) =>
            {
                if (success)
                {
                    UpdateBuildCacheUpload();
                }
            });
            return request;
        }

#endif

        /// <summary>
        /// Our internal way to update when a build has been uploaded to Trail.
        /// </summary>
        internal static void UpdateBuildCacheUpload()
        {
            VerifyCache();
            cache.Uploaded = true;
            cache.UploadTime = DateTime.Now;
            cache.Save();
        }

        #endregion
    }

    [Serializable]
    internal class TrailBuildCache
    {
        [SerializeField] private string buildLocationPath = "";
        [SerializeField] private long buildTime = 0L;
        [SerializeField] private bool uploaded = false;
        [SerializeField] private long uploadTime = 0L;
        private string relativePath = "";

        public string BuildLocationPath { get { return buildLocationPath; } set { buildLocationPath = value; relativePath = ""; } }
        public DateTime BuildTime { get { return new DateTime(buildTime); } set { buildTime = value.Ticks; } }

        public bool Uploaded { get { return uploaded; } set { uploaded = value; } }
        public DateTime UploadTime { get { return new DateTime(uploadTime); } set { uploadTime = value.Ticks; } }

        public string BuildLocationRelativePath
        {
            get
            {
                if (string.IsNullOrEmpty(relativePath))
                {
                    if (string.IsNullOrEmpty(buildLocationPath))
                    {
                        return "";
                    }
                    if (System.IO.Path.IsPathRooted(buildLocationPath))
                    {
                        relativePath = buildLocationPath.Replace(TrailEditor.ProjectPath, "");
                    }
                    else
                    {
                        relativePath = buildLocationPath;
                    }
                }
                return relativePath;
            }
        }

        public TrailBuildCache() { }
        public TrailBuildCache(BuildReport report)
        {
            this.buildLocationPath = report.summary.outputPath;
            // calculate offset to use the local time instead of UTC time.
            var diff = DateTime.Now - DateTime.UtcNow;
            this.buildTime = (report.summary.buildEndedAt + diff).Ticks;
            uploaded = false;
            uploadTime = 0L;
        }

        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(this);
                System.IO.File.WriteAllText(TrailEditor.BuildInfoTrailPath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Load()
        {
            try
            {
                if (System.IO.File.Exists(TrailEditor.BuildInfoTrailPath))
                {
                    var json = System.IO.File.ReadAllText(TrailEditor.BuildInfoTrailPath);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
