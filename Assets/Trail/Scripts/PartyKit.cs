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
    /// <summary>
    /// Used to get lobby/party id secrets to get players together.
    /// </summary>
    public static partial class PartyKit
    {
        #region Consts

        public const int MaxInviteMessageLength = 255;

        public const int MaxLandingPageFieldsLength = 8;
        public const int MaxLandingPageInfoIdLength = 63;
        public const int MaxLandingPageInfoLabelLength = 63;
        public const int MaxLandingPageInfoValueLength = 511;

        public const int MaxPartyDataKeyLength = 63;
        public const int MaxPartyDataValueLength = 4095;

        #endregion

        #region Variables

        /// <summary>
        /// Used to get a callback every time the party data gets updated
        /// </summary>
        public delegate void PartyDataUpdateCallback();
        private static PartyDataUpdateCallback onPartyDataUpdateSimple;

        #endregion

        #region Properties

        /// <summary>
        /// Callback for when party data is updated.
        /// </summary>
        public static event PartyDataUpdateCallback OnPartyDataUpdated
        {
            add { onPartyDataUpdateSimple += value; }
            remove { onPartyDataUpdateSimple -= value; }
        }

        #endregion

        #region Constructor/Initialization

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            SDK.OnPreInitialized += (result) =>
            {
                if (result.IsOk())
                {
                    PartyKit.trail_ptk_set_on_party_data_updated(SDK.Raw,
                        Marshal.GetFunctionPointerForDelegate(
                         new PartyDataUpdateCB(PartyKit.OnPartyDataUpdateCb)
                     ), IntPtr.Zero);
                }
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Triggers a modal dialog that shows the user's party invite link.
        /// </summary>
        public static Result ShowInviteLink()
        {
            return trail_ptk_show_invite_link(SDK.Raw);
        }

        /// <summary>
        /// Updates the information displayed on the user's party invite landing page. 
        ///
        /// Only updates from the party leader will cause an immediate change in the information shown on the landing page. However, updates by other users will still be stored and used later to update the landing page if a new user becomes the party leader.
        /// </summary>
        /// <param name="infoFields">Refer to the [PartyKit documentation](https://docs.trail.gg/docs/rich-landing-page) to see how to use this parameter to control different parts of the landing page.</param>
        public static Result UpdateInviteLandingPageInfo(IList<LandingPageInfoField> infoFields)
        {
            return trail_ptk_update_invite_landing_page_info(SDK.Raw, new LandingPageInfo(infoFields).CBinding);
        }

        /// <summary>
        /// Updates the information displayed on the user's party invite landing page. 
        ///
        /// Only updates from the party leader will cause an immediate change in the information shown on the landing page. However, updates by other users will still be stored and used later to update the landing page if a new user becomes the party leader.
        /// </summary>
        /// <param name="info">Refer to the [PartyKit documentation](https://docs.trail.gg/docs/rich-landing-page) to see how to use this parameter to control different parts of the landing page.</param>
        public static Result UpdateInviteLandingPageInfo(LandingPageInfo info)
        {
            return trail_ptk_update_invite_landing_page_info(SDK.Raw, info.CBinding);
        }

        /// <summary>
        /// Updates the message displayed to other users in the Party while the user is loading the invite, i.e. loading the game and joining the lobby, match, server, etc.
        /// </summary>
        /// <param name="message">Keep the message short, around 40 characters max, or otherwise it will show up cropped.</param>
        public static Result UpdateInviteLoadingMessage(string message)
        {
            Int32 size;
            var messageHandle = Common.NewUTF8String(message + "\0", out size);
            var result = trail_ptk_update_invite_loading_message(SDK.Raw,
                    messageHandle.AddrOfPinnedObject());
            messageHandle.Free();
            return result;
        }

        /// <summary>
        /// The game always needs to call this method when the user has either successfully finished loading the invite or fail to do so due to any error.
        ///
        /// Only the first call to this method will have any effect, subsequent calls will be ignored.
        /// </summary>
        /// <param name="success">Pass <c>true</c> if the invite was successfully loaded (user joined the party, etc). If there was any error (party no longer exists, etc.) pass <c>false</c> instead. Passing <c>false</c> is equivalent to calling <c>LeaveParty()</c></param>
        public static Result FinalizeInviteLoading(bool success)
        {
            return trail_ptk_finalize_invite_loading(SDK.Raw, success);
        }

        /// <summary>
        /// It causes the user to leave the party. Calling this before the invite process has been finalized is equivalent to calling <c>FinalizeInviteLoading(false)</c>.
        /// </summary>
        public static Result LeaveParty()
        {
            return trail_ptk_leave_party(SDK.Raw);
        }

        /// <summary>
        /// Broadcasts the party data to all users in the party, including the user sending it.
        ///
        /// You should use this method to pass any information you need to share between clients so that all users can end up in the same match, lobby, etc. For example, you could use this to share a lobby ID and a server region.
        ///
        /// Only updates from the party leader will actually be sent to the rest of the party members. Updates by other users will still be stored, and if a user later becomes the party leader, their latest party data will be broadcasted to everybody.
        /// </summary>
        /// <param name="partyDataFields">The data to be broadcasted to all the members of the party.</param>
        public static Result UpdatePartyData(IList<PartyDataField> partyDataFields)
        {
            return trail_ptk_update_party_data(SDK.Raw, new PartyData(partyDataFields).CBinding);
        }

        /// <summary>
        /// Broadcasts the party data to all users in the party, including the user sending it.
        ///
        /// You should use this method to pass any information you need to share between clients so that all users can end up in the same match, lobby, etc. For example, you could use this to share a lobby ID and a server region.
        ///
        /// Only updates from the party leader will actually be sent to the rest of the party members. Updates by other users will still be stored, and if a user later becomes the party leader, their latest party data will be broadcasted to everybody.
        /// </summary>
        /// <param name="partyData">The data to be broadcasted to all the members of the party.</param>
        public static Result UpdatePartyData(PartyData partyData)
        {
            return trail_ptk_update_party_data(SDK.Raw, partyData.CBinding);
        }

        /// <summary>
        /// Provides the latest party data. This will always be the latest party data update sent by the current party leader.
        /// </summary>
        /// <param name="partyData">The latest version of the party data sent by the party leader.</param>
        public static Result GetPartyData(out PartyData partyData)
        {
            var ptr = IntPtr.Zero;
            var result = trail_ptk_get_party_data(SDK.Raw, out ptr);

            if(result.IsError()) {
                partyData = new PartyData();
                return result;
            }

            if(ptr == IntPtr.Zero) {
                partyData = new PartyData();
                return Result.Ok;
            }

            var c = (PartyDataC)Marshal.PtrToStructure(ptr, typeof(PartyDataC));
            partyData = c.ToData;

            return result;
        }

        /// <summary>
        /// Provides the latest party data. This will always be the latest party data update sent by the current party leader.
        /// </summary>
        /// <returns>The latest version of the party data sent by the party leader.</returns>
        public static PartyData GetPartyData()
        {
            PartyData partyData;
            var result = GetPartyData(out partyData);
            if (result.IsError())
            {
                SDK.Log(LogLevel.Error, "PartyKit::GetPartyData " + result.ToString());
            }
            return partyData;
        }

        /// <summary>
        /// Indicates if the game was launched from an invite and that invite has not yet been finalized by calling <c>FinalizeLoadingInvite()</c> or <c>LeaveParty()</c>.
        /// </summary>
        /// <param name="isLoading"><c>true</c> if the game was launched from an invite and the invite has not yet been finalized, false otherwise</param>
        public static Result IsInviteLoading(out bool isLoading)
        {
            return trail_ptk_is_invite_loading(SDK.Raw, out isLoading);
        }

        /// <summary>
        /// Indicates if the game was launched from an invite and that invite has not yet been finalized by calling <c>FinalizeLoadingInvite()</c> or <c>LeaveParty()</c>.
        /// </summary>
        /// <returns><c>true</c> if the game was launched from an invite and the invite has not yet been finalized, false otherwise</returns>
        public static bool IsInviteLoading()
        {
            bool isLoading = false;
            var result = IsInviteLoading(out isLoading);
            if (result.IsError())
            {
                SDK.Log(LogLevel.Error, "PartyKit::InviteIsLoading " + result.ToString());
            }
            return isLoading;
        }


        #endregion
    }
}
