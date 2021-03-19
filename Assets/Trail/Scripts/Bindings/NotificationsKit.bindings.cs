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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
#if AOT
using AOT;
#endif


namespace Trail
{
    public static partial class NotificationsKit
    {
        private class PermissionCBWrapper
        {
            public PermissionStatusCallback action;

            public PermissionCBWrapper(PermissionStatusCallback callback)
            {
                this.action = callback;
            }
        }

#if AOT
        [MonoPInvokeCallback(typeof(PermissionCB))]
#endif
        private static void onPermissionCB(Result error, bool granted, IntPtr callback_data)
        {
            var handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (PermissionCBWrapper)handle.Target;
                wrapper.action(error, granted);
            }
            finally { handle.Free(); }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PermissionCB(
            Result result,
            bool granted,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_ntk_request_permission(
            IntPtr sdk,
            IntPtr tags,
            Int32 tags_count,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_ntk_get_permission_status(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );

    }
}
