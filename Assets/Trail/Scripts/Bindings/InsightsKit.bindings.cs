#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WII || UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS4 || UNITY_XBOXONE || UNITY_TIZEN || UNITY_TVOS || UNITY_WSA || UNITY_FACEBOOK)
#define UNITY
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
#define BROWSER
#endif

#if (UNITY && ENABLE_IL2CPP)
#define AOT
#endif


using System;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
#if AOT
using AOT;
#endif
#if UNITY
using UnityEngine.SceneManagement;
#endif

namespace Trail
{
    public static partial class InsightsKit
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Scene
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = InsightsKit.MaxSceneIDLength + 1)]
            public byte[] id;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = InsightsKit.MaxSceneNameLength + 1)]
            public byte[] name;

            public Scene(string id, string name)
            {
                this.id = new byte[InsightsKit.MaxSceneIDLength + 1];
                Common.GetUTF8BytesSafely(id, this.id);

                this.name = new byte[InsightsKit.MaxSceneNameLength + 1];
                Common.GetUTF8BytesSafely(name, this.name);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct QualityLevel
        {
            public Int32 level;

            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = InsightsKit.MaxQualityLevelNameLength + 1
            )]
            public byte[] name;

            public QualityLevel(Int32 level, string name)
            {
                this.level = level;
                this.name = new byte[InsightsKit.MaxQualityLevelNameLength + 1];
                Common.GetUTF8BytesSafely(name, this.name);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CustomEvent
        {
            [MarshalAs(
                UnmanagedType.ByValArray,
                SizeConst = InsightsKit.MaxCustomEventNameLength + 1
            )]
            public byte[] name;
            public IntPtr payload_json;
            public Int32 payload_json_length;

            public CustomEvent(string name)
            {
                this.name = new byte[InsightsKit.MaxCustomEventNameLength + 1];
                Common.GetUTF8BytesSafely(name, this.name);
                this.payload_json = IntPtr.Zero;
                this.payload_json_length = 0;
            }
        }

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ink_report_scene_changed(IntPtr sdk, ref Scene scene);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ink_report_game_resolution_changed(
            IntPtr sdk,
            ref Resolution resolution
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ink_report_quality_level_changed(
            IntPtr sdk,
            ref QualityLevel quality_level
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ink_report_first_gameplay_event(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ink_send_custom_event(
            IntPtr sdk,
            ref CustomEvent custom_event
        );
    }
}
