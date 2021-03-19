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
using System.Text;
#if AOT
using AOT;
#endif

namespace Trail
{
    public static partial class PaymentsKit
    {
        private class GetProductPriceCBWrapper
        {
            public GetProductPriceCallback action;
        }

        private class RequestPaymentCBWrapper
        {
            public RequestPaymentCallback action;
        }

        private class GetEntitlementsCBWrapper
        {
            public GetEntitlementsCallback action;
        }

#if AOT
        [MonoPInvokeCallback(typeof(RequestPaymentCB))]
#endif
        private static void onRequestPaymentCB(
            Result result,
            [MarshalAs(UnmanagedType.LPStruct)] UUID order_id,
            [MarshalAs(UnmanagedType.LPStruct)] UUID entitlement_id,
            IntPtr callback_data)
        {
            GCHandle handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (RequestPaymentCBWrapper)handle.Target;
                wrapper.action(result, order_id, entitlement_id);
            }
            finally { handle.Free(); }
        }

#if AOT
        [MonoPInvokeCallback(typeof(GetProductPriceCB))]
#endif
        private static void onGetProductPriceCB(
            Result result,
            [MarshalAs(UnmanagedType.LPStruct)] PriceRaw price,
            IntPtr callback_data)
        {
            GCHandle handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (GetProductPriceCBWrapper)handle.Target;
                if (result == Result.Ok)
                {
                    wrapper.action(result, price.Marshal());
                }
                else
                {
                    wrapper.action(result, null);
                }
            }
            finally { handle.Free(); }
        }

#if AOT
        [MonoPInvokeCallback(typeof(GetEntitlementsCB))]
#endif
        private static void onGetEntitlementsCB(
            Result result,
            IntPtr entitlements,
            Int32 num_entitlements,
            IntPtr callback_data)
        {
            GCHandle handle = GCHandle.FromIntPtr(callback_data);
            try
            {
                var wrapper = (GetEntitlementsCBWrapper)handle.Target;
                if (result.IsOk())
                {
                    var ents = Common.PtrToStructArray<Entitlement, Entitlement>
                        (entitlements, num_entitlements, (i, x) => x);
                    wrapper.action(result, ents);
                }
                else
                {
                    wrapper.action(result, null);
                }
            }
            finally { handle.Free(); }
        }

        [StructLayout(LayoutKind.Sequential)]
        private class PriceRaw
        {
            public Int32 amount_dividend;
            public Int32 amount_divisor;
            public const Int32 currency_iso_4217_length = 3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = currency_iso_4217_length)]
            public byte[] currency_iso_4217;

            public Price Marshal()
            {
                string currency = Encoding.UTF8.GetString(
                    this.currency_iso_4217,
                    0,
                    currency_iso_4217_length
                );
                return new Price(amount_dividend, amount_divisor, currency);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void RequestPaymentCB(
            Result result,
            [MarshalAs(UnmanagedType.LPStruct)] UUID order_id,
            [MarshalAs(UnmanagedType.LPStruct)] UUID entitlement_id,
            IntPtr callback_data
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetProductPriceCB(
            Result result,
            [MarshalAs(UnmanagedType.LPStruct)] PriceRaw price,
            IntPtr callback_data
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetEntitlementsCB(
            Result result,
            IntPtr entitlements,
            Int32 num_entitlements,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_pmk_request_payment(
            IntPtr sdk,
            [MarshalAs(UnmanagedType.LPStruct)] UUID product_id,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_pmk_get_product_price(
            IntPtr sdk,
            [MarshalAs(UnmanagedType.LPStruct)] UUID product_id,
            IntPtr callback,
            IntPtr callback_data
        );

        [DllImport(Common.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void trail_pmk_get_entitlements(
            IntPtr sdk,
            IntPtr callback,
            IntPtr callback_data
        );
    }
}
