
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
#if UNITY
using UnityEngine;
using UnityEngine.SceneManagement;
#endif
#if AOT
using AOT;
#endif

// Allows Trail.Editor to access internal methods and variables
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Trail.Editor")]

namespace Trail
{
    /// <summary>
    /// Main part of Trail SDK, handles the basic like Initialization, logging, etc...
    /// </summary>
    public partial class SDK
    {
        #region Structs/Classes/Enums

        /// <summary>
        /// Small wrapper class for startup arguments.
        /// 
        /// </summary>
        public class StartupArg
        {
            public string Name;
            public byte[] Value;

            public StartupArg(string name, byte[] value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        #endregion

        #region Delegates

        public delegate void InitializedCallback(Result result);
        public delegate void GameFocusedChangedCallback(bool isFocused);
        public delegate void LogReceivedCallback(LogLevel level, string location, string message);

        #endregion

        #region Variables

        // Instance of sdk to keep track of.
        internal static SDK instance;

        // Internal references
        private IntPtr sdkRaw = IntPtr.Zero;
        private TrailMono mono;

        private static bool logEnabled = true;

        private static InitializedCallback onPreInitialized;
        private static InitializedCallback onInitialized;
        private static GameFocusedChangedCallback onFocusChanged;
        private static LogReceivedCallback onLogReceived;

        // This is used to block reporting game as loaded
        private static int reportGameLoadedStack = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Checks whether or not SDK is initialized.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                if (instance == null)
                {
                    return false;
                }
                return trail_sdk_is_initialized(instance.sdkRaw);
            }
        }

        /// <summary>
        /// Returns whether or not the game is currently in focus. Not being in any other tab or popup from Payments or invite link.
        /// </summary>
        public static bool IsGameFocused
        {
            get
            {
                bool active = false;
                var res = trail_sdk_is_game_active(instance.sdkRaw, out active);
                if (res.IsError())
                {
                    Common.LogError("IsGameFocused failed with error {0}", res.ToString());
                }
                return active;
            }
        }

        /// <summary>
        /// Callback when SDK is initialized, this also will call the subscribing method if SDK already is initialized.
        /// </summary>
        public static event InitializedCallback OnInitialized
        {
            add
            {
                onInitialized += value;
                if (IsInitialized)
                {
                    value(Result.Ok);
                }
            }
            remove { onInitialized -= value; }
        }

        /// <summary>
        /// Callback for when the User changes focus from the game or comes back. 
        /// </summary>
        public static event GameFocusedChangedCallback OnFocusChanged
        {
            add { onFocusChanged += value; }
            remove { onFocusChanged -= value; }
        }

        /// <summary>
        /// Callback whenever a log is being called from within SDK
        /// </summary>
        public static event LogReceivedCallback OnLogReceived
        {
            add { onLogReceived += value; }
            remove { onLogReceived -= value; }
        }

        /// <summary>
        /// Set/Get the minimum log level for the SDK.
        /// </summary>
        public static LogLevel LogLevel
        {
            get {
                return instance != null ? 
                    trail_sdk_get_min_log_level(instance.sdkRaw) :
                    editorLogLevel; 
            }
            set {
                if(instance != null) {
                    trail_sdk_set_min_log_level(instance.sdkRaw, value);
                }
            }
        }

        /// <summary>
        /// Enables or disables the logging by the SDK
        /// </summary>
        public static bool LogEnabled
        {
            get { return logEnabled; }
            set { logEnabled = value; }
        }

        /// <summary>
        /// Retrives the raw SDK pointer, used to do external method calls to the SDK.
        /// </summary>
        internal static IntPtr Raw { get { return instance == null ? throw new Exception("TrailSDK instance is not intialized") : instance.sdkRaw; } }

        /// <summary>
        /// Retrives the MonoBehaviour object that runs the Unity Version of Trail.
        /// </summary>
        internal static TrailMono Mono { get { return instance.mono; } }

        #endregion

        #region Constructor / Deconstructor

#if TRAIL
        /// <summary>
        /// Automated setup of Trail
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void InitAtStart()
        {
            logEnabled = TrailConfig.EnableLogging;
            if (TrailConfig.InitializeSDKAtStartup)
            {
                Init();
            }
        }

#endif

        /// <summary>
        /// Initializes Trail SDK with the Default Dev Server Settings.
        /// </summary>
        /// <returns></returns>
        public static Result Init()
        {
            return Init("127.0.0.1:" + TrailConfig.InitDevServerPortOverride);
        }

