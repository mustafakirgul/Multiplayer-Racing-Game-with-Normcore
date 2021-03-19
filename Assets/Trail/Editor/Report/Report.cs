using System.Collections.Generic;
using UnityEngine;

namespace Trail
{
    public delegate ReportState ReportStateCheck();

    /// <summary>
    /// A report to showcase issues and potential fixes to the project to help other users.
    /// </summary>
    public class Report : IComparer<Report>
    {
        #region Constant

        public static string DEFAULT_BUTTON = "Fix";
        public static string HTTP_LINK_GETTING_STARTED = @"https://docs.trail.gg/docs/getting-started";
        public static string HTTP_LINK_UNITY_MANUAL = @"https://docs.unity3d.com/Manual/index.html";

        #endregion

        #region Variables

        public string Name;
        public string Description;
        public ReportCategory Category = ReportCategory.None;
        public string HttpReference;
        public bool Threaded = false;

        private ReportState cachedState = ReportState.Unknown;
        private System.IAsyncResult asyncResult = null;
        private ReportStateCheck stateCheck = null;

        public ReportAction[] Actions;

        // Static
        private static List<Report> reports = new List<Report>();

        #endregion

        #region Properties
        /// <summary>
        /// A content class to be used by unity editor to draw name and description of the report.
        /// </summary>
        public GUIContent Content { get { return new GUIContent(Name, Description); } }
        /// <summary>
        /// Checks whether it has a http reference provided in the report.
        /// </summary>
        public bool HasHttpReference { get { return !string.IsNullOrEmpty(HttpReference); } }
        /// <summary>
        /// Gets the report state, if report state is unknown, it will try to get a new state by running state check once.
        /// </summary>
        public ReportState State
        {
            get
            {
                return cachedState;
            }
        }

        public bool IsUnknown { get { return State == ReportState.Unknown; } }
        /// <summary>
        /// Checks whether report state is hidden or not.
        /// </summary>
        public bool IsHidden { get { return State.HasFlag(ReportState.Hidden); } }
        /// <summary>
        /// Checks wether report state is required or not.
        /// </summary>
        public bool IsRequired { get { return State.HasFlag(ReportState.Required); } }

        public bool RequiresFix { get { return State.HasFlag(ReportState.Recommended) || State.HasFlag(ReportState.Required); } }

        public static int ReportsCount { get { return reports.Count; } }

        #endregion

        #region Constructor

        private Report(string name, string description, ReportCategory category, string httpReference, ReportStateCheck stateCheck, params ReportAction[] actions)
        {
            this.Name = name;
            this.Description = description;
            this.Category = category;
            this.HttpReference = httpReference;
            this.stateCheck = stateCheck;
            this.Actions = actions;

            // Handle a no description case. Should most of time not happen.
            if (string.IsNullOrEmpty(description))
            {
                if (string.IsNullOrEmpty(httpReference))
                {
                    this.Description = "No current information regarding this report is provided.\nIf you need support or help regarding this issue, talk to us on Discord: https://discord.gg/trail";
                }
                else
                {
                    this.Description = string.Format("More information about '{0}' exists at '{1}'\nUse the question mark '?' to enter the web page.", name, httpReference);
                }
            }
        }

        #endregion

