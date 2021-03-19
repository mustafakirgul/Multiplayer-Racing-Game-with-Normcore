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
    /// <summary>
    /// FileKit used to download content for the game as you play or sit in a loading screen.
    /// </summary>
    public static partial class FileKit
    {
        #region Delegates


        /// <summary>
        /// Callback used in <see cref="FileKit.ReadFile(string, FileReadComplete)" /> and <see cref="FileKit.ReadFileNonAlloc(string, byte[], FileReadComplete)" />.
        /// </summary>
        /// <param name="result">The result of the read operation.</param>
        /// <param name="data">The data buffer.</param>
        /// <param name="length">The length of <paramref name="data" />.</param>
        public delegate void FileReadComplete(Result result, byte[] data, int length);

        /// <summary>
        /// Callback used in <see cref="FileKit.SyncCloudStorage" />.
        /// </summary>
        /// <param name="result">The result of the synchronization.</param>
        public delegate void SyncCloudStorageCallback(Result result);

        #endregion

        #region Caching

#if BROWSER
        private static string cloudStoragePath = null;
        private static Result CacheCloudStoragePath()
        {
            if (!string.IsNullOrEmpty(cloudStoragePath))
            {
                return Result.Ok;
            }
            IntPtr ptr;
            var result = trail_flk_get_cloud_storage_path(SDK.Raw, out ptr);
            cloudStoragePath = result == Result.Ok ? Common.PtrToStringUTF8(ptr) : null;
            return result;
        }
#endif

        #endregion

        #region GetFileSize

        /// <summary>
        /// Gets the size of the file at the specified file path.
        /// </summary>
        /// <param name="path">path to the file, e.g. StreamingAssets/myfile.json</param>
        /// <returns>Returns the size of the file at the file path or 0 if any errors.</returns>
        public static int GetFileSize(string path)
        {
            int size;
            var result = GetFileSize(path, out size);
            if (result != Result.Ok)
            {
                Common.LogError("Failed to get file size: {0}", result);
                return 0;
            }
            return size;
        }

        /// <summary>
        /// Gets the size of the file at the specified file path.
        /// </summary>
        /// <param name="path">path to the file, e.g. StreamingAssets/myfile.json</param>
        /// <param name="size">output parameter giving the size of the file at the file path</param>
        /// <returns>Returns result whether succeedes or not.</returns>
        public static Result GetFileSize(string path, out int size)
        {
#if BROWSER
            // c/c++ api uses unsigned integer however c# array is limited to int.max. Converting to int for convinence on c# side.
            uint cSize = 0;
            GCHandle filepathHandle = new GCHandle();
            try
            {
                Int32 filepathLength;
                filepathHandle = Common.NewUTF8String(path, out filepathLength);
                var result = trail_flk_get_file_size(
                    SDK.Raw,
                    filepathHandle.AddrOfPinnedObject(),
                    filepathLength,
                    out cSize
                );

                if(cSize > int.MaxValue)
                {
                    size = 0;
                    return Result.FLKFileTooBig;
                }
                size = (int)cSize;
                return result;
            }
            finally { filepathHandle.Free(); }
#else
            size = 0;
            return Result.InternalError;
#endif
        }

        #endregion

        #region Read File

        /// <summary>
        /// Simple wrapper class to handle FileKit.ReadFile when using coroutines or async code.
        /// </summary>
        public class FileReadOperation : IEnumerator
        {
            /// <summary>
            /// The data buffer.
            /// </summary>
            public byte[] Data { get; internal set; }
            /// <summary>
            /// The length of <see cref="Data" />.
            /// </summary>
            public int Length { get; internal set; }
            /// <summary>
            /// The result of the read operation.
            /// </summary>
            public Result Result { get; internal set; }
            /// <summary>
            /// Is true if the read operation is done.
            /// </summary>
            public bool IsDone { get { return Length != -1; } }
            /// <summary>
            /// Is true if the read operation was canceled.
            /// </summary>
            public bool IsCanceled { get { return Result == Result.Canceled; } }
            /// <summary>
            /// Is true if the read operation had an error.
            /// </summary>
            public bool IsError { get { return Result.IsError(); } }

            object IEnumerator.Current { get { return null; } }

            bool IEnumerator.MoveNext()
            {
                return IsDone;
            }

            void IEnumerator.Reset() { }

            /// <summary>
            /// Cancel the read operation if it is not done.
            /// </summary>
            /// <remarks>
            /// This will set <see cref="Result" /> to <see cref="Result.Canceled" />.
            /// </remarks>
            public void Cancel()
            {
                if (!IsDone)
                {
                    Data = new byte[0];
                    Length = 0;
                    Result = Result.Canceled;
                }
            }

            internal FileReadOperation()
            {
                this.Length = -1;
            }

            internal void Done(Result result, byte[] data, int length)
            {
                this.Data = data;
                this.Length = length;
                this.Result = result;
            }
        }

        /// <summary>
        /// Reads the file at the specified file path.
        /// </summary>
        /// <param name="path">The specified path to read from.</param>
        /// <returns>Returns an file read operation for async code or coroutines.</returns>
        public static FileReadOperation ReadFile(string path)
        {
            var operation = new FileReadOperation();
            ReadFile(path, operation.Done);
            return operation;
        }

        /// <summary>
        /// Reads the file at the specified file path.
        /// </summary>
        /// <param name="path">The specified path to read from.</param>
        /// <param name="callback">Callback for Filekit to call when file is read.</param>
        public static void ReadFile(string path, FileReadComplete callback)
        {
#if BROWSER
            GCHandle filepathHandle = new GCHandle();
            try
            {
                Int32 filepathLength;
                filepathHandle = Common.NewUTF8String(path, out filepathLength);

                UInt32 size;
                var fileSizeResult = trail_flk_get_file_size(
                   SDK.Raw,
                    filepathHandle.AddrOfPinnedObject(),
                    filepathLength,
                    out size
                );

                if(size > int.MaxValue)
                {
                    callback.Invoke(Result.FLKFileTooBig, null, 0);
                    return;
                }

                if (fileSizeResult.IsError())
                {
                    callback.Invoke(fileSizeResult, null, 0);
                }

                var data = new byte[(int)size];

                var wrapper = new FileReadWrapper(data, (int)size, callback);
                trail_flk_read_file(
                        SDK.Raw,
                        filepathHandle.AddrOfPinnedObject(),
                        filepathLength,
                        wrapper.arrayHandle.AddrOfPinnedObject(),
                        size,
                        Marshal.GetFunctionPointerForDelegate(new ReadFileCB(FileKit.OnReadFileCB)),
                        GCHandle.ToIntPtr(GCHandle.Alloc(wrapper))
                    );
            }
            finally { filepathHandle.Free(); }
#else
            callback.Invoke(Result.InternalError, null, 0);
#endif
        }

        /// <summary>
        /// Reads the file at the specified file path without allocating a new byte array.
        /// </summary>
        /// <param name="path">The specified path to read from.</param>
        /// <param name="data">The byte buffer to load the file content into.</param>
        /// <returns>Returns an file read operation for async code or coroutines.</returns>
        public static FileReadOperation ReadFileNonAlloc(string path, byte[] data)
        {
            var operation = new FileReadOperation();
            ReadFileNonAlloc(path, data, operation.Done);
            return operation;
        }

        /// <summary>
        /// Reads the file at the specified file path without allocating a new byte array.
        /// </summary>
        /// <param name="path">The specified path to read from.</param>
        /// <param name="data">The byte buffer to load the file content into.</param>
        /// <param name="callback">Callback for Filekit to call when file is read.</param>
        public static void ReadFileNonAlloc(string path, byte[] data, FileReadComplete callback)
        {
#if BROWSER
            GCHandle filepathHandle = new GCHandle();
            try
            {
                Int32 filepathLength;
                filepathHandle = Common.NewUTF8String(path, out filepathLength);

                UInt32 size;
                var fileSizeResult = trail_flk_get_file_size(
                   SDK.Raw,
                    filepathHandle.AddrOfPinnedObject(),
                    filepathLength,
                    out size
                );

                if (size > int.MaxValue)
                {
                    callback.Invoke(Result.FLKFileTooBig, data, 0);
                    return;
                }

                if (fileSizeResult.IsError())
                {
                    callback.Invoke(fileSizeResult, data, 0);
                    return;
                }

                var wrapper = new FileReadWrapper(data, (int)size, callback);
                trail_flk_read_file(
                        SDK.Raw,
                        filepathHandle.AddrOfPinnedObject(),
                        filepathLength,
                        wrapper.arrayHandle.AddrOfPinnedObject(),
                        size,
                        Marshal.GetFunctionPointerForDelegate(new ReadFileCB(FileKit.OnReadFileCB)),
                        GCHandle.ToIntPtr(GCHandle.Alloc(wrapper))
                    );
            }
            finally { filepathHandle.Free(); }
#else
            callback.Invoke(Result.InternalError, data, 0);
#endif
        }

        #endregion

        #region Preload File

        /// <summary>
        /// Preloads a file, making it available for reading later.
        /// </summary>
        /// <param name="filepath">The specified file path to preload</param>
        /// <returns>Returns whether able to preload the file or not.</returns>
        public static Result PreloadFile(string filepath)
        {
#if BROWSER
            GCHandle filepathHandle = new GCHandle();
            try
            {
                Int32 filepathLength;
                filepathHandle = Common.NewUTF8String(filepath, out filepathLength);
                return trail_flk_preload_file(
                    SDK.Raw,
                    filepathHandle.AddrOfPinnedObject(),
                    filepathLength
                );
            }
            finally { filepathHandle.Free(); }
#else
            return Result.InternalError;
#endif
        }

        #endregion

        #region Cloud Storage

        /// <summary>
        /// Returns the directory where to put files to be synchronized.
        /// </summary>
        /// <returns>Returns the path to the directory with the file name in the path. Example /trail/cloud_storage/example.txt.</returns>
        public static string GetCloudStoragePathFormatted(string fileName) {
            return string.Format("{0}/{1}", GetCloudStoragePath(), fileName);
        }

        /// <summary>
        /// Returns the directory where to put files to be synchronized.
        /// </summary>
        /// <returns>Returns the path to the directory (usually /trail/cloud_storage).</returns>
        public static string GetCloudStoragePath() 
        {
            var path = "";
            var res = GetCloudStoragePath(out path);
            if(res.IsError()) {
                SDK.Log(LogLevel.Error, "GetCloudStoragePath returned with error: " + res);
                path = "cloud_storage";
            }
            return path;
        }

        /// <summary>
        /// Returns the directory where to put files to be synchronized.
        /// </summary>
        /// <param name="path">Returns the path to the directory (usually /trail/cloud_storage).</param>
        /// <returns>If the operation was successful or not.</returns>
        public static Result GetCloudStoragePath(out string path)
        {
#if BROWSER
            var result = CacheCloudStoragePath();
            path = cloudStoragePath;
            return result;
#else
            path = "";
            return Result.InternalError;
#endif
        }

        /// <summary>
        /// Synchronize all files in the cloud storage directory.
        /// </summary>
        /// <param name="callback">Callback returning result of the synchronization.</param>
        public static void SyncCloudStorage(SyncCloudStorageCallback callback)
        {
#if BROWSER
            var wrapper = new SyncCloudStorageCBWrapper();
            wrapper.action = callback;
            GCHandle callbackData = GCHandle.Alloc(wrapper);
            trail_flk_sync_cloud_storage(
                SDK.Raw,
                Marshal.GetFunctionPointerForDelegate(
                    new SyncCloudStorageCB(FileKit.onSyncCloudStorageCB)
                ),
                GCHandle.ToIntPtr(callbackData)
            );
#else
            callback(Result.InternalError);
#endif
        }

        #endregion
    }
}