        /// <summary>
        /// Initializes Trail SDK with custom dev host address. Example: 127.0.0.1:23000
        /// </summary>
        /// <param name="devHost">The address Trail SDK should connect to if running from Editor.</param>
        /// <returns>Whether creating the SDK worked. The result of Initialization get's provided in Initialize Callback</returns>
        public static Result Init(string devHost)
        {
            if (instance != null)
            {
                Log(LogLevel.Error, "Trail Init failed due to SDK already created!");
                return IsInitialized ? Result.SDKAlreadyInitialized : Result.SDKAlreadyCreated;
            }
            IntPtr sdkRaw;
#if UNITY
            var createParams = new CreateParams(Application.unityVersion);
            var result = trail_sdk_create(ref createParams, out sdkRaw);
#else
            var result = trail_sdk_create(out sdkRaw);
#endif
            if (result != Result.Ok)
            {
                Debug.LogError("Trail failed to create the SDK " + result);
#if UNITY_EDITOR
                if (result == Result.SDKAlreadyCreated || result == Result.SDKAlreadyInitialized)
                {
                    var todo = UnityEditor.EditorUtility.DisplayDialog("SDK Error", result.ToString() + "\nDo you want to restart the SDK?", "Yes", "Cancel");
                    if (todo)
                    {
                        trail_sdk_destroy(sdkRaw);
                        return Init(devHost);
                    }
                }
#endif
                return result;
            }

            instance = new SDK(sdkRaw);
            // Creates a new mono behaviour instance to connect Unity APIs to Trail.
            instance.mono = TrailMono.Create();

            // Disables standard output
            trail_sdk_set_log_standard_output_enabled(instance.sdkRaw, false);
            // Setup logging for c/c++
            trail_sdk_set_on_log(instance.sdkRaw, Marshal.GetFunctionPointerForDelegate(new LogCB(SDK.OnLogCB)), IntPtr.Zero);
            LogLevel = TrailConfig.DefaultLogLevel;

            // Setup callback for game focus changed
            trail_sdk_set_on_game_active_status_changed(
                instance.sdkRaw,
                Marshal.GetFunctionPointerForDelegate(
                    new GameActiveStatusChangedCB(SDK.OnGameActiveStatusChangedCB)),
                IntPtr.Zero);

            // Setup Init callback
#if BROWSER
                trail_sdk_init(
                    instance.sdkRaw,
                    Marshal.GetFunctionPointerForDelegate(new InitCB(SDK.onInitCB)),
                   IntPtr.Zero
                );
#elif !CSHARP_7_3_OR_NEWER
            var initParams = new InitParams(devHost);
            trail_sdk_init(
                    instance.sdkRaw,
                    ref initParams,
                    Marshal.GetFunctionPointerForDelegate(new InitCB(SDK.onInitCB)),
                    IntPtr.Zero
                );
#elif UNITY_EDITOR
            var initParams = new InitParams(devHost);
            WaitForDevServer(initParams);
#endif
            return Result.Ok;
        }

#if UNITY_EDITOR && CSHARP_7_3_OR_NEWER
        /// Used to wait for dev server to start up.
        private static async void WaitForDevServer(InitParams initParams)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            Type cli = null;
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("Trail.Editor"))
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.FullName.Contains("CLI"))
                        {
                            cli = type;
                            break;
                        }
                    }
                    break;
                }
            }

            if (cli != null)
            {
                var actuallyHasDevServerProp = cli.GetProperty("UseDevServer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var prop = cli.GetProperty("DevServerRunning", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (prop != null && actuallyHasDevServerProp != null && (bool)actuallyHasDevServerProp.GetValue(null) == true)
                {
                    while ((bool)prop.GetValue(null) == false)
                    {
                        await System.Threading.Tasks.Task.Delay(25);
                    }
                }
            }
            trail_sdk_init(
                    instance.sdkRaw,
                    ref initParams,
                    Marshal.GetFunctionPointerForDelegate(new InitCB(SDK.onInitCB)),
                    IntPtr.Zero
                );
        }
#endif

        private SDK(IntPtr sdkRaw)
        {
            this.sdkRaw = sdkRaw;
        }

        ~SDK()
        {
            Dispose();
        }

        /// <summary>
        /// Internal Dispose method used for cleanup of the library.
        /// This is an very unsafe method and might cause crashes, use with care.
        /// </summary>
        internal void Dispose()
        {
            if (this.sdkRaw == IntPtr.Zero)
            {
                return;
            }
            trail_sdk_destroy(this.sdkRaw);
            Log(LogLevel.Debug, "Removing SDK from memory");
            this.sdkRaw = IntPtr.Zero;
            instance = null;
        }

