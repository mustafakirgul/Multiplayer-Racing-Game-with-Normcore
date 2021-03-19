#if UNITY_EDITOR
#if UNITY_2018_1_OR_NEWER
#define BUILDREPORT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
#if BUILDREPORT
using UnityEditor.Build.Reporting;
#endif

namespace Trail.Example
{
#if BUILDREPORT
    /// <summary>
    /// Simple class to include "big.file" in postprocess of a build when TrailExample scene is active.
    /// </summary>
    internal class ExampleProjectEditor : IPostprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder { get { return 100; } }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
#if UNITY_WEBGL

            if (report.summary.platformGroup != BuildTargetGroup.WebGL)
            {
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            bool includesExample = false;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path.EndsWith("Trail/Example/Scenes/TrailExample.unity"))
                {
                    includesExample = true;
                }
            }

            if (!includesExample)
            {
                return;
            }

            if (report.summary.result == BuildResult.Failed || report.summary.result == BuildResult.Cancelled)
            {
                Debug.LogError("Build failed, can't apply big.file to the build! " + report.summary.result);
                return;
            }

            byte[] data = null;
            var bigFileAsset = AssetDatabase.FindAssets("big");
            for (int i = 0; i < bigFileAsset.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(bigFileAsset[i]);
                if (path.EndsWith("big.file"))
                {
                    data = System.IO.File.ReadAllBytes(path);
                }
            }

            if (data == null)
            {
                Debug.LogError("big.file does not exist, can't add it to Streaming Assets");
                return;
            }

            var outputPath = report.summary.outputPath;
            var streamingAssetsFolder = System.IO.Path.Combine(outputPath, "StreamingAssets");
            if (!System.IO.Directory.Exists(streamingAssetsFolder))
            {
                System.IO.Directory.CreateDirectory(streamingAssetsFolder);
            }

            var bigFilePath = System.IO.Path.Combine(streamingAssetsFolder, "big.file");

            System.IO.File.WriteAllBytes(bigFilePath, data);
#endif
        }

    }
#else
    /// <summary>
    /// Simple class to include "big.file" in postprocess of a build when TrailExample scene is active.
    /// </summary>
    internal class ExampleProjectEditor : IPostprocessBuild
    {
        int IOrderedCallback.callbackOrder { get { return 100; } }

        void IPostprocessBuild.OnPostprocessBuild(BuildTarget target, string outputPath)
        {

            if (target != BuildTarget.WebGL)
            {
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            bool includesExample = false;
            for (int i = 0; i < scenes.Length; i++)
            {
                Debug.LogError(scenes[i].path);
                if (scenes[i].path.EndsWith("Trail/Example/Scenes/TrailExample.unity"))
                {
                    Debug.Log("Found example scene!");
                    includesExample = true;
                }
            }

            if (!includesExample)
            {
                Debug.Log("No example scene provided");
                return;
            }

            byte[] data = null;
            var bigFileAsset = AssetDatabase.FindAssets("big");
            for (int i = 0; i < bigFileAsset.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(bigFileAsset[i]);
                if (path.EndsWith("big.file"))
                {
                    data = System.IO.File.ReadAllBytes(path);
                }
            }

            if (data == null)
            {
                Debug.LogError("big.file does not exist, can't add it to Streaming Assets");
                return;
            }

            var streamingAssetsFolder = System.IO.Path.Combine(outputPath, "StreamingAssets");
            if (!System.IO.Directory.Exists(streamingAssetsFolder))
            {
                System.IO.Directory.CreateDirectory(streamingAssetsFolder);
            }

            var bigFilePath = System.IO.Path.Combine(streamingAssetsFolder, "big.file");

            System.IO.File.WriteAllBytes(bigFilePath, data);
        }
    }
#endif
}
#endif // UNITY_EDITOR
