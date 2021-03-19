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
using UnityEngine;
#if AOT
using AOT;
#endif


namespace Trail
{
    /// <summary>
    /// PerformanceKit is used to integrate resolution changes when playing in browser.
    /// </summary>
    public static partial class PerformanceKit
    {
        #region Init

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            SDK.OnPreInitialized += SetupInstance;
        }

        private static void SetupInstance(Result result)
        {
            if (result.IsError())
            {
                return;
            }
#if UNITY
            var resolution = PerformanceKit.GetDisplayResolution();
#if BROWSER
            switch (TrailConfig.InitialAspectRatio) {
                case TrailConfig.AspectRatio.Aspect5by4:
                    trail_pfk_set_aspect_ratio(SDK.Raw, 5.0f / 4.0f);
                    break;
                case TrailConfig.AspectRatio.Aspect4by3:
                    trail_pfk_set_aspect_ratio(SDK.Raw, 4.0f / 3.0f);
                    break;
                case TrailConfig.AspectRatio.Aspect16by9:
                    trail_pfk_set_aspect_ratio(SDK.Raw, 16.0f / 9.0f);
                    break;
                case TrailConfig.AspectRatio.Aspect16by10:
                    trail_pfk_set_aspect_ratio(SDK.Raw, 16.0f / 10.0f);
                    break;
                default:
                    break;
            }
#endif
            PerformanceKit.SetResolution(resolution);
#if BROWSER
            trail_pfk_set_on_screen_resolution_changed(
                    SDK.Raw,
                    Marshal.GetFunctionPointerForDelegate(
                        new ScreenResolutionChangedCB(OnScreenResolutionChanged)
                    ),
                    IntPtr.Zero
                );
#endif
#endif
        }

        #endregion

        #region Display Resolution

        /// <summary>
        /// Used to get a callback every time the display resolution changes.
        /// </summary>
        /// <param name="resolution">The new display resolution.</param>
        public delegate void DisplayResolutionChangedCallback(Resolution resolution);
        private static DisplayResolutionChangedCallback onDisplayResolutionChanged;

        /// <summary>
        /// Callback for when the actual native resolution on the device changes, either changing display settings or changing monitor, etc.
        /// </summary>
        public static event DisplayResolutionChangedCallback OnDisplayResolutionChanged
        {
            add { onDisplayResolutionChanged += value; }
            remove { onDisplayResolutionChanged -= value; }
        }

        /// <summary>
        /// Returns the actual native resolution on the device.
        /// </summary>
        /// <param name="resolution">The output resolution.</param>
        /// <returns>Result whether it succeeded or not.</returns>
        public static Result GetDisplayResolution(out Resolution resolution)
        {
#if BROWSER
                resolution = new Resolution();
                return trail_pfk_get_screen_resolution(SDK.Raw, out resolution);
#else
            UnityEngine.Resolution res = Screen.currentResolution;
            resolution = new Resolution(res.width, res.height);
            return Result.Ok;
#endif
        }

        /// <summary>
        /// Returns the actual native resolution on the device.
        /// </summary>
        /// <returns>The display resolution from the user.</returns>
        public static Resolution GetDisplayResolution()
        {
            Resolution resolution;
            var result = GetDisplayResolution(out resolution);
            if (result != Result.Ok)
            {
                Common.LogError("Failed to get the screen resolution: {0}", result.ToString());
            }
            return resolution;
        }
#if BROWSER
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ScreenResolutionChangedCB(
            IntPtr callback_data
        );

#if AOT
        [MonoPInvokeCallback(typeof(ScreenResolutionChangedCB))]
#endif
        private static void OnScreenResolutionChanged(IntPtr callback_data)
        {
            var browserResolution = GetDisplayResolution();
            if(onDisplayResolutionChanged != null){
                onDisplayResolutionChanged.Invoke(browserResolution);
            }
        }
#endif

        #endregion

        #region Recommendations

        /// <summary>
        /// This is not tested or 100% supported. 
        /// Should not be used.
        /// </summary>
        /// <param name="qualityLevel"></param>
        /// <returns></returns>
        public static Result GetRecommendedQualityLevel(out Int32 qualityLevel)
        {
#if BROWSER
                return trail_pfk_get_recommended_quality_level(SDK.Raw, out qualityLevel);
#else
            qualityLevel = 100;
            return Result.Ok;
#endif
        }

