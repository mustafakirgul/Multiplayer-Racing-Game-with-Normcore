#if CSHARP_7_3_OR_NEWER
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Trail
{
    public class CLI
    {
        [Serializable]
        public class CLIData
        {
            [SerializeField] private bool enableDevServer = true;
            [SerializeField] private string roomId = "";
            [SerializeField] private string environment = "beta";

            public string Environment { get { return environment; } set { environment = value; } }

            public bool EnableDevServer { get { return enableDevServer; } }
            public string RoomId
            {
                get
                {
                    if (string.IsNullOrEmpty(roomId))
                    {
                        RoomId = CLIName.GetName();
                    }
                    return roomId;
                }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        roomId = CLIName.GetName();
                    }
                    else
                    {
                        roomId = value;
                    }
                    Save();
                }
            }

            public void Load()
            {
                try
                {
                    if (System.IO.File.Exists(CliStoragePath))
                    {
                        JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(CliStoragePath), this);
                    }
                }
                catch (System.Exception)
                {

                }
            }

            public void Save()
            {
                try
                {
                    var dir = System.IO.Path.GetDirectoryName(CliStoragePath);
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    System.IO.File.WriteAllText(CliStoragePath, JsonUtility.ToJson(this));
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        #region Delete Check

        /// <summary>
        /// Used to check if you attempting to delete the CLI files and stop the process so Unity can actually delete the file.
        /// </summary>
        private class DeleteCheck : UnityEditor.AssetModificationProcessor
        {
            public static UnityEditor.AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions option)
            {
#if UNITY_EDITOR_WIN
                if (System.IO.Path.GetFullPath(Application.dataPath + "\\Trail\\Editor\\CLI\\windows.exe") == path)
                {
                    StopProcess();
                }
#elif UNITY_EDITOR_OSX
                if(System.IO.Path.GetFullPath(Application.dataPath + "/Trail/Editor/CLI/macos") == path) {
                    StopProcess();
                }
#elif UNITY_EDITOR_LINUX
                if(System.IO.Path.GetFullPath(Application.dataPath + "/Trail/Editor/CLI/linux") == path) {
                    StopProcess();
                }
#else
#error Unsupported platform
#endif
                return AssetDeleteResult.DidNotDelete;
            }
        }

        #endregion

        #region Variables

        private static RunDevServerRequest runDevServerRequest = null;
        private static CLIData cliData = new CLIData();
        static Process process = null;
        static int daemonPort = 0;
        static List<string> errorData = new List<string>();

        #endregion

        #region Properties

        private static string CliStoragePath
        {
            get
            {
                return TrailEditor.CLIStoragePath;
            }
        }

        public enum ECState
        {
            None, // error

            Idle,           // dev server is idle. and waiting for Unity to enter play mode.
            Loading,        // dev server is loading up
            Waiting,        // editor companion not opened, but cli is running
            Running,        // editor companion running
            Disconnected,   // editor companion disconnected

            Disabled // disabled in config
        }

        public static bool DevServerRunning { get { return !cliData.EnableDevServer || (runDevServerRequest != null && runDevServerRequest.State == ECState.Waiting); } }
        public static double DevServerActiveTime
        {
            get
            {
                if (runDevServerRequest == null)
                    return 0d;
                return runDevServerRequest.TimePassed.TotalSeconds;
            }
        }
        public static ECState DevServerState
        {
            get
            {
                if (!cliData.EnableDevServer)
                {
                    return ECState.Disabled;
                }

                if (runDevServerRequest == null)
                {
                    return ECState.Idle;
                }

                return runDevServerRequest.State;
            }
        }
        public static bool UseDevServer { get { return cliData.EnableDevServer; } }

        public static string DevServerRoomId
        {
            get
            {
                return cliData.RoomId;
            }
        }

        public static string Environment { get { return cliData.Environment; } }

        public static void NewRoomId()
        {
            cliData.RoomId = "";
        }

        private static int Port
        {
            get
            {
                return daemonPort;
            }
            set
            {
                daemonPort = value;
            }
        }

        #endregion

        #region Initialization

        static CLI()
        {
            AssemblyReloadEvents.beforeAssemblyReload += StopProcess;
            EditorApplication.quitting += StopProcess;
            // Stop process while importing packages. This is to ensure updating SDK always work.
            AssetDatabase.importPackageStarted += (packageName) => StopProcess();
            cliData.Load();
            cliData.Save();
        }

        [RuntimeInitializeOnLoadMethod]
        private static void OnEnterPlaymode()
        {
#if TRAIL
            SDK.Log(LogLevel.Debug, "Trail CLI", "Starting dev server!");

            runDevServerRequest = new RunDevServerRequest(TrailConfig.InitDevServerPortOverride);
            runDevServerRequest.AddCallback((success, request) =>
            {
                if (!success && !request.IsCanceled)
                {
                    SDK.Log(LogLevel.Error, "Trail CLI", request.ErrorMessage);
                }
            });

            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode && runDevServerRequest != null)
                {
                    runDevServerRequest.Cancel();
                    runDevServerRequest = null;
                }
            };

            EditorUtility.ClearProgressBar();
