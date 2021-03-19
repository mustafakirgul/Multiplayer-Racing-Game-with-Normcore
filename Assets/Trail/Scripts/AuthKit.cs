using System;
using System.Runtime.InteropServices;

namespace Trail
{
    /// <summary>
    /// AuthKit is the base for authenticationw with Trail, allowing you to get a game user ID that is unique per game however linked to the users Trail account.
    /// This also allows you to get play token, a specific token generated for the session.
    /// </summary>
    public static partial class AuthKit
    {
        /// <summary>
        /// Callback when receiving result from get fingerprint
        /// </summary>
        /// <param name="result">Result whether get fingerprint succeeded or not.</param>
        /// <param name="fingerprint">The fingerprint.</param>
        public delegate void GetFingerprintCallback(Result result, string fingerprint);

        #region Variable Cache

        private static UUID gameUserID = new UUID();
        private static string username = null;

        #endregion

        #region Caching

        private static Result CacheGameUserID()
        {
            if (gameUserID != null && gameUserID.IsValid)
            {
                return Result.Ok;
            }
            var result = trail_sdk_get_game_user_id(SDK.Raw, gameUserID);
            return result;
        }

        private static Result CacheUsername()
        {
            if (!string.IsNullOrEmpty(username))
            {
                return Result.Ok;
            }
            IntPtr ptr;
            int length;
            var res = trail_sdk_get_username(SDK.Raw, out ptr, out length);
            username = res == Result.Ok ? Common.PtrToStringUTF8(ptr, length) : null;
            return res;
        }

        #endregion

        #region Game User ID

        /// <summary>
        /// Returns the user ID generated for the specific user for this specific game.
        /// </summary>
        /// <param name="gameUserID"></param>
        /// <returns>Returns result whether succeeded or not to retrive game user ID.</returns>
        public static Result GetGameUserID(out UUID gameUserID)
        {
            var result = CacheGameUserID();
            gameUserID = AuthKit.gameUserID;
            return result;
        }

        /// <summary>
        /// Returns the user ID generated for the specific user for this specific game.
        /// </summary>
        /// <returns>Returns the UUID, null if failed to get GameUserID</returns>
        public static UUID GetGameUserID()
        {
            UUID uuid;
            var result = GetGameUserID(out uuid);
            if (result != Result.Ok)
            {
                Common.LogError("Failed to get game user ID: {0}", result.ToString());
            }
            return uuid;
        }

        /// <summary>
        /// Returns a fingerprint of the device or browser the player is currently on.
        /// </summary>
        public static void GetFingerprint(GetFingerprintCallback callback)
        {
            var wrapper = new GetFingerprintCBWrapper();
            wrapper.action = callback;
            GCHandle callbackData = GCHandle.Alloc(wrapper);
            trail_sdk_get_fingerprint(
                SDK.Raw,
                Marshal.GetFunctionPointerForDelegate(
                    new GetFingerprintCB(AuthKit.onGetFingerprintCB)
                ),
                GCHandle.ToIntPtr(callbackData)
            );
        }

        #endregion

        #region PlayToken

        /// <summary>
        /// Get the current user's play token. 
        /// The play token is a unique token generated for each session (i.e. every time a user loads a game) 
        ///     which is used by Trail to validate that the user has access to such game.
        /// </summary>
        /// <param name="token">Returns the play token for this session.</param>
        /// <returns>Returns whether succeedes or not to retrieve the play token.</returns>
        public static Result GetPlayToken(out string token)
        {
            IntPtr ptr;
            int length;
            var result = trail_sdk_get_play_token(SDK.Raw, out ptr, out length);
            token = result.IsOk() ? Common.PtrToStringUTF8(ptr, length) : null;
            return result;
        }

        /// <summary>
        /// Get the current user's play token. 
        /// The play token is a unique token generated for each session (i.e. every time a user loads a game) 
        ///     which is used by Trail to validate that the user has access to such game.
        /// </summary>
        /// <returns>Returns the play token from Trail.</returns>
        public static string GetPlayToken()
        {
            string ret;
            var result = GetPlayToken(out ret);
            if (!result.IsOk())
            {
                Common.LogError("Failed to get play token: {0}", result.ToString());
            }
            return ret;
        }

        /// <summary>
        /// Get the current user's Trail username.
        /// </summary>
        /// <param name="token">Returns the user's Trail username.</param>
        /// <returns>Returns whether succeedes or not to retrieve the username.</returns>
        public static Result GetUsername(out string username)
        {
            var result = CacheUsername();
            username = AuthKit.username;
            return result;
        }

        /// <summary>
        /// Get the current user's Trail username.
        /// </summary>
        /// <returns>Returns the username from Trail.</returns>
        public static string GetUsername()
        {
            var result = CacheUsername();
            if (result != Result.Ok)
            {
                Common.LogError("Failed to get username: {0}", result.ToString());
            }
            return username;
        }

        #endregion
    }
}