        /// <summary>
        /// This is not tested or 100% supported. 
        /// Should not be used.
        /// </summary>
        /// <returns></returns>
        public static Int32 GetRecommendedQualityLevel()
        {
            Int32 qualityLevel;
            var result = GetRecommendedQualityLevel(out qualityLevel);
            if (result != Result.Ok)
            {
                Common.LogError(
                    "Failed to get the recommended quality level: {0}",
                    result.ToString()
                );
            }
            return qualityLevel;
        }

        /// <summary>
        /// Used to get the recommended resolution for the user's device.
        /// </summary>
        /// <param name="resolution">Output paramter for the game resolution</param>
        /// <returns>Returns result whether it succeeded or not.</returns>
        public static Result GetRecommendedResolution(out Resolution resolution)
        {
#if BROWSER
                resolution = new Resolution();
                return trail_pfk_get_recommended_game_resolution(SDK.Raw, out resolution);
#else
            return GetDisplayResolution(out resolution);
#endif
        }

        /// <summary>
        /// Used to get the recommended resolution for the user's device.
        /// </summary>
        /// <returns>Returns the recommended resolution.</returns>
        public static Resolution GetRecommendedResolution()
        {
            Resolution resolution;
            var result = GetRecommendedResolution(out resolution);
            if (result != Result.Ok)
            {
                Common.LogError(
                    "Failed to get the recommended game resolution: {0}",
                    result.ToString()
                );
            }
            return resolution;
        }

        #endregion

        #region Resolution

#if UNITY
        /// <summary>
        /// Gets the resolution at which the game is currently rendering.
        /// </summary>
        /// <param name="resolution">Output paramter for the current resolution game is rendering in.</param>
        /// <returns>Returns whether it succeeded or not.</returns>
        public static Result GetResolution(out Resolution resolution)
        {
#if BROWSER
                    resolution = new Resolution();
                    return trail_pfk_unity_get_game_resolution(SDK.Raw, out resolution);
#else
            UnityEngine.Resolution res = Screen.currentResolution;
            resolution = new Resolution(res.width, res.height);
            return Result.Ok;
#endif
        }

        /// <summary>
        /// Gets the resolution at which the game is currently rendering.
        /// </summary>
        /// <returns>Returns current resolution game is rendering in.</returns>
        public static Resolution GetResolution()
        {
            Resolution resolution;
            var result = GetResolution(out resolution);
            if (result != Result.Ok)
            {
                Common.LogError(
                    "Failed to get the game resolution: {0}",
                    result.ToString()
                );
            }
            return resolution;
        }

        /// <summary>
        /// Sets the resolution at which the game will render.
        /// </summary>
        /// <param name="width">Target resolution width.</param>
        /// <param name="height">Target resolution height.</param>
        /// <returns>Whether it succeeded or not.</returns>
        public static Result SetResolution(int width, int height)
        { 
            return SetResolution(new Resolution(width, height), true, true);
        }

        /// <summary>
        /// Sets the resolution at which the game will render.
        /// </summary>
        /// <param name="width">Target resolution width.</param>
        /// <param name="height">Target resolution height.</param>
        /// <param name="matchCommon">Whether to adjust resolution to the closest common resolution.</param>
        /// <param name="capToScreen">Whether to adjust resolution to not be greater than screen resolution.</param>
        /// <returns>Whether it succeeded or not.</returns>
        public static Result SetResolution(int width, int height, bool matchCommon, bool capToScreen)
        {
            return SetResolution(new Resolution(width, height), matchCommon, capToScreen);
        }

        /// <summary>
        /// Sets the resolution at which the game will render.
        /// </summary>
        /// <param name="resolution">The target resolution to set.</param>
        /// <returns>Whether it succeeded or not.</returns>
        public static Result SetResolution(Resolution resolution)
        {
            return SetResolution(resolution, true, true);
        }

        /// <summary>
        /// Sets the resolution at which the game will render.
        /// </summary>
        /// <param name="resolution">The target resolution to set.</param>
        /// <param name="matchCommon">Whether to adjust resolution to the closest common resolution.</param>
        /// <param name="capToScreen">Whether to adjust resolution to not be greater than screen resolution.</param>
        /// <returns>Whether it succeeded or not.</returns>
        public static Result SetResolution(
            Resolution resolution,
            bool matchCommon,
            bool capToScreen
        )
        {
            var res = Result.Ok;
#if BROWSER
                    res = trail_pfk_unity_set_game_resolution(SDK.Raw, ref resolution,
                            matchCommon, capToScreen);
#else
            if (resolution.Width <= 0 || resolution.Height <= 0)
            {
                return Result.InvalidArguments;
            }

            Screen.SetResolution(resolution.Width, resolution.Height, true);
#endif
            return res;
        }
#endif

