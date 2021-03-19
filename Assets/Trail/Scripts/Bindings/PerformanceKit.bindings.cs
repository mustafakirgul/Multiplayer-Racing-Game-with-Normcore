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
using System.Runtime.InteropServices;

namespace Trail
{
    public static partial class PerformanceKit
    {
#if BROWSER

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_get_recommended_quality_level(
            IntPtr sdk,
            out Int32 quality_level
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_get_recommended_game_resolution(
            IntPtr sdk,
            [Out, MarshalAs(UnmanagedType.LPStruct)] out Resolution resolution
        );

#if UNITY
        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_unity_get_game_resolution(
            IntPtr sdk,
            [Out, MarshalAs(UnmanagedType.LPStruct)] out Resolution resolution
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_unity_set_game_resolution(
            IntPtr sdk,
            [In, MarshalAs(UnmanagedType.LPStruct)] ref Resolution resolution,
            bool match_common,
            bool cap_to_screen
        );
#endif

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_get_screen_resolution(
            IntPtr sdk,
            [Out, MarshalAs(UnmanagedType.LPStruct)] out Resolution resolution
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_get_common_resolutions(
            IntPtr sdk,
            out IntPtr resolutions,
            out Int32 resolutions_count
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_set_on_screen_resolution_changed(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private extern static Result trail_pfk_set_aspect_ratio(
            IntPtr sdk,
            float aspect_ratio
        );
#endif
    }
}
