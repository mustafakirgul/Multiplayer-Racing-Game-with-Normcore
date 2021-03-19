using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Trail
{
    public class ReportWindow : EditorWindow
    {
        private Texture2D criticalIcon;
        private Texture2D hiddenCriticalIcon;
        private Texture2D checkmarkIcon;
        private Texture2D loadingIcon;

        private GUIStyle headerStyle;
        private GUIStyle normalStyle;
        private GUIStyle hiddenStyle;

        private bool showHidden = true;
        private Vector2 scroll;
        private bool resourcesLoaded = false;
        private int index = 0;

        public static string AssetPath
        {
            get 
            { 
                return TrailEditor.GetProjectRelativePath(string.Format("{0}/Editor/Report/Content", TrailEditor.GetTrailDirectory()));
            }
        }

        [MenuItem("Window/Trail/Report Window")]
        public static void Open()
        {
            var w = GetWindow<ReportWindow>("Report Window", true);
        }

        private void OnEnable()
        {
            resourcesLoaded = false;
        }

        void LoadResources()
        {
            string assetPath = AssetPath + "/";
            criticalIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "critical.png");
            hiddenCriticalIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "hiddencritical.png");
            checkmarkIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath + "checkmark.png");
            loadingIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(TrailSDKEditorWindow.AssetPath + "/" + (EditorGUIUtility.isProSkin ? "spinnerdarkmode.png" : "spinner.png"));

            if (EditorStyles.label == null)
            {
                return;
            }
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 18;
            headerStyle.stretchWidth = true;
            headerStyle.stretchHeight = true;

            normalStyle = new GUIStyle(EditorStyles.label);
            normalStyle.clipping = TextClipping.Overflow;
            normalStyle.stretchWidth = true;
            normalStyle.stretchHeight = true;

            hiddenStyle = new GUIStyle(normalStyle);
            var color = hiddenStyle.normal.textColor;
            color.a = 0.5f;
            hiddenStyle.normal.textColor = color;
            resourcesLoaded = true;
        }

        private void OnFocus()
        {
            // To make sure update is not running multiple times a frame.
            EditorApplication.update -= OnUpdate;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private void OnLostFocus()
        {
            EditorApplication.update -= OnUpdate;
        }

        void OnUpdate()
        {

            // Check for report updates.
            var r = Report.GetReport(index);
            if (r.State == ReportState.Unknown)
            {
                r.Update();
            }
            index = (index + 1) % Report.ReportsCount;
        }

        public static int RunFullReport()
        {
            var count = Report.ReportsCount;
            int required = 0;
            try
            {
                EditorUtility.DisplayProgressBar("Report", "Running reports...", 0f);
                for (int i = 0; i < count; i++)
                {
                    var r = Report.GetReport(i);
                    EditorUtility.DisplayProgressBar("Report", string.Format("{0}\n{1}", r.Name, r.Description), 0f);
                    var state = r.DoStateCheck();
                    if (state.HasFlag(ReportState.Required))
                    {
                        required++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return required;
        }

        private void OnGUI()
        {
            if (resourcesLoaded == false)
            {
                LoadResources();
            }
            var area = new Rect(20, 5, position.size.x - 20, position.size.y - 10);
            GUILayout.BeginArea(area);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Refresh report", "Force a refresh on all reports, this might take some time"), GUILayout.Height(24), GUILayout.Width(100f)))
            {
                for (int i = 0, length = Report.ReportsCount; i < length; i++)
                {
                    Report.GetReport(i).Refresh();
                }
                index = 0;
            }
            GUILayout.FlexibleSpace();
            showHidden = !EditorGUILayout.ToggleLeft(new GUIContent("Hide fixed issues", "This will prevent the list from showing cases that already are completed"), !showHidden, GUILayout.Width(120f));
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Hover over an issue for more information.");
            EditorGUILayout.Space();

            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < 32; i++)
            {
                var cat = (ReportCategory)(1 << i);
                if (System.Enum.IsDefined(typeof(ReportCategory), cat))
                {
                    DrawCategory(cat);
                }
            }

            DrawCategory(ReportCategory.None);
            //  DrawCategory(ReportCategory.All);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawCategory(ReportCategory category)
        {
            var first = true;
            if (category == ReportCategory.All)
            {
                for (int i = 0, length = Report.ReportsCount; i < length; i++)
                {
                    var r = Report.GetReport(i);
                    if (showHidden || !r.IsHidden)
                    {
                        if (first)
                        {
                            first = false;
                            DrawCategoryHeader(category);
                        }
                        DrawReport(r);
                    }
                }
            }
            else if (category == ReportCategory.None)
            {
                for (int i = 0, length = Report.ReportsCount; i < length; i++)
                {
                    var r = Report.GetReport(i);
                    if (r.Category == ReportCategory.None)
                    {
                        if (showHidden || !r.IsHidden)
                        {
                            if (first)
                            {
                                first = false;
                                DrawCategoryHeader(category);
                            }
                            DrawReport(r);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0, length = Report.ReportsCount; i < length; i++)
                {
                    var r = Report.GetReport(i);
                    if (r.Category.HasFlag(category))
                    {
                        if (showHidden || !r.IsHidden)
                        {
                            if (first)
                            {
                                first = false;
                                DrawCategoryHeader(category);
                            }
                            DrawReport(r);
                        }
                    }
                }
            }
        }

        void DrawCategoryHeader(ReportCategory category)
        {
            GUILayout.Space(16f);
            EditorGUILayout.LabelField(PascalSplit(category.ToString()), headerStyle, GUILayout.Height(24f));
            GUILayout.Space(6f);
        }

        void DrawReport(Report report)
        {
            EditorGUILayout.BeginHorizontal();

            var icon = GUILayoutUtility.GetRect(16, 16);
            icon.y += 4;

            switch (report.State)
            {
                case ReportState.Unknown: // Draw spinner
                    DrawLoadingIcon(new Rect(icon.position, new Vector2(13, 13)), loadingIcon, 4, 4, 1f);
                    Repaint();
                    break;
                case ReportState.Hidden:
                    GUI.DrawTexture(icon, checkmarkIcon);
                    break;
                case ReportState.Required:
                    GUI.DrawTexture(icon, criticalIcon);
                    break;
                case ReportState.Required | ReportState.Hidden:
                    GUI.DrawTexture(icon, hiddenCriticalIcon);
                    break;
            }

            GUILayout.Label(report.Content, report.IsHidden ? hiddenStyle : normalStyle, GUILayout.Height(SingleReportLineHeight));

            GUILayout.FlexibleSpace();
            if (report.RequiresFix)
            {
                var actions = report.Actions;
                for (int i = 0; i < actions.Length; i++)
                {
                    if (GUILayout.Button(actions[i].Content, GUILayout.Height(SingleReportLineHeight), GUILayout.Width(50f)))
                    {
                        report.UseAction(i);
                    }
                }
            }
            if (report.HasHttpReference)
            {
                if (GUILayout.Button(new GUIContent("?", report.HttpReference), GUILayout.Width(20), GUILayout.Height(SingleReportLineHeight)))
                {
                    Application.OpenURL(report.HttpReference);
                }
            }
            else
            {
                GUILayout.Space(23f);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static string PascalSplit(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(System.Text.RegularExpressions.Regex.Replace(input, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        private static float SingleReportLineHeight = 20f;

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
}