        #endregion

        #region Common Resolutions

        /// <summary>
        /// Gets a list of 10 well-known resolutions based on the device display, ranging from 360p to 4K (adjusted for screen ratio). 
        /// Always includes the native screen resolution.
        /// 
        /// Useful for presenting a list of resolutions to the player which they may select from, e.g. in the settings view.
        /// </summary>
        /// <param name="array">A pre allocated array used to store the resolution in.</param>
        /// <returns>The amount of items in the array that was assigned.</returns>
        public static int GetCommonResolutionNonAlloc(Resolution[] array)
        {
            int count;
            var result = GetCommonResolutionNonAlloc(array, out count);
            if (result != Result.Ok)
            {
                Common.LogError(
                  "Failed to get supported game resolutions: {0}",
                  result.ToString()
              );
            }
            return count;
        }

        /// <summary>
        /// Gets a list of 10 well-known resolutions based on the device display, ranging from 360p to 4K (adjusted for screen ratio). 
        /// Always includes the native screen resolution.
        /// 
        /// Useful for presenting a list of resolutions to the player which they may select from, e.g. in the settings view.
        /// </summary>
        /// <param name="array">A pre allocated array used to store the resolution in.</param>
        /// <param name="count">The amount of items assigned to the array.</param>
        /// <returns>Returns whether it succeeded or not.</returns>
        public static unsafe Result GetCommonResolutionNonAlloc(Resolution[] array, out int count)
        {
            if (array == null)
            {
                count = 0;
                return Result.InvalidArguments;
            }
            if (array.Length == 0)
            {
                count = 0;
                return Result.Ok;
            }
#if BROWSER
            var ptr = IntPtr.Zero;
            var result = trail_pfk_get_common_resolutions(
                SDK.Raw,
                out ptr,
                out count
            );

            if(result != Result.Ok)
            {
                count = 0;
                return result;
            }

            var length = Math.Min(count, array.Length);
            var resPtr = (Resolution*)ptr.ToPointer();
            for (int i = 0; i < length; i++)
            {
                array[i] = *resPtr;
            }
            return Result.Ok;
#else
            var builtIn = Screen.resolutions;
            count = Math.Min(builtIn.Length, array.Length);
            for (int i = 0; i < count; i++)
            {
                array[i] = new Resolution(builtIn[i]);
            }
            return Result.Ok;
#endif
        }

        /// <summary>
        /// Gets a list of 10 well-known resolutions based on the device display, ranging from 360p to 4K (adjusted for screen ratio). 
        /// Always includes the native screen resolution.
        /// 
        /// Useful for presenting a list of resolutions to the player which they may select from, e.g. in the settings view.
        /// </summary>
        /// <param name="resolutions">The array where the common resolution values is going to be allocated to.</param>
        /// <returns>Returns whether it succeeded or not.</returns>
        public static Result GetCommonResolutions(out Resolution[] resolutions)
        {
#if BROWSER
                Int32 count;
                var ptr = IntPtr.Zero;
                var result = trail_pfk_get_common_resolutions(
                    SDK.Raw,
                    out ptr,
                    out count
                );

                if (result != Result.Ok)
                {
                    resolutions = null;
                    return result;
                }

                resolutions = Common.PtrToStructArray<Resolution, Resolution>(
                    ptr,
                    count,
                    (i, v) => { return v; }
                );

                return result;
#else
            resolutions = new Resolution[Screen.resolutions.Length];
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                resolutions[i] = new Resolution(Screen.resolutions[i]);
            }
            return Result.Ok;
#endif
        }

        /// <summary>
        /// Gets a list of 10 well-known resolutions based on the device display, ranging from 360p to 4K (adjusted for screen ratio). 
        /// Always includes the native screen resolution.
        /// 
        /// Useful for presenting a list of resolutions to the player which they may select from, e.g. in the settings view.
        /// </summary>
        /// <returns>New array of 10 common resolutions.</returns>
        public static Resolution[] GetCommonResolutions()
        {
            Resolution[] resolutions;
            var result = GetCommonResolutions(out resolutions);
            if (result != Result.Ok)
            {
                Common.LogError(
                    "Failed to get supported game resolutions: {0}",
                    result.ToString()
                );
            }
            return resolutions;
        }

        #endregion
    }
}
