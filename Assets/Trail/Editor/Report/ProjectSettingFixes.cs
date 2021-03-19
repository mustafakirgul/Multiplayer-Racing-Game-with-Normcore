using System;
using UnityEditor;
using UnityEngine.Rendering;

namespace Trail
{
    class ProjectSettingFixes
    {
        [InitializeOnLoadMethod]
        static void SetupWebGLBuildSettings()
        {
            Report.Create(
                "Disable Compression Format",
                "Trail requires the builds to be uploaded uncompressed so it can apply Post-Processing. But don't worry, it will still apply its own compression after the patching is done.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.compressionFormat != WebGLCompressionFormat.Disabled ? ReportState.Required : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled));

            Report.Create(
                "Disable data caching",
                "Trail will handle caching of games to optimize load times.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.dataCaching != false ? ReportState.Required : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.dataCaching = false));

            Report.Create(
                "Disable debug symbols",
                "This Unity setting needs to be disabled so that the Trail Post-Processing can be applied. Don't worry though, debug symbols are still enabled when building for Trail.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.debugSymbols != false ? ReportState.Required : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.debugSymbols = false));

            Report.Create(
                "Enable exception support",
                "Due to a bug with DateTime.Now, games without exception support might crash in some countries. This will cause some impact on performance.",
                ReportCategory.ProjectSettings,
                @"https://docs.unity3d.com/Manual/webgl-performance.html#WebGL-specific_settings_which_affect_performance",
                () =>
                {
#if UNITY_2019_3_OR_NEWER
                    return PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.None ? ReportState.Recommended : ReportState.Hidden;
#else
                    return PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.None ? ReportState.Required : ReportState.Hidden;
#endif
                },
                new ReportAction(new UnityEngine.GUIContent("Fix", "Optional"), () => PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly));

            Report.Create(
                "Assign 256 MB minimum memory",
                "",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.memorySize < 256 ? ReportState.Recommended : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.memorySize = 256));

            Report.Create(
                "Disable file name hashing",
                "",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.nameFilesAsHashes != false ? ReportState.Recommended : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.nameFilesAsHashes = false));

#if UNITY_2018 || UNITY_2019 || UNITY_2020
            Report.Create(
                "Build to Wasm",
                "Trail only support Web Assembly and has to have 'Linker Target' set to Wasm.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.linkerTarget != WebGLLinkerTarget.Wasm ? ReportState.Required : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm));
#endif

#if UNITY_2019
            Report.Create(
                "Disable Wasm streaming",
                "Trail will handle WebAssembly streaming compilation.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.wasmStreaming != false ? ReportState.Required : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.wasmStreaming = false));
#endif

#if UNITY_2019_1_OR_NEWER
            Report.Create(
                "Disable internal thread support",
                "Unity has experimental internal threading support. This will sometimes cause crashes or unexpected errors with Unity and only works on Chrome to some extent.",
                ReportCategory.ProjectSettings,
                @"https://docs.unity3d.com/Manual/webgl-gettingstarted.html#Platform_support",
                () => PlayerSettings.WebGL.threadsSupport != false ? ReportState.Recommended : ReportState.Hidden,
                new ReportAction("Fix", () => PlayerSettings.WebGL.threadsSupport = false));
#endif

            Report.Create(
                "Add hidden emscripten build arguments",
                "When building, a special argument is required for Trail to patch, optimize, and remove overhead.",
                ReportCategory.ProjectSettings,
                @"",
                () => PlayerSettings.WebGL.emscriptenArgs.Contains("-g") ? ReportState.Hidden : ReportState.Required,
                new ReportAction("Fix", () => PlayerSettings.WebGL.emscriptenArgs += " -g"));

            Report.Create(
                "Set graphics API to OpenGLES3 only",
                "Disable old graphics API to get better performance and more features.",
                ReportCategory.ProjectSettings,
                @"",
                () => (PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL).Length > 1 || PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.WebGL)) ? ReportState.Recommended : ReportState.Hidden,
                new ReportAction("Fix", () =>
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });
                }));

            Report.Create(
                "Trail WebGL Template",
                "Select the Trail WebGL template to make it possible to upload. This is only required for Unity 2020.1+",
                ReportCategory.ProjectSettings,
                @"https://docs.unity3d.com/Manual/webgl-templates.html",
#if UNITY_2020_1_OR_NEWER
                () => (PlayerSettings.WebGL.template != "PROJECT:Trail" ? ReportState.Required : ReportState.Hidden),
#else
                () => (PlayerSettings.WebGL.template != "PROJECT:Trail" ? ReportState.Recommended : ReportState.Hidden),
#endif
                new ReportAction("Fix", () => PlayerSettings.WebGL.template = "PROJECT:Trail"));
        }
    }
}
