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
    /// <summary>
    /// NotificationsKit allows the user to subscribe to desktop notifications.
    /// </summary>
    public static partial class NotificationsKit
    {
        #region Delegate

        /// <summary>
        /// Callback used to get the status for notifications.
        /// </summary>
        /// <param name="result">Result of the request.</param>
        /// <param name="allowed">Permission status. Set to true if the game has permission to send notifications.</param>
        public delegate void PermissionStatusCallback(Result result, bool allowed);

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to request permission to send notifications to the user. 
        /// Any tags set can later be used for targetting when sending notificatons.
        /// </summary>
        /// <param name="tags">Tags used for targetting.</param>
        /// <param name="callback">Callback returning the permission status.</param>
        public static void RequestPermission(
            KeyValueList tags,
            PermissionStatusCallback callback)
        {
            var wrapper = new PermissionCBWrapper(callback);
            IntPtr tagsPtr = IntPtr.Zero;
            try
            {
                tagsPtr = tags.BinaryPtr;
                if (tagsPtr == IntPtr.Zero)
                {
                    callback(Result.InvalidArguments, false);
                    return;
                }

                trail_ntk_request_permission(
                    SDK.Raw,
                    tagsPtr,
                    tags.Count,
                    Marshal.GetFunctionPointerForDelegate(
                        new PermissionCB(NotificationsKit.onPermissionCB)
                    ),
                    GCHandle.ToIntPtr(GCHandle.Alloc(wrapper))
                );
            }
            finally { Marshal.FreeHGlobal(tagsPtr); }
        }

        /// <summary>
        /// Used to check if the game has permission to send notifications to the user.
        /// </summary>
        /// <param name="callback">Callback returning the permission status.</param>
        public static void GetPermissionStatus(PermissionStatusCallback callback)
        {
            var wrapper = new PermissionCBWrapper(callback);
            trail_ntk_get_permission_status(
                SDK.Raw,
                Marshal.GetFunctionPointerForDelegate(
                    new PermissionCB(NotificationsKit.onPermissionCB)
                ),
                GCHandle.ToIntPtr(GCHandle.Alloc(wrapper))
            );
        }

        #endregion
    }
}
