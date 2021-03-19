#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WII || UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS4 || UNITY_XBOXONE || UNITY_TIZEN || UNITY_TVOS || UNITY_WSA || UNITY_FACEBOOK)
#define UNITY
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
#define BROWSER
#endif

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Trail
{
    public partial class SDK
    {
        #region Structs

#if UNITY
        [StructLayout(LayoutKind.Sequential)]
        private struct CreateParams
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = unity_version_length)]
            public byte[] unity_version;
            public const Int32 unity_version_length = 64;

            public CreateParams(string unityVersion)
            {
                System.Diagnostics.Debug.Assert(unityVersion.Length < unity_version_length);
                this.unity_version = new byte[unity_version_length];
                Encoding.UTF8.GetBytes(
                    unityVersion,
                    0,
                    unityVersion.Length,
                    this.unity_version,
                    0
                );
            }
        }
#endif

#if !BROWSER
        [StructLayout(LayoutKind.Sequential)]
        private struct InitParams
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = dev_host_length)]
            public byte[] dev_host;
            public const Int32 dev_host_length = 256;

            public InitParams(string devHost)
            {
                System.Diagnostics.Debug.Assert(devHost.Length < dev_host_length);
                this.dev_host = new byte[dev_host_length];
                Encoding.UTF8.GetBytes(devHost, 0, devHost.Length, this.dev_host, 0);
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private struct StartupArgRaw
        {
            public const Int32 name_length = 256;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = name_length)]
            public byte[] name;
            public IntPtr value;
            public Int32 value_size;
        }

        #endregion

        #region Delegates

        // Used for communication between c/c++ and c#
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void InitCB(Result result, IntPtr callback_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GameActiveStatusChangedCB(bool gameActive, IntPtr callback_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogCB(
            LogLevel level,
            IntPtr location,
            Int32 location_length,
            IntPtr message,
            Int32 message_length,
            IntPtr callback_data
        );

        #endregion

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_create(
#if UNITY
            ref CreateParams config,
#endif
            out IntPtr sdk
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_destroy(IntPtr sdk);

#if !BROWSER
        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_sdk_run_loop_iteration(IntPtr sdk);
#endif

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_sdk_init(
            IntPtr sdk,
#if !BROWSER
                ref InitParams initParams,
#endif
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool trail_sdk_is_initialized(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_get_game_params(
            IntPtr sdk,
            out IntPtr game_params,
            out Int32 game_params_count);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_report_game_loaded(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_set_on_game_active_status_changed(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_is_game_active(
            IntPtr sdk,
            out bool is_game_active);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_exit_game(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_crash_game(
            IntPtr sdk,
            IntPtr error_message,
            Int32 error_message_length);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_sdk_set_on_log(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern LogLevel trail_sdk_get_min_log_level(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_sdk_set_min_log_level(
            IntPtr sdk,
            LogLevel level);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool trail_sdk_is_log_standard_output_enabled(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_sdk_set_log_standard_output_enabled(
            IntPtr sdk,
            bool enabled
        );
    }
}
