#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WII || UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS4 || UNITY_XBOXONE || UNITY_TIZEN || UNITY_TVOS || UNITY_WSA || UNITY_FACEBOOK)
#define UNITY
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Trail
{
    /// <summary>
    /// Log level used by Trail c/c++ API.
    /// To change minimum level to print, see SDK.LogLevel.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// (internal) Only used in internal debug builds of the SDK. Public releases have no log calls with Debug level.
        /// </summary>
        Debug = 0,
        /// <summary>
        /// Useful information on important SDK events.
        /// </summary>
        Info = 1,
        /// <summary>
        /// Something wrong or unexpected going outside the SDK, either the game is doing weird stuff with the SDK or the communication with the rest of Trail is going wrong.
        /// </summary>
        Warning = 2,
        /// <summary>
        /// "Should-never-happen" stuff, most likely a bug in the SDK
        /// </summary>
        Error = 3
    }

    internal static class Common
    {
        #region Consts

#if (UNITY_WEBGL && !UNITY_EDITOR)
        public const string DllName = "__Internal";
#else
        public const string DllName = "trailsdk";
#endif

        #endregion

        #region Logging

        private static string[] logLevelLookup = { "Debug", "Info", "Warning", "Error" };

        public static void LogError(string format, params object[] values)
        {
#if UNITY
            UnityEngine.Debug.LogErrorFormat(format, values);
#else
                System.Diagnostics.Debug.Print(format, values);
#endif
        }

        public static LogType ConvertToLogType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return LogType.Log;
                case LogLevel.Info: return LogType.Log;
                case LogLevel.Warning: return LogType.Warning;
                case LogLevel.Error: return LogType.Error;
            }
            return LogType.Error;
        }

        public static string ConvertToString(LogLevel level) { return logLevelLookup[(int)level]; }

        #endregion


        #region Strings

        public static unsafe string PtrToStringUTF8(IntPtr ptr, Int32 length)
        {
#if CSHARP_7_3_OR_NEWER
            return Encoding.UTF8.GetString((byte*)ptr.ToPointer(), length);
#else
            if (ptr == IntPtr.Zero)
            {
                return "";
            }
            byte[] buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
#endif
        }

        public static unsafe string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return "";
            }
			int length = 0;

			while (Marshal.ReadByte(ptr, length) != 0) {
				++length;
            }

			if (length == 0) {
				return "";
            }

			byte[] buffer = new byte[length];
			Marshal.Copy(ptr, buffer, 0, buffer.Length);
			return Encoding.UTF8.GetString(buffer);
        }

        public static GCHandle NewUTF8String(string str, out Int32 size)
        {
            byte[] buffer;
            if (str == null)
            {
                size = 0;
                buffer = new byte[0];
            }
            else
            {
                size = Encoding.UTF8.GetByteCount(str);
                buffer = new byte[size];
                Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            }
            return GCHandle.Alloc(buffer, GCHandleType.Pinned);
        }

        public static void GetUTF8BytesSafely(string str, byte[] buffer)
        {
            if (str == null)
            {
                return;
            }

            Encoding.UTF8.GetBytes(str, 0, Math.Min(str.Length, buffer.Length - 1), buffer, 0);
        }

        public static string GetUTF8StringSafely(byte[] buffer)
        {
            if (buffer == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(buffer, 0,
					Math.Min(Array.IndexOf<byte>(buffer, 0), buffer.Length));
        }

        #endregion


        #region Arrays

        public static IntPtr NewStructArray<T, U>(T[] structs, Func<int, T, U> toUnmanaged)
        {
            var size = Marshal.SizeOf(typeof(U));
            IntPtr array = Marshal.AllocHGlobal(structs.Length * size);
            for (int i = 0; i < structs.Length; i++)
            {
                U item = toUnmanaged(i, structs[i]);
                if (item == null)
                {
                    Marshal.FreeHGlobal(array);
                    return IntPtr.Zero;
                }

                IntPtr offset = new IntPtr(array.ToInt64() + i * size);
                Marshal.StructureToPtr(item, offset, false);
            }

            return array;
        }

        public static IntPtr NewStructArray<K, V, U>(
            Dictionary<K, V> dictionary,
            Func<int, K, V, U> toUnmanaged)
        {
            var size = Marshal.SizeOf(typeof(U));
            IntPtr array = Marshal.AllocHGlobal(dictionary.Count * size);
            int i = 0;
            foreach (var pair in dictionary)
            {
                U item = toUnmanaged(i, pair.Key, pair.Value);
                if (item == null)
                {
                    Marshal.FreeHGlobal(array);
                    return IntPtr.Zero;
                }

                IntPtr offset = new IntPtr(array.ToInt64() + i * size);
                Marshal.StructureToPtr(item, offset, false);
                i++;
            }

            return array;
        }

        public static T[] PtrToStructArray<T, U>(
            IntPtr ptr,
            Int32 count,
            Func<int, U, T> toManaged)
            where U : new()
        {
            T[] structs = new T[count];
            var size = Marshal.SizeOf(typeof(U));
            for (int i = 0; i < count; i++)
            {
                if (typeof(U).IsValueType)
                {
                    var unmanaged = Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + i * size),
                            typeof(U));
                    structs[i] = toManaged(i, (U)unmanaged);
                }
                else
                {
                    var unmanaged = new U();
                    Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + i * size), unmanaged);
                    structs[i] = toManaged(i, unmanaged);
                }
            }

            return structs;
        }

        #endregion
    }
}
