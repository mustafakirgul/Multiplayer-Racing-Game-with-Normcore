#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WII || UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS4 || UNITY_XBOXONE || UNITY_TIZEN || UNITY_TVOS || UNITY_WSA || UNITY_FACEBOOK)
#define UNITY
#endif

#if (UNITY && ENABLE_IL2CPP)
#define AOT
#endif

using System;
using System.Runtime.InteropServices;
#if AOT
using AOT;
#endif

namespace Trail
{
    public static partial class AuthKit
    {
        private class GetFingerprintCBWrapper 
        {
            public GetFingerprintCallback action;
        }

#if AOT
        [MonoPInvokeCallback(typeof(GetFingerprintCB))]
#endif
        private static void onGetFingerprintCB(
            Result result,
            IntPtr fingerprint,
            Int32 fingerprint_len,
            IntPtr callback_data)
        {
            GCHandle handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (GetFingerprintCBWrapper)handle.Target;
                var fp = result.IsOk() ?
                    Common.PtrToStringUTF8(fingerprint, fingerprint_len) : null;
                wrapper.action(result, fp);
            }
            finally { handle.Free(); }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetFingerprintCB(
            Result result,
            IntPtr fingerprint,
            Int32 fingerprint_len,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_get_game_user_id(
            IntPtr sdk,
            [Out, MarshalAs(UnmanagedType.LPStruct)] UUID id
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_get_play_token(
            IntPtr sdk,
            out IntPtr play_token,
            out Int32 play_token_length
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_get_username(
            IntPtr sdk,
            out IntPtr username,
            out Int32 username_length
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_sdk_get_fingerprint(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );
    }
}