#endif
        }

        #endregion

        #region Daemon

        private static Process CreateProcess()
        {
            daemonPort = GetFreePort();
            SDK.Log(LogLevel.Debug, "Trail CLI", "Launching daemon on port " + daemonPort);
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "daemon --port=" + daemonPort.ToString();

#if UNITY_EDITOR_WIN
            start.FileName = Application.dataPath + "\\Trail\\Editor\\CLI\\Trail CLI Windows.exe";
#elif UNITY_EDITOR_OSX
            start.FileName = Application.dataPath + "/Trail/Editor/CLI/Trail CLI MacOS";
#elif UNITY_EDITOR_LINUX
            start.FileName = Application.dataPath + "/Trail/Editor/CLI/Trail CLI Linux";
#else
#error Unsupported platform
#endif

            if (!System.IO.File.Exists(start.FileName))
            {
                SDK.Log(LogLevel.Error, "Trail CLI", string.Format("File is missing at '{0}'", start.FileName));
            }

#if UNITY_EDITOR_OSX
            Action<string> unQuarantine = (string path) => {
                Process proc  = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "xattr",
                        Arguments = "-d com.apple.quarantine \"" + path + "\"",
                        UseShellExecute = false,
                    },
                };
                proc.Start();
                proc.WaitForExit(1000);
            };

            unQuarantine(start.FileName);
            unQuarantine("Assets/Trail/Editor/Plugins/Grpc.Core/runtimes/osx/x64/grpc_csharp_ext.bundle");
            unQuarantine("Assets/Trail/Editor/Plugins/Grpc.Core/runtimes/osx/x86/grpc_csharp_ext.bundle");
