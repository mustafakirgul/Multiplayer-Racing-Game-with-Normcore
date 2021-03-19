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
    /// <summary>
    /// PaymentsKit can be used to integrate in-app purchases in your game.
    /// </summary>
    public static partial class PaymentsKit
    {
        #region Delegates

        /// <summary>
        /// Callback when receiving result from payment.
        /// </summary>
        /// <param name="result">Result whether request payment succeeded or not.</param>
        /// <param name="orderId">order id for the purchase</param>
        /// <param name="entitlementId">entitlement id for the item in the purchase</param>
        public delegate void RequestPaymentCallback(Result result, UUID orderId, UUID entitlementId);

        /// <summary>
        /// Callback when receiving result from get product price.
        /// </summary>
        /// <param name="result">Result whether get product price succeeded or not.</param>
        /// <param name="price">Price of the product.</param>
        public delegate void GetProductPriceCallback(Result result, Price price);

        /// <summary>
        /// Callback when receiving result from get entitlements.
        /// </summary>
        /// <param name="result">Result whether get entitlements succeeded or not.</param>
        /// <param name="entitlements">The entitlements of the current player.</param>
        public delegate void GetEntitlementsCallback(Result result, Entitlement[] entitlements);

        #endregion

        #region Public Structs

        public struct Entitlement {
            public UUID EntitlementID;
            public UUID ProductID;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to start a purchase. The product ID can be found in the Dev Area after you have created the product.
        /// </summary>
        /// <param name="productID">Product id for the product you want to request a purchase for.</param>
        /// <param name="callback">Callback returning the purchase.</param>
        public static void RequestPayment(
            UUID productID,
            RequestPaymentCallback callback)
        {
            var wrapper = new RequestPaymentCBWrapper();
            wrapper.action = callback;
            GCHandle callbackData = GCHandle.Alloc(wrapper);

            trail_pmk_request_payment(
               SDK.Raw,
                productID,
                Marshal.GetFunctionPointerForDelegate(
                    new RequestPaymentCB(PaymentsKit.onRequestPaymentCB)
                ),
                GCHandle.ToIntPtr(callbackData)
            );
        }

        /// <summary>
        /// Used to get the price for a product. The product ID can be found in the Dev Area after you have created the product.
        /// </summary>
        /// <param name="productID">Product id for the product you want to get the price of.</param>
        /// <param name="callback">Callback returning the price.</param>
        public static void GetProductPrice(UUID productID, GetProductPriceCallback callback)
        {
            var wrapper = new GetProductPriceCBWrapper();
            wrapper.action = callback;
            GCHandle callbackData = GCHandle.Alloc(wrapper);
            trail_pmk_get_product_price(
                SDK.Raw,
                productID,
                Marshal.GetFunctionPointerForDelegate(
                    new GetProductPriceCB(PaymentsKit.onGetProductPriceCB)
                ),
                GCHandle.ToIntPtr(callbackData)
            );
        }

        /// <summary>
        /// Retrieves the entitlements of the player. This can be used to check if the player has
        /// purchased a particular entitlement in single-player games.
        /// </summary>
        /// <param name="callback">Callback returning the entitlements.</param>
        public static void GetEntitlements(GetEntitlementsCallback callback)
        {
            var wrapper = new GetEntitlementsCBWrapper();
            wrapper.action = callback;
            GCHandle callbackData = GCHandle.Alloc(wrapper);
            trail_pmk_get_entitlements(
                SDK.Raw,
                Marshal.GetFunctionPointerForDelegate(
                    new GetEntitlementsCB(PaymentsKit.onGetEntitlementsCB)
                ),
                GCHandle.ToIntPtr(callbackData)
            );
        }

        #endregion
    }
}
