using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Trail
{
    /// <summary>
    /// NOTE: This feature is only supported for 2018 or newer.
    /// </summary>
    public class WebGLEditorWindow
    {
#if UNITY_2018_3_OR_NEWER
        private static string[] searchWords =
        {
            "webgl",
            "memory"
        };


        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Player/WebGL", SettingsScope.Project)
            {
                keywords = searchWords,
                guiHandler = OnGUI
            };
        }

        private static void OnGUI(string s)
        {
            EditorGUI.indentLevel++;
            var enabled = GUI.enabled;
            GUI.enabled = false;
            GUILayout.Label("Trail Extension");
            GUI.enabled = enabled;
            EditorGUILayout.Space();
            // Memory Size limit -> https://blogs.unity3d.com/2016/09/20/understanding-memory-in-unity-webgl/
            PlayerSettings.WebGL.memorySize = EditorGUILayout.IntSlider("Memory Size", PlayerSettings.WebGL.memorySize, 256, 2032);
            PlayerSettings.WebGL.dataCaching = EditorGUILayout.Toggle("Data Caching", PlayerSettings.WebGL.dataCaching);
            EditorGUILayout.Space();
            PlayerSettings.WebGL.exceptionSupport = (WebGLExceptionSupport)EditorGUILayout.EnumPopup("WebGL Exception Support", PlayerSettings.WebGL.exceptionSupport);
            PlayerSettings.WebGL.emscriptenArgs = EditorGUILayout.DelayedTextField("Emscripten Args", PlayerSettings.WebGL.emscriptenArgs);
            PlayerSettings.WebGL.modulesDirectory = EditorGUILayout.DelayedTextField("Modules Directory", PlayerSettings.WebGL.modulesDirectory);
            PlayerSettings.WebGL.template = EditorGUILayout.DelayedTextField("Template", PlayerSettings.WebGL.template);
            EditorGUILayout.Space();
            PlayerSettings.WebGL.analyzeBuildSize = EditorGUILayout.Toggle("Analyze Build Size", PlayerSettings.WebGL.analyzeBuildSize);
            PlayerSettings.WebGL.useEmbeddedResources = EditorGUILayout.Toggle("Use Embedded Resources", PlayerSettings.WebGL.useEmbeddedResources);
            PlayerSettings.WebGL.threadsSupport = EditorGUILayout.Toggle("Threads Support", PlayerSettings.WebGL.threadsSupport);
            PlayerSettings.WebGL.linkerTarget = (WebGLLinkerTarget)EditorGUILayout.EnumPopup("Linker Target", PlayerSettings.WebGL.linkerTarget);
            PlayerSettings.WebGL.compressionFormat = (WebGLCompressionFormat)EditorGUILayout.EnumPopup("Compression Format", PlayerSettings.WebGL.compressionFormat);
            EditorGUILayout.Space();
            PlayerSettings.WebGL.nameFilesAsHashes = EditorGUILayout.Toggle("Name Files As Hashes", PlayerSettings.WebGL.nameFilesAsHashes);
            PlayerSettings.WebGL.debugSymbols = EditorGUILayout.Toggle("Debug Symbols", PlayerSettings.WebGL.debugSymbols);
#if UNITY_2019_1_OR_NEWER && !UNITY_2020_1_OR_NEWER
            PlayerSettings.WebGL.wasmStreaming = EditorGUILayout.Toggle("Wasm Streaming", PlayerSettings.WebGL.wasmStreaming);
#else
            GUI.enabled = false;
            EditorGUILayout.Toggle(new GUIContent("Wasm Streaming", "This feature is only supported on 2019 or newer"), false);
            GUI.enabled = enabled;
#endif
            EditorGUI.indentLevel--;
        }
#endif
    }
}
