using System;
using UnityEngine;

namespace Trail
{
    /// <summary>
    /// Configuration class to set some basic settings for the SDK.
    /// </summary>
    internal class TrailConfig : ScriptableObject
    {
        #region Singleton

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitTrailConfig()
        {
            var c = Config;
        }
#endif

        private static TrailConfig config;
        private static TrailConfig Config
        {
            get
            {
                if (!config)
                {
                    var c = Resources.Load<TrailConfig>("TrailConfig");
                    if (!c)
                    {
                        c = CreateInstance<TrailConfig>();
                        c.name = "TrailConfig";
#if UNITY_EDITOR
                        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Trail/Resources"))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets/Trail", "Resources");
                        }
                        UnityEditor.AssetDatabase.CreateAsset(c, "Assets/Trail/Resources/TrailConfig.asset");
#endif
                    }
                    config = c;
                }
                return config;
            }
        }

        #endregion

        public enum AspectRatio {
            AspectFree,
            Aspect5by4,
            Aspect4by3,
            Aspect16by9,
            Aspect16by10,
        }

        [Space]
        [SerializeField] internal bool initializeSDKAtStartup = true;
        [SerializeField] internal ushort devServerPort = 23000;

        [Header("Logging")]
        [SerializeField] internal bool enableLogging = true;
        [SerializeField] internal LogLevel logLevel = LogLevel.Info;

        [Header("InsightsKit")]
        [SerializeField] internal bool reportSceneChanges = true;
        [SerializeField] internal bool reportQualityChanges = true;

        [Header("PerformanceKit")]
        [SerializeField] internal AspectRatio initialAspectRatio = AspectRatio.AspectFree;

        public static bool InitializeSDKAtStartup { get { return Config.initializeSDKAtStartup; } }
        public static ushort InitDevServerPortOverride { get { return Config.devServerPort; } }

        public static bool EnableLogging { get { return Config.enableLogging; } }
        public static LogLevel DefaultLogLevel { get { return Config.logLevel; } }

        public static bool ReportSceneChanges { get { return Config.reportSceneChanges; } }
        public static bool ReportQualityChanges { get { return Config.reportQualityChanges; } }

        public static AspectRatio InitialAspectRatio { get { return Config.initialAspectRatio; } }
    }
}
