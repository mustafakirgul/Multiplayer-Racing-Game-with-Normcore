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
using System.Linq;
#if AOT
using AOT;
#endif


namespace Trail
{
    public static partial class PartyKit
    {
        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_show_invite_link(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_update_invite_landing_page_info(IntPtr sdk, [In, MarshalAs(UnmanagedType.LPStruct)] LandingPageInfoC info);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_update_invite_loading_message(IntPtr sdk, IntPtr message);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_finalize_invite_loading(IntPtr sdk, bool success);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_leave_party(IntPtr sdk);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_update_party_data(IntPtr sdk, [In, MarshalAs(UnmanagedType.LPStruct)] PartyDataC data);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_set_on_party_data_updated(IntPtr sdk, IntPtr callback, IntPtr callback_data);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_get_party_data(IntPtr sdk, [Out] out IntPtr ptr);

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result trail_ptk_is_invite_loading(IntPtr sdk, out bool loading);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PartyDataUpdateCB(IntPtr callback_data);

        [StructLayout(LayoutKind.Sequential)]
        public struct LandingPageInfoField
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxLandingPageInfoIdLength + 1)]
            private byte[] id;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxLandingPageInfoLabelLength + 1)]
            private byte[] label;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxLandingPageInfoValueLength + 1)]
            private byte[] value;

            public string ID
            {
                get
                {
                    return Common.GetUTF8StringSafely(id);
                }
                set
                {
                    this.id = new byte[PartyKit.MaxLandingPageInfoIdLength + 1];
                    Common.GetUTF8BytesSafely(value, id);
                }
            }

            public string Label
            {
                get
                {
                    return Common.GetUTF8StringSafely(label);
                }
                set
                {
                    this.label = new byte[PartyKit.MaxLandingPageInfoLabelLength + 1];
                    Common.GetUTF8BytesSafely(value, label);
                }
            }

            public string Value
            {
                get
                {
                    return Common.GetUTF8StringSafely(value);
                }
                set
                {
                    this.value = new byte[PartyKit.MaxLandingPageInfoValueLength + 1];
                    Common.GetUTF8BytesSafely(value, this.value);
                }
            }

            public LandingPageInfoField(string id, string label, string value)
            {
                this.id = new byte[PartyKit.MaxLandingPageInfoIdLength + 1];
                Common.GetUTF8BytesSafely(id, this.id);

                this.label = new byte[PartyKit.MaxLandingPageInfoLabelLength + 1];
                Common.GetUTF8BytesSafely(label, this.label);

                this.value = new byte[PartyKit.MaxLandingPageInfoValueLength + 1];
                Common.GetUTF8BytesSafely(value, this.value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LandingPageInfoC
        {
            public int count;
            public IntPtr fields;

            public LandingPageInfoC(LandingPageInfo info)
            {
                count = info.Count;
                fields = Common.NewStructArray<LandingPageInfoField, LandingPageInfoField>(info.fields, (i, x) => x);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PartyDataC
        {
            public int count;
            public IntPtr fields;

            public PartyData ToData
            {
                get
                {
                    return new PartyData(Common.PtrToStructArray<PartyDataField, PartyDataField>(fields, count, (i, x) => x));
                }
            }

            public PartyDataC(PartyData data)
            {
                count = data.Count;
                fields = Common.NewStructArray<PartyDataField, PartyDataField>(data.fields, (i, x) => x);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct LandingPageInfo
        {
            private int count;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxLandingPageFieldsLength)]
            internal LandingPageInfoField[] fields;

            public int Count { get { return count; } }
            public int MaxLength { get { return PartyKit.MaxLandingPageFieldsLength; } }

            internal LandingPageInfoC CBinding
            {
                get
                {
                    return new LandingPageInfoC(this);
                }
            }

            public LandingPageInfoField this[int index]
            {
                get
                {
                    return fields[index];
                }
                set
                {
                    if (fields == null)
                    {
                        fields = new LandingPageInfoField[PartyKit.MaxLandingPageFieldsLength];
                    }
                    fields[index] = value;
                    count = Math.Max(count, index);
                }
            }

            public LandingPageInfo(IList<LandingPageInfoField> fields)
            {
                count = Math.Min(fields.Count, PartyKit.MaxLandingPageFieldsLength);
                this.fields = new LandingPageInfoField[PartyKit.MaxLandingPageFieldsLength];
                for (int i = 0; i < count; i++)
                {
                    this.fields[i] = fields[i];
                }
            }

            public void SetPageInfo(int index, LandingPageInfoField field)
            {
                fields[index] = field;
                count = Math.Max(count, index);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PartyDataField
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxPartyDataKeyLength + 1)]
            private byte[] key;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PartyKit.MaxPartyDataValueLength + 1)]
            private byte[] value;

            public string Key
            {
                get
                {
                    return Common.GetUTF8StringSafely(key);
                }
                set
                {
                    key = new byte[PartyKit.MaxPartyDataKeyLength + 1];
                    Common.GetUTF8BytesSafely(value, key);
                }
            }

            public string Value
            {
                get
                {
                    return Common.GetUTF8StringSafely(value);
                }
                set
                {
                    this.value = new byte[PartyKit.MaxPartyDataValueLength + 1];
                    Common.GetUTF8BytesSafely(value, this.value);
                }
            }

            public PartyDataField(string key, string value)
            {
                this.key = new byte[PartyKit.MaxPartyDataKeyLength + 1];
                Common.GetUTF8BytesSafely(key, this.key);

                this.value = new byte[PartyKit.MaxPartyDataValueLength + 1];
                Common.GetUTF8BytesSafely(value, this.value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PartyData
        {
            private int count;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal PartyDataField[] fields;

            public int Count { get { return count; } }
            public int MaxSize { get { return 8; } }

            public PartyDataField this[int index]
            {
                get
                {
                    return fields[index];
                }
                set
                {
                    if (fields == null)
                    {
                        fields = new PartyDataField[8];
                    }
                    fields[index] = value;
                    this.count = Math.Max(this.count, index);
                }
            }

            internal PartyDataC CBinding
            {
                get
                {
                    return new PartyDataC(this);
                }
            }

            public PartyData(IList<PartyDataField> fields)
            {
                this.fields = new PartyDataField[8];
                this.count = Math.Min(fields.Count, this.fields.Length);
                for (int i = 0; i < count; i++)
                {
                    this.fields[i] = fields[i];
                }
            }
        }

#if AOT
        [MonoPInvokeCallback(typeof(PartyDataUpdateCB))]
#endif
        private static void OnPartyDataUpdateCb(IntPtr callback_data)
        {
            if (onPartyDataUpdateSimple != null)
            {
                onPartyDataUpdateSimple.Invoke();
            }
        }

    }
}
