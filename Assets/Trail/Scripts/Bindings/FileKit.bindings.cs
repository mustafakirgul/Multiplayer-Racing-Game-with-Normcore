#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WII || UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS4 || UNITY_XBOXONE || UNITY_TIZEN || UNITY_TVOS || UNITY_WSA || UNITY_FACEBOOK)
#define UNITY
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
#define BROWSER
#endif

#if (UNITY && ENABLE_IL2CPP)
#define AOT
#endif


#if AOT
using AOT;
#endif
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Trail
{
    public static partial class FileKit
    {
#if BROWSER
        
        private class FileReadWrapper
        {
            private FileReadComplete callback;
            private int length;
            public GCHandle arrayHandle;

            public FileReadWrapper(byte[] data, int length, FileReadComplete callback)
            {
                this.arrayHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                this.callback = callback;
                this.length = length;
            }

            public void Done(Result result)
            {
                callback.Invoke(result, (byte[])arrayHandle.Target, length);
            }
        }

        private class SyncCloudStorageCBWrapper
        {
            public SyncCloudStorageCallback action;
        }

#if AOT
        [MonoPInvokeCallback(typeof(ReadFileCB))]
#endif
        private static void OnReadFileCB(Result result, IntPtr data)
        {
            var handle = GCHandle.FromIntPtr(data);
            FileReadWrapper wrapper = null;
            try
            {
                wrapper = (FileReadWrapper)handle.Target;
                wrapper.Done(result);
            }
            finally
            {
                handle.Free();
                if (wrapper != null) { wrapper.arrayHandle.Free(); }
            }
        }

#if AOT
        [MonoPInvokeCallback(typeof(SyncCloudStorageCB))]
#endif
        private static void onSyncCloudStorageCB(
            Result result,
            IntPtr callback_data)
        {
            GCHandle handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (SyncCloudStorageCBWrapper)handle.Target;
                wrapper.action(result);
            }
            finally { handle.Free(); }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReadFileCB(Result result, IntPtr callback_data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SyncCloudStorageCB(Result result, IntPtr callback_data);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_flk_get_file_size(
            IntPtr sdk,
            IntPtr filepath,
            Int32 filepath_length,
            out UInt32 file_size
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_flk_read_file(
            IntPtr sdk,
            IntPtr filepath,
            Int32 filepath_length,
            IntPtr buffer,
            UInt32 buffer_size,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_flk_preload_file(
            IntPtr sdk,
            IntPtr filepath,
            Int32 filepath_length
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_flk_get_cloud_storage_path(
            IntPtr sdk,
            out IntPtr ptr
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_flk_sync_cloud_storage(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );
#endif

    }
}
