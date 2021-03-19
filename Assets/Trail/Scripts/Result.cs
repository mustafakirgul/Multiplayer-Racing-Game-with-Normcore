using System;

namespace Trail
{
    /// <summary>
    /// Trail API result to show errors instead of printing exceptions.
    /// Using Exception support in browser add a lot of overhead.
    /// </summary>
    public enum Result : int
    {
        Ok = 0,

        InternalError = -1,
        InvalidArguments = -2,
        Canceled = -3,
        Generic = -4,

        SDKAlreadyCreated = -100,
        SDKNotInitialized = -101,
        SDKAlreadyInitialized = -102,
        SDKNotConnected = -103,
        SDKHostDisconnected = -104,
        SDKHostConnectionError = -105,
        SDKReportGameLoadedCalledMultipleTimes = -106,

        FLKFileNotFound = -200,
        FLKFileTooBig = -201,
        FLKBufferTooSmall = -202,
        FLKCloudStorageConflict = -203,

        INKFirstGameplayEventAlreadySent = -301,
        INKCustomEventJsonTooBig = -302,
        INKCustomEventJsonInvalid = -303,

        PMKPaymentDeclined = -400,
        PMKProductNotFound = -401,

        PTKNoInviteLoading = -500
    }

    public static class TrailResultExtensions
    {
        /// <summary>
        /// Returns true if res is less than 0
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool IsError(this Result res)
        {
            return res < 0;
        }

        /// <summary>
        /// Returns true if result is greater or equal to 0
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool IsOk(this Result res)
        {
            return res >= 0;
        }
    }
}