#endif

            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            var process = Process.Start(start);
            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExit;
            process.ErrorDataReceived += OnErrorDataReceived;
            process.BeginErrorReadLine();
            return process;
        }

        public static void StopProcess()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    SDK.Log(LogLevel.Debug, "Trail CLI", "Killing daemon");
                    process.Kill();
                }
                process = null;
            }
        }

        private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                errorData.Add(e.Data);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            SDK.Log(LogLevel.Debug, "Trail CLI", "Daemon exited");
            if (errorData.Count > 0)
            {
                SDK.Log(LogLevel.Error, "Trail CLI", "Daemon crashed:\n" + string.Join("\n", errorData));
            }
            process = null;
        }

        private static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private static async Task<Trail.Cli.Service.Cli.V1.CLIService.CLIServiceClient> GetCLIClient()
        {
            await Task.Yield();
            if (process == null || process.HasExited)
            {
                process = CreateProcess();
            }

            var channel = new Grpc.Core.Channel("127.0.0.1:" + daemonPort.ToString(),
                    Grpc.Core.ChannelCredentials.Insecure, new[] {
                        new Grpc.Core.ChannelOption("grpc.enable_retries", 1)
                    });

            var client = new Trail.Cli.Service.Cli.V1.CLIService.CLIServiceClient(channel);
            var res = await client.GetLoginStatusAsync(
                    new Trail.Cli.Service.Cli.V1.GetLoginStatusReq(),
                    new Grpc.Core.CallOptions().WithWaitForReady(true));

            return client;
        }

        #endregion

        #region public static CLI Requests

        public static LoginStatusRequest GetLoginStatus()
        {
            return new LoginStatusRequest();
        }

        public static LoginRequest LogIn(string user, string password)
        {
            return new LoginRequest(user, password);
        }

        public static LogoutRequest LogOut()
        {
            return new LogoutRequest();
        }

        public static ListGamesRequest ListGames()
        {
            return new ListGamesRequest();
        }

        public static UploadBuildRequest UploadBuild()
        {
            return new UploadBuildRequest();
        }

        public static UploadBuildRequest UploadBuild(string path)
        {
            return new UploadBuildRequest(path);
        }

        public static InitializeGameRequest InitializeGame(string gameId, string projectPath)
        {
            return new InitializeGameRequest(gameId, projectPath);
        }

        public static RunBuildRequest RunBuild()
        {
            return new RunBuildRequest();
        }

        public static RunBuildRequest RunBuild(string path)
        {
            return new RunBuildRequest(path);
        }

        #endregion

        #region CLI Request Base Classes

        public class CLIRequest
        {
            public bool IsComplete { get; protected set; }
            public delegate void CLIRequestCallback(bool success);
            private event CLIRequestCallback OnRequestComplet;
            protected string errorMessage;
            private DateTime requestTime;
            public TimeSpan TimePassed { get { return DateTime.UtcNow - requestTime; } }
            public string ErrorMessage { get { return errorMessage; } }

            protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            protected CLIRequest()
            {
                requestTime = DateTime.UtcNow;
            }

            public void AddCallback(CLIRequestCallback callback)
            {
                OnRequestComplet += callback;
            }

            protected void RunCallback(bool success)
            {
                if (OnRequestComplet != null)
                {
                    OnRequestComplet.Invoke(success);
                }
                IsComplete = true;
            }

            public bool IsCanceled { get { return cancellationTokenSource.IsCancellationRequested; } }
            public virtual void Cancel()
            {
                cancellationTokenSource.Cancel();
            }
        }

        public class CLIRequest<T> : CLIRequest
        {
            public delegate void CLIRequestCallbackT(bool success, T requestResult);
            private event CLIRequestCallbackT OnRequestCompletT;

            protected CLIRequest() : base()
            {

            }

            public void AddCallback(CLIRequestCallbackT callback)
            {
                OnRequestCompletT += callback;
            }

            protected void RunCallback(bool success, T requestResult)
            {
                if (OnRequestCompletT != null)
                {
                    OnRequestCompletT(success, requestResult);
                }
                base.RunCallback(success);
            }
        }

        #endregion

        #region CLI Request Classes

        [Serializable]
        public struct Game
        {
            public string Title;
            public string Id;

            public Game(string title, string id)
            {
                this.Title = title;
                this.Id = id;
            }
        }

        public class InitializeGameRequest : CLIRequest<InitializeGameRequest>
        {
            public InitializeGameRequest(string gameId, string projectPath) : base()
            {
                InitializeGame(gameId, projectPath);
            }

            private async void InitializeGame(string gameId, string projectPath)
            {
                try
                {
                    var client = await GetCLIClient();

                    var req = new Trail.Cli.Service.Cli.V1.InitializeGameReq
                    {
                        GameId = gameId,
                        Path = projectPath,
                    };
                    var res = await client.InitializeGameAsync(req, cancellationToken: cancellationTokenSource.Token);

                    if (res.Payload != null)
                    {
                        RunCallback(true, this);
                    }
                    else if (res.Error != null)
                    {
                        errorMessage = res.Error.Message;
                        RunCallback(false, this);
                    }
                    else
                    {
                        errorMessage = cancellationTokenSource.IsCancellationRequested ? "Canceled" : "Unexpected Error";
                        RunCallback(false, this);
                    }
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        public class ListGamesRequest : CLIRequest<Game[]>
        {
            public ListGamesRequest() : base()
            {
                GetListOfGames();
            }

            private async void GetListOfGames()
            {
                try
                {
                    var client = await GetCLIClient();
                    var res = await client.ListGamesAsync(new Trail.Cli.Service.Cli.V1.ListGamesReq(), cancellationToken: cancellationTokenSource.Token);

                    if (res.Payload == null)
                    {
                        RunCallback(false, null);
                    }
                    else
                    {
                        RunCallback(true, res.Payload.Games.Select(x => new Game(x.Title, x.GameId)).ToArray());
                    }
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, null);
                }
            }
        }

        public class UploadBuildRequest : CLIRequest<UploadBuildRequest>
        {
            public float Progress { get; private set; }
            public float MbPerSecond { get; private set; }

            public event Action<float> OnProgressUpdate;

            public UploadBuildRequest() : base()
            {
                UploadBuild();
            }

            public UploadBuildRequest(string path) : base()
            {
                UploadBuild(path);
            }

            private async void UploadBuild(string path = null)
            {
                try
                {
                    var client = await GetCLIClient();

                    var req = new Trail.Cli.Service.Cli.V1.UploadBuildReq
                    {
                        BuildPath = string.IsNullOrEmpty(path) ? EditorUtility.OpenFolderPanel("Select build path", "", "") : path,
                        ProjectPath = "",
                    };
                    var call = client.UploadBuild(req);

                    var error = false;
                    while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                    {
                        if (call.ResponseStream.Current.Error != null)
                        {
                            error = true;
                            errorMessage += call.ResponseStream.Current.Error.Message + "\n";
                            break;
                        }
                        if (call.ResponseStream.Current.Progress != null && call.ResponseStream.Current.Progress.Update != null)
                        {
                            var p = call.ResponseStream.Current.Progress.Update.Progress;
                            if (p >= 0f)
                            {
                                Progress = p;
                            }
                        }
                        if (OnProgressUpdate != null)
                        {
                            OnProgressUpdate.Invoke(Progress);
                        }
                    }
                    RunCallback(!error, this);
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        public class LogoutRequest : CLIRequest<LogoutRequest>
        {
            public LogoutRequest() : base()
            {
                Logout();
            }

            private async void Logout()
            {
                try
                {
                    var client = await GetCLIClient();
                    var res = await client.LogOutAsync(new Trail.Cli.Service.Cli.V1.LogOutReq(), cancellationToken: cancellationTokenSource.Token);
                    RunCallback(res.Payload != null, this);
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        public class LoginRequest : CLIRequest<LoginRequest>
        {
            public Cli.Service.Cli.V1.LogInRes Result;

            public LoginRequest(string user, string password) : base()
            {
                Login(user, password);
            }

            private async void Login(string user, string password)
            {
                try
                {
                    var client = await GetCLIClient();

                    var res = await client.LogInAsync(new Trail.Cli.Service.Cli.V1.LogInReq
                    {
                        Email = user,
                        Password = password,
                    }, cancellationToken: cancellationTokenSource.Token);
                    Result = res;

                    RunCallback(res.Payload != null, this);
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        public class LoginStatusRequest : CLIRequest<bool>
        {
            public LoginStatusRequest() : base()
            {
                LoginStatus();
            }

            private async void LoginStatus()
            {
                try
                {
                    var client = await GetCLIClient();

                    var res = await client.GetLoginStatusAsync(
                            new Trail.Cli.Service.Cli.V1.GetLoginStatusReq(),
                            cancellationToken: cancellationTokenSource.Token
                            );

                    RunCallback(res.Payload != null, res.Payload.IsLoggedIn);
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, false);
                }
            }
        }

        public class RunDevServerRequest : CLIRequest<RunDevServerRequest>
        {
            public ECState State { get; private set; }

            public RunDevServerRequest(ushort port) : base()
            {
                State = ECState.Loading;
                RunDevServer(port);
            }

            private async void RunDevServer(ushort port)
            {
                try
                {
                    var client = await GetCLIClient();
                    var call = client.RunDevServer();
                    var req = new Trail.Cli.Service.Cli.V1.RunDevServerReq
                    {
                        Start = new Trail.Cli.Service.Cli.V1.RunDevServerReq.Types.Start
                        {
                            ProjectPath = "",
                            Port = port,
                            RoomId = cliData.RoomId,
                        },
                    };

                    await call.RequestStream.WriteAsync(req);
                    var completed = false;

                    var responseReaderTask = Task.Run(async () =>
                    {
                        try
                        {
                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                                if (call.ResponseStream.Current.Error != null)
                                {
                                    errorMessage = call.ResponseStream.Current.Error.Message;
                                    RunCallback(false, this);
                                }
                                else if (call.ResponseStream.Current.Started != null)
                                {
                                    State = ECState.Waiting;
                                }
                                else if (call.ResponseStream.Current.PlayerConnected != null && State != ECState.Disconnected)
                                {
                                    State = ECState.Running;
                                }
                                else if (call.ResponseStream.Current.PlayerDisconnected != null)
                                {
                                    State = ECState.Disconnected;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            // For some reason MoveNext throws an exception when the server closes
                            // the channel
                            if (completed && exception is Grpc.Core.RpcException)
                            {
                                var rpcException = (Grpc.Core.RpcException)exception;
                                if (rpcException.StatusCode == Grpc.Core.StatusCode.Unavailable)
                                {
                                    State = ECState.Disconnected;
                                    errorMessage = rpcException.Message;
                                    RunCallback(false, this);
                                    return;
                                }
                            }
                            throw exception;
                        }
                    });

                    await responseReaderTask;
                    await call.RequestStream.CompleteAsync();
                    completed = true;
                    State = ECState.Disconnected;
                    RunCallback(true, this);
                }
                catch (System.Exception e)
                {
                    State = ECState.Disconnected;
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        public class RunBuildRequest : CLIRequest<RunBuildRequest>
        {
            public RunBuildRequest() : this(EditorUtility.OpenFolderPanel("Select build path", "", ""))
            {

            }

            public RunBuildRequest(string path) : base()
            {
                RunBuild(path);
            }

            private async void RunBuild(string path)
            {
                try
                {
                    var client = await GetCLIClient();
                    var call = client.RunBuild();
                    var req = new Trail.Cli.Service.Cli.V1.RunBuildReq
                    {
                        Start = new Trail.Cli.Service.Cli.V1.RunBuildReq.Types.Start
                        {
                            BuildPath = path,
                            ProjectPath = "",
                            RoomId = cliData.RoomId,
                        },
                    };

                    await call.RequestStream.WriteAsync(req);
                    var completed = false;

                    var responseReaderTask = Task.Run(async () =>
                    {
                        try
                        {
                            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                            {
                                if (call.ResponseStream.Current.Error != null)
                                {
                                    errorMessage = call.ResponseStream.Current.Error.Message;
                                    RunCallback(false, this);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            // For some reason MoveNext throws an exception when the server closes
                            // the channel
                            if (completed && exception is Grpc.Core.RpcException)
                            {
                                var rpcException = (Grpc.Core.RpcException)exception;
                                if (rpcException.StatusCode == Grpc.Core.StatusCode.Unavailable)
                                {
                                    errorMessage = rpcException.Message;
                                    RunCallback(false, this);
                                    return;
                                }
                            }
                            throw exception;
                        }
                    });

                    await responseReaderTask;
                    await call.RequestStream.CompleteAsync();
                    completed = true;
                    RunCallback(true, this);
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                    RunCallback(false, this);
                }
            }
        }

        #endregion
    }

    internal static class CLIName
    {
        private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int count = 62;

        private static string RandomString(int length = 10)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = characters[UnityEngine.Random.Range(0, count)];
            }
            return new string(chars);
        }

        public static string GetName()
        {
            return RandomString(16);
        }
    }
}
#endif
