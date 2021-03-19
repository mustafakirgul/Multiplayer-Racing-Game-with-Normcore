using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Trail
{
#if CSHARP_7_3_OR_NEWER
    public class TrailSDKEditorWindow : EditorWindow
    {

        #region Variables

        private const string MANAGE_DEV_AREA_URL = "https://manage.trail.gg/"; // 0 = Environment
        private const string MANAGE_DEV_AREA_URL_GAME = "https://manage.trail.gg/g/{0}/builds"; // 0 = GameId
        private const string RUN_BUILD_URL = "https://manage.trail.gg/run/{0}"; // 0 = Room Id
        private const string EDITOR_COMPANION_URL = "https://manage.trail.gg/dev/{0}"; // 0 = Room Id
        private const string DOCUMENTATION_URL = @"https://docs.trail.gg/docs/getting-started";

        private static string TrailYaml { get { return TrailEditor.TrailYamlPath; } }

        private bool resourcesLoaded = false;
        private Texture2D trailIcon;
        private Texture2D trailFooterTexture;
        private Texture2D attentionIcon;
        private Texture2D buildIcon;
        private Texture2D buildNotFoundIcon;
        private Texture2D checkmarkIcon;
        private Texture2D errorIcon;
        private Texture2D externalLinkIcon;
        private Texture2D gearIcon;
        private Texture2D userIcon;
        private Texture2D warningIcon;
        private Texture2D loadingIcon;
        private Texture2D arrowIcon;

        private Texture2D purpleButtonTexture;
        private Texture2D purpleButtonTexture2x;
        private Texture2D trailPurpleTexture;
        private Texture2D trailFooterPurpleTexture;
        private Texture2D grayTexture;
        private Texture2D errorBackgroundTexture;
        private Texture2D uploadBuildBackgroundTexture;

        private Color32 trailColor;
        private Color32 trailFooterColor;

        private GUIStyle headerTrailSDKStyle;
        private GUIStyle headerVersionStyle;
        private GUIStyle footerLogoutLabelStyle;
        private GUIStyle gameSelectStyle;
        private GUIStyle uploadBuildStyle;
        private GUIStyle reportLabelStyle;
        private GUIStyle buildButtonStyle;
        private GUIStyle buildButtonNormalStyle;
        private GUIStyle loginButtonStyle;
        private GUIStyle buildPathStyle;

        private GUIStyle errorMessageStyle;
        private GUIStyle errorDescriptionStyle;

        private bool isPro = false;
        private string mail = "";
        private string password = "";

        private static Cache cache;
        private Vector2 mainScroll;

        private Error error = null;
        private CLI.LoginRequest loginRequest = null;
        private CLI.UploadBuildRequest uploadBuildRequest = null;
        private CLI.RunBuildRequest runBuildRequest = null;
        private Version currentSDKVersion;

        #endregion

        #region Properties

        public bool IsLoggedIn { get { return cache.IsLoggedIn; } }
        public bool HasPatchAvailable { get { return false; } }

        public static string AssetPath
        {
            get 
            { 
                return TrailEditor.GetProjectRelativePath(string.Format("{0}/Editor/SDKEditor", TrailEditor.GetTrailDirectory()));
            }
        }

        #endregion

        #region Setup

        [MenuItem("Window/Trail/SDK")]
        public static void Open()
        {
            var w = GetWindow<TrailSDKEditorWindow>("Trail SDK", true);
            w.Show();
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath + "/traillogoborder.png");
            w.titleContent = new GUIContent("Trail SDK", icon);
        }

        private void OnEnable()
        {
            resourcesLoaded = false;
            string versionPath = TrailEditor.GetTrailDirectory() + "/VERSION";
            if (System.IO.File.Exists(versionPath))
            {
                Version.TryParse(System.IO.File.ReadAllText(versionPath), out currentSDKVersion);
            }
            mail = EditorPrefs.GetString("trail_mail", "");
            cache.IsLoggedIn = EditorPrefs.GetBool("trail_loggedin", false);

            var loginRequest = CLI.GetLoginStatus();
            loginRequest.AddCallback((success, loggedIn) =>
            {
                cache.IsLoggedIn = loggedIn;
                if (success && loggedIn)
                {
                    ListGames();
                }
            });
        }

        #endregion

        private void OnGUI()
        {
            var pro = EditorGUIUtility.isProSkin;
            if (isPro != pro || !resourcesLoaded || trailFooterPurpleTexture == null)
            {
                LoadResources();
            }

            var area = new Rect(0, 0, position.width, position.height);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                DrawHeaderError(new Rect(0, area.y, area.width, 30), "Network Error.", "Please check your connection.");
                area.y += 30;
                area.height -= 30;
            }
#if !TRAIL
            if (IsLoggedIn)
            {
                DrawHeaderError(new Rect(0, area.y, area.width, 30), "", "WebGL build target required.");
                area.y += 30;
                area.height -= 30;
            }
#endif

            if (error != null)
            {
                DrawHeaderError(new Rect(0, area.y, area.width, 30), error.title, error.description);
                area.y += 30;
                area.height -= 30;
            }

            if (!IsLoggedIn)
            {
                DrawLogin(new Rect(area.x, area.y, area.width, area.height - 50));
            }
            else
            {
                DrawMain(new Rect(area.x, area.y, area.width, area.height - 50));
            }

            DrawFooter(new Rect(0, area.y + area.height - 50, area.width, 50));
        }

        #region Caches

        class Error
        {
            public string title;
            public string description;

            public Error() { }
            public Error(string title, string description)
            {
                this.title = title;
                this.description = description;
            }
        }

        [Serializable]
        public struct Cache
        {
            public string user;
            private bool isLoggedIn;
            public string selectedGameId;
            public CLI.Game[] games;
            public string[] gamesTitles;

            public bool IsLoggedIn
            {
                get
                {
                    return isLoggedIn;
                }
                set
                {
                    isLoggedIn = value;
                    EditorPrefs.SetBool("trail_loggedin", value);
                }
            }

            public int SelectedGameIndex
            {
                get
                {
                    int index = -1;
                    if (games == null || games.Length == 0)
                    {
                        return -1;
                    }
                    for (int i = 0; i < games.Length; i++)
                    {
                        if (string.Equals(games[i].Id, selectedGameId))
                        {
                            index = i;
                        }
                    }
                    return index;
                }
                set
                {
                    if (games != null && value >= 0 && value < games.Length)
                    {
                        var id = games[value].Id;
                        if (id != selectedGameId)
                        {
                            selectedGameId = id;
                            CLI.InitializeGame(selectedGameId, Application.dataPath.Replace("/Assets", ""));
                            Debug.Log("[TRAIL] - New gameId selected: " + selectedGameId);
                        }
                    }
                }
            }

            public void FindCurrentGameId()
            {
                if (System.IO.File.Exists(TrailYaml))
                {
                    var text = System.IO.File.ReadAllText(TrailYaml);
                    selectedGameId = text.Remove(0, 9).Trim();
                }
            }

            public void Populate(CLI.Game[] games)
            {
                this.games = games;
                if (games == null)
                {
                    return;
                }
                this.gamesTitles = games
                    .Select(x => x.Title)
                    .ToArray();
                FindCurrentGameId();
            }
        }

        [Serializable]
        public struct Version
        {
            public int Major;
            public int Minor;
            public int Patch;

            public Version(int major, int minor, int patch)
            {
                this.Major = major;
                this.Minor = minor;
                this.Patch = patch;
            }

            #region Overrides

            public override string ToString()
            {
                return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Version))
                {
                    return false;
                }
                var other = (Version)obj;


                return
                    this.Major == other.Major &&
                    this.Minor == other.Minor &&
                    this.Patch == other.Patch;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            #endregion

            #region Operators

            public static bool operator >(Version a, Version b)
            {
                return a.Major > b.Major ||
                    (a.Major == b.Major &&
                    (a.Minor > b.Minor ||
                    (a.Minor == b.Minor && a.Patch > b.Patch)));
            }

            public static bool operator <(Version a, Version b)
            {
                return a.Major < b.Major ||
                    (a.Major == b.Major &&
                    (a.Minor < b.Minor ||
                    (a.Minor == b.Minor && a.Patch < b.Patch)));
            }

            #endregion

            #region Parse

            public static Version Parse(string text)
            {
                var splitText = text.Split('.');
                if (splitText.Length == 3)
                {
                    Version version = new Version();
                    version.Major = int.Parse(splitText[0]);
                    version.Minor = int.Parse(splitText[1]);
                    version.Patch = int.Parse(splitText[2]);
                    return version;
                }
                else
                {
                    throw new Exception(string.Format("Version failed to parse text '{0}'", text));
                }
            }

            public static bool TryParse(string text, out Version version)
            {
                var splitText = text.Split('.');
                if (splitText.Length == 3)
                {
                    var v = new Version();
                    bool success =
                        int.TryParse(splitText[0], out v.Major) &&
                        int.TryParse(splitText[1], out v.Minor) &&
                        int.TryParse(splitText[2], out v.Patch);
                    version = v;
                    return success;
                }
                else
                {
                    version = default(Version);
                    return false;
                }
            }

            #endregion
        }

        #endregion

        #region Resource loading

        void LoadResources()
        {
            trailColor = new Color32(110, 50, 244, 255);
            trailFooterColor = new Color32(43, 20, 92, 255);

            string assetPath = AssetPath + "/";

            isPro = EditorGUIUtility.isProSkin;
            trailIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "traillogo.png");
            trailFooterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "trailFooter.png");
            attentionIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "attention.png");
            buildIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "build.png");
            buildNotFoundIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "buildnotfound.png");
            checkmarkIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "checkmark.png");
            errorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "error.png");
            externalLinkIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + (isPro ? "externallinkdarkmode.png" : "externallink.png"));
            gearIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "gear.png");
            userIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "user.png");
            warningIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "warning.png");
            loadingIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + (isPro ? "spinnerdarkmode.png" : "spinner.png"));
            arrowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "arrow.png");
            purpleButtonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "button.png");
            purpleButtonTexture2x = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "button2x.png");

            Texture2D[] purpleButtonScaledArray = new Texture2D[]
            {
                purpleButtonTexture,
                purpleButtonTexture2x
            };

            if (grayTexture == null)
            {
                grayTexture = new Texture2D(1, 1);
            }
            grayTexture.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.4f, 0.4f, 0.4f));
            grayTexture.Apply();

            if (trailPurpleTexture == null)
            {
                trailPurpleTexture = new Texture2D(1, 1);
            }
            trailPurpleTexture.SetPixel(0, 0, trailColor);
            trailPurpleTexture.Apply();

            if (trailFooterPurpleTexture == null)
            {
                trailFooterPurpleTexture = new Texture2D(1, 1);
            }
            trailFooterPurpleTexture.SetPixel(0, 0, trailFooterColor);
            trailFooterPurpleTexture.Apply();

            if (errorBackgroundTexture == null)
            {
                errorBackgroundTexture = new Texture2D(1, 1);
            }
            errorBackgroundTexture.SetPixel(0, 0, new Color(0.95f, 0.8f, 0.8f));
            errorBackgroundTexture.Apply();

            if (uploadBuildBackgroundTexture == null)
            {
                uploadBuildBackgroundTexture = new Texture2D(1, 1);
            }
            uploadBuildBackgroundTexture.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f, 0.65f) : new Color(0.4f, 0.4f, 0.4f, 0.65f));
            uploadBuildBackgroundTexture.Apply();

            if (EditorStyles.boldLabel == null)
            {
                return;
            }
            headerTrailSDKStyle = new GUIStyle(GUI.skin.label);
            headerTrailSDKStyle.fontSize = 20;
            headerTrailSDKStyle.fontStyle = FontStyle.Bold;
            headerTrailSDKStyle.normal.textColor = isPro ? new Color32(235, 235, 235, 255) : new Color32(36, 0, 67, 255);

            headerVersionStyle = new GUIStyle(GUI.skin.label);
            headerVersionStyle.padding = new RectOffset(-4, 0, 4, 0);
            headerVersionStyle.fontSize = 16;
            headerVersionStyle.clipping = TextClipping.Overflow;
            headerVersionStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f, 0.65f) : new Color(0.4f, 0.4f, 0.4f, 0.65f);

            errorMessageStyle = new GUIStyle(EditorStyles.boldLabel);
            errorMessageStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            errorMessageStyle.fontStyle = FontStyle.Bold;
            errorDescriptionStyle = new GUIStyle(errorMessageStyle);
            errorDescriptionStyle.fontStyle = FontStyle.Normal;

            gameSelectStyle = new GUIStyle(EditorStyles.label);
            gameSelectStyle.stretchWidth = false;

            uploadBuildStyle = new GUIStyle(EditorStyles.label);
            var uploadColor = uploadBuildStyle.normal.textColor;
            uploadColor.a = 0.8f;
            uploadBuildStyle.normal.textColor = uploadColor;
            uploadBuildStyle.fontSize = 11;
            uploadBuildStyle.wordWrap = true;
            uploadBuildStyle.alignment = TextAnchor.MiddleLeft;

            reportLabelStyle = new GUIStyle(EditorStyles.label);
            reportLabelStyle.alignment = TextAnchor.MiddleLeft;
            reportLabelStyle.fixedWidth = 0;
            reportLabelStyle.padding = new RectOffset(0, 0, 5, 4);

            buildPathStyle = new GUIStyle(EditorStyles.boldLabel);
            buildPathStyle.alignment = TextAnchor.UpperLeft;
            buildPathStyle.padding = new RectOffset(2, 2, 2, 2);
            buildPathStyle.margin = new RectOffset(0, 0, 0, 0);

            buildButtonStyle = new GUIStyle(GUI.skin.button);
            buildButtonStyle.normal.textColor = Color.white;
            buildButtonStyle.normal.background = purpleButtonTexture;
            buildButtonStyle.normal.scaledBackgrounds = purpleButtonScaledArray;
            buildButtonStyle.onNormal.textColor = Color.white * 0.9f;
            buildButtonStyle.onNormal.background = purpleButtonTexture;
            buildButtonStyle.onNormal.scaledBackgrounds = purpleButtonScaledArray;

            buildButtonStyle.hover.textColor = Color.white * 0.9f;
            buildButtonStyle.hover.background = purpleButtonTexture;
            buildButtonStyle.hover.scaledBackgrounds = purpleButtonScaledArray;
            buildButtonStyle.onHover.textColor = Color.white * 0.9f;
            buildButtonStyle.onHover.background = purpleButtonTexture;
            buildButtonStyle.onHover.scaledBackgrounds = purpleButtonScaledArray;

            buildButtonStyle.active.textColor = Color.white * 0.8f;
            buildButtonStyle.active.background = purpleButtonTexture;
            buildButtonStyle.active.scaledBackgrounds = purpleButtonScaledArray;
            buildButtonStyle.onActive.textColor = Color.white * 0.8f;
            buildButtonStyle.onActive.background = purpleButtonTexture;
            buildButtonStyle.onActive.scaledBackgrounds = purpleButtonScaledArray;

            buildButtonStyle.focused.textColor = Color.white * 0.8f;
            buildButtonStyle.focused.background = purpleButtonTexture;
            buildButtonStyle.focused.scaledBackgrounds = purpleButtonScaledArray;
            buildButtonStyle.onFocused.textColor = Color.white * 0.8f;
            buildButtonStyle.onFocused.background = purpleButtonTexture;
            buildButtonStyle.onFocused.scaledBackgrounds = purpleButtonScaledArray;

            buildButtonStyle.fontStyle = FontStyle.Bold;
            buildButtonStyle.fontSize = 13;
            buildButtonStyle.border = new RectOffset(4, 4, 4, 4);

            buildButtonNormalStyle = new GUIStyle(GUI.skin.button);
            buildButtonNormalStyle.fontStyle = FontStyle.Bold;
            buildButtonNormalStyle.fontSize = 13;

            loginButtonStyle = new GUIStyle(GUI.skin.button);
            loginButtonStyle.normal.textColor = Color.white;
            loginButtonStyle.normal.background = purpleButtonTexture;
            loginButtonStyle.fontStyle = FontStyle.Bold;
            loginButtonStyle.hover.textColor = Color.white * 0.9f;
            loginButtonStyle.hover.background = purpleButtonTexture;
            loginButtonStyle.active.textColor = Color.white * 0.8f;
            loginButtonStyle.active.background = purpleButtonTexture;
            loginButtonStyle.border = new RectOffset(4, 6, 4, 6);

            resourcesLoaded = true;
        }

        private void OnDestroy()
        {
            if (trailPurpleTexture)
            {
                DestroyImmediate(trailPurpleTexture);
            }
            if (trailFooterPurpleTexture)
            {
                DestroyImmediate(trailFooterPurpleTexture);
            }
            if (grayTexture)
            {
                DestroyImmediate(grayTexture);
            }
            if (errorBackgroundTexture)
            {
                DestroyImmediate(errorBackgroundTexture);
            }
            if (uploadBuildBackgroundTexture)
            {
                DestroyImmediate(uploadBuildBackgroundTexture);
            }
            resourcesLoaded = false;
        }

        #endregion

        #region Error

        void DrawHeaderError(Rect area, string error, string description)
        {

            GUI.DrawTexture(area, errorBackgroundTexture);
            GUILayout.BeginArea(area);
            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            var iconArea = GUILayoutUtility.GetRect(12, 12);
            iconArea.y += 4;
            GUI.DrawTexture(iconArea, errorIcon);
            GUILayout.Label(error, errorMessageStyle);
            GUILayout.Label(description, errorDescriptionStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        #endregion

        #region Shared

        void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            // Header + Version
            GUILayout.Label("Trail SDK", headerTrailSDKStyle);
            GUILayout.Label("v" + currentSDKVersion.ToString(), headerVersionStyle, GUILayout.Width(44f));

            // Spacing
            GUILayout.FlexibleSpace();

            //TODO: Check for patches?
            // Patch Button
            if (HasPatchAvailable)
            {
                GUILayout.Button("PATCH AVAILABLE", GUILayout.Width(160f), GUILayout.Height(24));
                var buttonArea = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(buttonArea.x + 6, buttonArea.y + 6, 12, 12), attentionIcon);
                GUI.DrawTexture(new Rect(buttonArea.x + buttonArea.width - 18, buttonArea.y + 6, 12, 12), externalLinkIcon);
            }

            GUILayout.EndHorizontal();
        }

        void DrawFooter(Rect area)
        {
            var logoPosition = new Rect(15, area.y + 13, 70, 24);
            var labelPosition = new Rect(43, area.y + 13, 60, 24);

            GUI.DrawTexture(area, trailFooterPurpleTexture);
            GUI.DrawTexture(logoPosition, trailFooterTexture);

            if (IsLoggedIn)
            {
                if (true || footerLogoutLabelStyle == null)
                {
                    footerLogoutLabelStyle = new GUIStyle(EditorStyles.label);
                    footerLogoutLabelStyle.normal.textColor = Color.white;
                    footerLogoutLabelStyle.padding = new RectOffset(0, 0, 2, 0);
                }

                var logoutArea = new Rect(area.x + 100, area.y + 15, area.width - 115f, area.height - 30f);
                GUILayout.BeginArea(logoutArea);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var iconRect = GUILayoutUtility.GetRect(12, 12);
                iconRect.y += 6f;
                GUI.DrawTexture(iconRect, userIcon);

                GUILayout.Label(mail, footerLogoutLabelStyle);
                GUILayout.Space(4f);
                if (GUILayout.Button("Log out"))
                {
                    CLI.LogOut().AddCallback((success) =>
                    {
                        cache.IsLoggedIn = !success;
                    });
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        #endregion

        #region Main

        void DrawMain(Rect area)
        {
            area = new Rect(area.x + 10, area.y + 8, area.width - 17, area.height - 20);
            GUILayout.BeginArea(area);
            mainScroll = GUILayout.BeginScrollView(mainScroll);
            DrawHeader();
            GUILayout.Space(10f);
            DrawGameSelect();
            GUILayout.Space(20f);
            DrawBuildButtons();
            GUILayout.Space(16f);
            DrawBuildReportWarnings();
            GUILayout.Space(16f);
            DrawLatestBuild();
            GUILayout.Space(24f);
            DrawMainServices();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawGameSelect()
        {
            var curEv = Event.current;

            GUILayout.BeginHorizontal();

            // Game Selector
            GUILayout.Label("Game:", GUILayout.Width(40));

            if (cache.games != null)
            {
                var selected = cache.SelectedGameIndex;
                var currentTitle = selected >= 0 ? cache.gamesTitles[selected] : "Not selected";
                var contentSize = gameSelectStyle.CalcSize(new GUIContent(currentTitle)).x;
                var size = Mathf.Min(contentSize, Mathf.Max(50f, position.size.x - 200f));
                cache.SelectedGameIndex = EditorGUILayout.Popup(cache.SelectedGameIndex, cache.gamesTitles, gameSelectStyle, GUILayout.Width(size));

                var lineArea = GUILayoutUtility.GetLastRect();
                lineArea.y += lineArea.height - 2;
                lineArea.height = 1;
                GUI.DrawTexture(lineArea, grayTexture);

                var textureArea = GUILayoutUtility.GetRect(12, 12);
                textureArea.y += 7;
                GUI.DrawTexture(textureArea, gearIcon);
                var gameSelector = GUILayoutUtility.GetLastRect();
                gameSelector.width += textureArea.width + 4;
                if (curEv.type == EventType.MouseDown && curEv.button == 0 && gameSelector.Contains(curEv.mousePosition))
                {
                    curEv.Use();

                }
            }

            GUILayout.FlexibleSpace();

            // Game Manager
            GUILayout.Label("Game Manager", GUILayout.Width(110));
            var gameManagerArea = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(new Rect(gameManagerArea.x + gameManagerArea.width - 18, gameManagerArea.y + 2, 12, 12), externalLinkIcon);
            if (curEv.type == EventType.MouseDown && curEv.button == 0 && gameManagerArea.Contains(curEv.mousePosition))
            {
                curEv.Use();
                if (cache.SelectedGameIndex == -1)
                {
                    Application.OpenURL(MANAGE_DEV_AREA_URL);
                }
                else
                {
                    Application.OpenURL(string.Format(MANAGE_DEV_AREA_URL_GAME, cache.selectedGameId));
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawBuildButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build", buildButtonStyle, GUILayout.MinWidth(60f), GUILayout.Height(38f)))
            {
                EditorApplication.delayCall += () => Build();
            }
            if (GUILayout.Button("Build & Run", buildButtonNormalStyle, GUILayout.MinWidth(96f), GUILayout.Height(40f)))
            {
                EditorApplication.delayCall += () => BuildAndRun();
            }
            if (GUILayout.Button("Build & Upload", buildButtonNormalStyle, GUILayout.MinWidth(120f), GUILayout.Height(40f)))
            {
                EditorApplication.delayCall += () => BuildAndUpload();
            }
            GUILayout.EndHorizontal();
        }

        void DrawBuildReportWarnings()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open report window", GUILayout.Height(24)))
            {
                ReportWindow.Open();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawLatestBuild()
        {
            var hasBuild = HasBuild();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("LATEST BUILD");
            GUILayout.FlexibleSpace();
            if (hasBuild)
            {
                var time = BuildTime();
                GUILayout.Label(string.Format("{0} {1}", time.ToShortDateString(), time.ToShortTimeString()), uploadBuildStyle); // Date
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            var iconArea = GUILayoutUtility.GetRect(16, 14);
            iconArea.width -= 4;
            iconArea.x += 4;
            iconArea.y += 2;
            GUI.DrawTexture(iconArea, hasBuild ? buildIcon : buildNotFoundIcon);
            if (hasBuild)
            {
                GUILayout.Label(TrailBuild.BuildLocationRelativePath, buildPathStyle);
                var pathArea = GUILayoutUtility.GetLastRect();
                var ev = Event.current;
                if (ev.type == EventType.MouseDown && ev.button == 0 && pathArea.Contains(ev.mousePosition) && TrailBuild.HasBuild() && System.IO.Directory.Exists(TrailBuild.BuildLocationPath))
                {
                    try
                    {
                        EditorApplication.delayCall += () => System.Diagnostics.Process.Start(TrailBuild.BuildLocationPath);
                    }
                    catch (System.Exception)
                    {
                    }
                }

                GUILayout.FlexibleSpace();

                if (runBuildRequest != null)
                {
                    if (GUILayout.Button("Stop Running"))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            runBuildRequest.Cancel();
                            runBuildRequest = null;
                        };
                    }
                }
                else if (uploadBuildRequest == null || uploadBuildRequest.IsComplete)
                {
                    if (GUILayout.Button("Run"))
                    {
                        EditorApplication.delayCall += RunLatestBuild;
                    }
                    if (GUILayout.Button("Upload"))
                    {
                        EditorApplication.delayCall += () => { UploadLatestBuild(); Repaint(); };
                    }
                }
                else
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        EditorApplication.delayCall += uploadBuildRequest.Cancel;
                    }
                }
            }
            else
            {
                GUILayout.Label("No Trail build found", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();

            if (runBuildRequest != null)
            {
                GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4f);
                var icon = GUILayoutUtility.GetRect(14f, 14f);
                icon.y += 2f;
                // Draw Spinner

                DrawLoadingIcon(new Rect(icon.position, new Vector2(13, 13)), loadingIcon, 4, 4, 1f);

                // RUnning build
                GUILayout.Label("Running build...", uploadBuildStyle);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open in browser"))
                {
                    Application.OpenURL(string.Format(RUN_BUILD_URL, CLI.DevServerRoomId));
                }

                GUILayout.EndHorizontal();

                if (runBuildRequest.IsComplete || runBuildRequest.IsCanceled)
                {
                    runBuildRequest = null;
                }
                else
                {
                    Repaint();
                }
            }
            if (uploadBuildRequest != null)
            {
                //Uploading Requests
                GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4f);
                if (uploadBuildRequest.IsComplete)
                {
                    var icon = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f));
                    icon.y += 2f;
                    if (!string.IsNullOrEmpty(uploadBuildRequest.ErrorMessage))
                    {
                        GUI.DrawTexture(icon, errorIcon);
                        GUILayout.Label("Failed to upload to Trail - " + uploadBuildRequest.ErrorMessage, uploadBuildStyle);
                    }
                    else
                    {
                        if (!TrailBuild.IsUploaded)
                        {
                            // To clear if new build has been done and upload request still exists.
                            EditorApplication.delayCall += () => uploadBuildRequest = null;
                        }
                        GUI.DrawTexture(icon, checkmarkIcon);
                        GUILayout.Label("Uploaded to Trail", uploadBuildStyle);
                        GUILayout.FlexibleSpace();
                        var time = TrailBuild.UploadTime;
                        GUILayout.Label(string.Format("{0} {1}", time.ToShortDateString(), time.ToShortTimeString()), uploadBuildStyle);
                    }
                }
                else
                {
                    // Currently uploading
                    var icon = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f));
                    icon.y += 2f;
                    DrawLoadingIcon(icon, this.loadingIcon, 4, 4, 1f);
                    GUILayout.Label(uploadBuildRequest.Progress >= 1f ? "Processing..." : "Uploading build...", uploadBuildStyle, GUILayout.Width(100f));
                    // TODO: Implement upload speed.
                    //  GUILayout.Label(string.Format("{0:0.0} Mb/s", uploadBuildRequest.MbPerSecond), uploadBuildStyle, GUILayout.Width(50f));
                    GUILayout.FlexibleSpace();

                    var loadingBarArea = GUILayoutUtility.GetRect(40f, 200f, 4f, 12f);
                    loadingBarArea.height = 4f;
                    loadingBarArea.y += 8f;
                    loadingBarArea.x += 20f;
                    loadingBarArea.width -= 24f;

                    GUI.DrawTexture(loadingBarArea, uploadBuildBackgroundTexture);
                    loadingBarArea.width *= Mathf.Abs(uploadBuildRequest.Progress);
                    GUI.DrawTexture(loadingBarArea, trailPurpleTexture);

                    Repaint();
                }
                GUILayout.EndHorizontal();
            }
            else if (hasBuild && TrailBuild.IsUploaded)
            {
                GUILayout.Space(8f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4f);
                var icon = GUILayoutUtility.GetRect(14f, 14f);
                icon.y += 2f;
                GUI.DrawTexture(icon, checkmarkIcon);
                GUILayout.Label("Uploaded to Trail", uploadBuildStyle);
                GUILayout.FlexibleSpace();
                var time = TrailBuild.UploadTime;
                GUILayout.Label(string.Format("{0} {1}", time.ToShortDateString(), time.ToShortTimeString()), uploadBuildStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            GUILayout.EndVertical();
        }

        void DrawMainServices()
        {
            var running = CLI.DevServerRunning;
            //Companion
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            GUILayout.Label("EDITOR COMPANION");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open", GUILayout.Width(80f)))
            {
                Application.OpenURL(string.Format(EDITOR_COMPANION_URL, CLI.DevServerRoomId));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

#if TRAIL
            var state = CLI.DevServerState;
            switch (state)
            {

                case CLI.ECState.Waiting:
                    var spinnerLocation = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(16));
                    spinnerLocation.y += 4f;
                    spinnerLocation.x += 4f;

                    DrawLoadingIcon(new Rect(spinnerLocation.position, new Vector2(11, 11)), loadingIcon, 4, 4, 1f);
                    if (CLI.DevServerActiveTime > 3d)
                    {
                        GUILayout.Label("Open Editor Companion to initialize SDK", uploadBuildStyle);
                    }
                    else
                    {
                        GUILayout.Label("Initializing SDK", uploadBuildStyle);
                    }
                    Repaint();
                    break;
                case CLI.ECState.Running:
                    var runnerLocation = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(16));
                    runnerLocation.y += 4f;
                    runnerLocation.x += 4f;
                    runnerLocation.width = 12;
                    GUI.DrawTexture(runnerLocation, checkmarkIcon);
                    GUILayout.Label("SDK initialization successful. Running...", uploadBuildStyle);
                    break;

                case CLI.ECState.Idle:
                case CLI.ECState.Loading:
                    GUILayout.Label("Enter Play Mode in Unity to run", uploadBuildStyle);
                    break;

                case CLI.ECState.Disconnected:
                    var disconnectIconLocation = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(16));
                    disconnectIconLocation.y += 4f;
                    disconnectIconLocation.x += 4f;
                    GUI.DrawTexture(new Rect(disconnectIconLocation.position, new Vector2(12, 12)), errorIcon);
                    GUILayout.Label("Disconnected. Re-enter Play Mode to reload Trail SDK.", uploadBuildStyle);
                    break;

                // Error states
                case CLI.ECState.None:
                    GUILayout.Label("Missing state! Contact Trail developer on Discord.", uploadBuildStyle);
                    break;
                case CLI.ECState.Disabled:
                    GUILayout.Label("Dev server disabled. Contact Trail developer on Discord.", uploadBuildStyle);
                    break;
            }
#else
                var location = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(12));
                location.y += 3f;
                location.x += 3f;
                GUI.DrawTexture(location, errorIcon);
                GUILayout.Label(" WebGL build target required", uploadBuildStyle);