#if AOT
        [MonoPInvokeCallback(typeof(InitCB))]
#endif
        private static void onInitCB(Result result, IntPtr callback_data)
        {
            // reports game loaded if this setting is Enabled (Enabled by Default)
            if (result.IsOk() && TrailConfig.InitializeSDKAtStartup)
            {
                FinishGameLoadTask();
            }

            if (onPreInitialized != null)
            {
                onPreInitialized.Invoke(result);
            }

            if (onInitialized != null)
            {
                onInitialized.Invoke(result);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This will call game loaded to Trail to remove the loading screen. 
        /// This can further be blocked by Adding Extra tasks to be finished using SDK.AddExtraGameLoadTask()
        /// </summary>
        public static Result FinishGameLoadTask()
        {
            reportGameLoadedStack--;
            if (reportGameLoadedStack <= 0)
            {
                return trail_sdk_report_game_loaded(instance.sdkRaw);
            }
            Log(LogLevel.Info, "Finish game loaded is blocked by" + reportGameLoadedStack + " more task(s).");
            return Result.Canceled;
        }

        /// <summary>
        /// This will block any call to report game loaded until "FinishGameLoadTask" gets called.
        /// </summary>
        public static void AddExtraGameLoadTask()
        {
            reportGameLoadedStack++;
        }

        /// <summary>
        /// Simple and short method to force a crash on Trail.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Result CrashGame(string errorMessage = null)
        {
#if UNITY_EDITOR
            Debug.LogError("Trail Crashed!\n" + errorMessage);
            UnityEditor.EditorApplication.isPlaying = false;
            return Result.Ok;
#else
                Int32 size;
                var errorMessageHandle = Common.NewUTF8String(errorMessage, out size);
                var res = trail_sdk_crash_game(
                    instance.sdkRaw,
                    errorMessageHandle.AddrOfPinnedObject(),
                    size
                );
                errorMessageHandle.Free();
                return res;
#endif
        }

        /// <summary>
        /// Retrives the startup arguments from an invite link, notification or something else to provide some extra functionalities.
        /// </summary>
        /// <param name="startupArgs"></param>
        /// <returns>Whether it succeeded or failed to retrive the Startup Arguments.</returns>
        public static Result GetStartupArgs(out StartupArg[] startupArgs)
        {
            IntPtr ptr;
            int count;
            var res = trail_sdk_get_game_params(
                instance.sdkRaw,
                out ptr,
                out count
            );

            if (res != Result.Ok)
            {
                startupArgs = null;
            }
            else
            {
                startupArgs = Common.PtrToStructArray<StartupArg, StartupArgRaw>(
                    ptr,
                    count,
                    (i, arg) =>
                    {
                        string name = Encoding.UTF8.GetString(
                            arg.name,
                            0,
                            Array.IndexOf<byte>(arg.name, 0)
                        );
                        byte[] value = new byte[arg.value_size];
                        Marshal.Copy(arg.value, value, 0, arg.value_size);
                        return new StartupArg(name, value);
                    }
                );
            }

            return res;
        }

        /// <summary>
        /// Retrives the startup arguments from an invite link, notification or something else to provide some extra functionalities.
        /// </summary>
        /// <returns>Returns an array of the startup arguments.</returns>
        public static StartupArg[] GetStartupArgs()
        {
            StartupArg[] args = null;
            var result = GetStartupArgs(out args);
            if (result.IsError())
            {
                Log(LogLevel.Error, "Could not retrive startup arguments: " + result);
            }
            return args;
        }

        /// <summary>
        /// Exit method to close the game and return to the main page of Trail. 
        /// This sadly won't trigger OnApplicationQuit as Unity currently do not support it!
        /// https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html
        /// </summary>
        /// <returns>Whether it was able to quit the game.</returns>
        public static Result ExitGame()
        {
#if UNITY_EDITOR
            if (!IsInitialized) { return Result.SDKNotInitialized; }
            UnityEditor.EditorApplication.isPlaying = false;
            return Result.Ok;
#else
            return trail_sdk_exit_game(SDK.Raw);
#endif
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Internal Tick method to run Trail locally in Editor.
        /// This always get's called by TrailMono.cs
        /// </summary>
        internal static void Tick()
        {
#if !BROWSER
            if (SDK.Raw != IntPtr.Zero)
            {
                trail_sdk_run_loop_iteration(SDK.Raw);
            }
#endif
        }

        /// <summary>
        /// Callback when SDK is initialized, this also will call the subscribing method if SDK already is initialized.
        /// </summary>
        internal static event InitializedCallback OnPreInitialized
        {
            add
            {
                onPreInitialized += value;
                if (IsInitialized)
                {
                    value(Result.Ok);
                }
            }
            remove { onPreInitialized -= value; }
        }

        #endregion

        #region Private static Callbacks

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void SetupLogColor()
        {
            isDarkMode = UnityEditor.EditorGUIUtility.isProSkin;
            editorLogLevel = TrailConfig.DefaultLogLevel;
        }
#endif

#if UNITY_EDITOR
        private static bool isDarkMode = true;
#endif
        private static LogLevel editorLogLevel = LogLevel.Info;
        private const string COLOR_DARKMODE = "#AA99FF";
        private const string COLOR_LIGHTMODE = "#6E32F4";

        private const string TAG = "[Trail SDK]";

#if AOT
        [MonoPInvokeCallback(typeof(GameActiveStatusChangedCB))]
#endif
        private static void OnGameActiveStatusChangedCB(bool gameActive, IntPtr callback_data)
        {
            // callback_data should return IntPtr.Zero
            if (onFocusChanged != null)
            {
                onFocusChanged.Invoke(gameActive);
            }
        }

#if AOT
        [MonoPInvokeCallback(typeof(LogCB))]
#endif
        private static void OnLogCB(
            LogLevel level,
            IntPtr location,
            Int32 location_length,
            IntPtr message,
            Int32 message_length,
            IntPtr callback_data)
        {
            // callback_data should return IntPtr.Zero
            if (!logEnabled) { return; }
            string loc = Common.PtrToStringUTF8(location, location_length);
            string msg = Common.PtrToStringUTF8(message, message_length);

#if UNITY_EDITOR
            UnityEngine.Debug.unityLogger.Log(
                Common.ConvertToLogType(level),
                string.Format("<color={1}>{0}</color>", TAG, isDarkMode ? COLOR_DARKMODE : COLOR_LIGHTMODE),
                string.IsNullOrEmpty(loc) ?
                    string.Format("{0} - {1}", Common.ConvertToString(level), msg) :
                    string.Format("{0} {1} - {2}", Common.ConvertToString(level), loc, msg));
#else
            UnityEngine.Debug.unityLogger.Log(
                Common.ConvertToLogType(level),
                TAG,
                string.IsNullOrEmpty(loc) ?
                    string.Format("{0} - {1}", Common.ConvertToString(level), msg) :
                    string.Format("{0} {1} - {2}", Common.ConvertToString(level), loc, msg));
#endif
            if (onLogReceived != null)
            {
                onLogReceived.Invoke(level, loc, msg);
            }
        }

        internal static void Log(LogLevel level, string str)
        {
            if (!logEnabled || LogLevel > level)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEngine.Debug.unityLogger.Log(
                Common.ConvertToLogType(level),
                string.Format("<color={1}>{0}</color>", TAG, isDarkMode ? COLOR_DARKMODE : COLOR_LIGHTMODE),
                string.Format("{0} - {1}", Common.ConvertToString(level), str));
#else
            UnityEngine.Debug.unityLogger.Log(
                Common.ConvertToLogType(level),
                TAG,
                string.Format("{0} - {1}", Common.ConvertToString(level), str));
#endif
            if (onLogReceived != null)
            {
                onLogReceived.Invoke(level, "", str);
            }
        }

        internal static void Log(LogLevel level, string tag, string str)
        {
            // This is required to check with Trail Config instead of the set level due to editor logs using this.
            if(!logEnabled || LogLevel > level) {
                return;
            }
#if UNITY_EDITOR
            tag = string.Format("<color={1}>[{0}]</color>", tag, isDarkMode ? COLOR_DARKMODE : COLOR_LIGHTMODE);
#endif

            UnityEngine.Debug.unityLogger.Log(
                Common.ConvertToLogType(level),
                tag,
                string.Format("{0} - {1}", Common.ConvertToString(level), str));
            if (onLogReceived != null)
            {
                onLogReceived.Invoke(level, "", str);
            }
        }

        #endregion
    }
}