        #region Static Create


        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window.
        /// Without description and http reference, basically giving no information to the users.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, ReportCategory category, ReportStateCheck stateCheck, string actionName, ReportCallback callback)
        {
            return Create(name, "", category, "", stateCheck, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window.
        /// Without a http reference. The user won't be able to read more about this issue.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, string description, ReportCategory category, ReportStateCheck stateCheck, string actionName, ReportCallback callback)
        {
            return Create(name, description, category, "", stateCheck, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. This will only have one action to fix the report.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, ReportCategory category, string httpReference, ReportStateCheck stateCheck, string actionName, ReportCallback callback)
        {
            return Create(name, "", category, httpReference, stateCheck, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. This will only have one action to fix the report.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, string description, ReportCategory category, string httpReference, ReportStateCheck stateCheck, string actionName, ReportCallback callback)
        {
            return Create(name, description, category, httpReference, stateCheck, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. Supports multiple actions and no action.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actions">An array of potential actions to be made to hide the report.</param>
        public static Report Create(string name, string description, ReportCategory category, string httpReference, ReportStateCheck stateCheck, params ReportAction[] actions)
        {
            var report = new Report(name, description, category, httpReference, stateCheck, actions);
            reports.Add(report);
            return report;
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window.
        /// Without description and http reference, basically giving no information to the users.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, ReportCategory category, ReportStateCheck stateCheck, string actionName, ReportCallback callback, bool threaded)
        {
            return Create(name, "", category, "", stateCheck, threaded, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window.
        /// Without a http reference. The user won't be able to read more about this issue.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, string description, ReportCategory category, ReportStateCheck stateCheck, string actionName, ReportCallback callback, bool threaded)
        {
            return Create(name, description, category, "", stateCheck, threaded, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. This will only have one action to fix the report.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, ReportCategory category, string httpReference, ReportStateCheck stateCheck, string actionName, ReportCallback callback, bool threaded)
        {
            return Create(name, "", category, httpReference, stateCheck, threaded, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. This will only have one action to fix the report.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actionName">Action name, what the button should say in the report window.</param>
        /// <param name="callback">The callback of the action to fix the report.</param>
        public static Report Create(string name, string description, ReportCategory category, string httpReference, ReportStateCheck stateCheck, string actionName, ReportCallback callback, bool threaded)
        {
            return Create(name, description, category, httpReference, stateCheck, threaded, new ReportAction(actionName, callback));
        }

        /// <summary>
        /// This will create and add a new report to the report system and be visible in the Report Window. Supports multiple actions and no action.
        /// </summary>
        /// <param name="name">Name of the report</param>
        /// <param name="description">Description to why this report exists and/or what Trail is doing behind the scenes.</param>
        /// <param name="category">Category the report should show up in. Category is a mask and can show up in multiple categories.</param>
        /// <param name="httpReference">A reference link to open a web page for the users to read more about the report.</param>
        /// <param name="stateCheck">A function to check whether to show or hide the report from the window.</param>
        /// <param name="actions">An array of potential actions to be made to hide the report.</param>
        public static Report Create(string name, string description, ReportCategory category, string httpReference, ReportStateCheck stateCheck, bool threaded, params ReportAction[] actions)
        {
            var report = new Report(name, description, category, httpReference, stateCheck, actions);
            report.Threaded = threaded;
            reports.Add(report);
            return report;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the cached state and checks if report is still valid.
        /// </summary>
        public void Refresh()
        {
            cachedState = ReportState.Unknown;
        }

        public ReportState DoStateCheck()
        {
            cachedState = stateCheck.Invoke();
            return cachedState;
        }

        public void LoadState()
        {
            if (Threaded)
            {
                if (asyncResult == null)
                {
                    asyncResult = stateCheck.BeginInvoke((a) => { }, this);
                }
            }
            else
            {
                cachedState = stateCheck.Invoke();
            }
        }

        public void Update()
        {
            if (Threaded && asyncResult != null && asyncResult.IsCompleted)
            {
                try
                {
                    cachedState = stateCheck.EndInvoke(asyncResult);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    asyncResult = null;
                }
            }
            if (cachedState == ReportState.Unknown)
            {
                if (Threaded)
                {
                    if (asyncResult == null)
                    {
                        asyncResult = stateCheck.BeginInvoke((a) => { }, this);
                    }
                }
                else
                {
                    cachedState = stateCheck.Invoke();
                }
            }
        }


        /// <summary>
        /// Uses the first action provided in the report to fix potential issues.
        /// </summary>
        public void UseAction()
        {
            UseAction(0);
        }
        /// <summary>
        /// Uses an action at index to fix potential issues in the report.
        /// </summary>
        /// <param name="index">The action to be called.</param>
        public void UseAction(int index)
        {
            Actions[index].Callback.Invoke();
            LoadState();
        }

        #endregion

        #region Public Static Methods

        public static void DeleteReport(Report report)
        {
            reports.Remove(report);
        }

        public static List<Report> GetReports(ReportCategory category)
        {
            List<Report> temporary = new List<Report>();
            if (category == ReportCategory.None)
            {
                for (int i = 0, length = reports.Count; i < length; i++)
                {
                    if (reports[i].Category == ReportCategory.None)
                    {
                        temporary.Add(reports[i]);
                    }
                }
            }
            else
            {
                for (int i = 0, length = reports.Count; i < length; i++)
                {
                    if (reports[i].Category.HasFlag(category))
                    {
                        temporary.Add(reports[i]);
                    }
                }
            }
            return temporary;
        }

        public static Report GetReport(int index)
        {
            return reports[index];
        }

        public static void SortReports()
        {
            reports.Sort();
        }

        #endregion

        #region Sorting

        // Used for sorting algorithms
        int IComparer<Report>.Compare(Report x, Report y)
        {
            return x.Name.CompareTo(y.Name);
        }

        #endregion
    }

#if !CSHARP_7_3_OR_NEWER
    internal static class ReportExtensions
    {
        public static bool HasFlag(this ReportState state, ReportState value)
        {
            var obj = (int)state;
            var toCheck = (int)value;
            return (obj & toCheck) == toCheck;
        }

        public static bool HasFlag(this ReportCategory state, ReportCategory value)
        {
            var obj = (int)state;
            var toCheck = (int)value;
            return (obj & toCheck) == toCheck;
        }
    }
#endif
}
