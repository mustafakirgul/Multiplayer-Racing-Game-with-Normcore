using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Trail.Example
{
    public class TrailExample : MonoBehaviour
    {
        public bool alwaysInitializeSDKAtStartup = true;

        [Space]
        public string paymentKitProduct = "00000000-0000-0000-0000-000000000000";

        [Space]
        public Text GameUserID;
        public Text PlayToken;
        public Text GameActive;
        public Text RecommendedQualityLevel;
        public Text RecommendedResolution;
        public Text Resolution;
        public Text DisplayResolution;
        public Dropdown CommonResolutions;
        public InputField inputField;

        private Trail.Resolution[] commonResolutionsData;

        void Start()
        {
            if (!TrailConfig.InitializeSDKAtStartup && alwaysInitializeSDKAtStartup)
            {
                Debug.LogWarning("[TrailSDK] - Automatic Initialization is <color=red>disabled</color>, will manually call SDK.Init();");
                SDK.Init();
            }

            SDK.OnInitialized += (Trail.Result result) =>
            {
                if (result != Trail.Result.Ok)
                {
                    Debug.LogErrorFormat("Trail SDK init failed: {0}", result.ToString());
                    return;
                }

                Debug.LogFormat("Trail SDK init succeeded");
                if (!TrailConfig.InitializeSDKAtStartup)
                {
                    var finishGameLoadTaskResult = SDK.FinishGameLoadTask();
                    if (finishGameLoadTaskResult != Trail.Result.Ok)
                    {
                        Debug.LogErrorFormat("Failed to finish game load task: {0}", finishGameLoadTaskResult);
                    }
                }

                this.GameUserID.text = string.Format("Game user ID: {0}", AuthKit.GetGameUserID());
                this.PlayToken.text = string.Format("Play token: {0}", AuthKit.GetPlayToken());
                SDK.OnFocusChanged += (this.OnGameActiveStatusChanged);
                this.OnGameActiveStatusChanged(SDK.IsGameFocused);

                PerformanceKit.OnDisplayResolutionChanged += OnBrowserResolutionChanged;
                OnBrowserResolutionChanged(Trail.Resolution.Zero);

                Debug.Log(string.Format("Is loading invite: {0}", PartyKit.IsInviteLoading()));
                var partyData = PartyKit.GetPartyData();
                for (int i = 0; i < partyData.Count; i++)
                {
                    var field = partyData[i];
                    Debug.Log(string.Format("Party data: {0}  -  Key: {1} - Value: {2}", i, field.Key, field.Value));
                }
      
                PartyKit.OnPartyDataUpdated += OnPartyDataUpdated;

                SDK.StartupArg[] args;
                SDK.GetStartupArgs(out args);

                for (int i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    Debug.Log(string.Format("Startup Arg #{0}: Name: \"{1}\", Length {2}",
                        i, arg.Name, arg.Value.Length));
                }

                string cloudStoragePath;
                FileKit.GetCloudStoragePath(out cloudStoragePath);
                Debug.Log("Cloud storage path: " + cloudStoragePath);

                AuthKit.GetFingerprint((res, fingerprint) => {
                    if (res.IsOk()) {
                        Debug.Log("Fingerprint: " + fingerprint);
                    } else {
                        Debug.LogErrorFormat("Failed to get fingerprint: {0}",
                            res.ToString());
                    }
                });

#if !UNITY_EDITOR && UNITY_WEBGL
                var path = FileKit.GetCloudStoragePathFormatted("example.txt");
                if(System.IO.File.Exists(path)) {
                    inputField.text = System.IO.File.ReadAllText(path);
                }
#endif
            };

            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        }

        private void OnBrowserResolutionChanged(Resolution resolution)
        {
            PerformanceKit.SetResolution(PerformanceKit.GetResolution());

            this.RecommendedQualityLevel.text = string.Format(
                "Recommended quality level: {0}",
                PerformanceKit.GetRecommendedQualityLevel()
            );
            this.RecommendedResolution.text = string.Format(
                "Recommended resolution: {0}",
                PerformanceKit.GetRecommendedResolution()
            );
            var gameResolution = PerformanceKit.GetResolution();
            this.Resolution.text = string.Format("Resolution: {0}", gameResolution);
            this.DisplayResolution.text = string.Format(
                "Display resolution: {0}",
                PerformanceKit.GetDisplayResolution()
            );

            this.commonResolutionsData = PerformanceKit.GetCommonResolutions();
            this.CommonResolutions.ClearOptions();
            for (int i = 0; i < commonResolutionsData.Length; i++)
            {
                var res = commonResolutionsData[i];
                this.CommonResolutions.options.Add(new Dropdown.OptionData(res.ToString()));
                if (res.Width == gameResolution.Width && res.Height == gameResolution.Height)
                {
                    this.CommonResolutions.value = i;
                }
            }
            this.CommonResolutions.RefreshShownValue();
        }

        public void OnExitButtonClicked()
        {
#if TRAIL
            Trail.SDK.ExitGame();
#else
            Application.Quit();
#endif
            // Trail SDK will exit application when Unity is closing the game.
        }

        public void OnGameActiveStatusChanged(bool isGameActive)
        {
            this.GameActive.text = string.Format("Game active: {0}", isGameActive);
        }


        #region InsightsKit

        public void OnReportSceneChangedButtonClicked()
        {
            var scene = SceneManager.GetActiveScene();
            Debug.LogFormat("Reporting scene changed: id = {0} name = {1}", scene.path, scene.name);
            var result = InsightsKit.ReportSceneChanged(scene);
            if (result == Trail.Result.Ok)
            {
                Debug.Log("Report scene changed succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Report scene changed failed: {0}", result.ToString());
            }
        }

        public void OnReportGameResolutionChangedButtonClicked()
        {
            var rs = Screen.currentResolution;
            Debug.LogFormat(
                "Reporting resolution changed: width = {0} height = {1}",
                rs.width,
                rs.height
            );
            var result = InsightsKit.ReportResolutionChanged(rs);
            if (result == Trail.Result.Ok)
            {
                Debug.Log("Report resolution changed succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Report resolution changed failed: {0}", result.ToString());
            }
        }

        public void OnReportQualityLevelChangedButtonClicked()
        {
            var level = QualitySettings.GetQualityLevel();
            var name = QualitySettings.names[level];
            int ql = (int)Math.Round(100.0f * (float)(level + 1) / QualitySettings.names.Length);
            Debug.LogFormat("Reporting quality level changed: level = {0} name = {1}", ql, name);
            var result = InsightsKit.ReportQualityLevelChanged(ql, name);
            if (result == Trail.Result.Ok)
            {
                Debug.Log("Report quality level changed succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Report quality level changed failed: {0}", result.ToString());
            }
        }

        public void OnReportFirstGameplayEventButtonClicked()
        {
            var result = InsightsKit.ReportFirstGameplayEvent();
            if (result == Trail.Result.Ok)
            {
                Debug.Log("Report first gameplay event succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Report first gameplay event failed: {0}", result.ToString());
            }
        }

        public void OnCrash()
        {
            var msg = "Test error";
            Debug.LogFormat("Crashing: message = {0}", msg);
            var result = SDK.CrashGame(msg);
            if (!result.IsError())
            {
                Debug.Log("Crash game succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Crash game failed: {0}", result.ToString());
            }
        }

        public void OnSendTrailCustomEvent()
        {
            var name = "custom-event";
            var payload = "{\"json\": \"payload\"}";
            Debug.LogFormat("Reporting custom event: name = \"{0}\" payloadJSON = {1}", name, payload);
            var result = InsightsKit.SendCustomEvent(name, payload);
            if (result == Trail.Result.Ok)
            {
                Debug.Log("Send custom event succeeded");
            }
            else
            {
                Debug.LogErrorFormat("Send custom event failed: {0}", result.ToString());
            }
        }

        #endregion


        #region FileKit

        public void OnGetFileSizeButtonClicked()
        {
            int size;
            var result = FileKit.GetFileSize("StreamingAssets/big.file", out size);
            if (result == Trail.Result.Ok)
            {
                Debug.LogFormat("Get file size succeeded: {0} bytes", size);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogFormat("Get file can't be called in Editor");
#else
                Debug.LogFormat("Get file size failed: {0}", result.ToString());
#endif
            }
        }

        public void OnReadFileButtonClicked()
        {
            FileKit.ReadFile("StreamingAssets/big.file", (result, buffer, length) =>
            {
                if (result == Trail.Result.Ok)
                {
                    Debug.LogFormat("Read file succeeded: {0} bytes", length);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogFormat("Read file can't be called in Editor");
#else
                    Debug.LogFormat("Read file failed: {0}", result.ToString());
#endif
                }
            });
        }

        public void OnPreloadFileButtonClicked()
        {
            var result = FileKit.PreloadFile("StreamingAssets/big.file");
            if (result == Trail.Result.Ok)
            {
                Debug.LogFormat("Preload file succeeded");
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogFormat("Preload file can't be called in Editor");
#else
                Debug.LogFormat("Preload file failed: {0}", result.ToString());
#endif
            }
        }

        public void OnSyncCloudStorageButtonClicked()
        {
            FileKit.SyncCloudStorage((result) => {
                if (result == Trail.Result.Ok)
                {
                    Debug.LogFormat("Sync cloud storage succeeded");
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogFormat("Sync cloud storage can't be called in Editor");
#else
                    Debug.LogFormat("Sync cloud storage failed: {0}", result.ToString());
#endif
                }
            });
        }

        public void OnCloudStorageTextUpdated(string text) {
#if !UNITY_EDITOR && UNITY_WEBGL
            var path = FileKit.GetCloudStoragePath();
            if(!System.IO.Directory.Exists(path)) {
                System.IO.Directory.CreateDirectory(path);
            }
            System.IO.File.WriteAllText(string.Format("{0}/{1}", path, "example.txt"), text);
#endif
        }

        #endregion


        #region NotificationsKit

        public void OnRequestNotificationPermissionButtonClicked()
        {
            NotificationsKit.RequestPermission(
                new KeyValueList(2)
                    .Add("tag-a", "value-a")
                    .Add("tag-b", "value-b")
                ,
                (result, granted) =>
                {
                    if (result == Trail.Result.Ok)
                    {
                        if (granted)
                        {
                            Debug.Log("Notifications granted");
                        }
                        else
                        {
                            Debug.Log("Notifications not granted");
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Request permission failed: {0}", result.ToString());
                    }
                }
            );
        }

        public void OnGetNotificationPermissionStatusButtonClicked()
        {
            NotificationsKit.GetPermissionStatus((result, granted) =>
            {
                if (result == Trail.Result.Ok)
                {
                    if (granted)
                    {
                        Debug.Log("Notifications granted");
                    }
                    else
                    {
                        Debug.Log("Notifications not granted");
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Request permission failed: {0}", result.ToString());
                }
            });
        }

        #endregion


        #region PartyKit

        public void OnShowInviteLinkButtonClicked()
        {
            var r = PartyKit.ShowInviteLink();

            if (r == Trail.Result.Ok)
            {
                Debug.Log("Show invite link succeeded");
            }
            else
            {
                Debug.LogWarningFormat("Show invite link failed: {0}", r);
            }
        }

        public void OnUpdateLandingPageButtonClicked()
        {
            var landingPageInfoFields = new PartyKit.LandingPageInfoField[]
            {
                new PartyKit.LandingPageInfoField() { ID = "text1", Label = "Label 1", Value = "test text1" },
                new PartyKit.LandingPageInfoField() { ID = "text2", Label = "Label 2", Value = "test text2" },
                new PartyKit.LandingPageInfoField() { ID = "partySize", Label = "", Value = "4" },
                new PartyKit.LandingPageInfoField() { ID = "partyMaxSize", Label = "", Value = "6" }
            };

            var r = PartyKit.UpdateInviteLandingPageInfo(landingPageInfoFields);

            if (r == Trail.Result.Ok)
            {
                Debug.Log("Update landing page info succeeded");
            }
            else
            {
                Debug.LogWarningFormat("Update landing page info failed: {0}", r);
            }
        }

        public void OnUpdatePartyDataButtonClicked()
        {
            var partyDataFields = new PartyKit.PartyDataField[]
            {
                new PartyKit.PartyDataField("serverId", "xyz123"),
                new PartyKit.PartyDataField("serverMotD", "Hello, World")
            };

            var r = PartyKit.UpdatePartyData(partyDataFields);

            if (r == Trail.Result.Ok)
            {
                Debug.Log("Update party data succeeded");
            }
            else
            {
                Debug.LogWarningFormat("Update party data failed: {0}", r);
            }
        }

        public void OnLeavePartyButtonClicked()
        {
            var r = PartyKit.LeaveParty();

            if (r.IsError())
            {
                Debug.LogWarningFormat("Leave party failed: {0}", r);
            }
            else
            {
                Debug.Log("Leave party succeeded");
            }
        }

        public void OnFinalizeInviteLoadingSuccessButtonClicked()
        {
            var r = PartyKit.FinalizeInviteLoading(true);

            if (r.IsError())
            {
                Debug.LogWarningFormat("Finalize invite loading failed: {0}", r);
            }
            else
            {
                Debug.Log("Finalize invite loading succeeded");
            }
        }

        public void OnFinalizeInviteLoadingFailureButtonClicked()
        {
            var r = PartyKit.FinalizeInviteLoading(false);

            if (r.IsError())
            {
                Debug.LogWarningFormat("Finalize invite loading failed: {0}", r);
            }
            else
            {
                Debug.Log("Finalize invite loading succeeded");
            }
        }

        public void OnUpdateInviteLoadingMessageButtonClicked()
        {
            var r = PartyKit.UpdateInviteLoadingMessage(
                    string.Format("loading message #{0}", new System.Random().Next(1, 10000)));

            if (r.IsError())
            {
                Debug.LogWarningFormat("Update invite loading message failed: {0}", r);
            }
            else
            {
                Debug.Log("Update invite loading message succeeded");
            }
        }

        public void OnIsInviteLoadingButtonClicked()
        {
            Debug.Log(string.Format("Is invite loading: {0}", PartyKit.IsInviteLoading()));
        }

        private void OnPartyDataUpdated()
        {
            Debug.Log("Party data updated");

            var data = PartyKit.GetPartyData();

            for (int i = 0; i < data.Count; i++)
            {
                var field = data[i];
                Debug.Log(string.Format("Entry: {0}  -  Key: {1} - Value: {2}", i, field.Key, field.Value));
            }
        }

        #endregion

        #region PaymentsKit

        public void OnRequestPaymentButtonClicked()
        {
            var productID = Trail.UUID.FromString(paymentKitProduct);
            Debug.Assert(productID != null, "Failed to parse productID UUID string");
            PaymentsKit.RequestPayment(
                productID,
               (Trail.Result result, Trail.UUID orderID, Trail.UUID entitlementID) =>
            {
                if (result == Trail.Result.Ok)
                {
                    Debug.LogFormat(
                        "Payment successful:\nOrder ID = {0}\nEntitlement IDs = \n{1}",
                        orderID,
                        entitlementID.ToString()
                    );
                }
                else if (result == Trail.Result.PMKPaymentDeclined)
                {
                    Debug.LogFormat("Payment declined by the user");
                }
                else
                {
                    Debug.LogWarningFormat("Request payment failed: {0}", result);
                }
            });
        }

        public void OnGetProductPriceButtonClicked()
        {
            var productID = Trail.UUID.FromString(paymentKitProduct);
            Debug.Assert(productID != null, "Failed to parse productID UUID string");

            PaymentsKit.GetProductPrice(
                productID,
               (Trail.Result result, Trail.Price price) =>
            {
                if (result == Trail.Result.Ok)
                {
                    Debug.LogFormat("Get product price successful: Price = {0}", price);
                }
                else
                {
                    Debug.LogWarningFormat("Get product price failed: {0}", result.ToString());
                }
            });
        }

        public void OnGetEntitlementsButtonClicked()
        {
            PaymentsKit.GetEntitlements(
               (Trail.Result result, Trail.PaymentsKit.Entitlement[] entitlements) =>
            {
                if (result == Trail.Result.Ok)
                {
                    Debug.LogFormat("Get entitlements successful");
                    foreach (var ent in entitlements) {
                        Debug.LogFormat("Entitlement ID: {0}, Product ID: {1}",
                            ent.EntitlementID, ent.ProductID);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Get entitlements failed: {0}", result.ToString());
                }
            });
        }

        #endregion


        #region PerformanceKit

        public void OnGameResolutionSelected()
        {
            Debug.LogFormat("Selected game resolution {0}", this.CommonResolutions.value);
            if (this.CommonResolutions.value < 0 || this.commonResolutionsData == null)
            {
                return;
            }

            var resolution = this.commonResolutionsData[this.CommonResolutions.value];
            Debug.LogFormat("Setting game resolution to {0}", resolution.ToString());
            PerformanceKit.SetResolution(resolution, false, false);
            this.Resolution.text = string.Format("Resolution: {0}", resolution);
        }

        #endregion
    }
}
