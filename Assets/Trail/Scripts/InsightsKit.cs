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
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
#if AOT
using AOT;
#endif
#if UNITY
using UnityEngine.SceneManagement;
#endif

namespace Trail
{
    /// <summary>
    /// InsightsKit allows you to get some insight to how the game is running.
    /// </summary>
    public static partial class InsightsKit
    {
        #region Consts

        public const int MaxSceneIDLength = 255;
        public const int MaxSceneNameLength = 255;
        public const int MaxQualityLevelNameLength = 255;
        public const int MaxCustomEventNameLength = 255;

        #endregion

        #region Initialization

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            SDK.OnPreInitialized += RunAnalytics;
        }

        private static void RunAnalytics(Result result)
        {
            if (result.IsError())
            {
                return;
            }
#if UNITY
            ReportSceneChanged(SceneManager.GetActiveScene());
            ReportUnityQuality();
            // Remove activeSceneChange for safety reasons.
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
            if (TrailConfig.ReportSceneChanges)
            {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
            }
            if (TrailConfig.ReportQualityChanges)
            {
                SDK.Mono.gameObject.AddComponent<TrailInsightsMono>();
            }
#endif
        }

        #endregion

        #region Unity Reporting

#if UNITY
        private static void OnSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            InsightsKit.ReportSceneChanged(next);
        }

        internal static void ReportUnityQuality()
        {
            var names = QualitySettings.names;
            var level = QualitySettings.GetQualityLevel();
            if(names == null || names.Length == 0) {
                InsightsKit.ReportQualityLevelChanged(100, "no name");
                return;
            }
            if(names.Length == 1) {
                InsightsKit.ReportQualityLevelChanged(100, names[0]);
                return;
            }
            var normLevel = (float)level /
                Mathf.Max(1f, (float)names.Length - 1.0f);
            var quality = normLevel * 99.0f + 1.0f;
            InsightsKit.ReportQualityLevelChanged(
                Mathf.RoundToInt(quality),
                names[level]
            );
        }
#endif

        #endregion

        #region Report Scene Changes

        /// <summary>
        /// Reports that the scene has changed. 
        /// </summary>
        /// <param name="id">The scene id</param>
        /// <param name="name">The scene name</param>
        /// <returns>Returns whether succeeded or not to report the scene change.</returns>
        public static Result ReportSceneChanged(string id, string name = null)
        {
            var sc = new Scene(id, name);
            return trail_ink_report_scene_changed(SDK.Raw, ref sc);
        }

#if UNITY
        /// <summary>
        /// Reports that the scene has changed. 
        /// </summary>
        /// <param name="scene">The unity scene to report change to.</param>
        /// <returns>Returns whether succeeded or not to report the scene change.</returns>
        public static Result ReportSceneChanged(UnityEngine.SceneManagement.Scene scene)
        {
            return ReportSceneChanged(scene.path, scene.name);
        }
#endif

        #endregion

        #region Resolution Changes

        /// <summary>
        /// Reports that the resolution has changed.
        /// </summary>
        /// <param name="resolution">Resolution value to provide in the report.</param>
        /// <returns>Returns whether it succeeded or not to report.</returns>
        public static Result ReportResolutionChanged(Trail.Resolution resolution)
        {
            return trail_ink_report_game_resolution_changed(SDK.Raw, ref resolution);
        }

#if UNITY
        /// <summary>
        /// Reports that the resolution has changed.
        /// </summary>
        /// <param name="resolution">Resolution value to provide in the report.</param>
        /// <returns>Returns whether it succeeded or not to report.</returns>
        public static Result ReportResolutionChanged(UnityEngine.Resolution resolution)
        {
            var rs = new Resolution(resolution);
            return trail_ink_report_game_resolution_changed(SDK.Raw, ref rs);
        }
#endif

        #endregion

        #region Quality Level Changes

        /// <summary>
        /// Reports that the quality level has changed.
        /// </summary>
        /// <param name="level">New quality level</param>
        /// <param name="name">Name of new quality level. (Optional).</param>
        /// <returns>Returns whether succeeded or not to report.</returns>
        public static Result ReportQualityLevelChanged(Int32 level, string name = null)
        {
            var ql = new QualityLevel(level, name);
            return trail_ink_report_quality_level_changed(SDK.Raw, ref ql);
        }

        #endregion

        #region Custom Event

        /// <summary>
        /// Reports that the user has started playing.
        /// </summary>
        /// <returns></returns>
        public static Result ReportFirstGameplayEvent()
        {
            return trail_ink_report_first_gameplay_event(SDK.Raw);
        }

        /// <summary>
        /// Sends a custom insights event to Trail.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="payloadJSON">JSON payload with any extra event properties. (Optional)</param>
        /// <returns></returns>
        public static Result SendCustomEvent(string name, string payloadJSON = null)
        {
            var ev = new CustomEvent(name);

            GCHandle payloadHandle = new GCHandle();
            try
            {
                if (payloadJSON != null)
                {
                    payloadHandle = Common.NewUTF8String(payloadJSON, out ev.payload_json_length);
                    ev.payload_json = payloadHandle.AddrOfPinnedObject();
                }

                return trail_ink_send_custom_event(SDK.Raw, ref ev);
            }
            finally
            {
                if (payloadHandle.IsAllocated)
                {
                    payloadHandle.Free();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Internal Insights class to track quality changes in Unity.
    /// </summary>
    internal class TrailInsightsMono : MonoBehaviour
    {
        private int lastQualityIdx = -1;
        private void Update()
        {
            int idx = QualitySettings.GetQualityLevel();
            if (idx != lastQualityIdx)
            {
                InsightsKit.ReportUnityQuality();
                lastQualityIdx = idx;
            }
        }
    }
}