#endif

            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.EndVertical();
        }

        #endregion

        #region Login

        void DrawLogin(Rect area)
        {
            area = new Rect(area.x + 10, area.y + 10, area.width - 20, area.height - 20);
            GUILayout.BeginArea(area);
            DrawHeader();
            GUILayout.Space(10f);
            DrawLoginText();
            GUILayout.Space(10f);
            DrawLoginField();
            GUILayout.FlexibleSpace();
            DrawWhatIsThisLink();

            GUILayout.EndArea();
        }

        void DrawLoginText()
        {
            var loginTextStyle = new GUIStyle(EditorStyles.label);
            loginTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f);
            loginTextStyle.wordWrap = true;
            GUILayout.Label("Welcome to the Trail SDK for Unity. Log in with your developer account to get started.", loginTextStyle);
            GUILayout.Space(8);
            GUILayout.Label("If you don't yet have a developer account for Trail, please get in touch with your contact at Trail and we'll set you up.", loginTextStyle);
        }

        void DrawLoginField()
        {
            // Need to check for input before mail/password field is using them
            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Return)
            {
                ev.Use();
                var control = GUI.GetNameOfFocusedControl();
                if (!string.IsNullOrEmpty(control) == control.Equals("Mail"))
                {
                    // Delay call to get around Return being pressed and unfocuses 'Password' field
                    EditorGUI.FocusTextInControl("Password");
                    EditorApplication.delayCall += () => EditorGUI.FocusTextInControl("Password");
                }
                else
                {
                    Login(mail, password);
                }
            }

            GUI.enabled = loginRequest == null || loginRequest.IsComplete;

            // Log in fields
            GUI.SetNextControlName("Mail");
            mail = EditorGUILayout.TextField("Email", mail);
            GUILayout.Space(8f);
            GUI.SetNextControlName("Password");
            password = EditorGUILayout.PasswordField("Password", password);
            GUILayout.Space(8f);

            GUI.enabled = true;

            // Log in button!
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (loginRequest == null || loginRequest.IsComplete)
            {
                if (GUILayout.Button("Log in    ", loginButtonStyle, GUILayout.Width(100f), GUILayout.Height(30)))
                {
                    Login(mail, password);
                }
                var arrowArea = GUILayoutUtility.GetLastRect();
                arrowArea.x += arrowArea.width / 2f + 12f;
                arrowArea.width = 12;
                arrowArea.height = 8;
                arrowArea.y += 12;
                GUI.DrawTexture(arrowArea, arrowIcon);
            }
            else
            {
                if (GUILayout.Button("Cancel", loginButtonStyle, GUILayout.Width(100f), GUILayout.Height(30)))
                {
                    loginRequest.Cancel();
                }

                var location = GUILayoutUtility.GetLastRect();
                location.y += 8f;
                location.x -= 28f;
                DrawLoadingIcon(new Rect(location.position, new Vector2(17, 17)), loadingIcon, 4, 4, 1f);
                Repaint();
            }

            GUILayout.EndHorizontal();
        }

        void DrawWhatIsThisLink()
        {
            GUILayout.Label("Not sure what this is? Read the docs!");
            var docsLink = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(new Rect(docsLink.x + 125f, docsLink.y + docsLink.height - 2, 80f, 1f), grayTexture);
            GUI.DrawTexture(new Rect(docsLink.x + 212f, docsLink.y + 2f, 12, 12), externalLinkIcon);

            var curEvent = Event.current;
            if (curEvent != null && curEvent.type == EventType.MouseDown && curEvent.button == 0 && docsLink.Contains(curEvent.mousePosition))
            {
                curEvent.Use();
                Application.OpenURL(DOCUMENTATION_URL);
            }
        }

        #endregion

        #region Helper Methods

        private void ClearStates()
        {
            error = null;
            if (uploadBuildRequest != null && uploadBuildRequest.IsComplete && string.IsNullOrEmpty(uploadBuildRequest.ErrorMessage))
            {
                uploadBuildRequest = null;
            }
        }

        private void Logout()
        {
            ClearStates();
            cache.IsLoggedIn = false;
            var logoutRequest = CLI.LogOut();
            logoutRequest.AddCallback((success) => cache.IsLoggedIn = !success);
        }

        private void Login(string user, string password)
        {
            ClearStates();
            EditorPrefs.SetString("trail_mail", user);

            if (loginRequest != null && !loginRequest.IsComplete)
            {
                return;
            }

            loginRequest = CLI.LogIn(user, password);
            loginRequest.AddCallback((success, result) =>
            {
                //Success if managed to login...
                if (success)
                {
                    cache.IsLoggedIn = result.Result.Payload != null;
                    ListGames();
                }
                else
                {
                    if(result.ErrorMessage.Contains("connection error:")) {
                        error = new Error("Connection error.", "Could not connect to Trail");
                    }
                    else {
                        error = new Error("Login error.", "Check your credentials and try again");
                    }
                }
                Repaint();
            });

            Repaint();
        }

        private void ListGames()
        {
            ClearStates();
            var listGamesRequest = new CLI.ListGamesRequest();
            listGamesRequest.AddCallback((success, games) =>
            {
                cache.Populate(games);

                // Handle selection of game if no games are selected.
                var selectedIndex = cache.SelectedGameIndex;
                if (selectedIndex == -1 && games != null && games.Length > 0)
                {
                    cache.SelectedGameIndex = 0;
                }
            });
        }

        private string Build()
        {
            ClearStates();
            var report = TrailBuild.Build();
            return report != null && report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? report.summary.outputPath : "";
        }

        private void BuildAndRun()
        {
            ClearStates();
            string result = Build();
            if (!string.IsNullOrEmpty(result))
            {
                RunLatestBuild();
            }
        }

        private bool HasBuild()
        {
            ClearStates();
            return TrailBuild.HasBuild();
        }

        private DateTime BuildTime()
        {
            if (HasBuild())
            {
                return TrailBuild.BuildTime;
            }
            return default(DateTime);
        }

        private void BuildAndUpload()
        {
            ClearStates();
            var path = Build();
            if (!string.IsNullOrEmpty(path))
            {
                UploadLatestBuild();
            }
        }

        private void RunLatestBuild()
        {
            ClearStates();
            var hasBuild = HasBuild();
            if (!hasBuild)
            {
                return;
            }
            runBuildRequest = CLI.RunBuild(TrailBuild.BuildLocationPath);
            runBuildRequest.AddCallback((success, request) =>
            {
                if (!success && !request.IsCanceled && !string.IsNullOrEmpty(request.ErrorMessage))
                {
                    SDK.Log(LogLevel.Error, "Trail CLI", request.ErrorMessage);
                }
            });
        }

        private void UploadLatestBuild()
        {
            ClearStates();
            var hasBuild = HasBuild();
            if (!hasBuild)
            {
                return;
            }
            if (uploadBuildRequest != null && !uploadBuildRequest.IsComplete)
            {
                SDK.Log(LogLevel.Error, "Trail CLI", "Already uploading build, cancel build before attempting to upload a new build!");
                return;
            }
            uploadBuildRequest = TrailBuild.Upload();
            uploadBuildRequest.AddCallback((success, request) =>
            {
                if (success)
                {
                    EditorApplication.delayCall += () =>
                      Application.OpenURL(string.Format(MANAGE_DEV_AREA_URL_GAME, cache.selectedGameId));
                }
            });
        }

        private void OpenCompanion()
        {
            ClearStates();
            EditorUtility.DisplayDialog("Not Implemented", "This functionality is not yet implemented.", "Ok");
        }

        private void ToggleDevServer()
        {
            ClearStates();
            EditorUtility.DisplayDialog("Not Implemented", "This functionality is not yet implemented.", "Ok");
        }

        #endregion

        #region Loading Icon

        public static void DrawLoadingIcon(Rect area, Texture2D icon, int width, int height, float duration)
        {
            var total = width * height;
            var index = ((int)((EditorApplication.timeSinceStartup / (double)duration) * (double)total)) % total;
            Rect uv = new Rect((index % width) / (float)width, 1f - (index / height) / (float)height - 1f / height, 1f / width, 1f / height);
            GUI.DrawTextureWithTexCoords(area, icon, uv);
        }

        #endregion
    }
#else
    public class TrailSDKEditorWindow : EditorWindow
    {
        [MenuItem("Window/Trail/SDK (disabled)")]
        public static void Open()
        {

        }

        [MenuItem("Window/Trail/SDK (disabled)", validate =true)]
        private static bool CanOpen()
        {
            return false;
        }
    }
#endif
}
