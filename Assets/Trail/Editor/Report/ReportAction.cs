using UnityEngine;

namespace Trail
{
    public delegate void ReportCallback();

    /// <summary>
    /// Container for a single action with a dedicated name. 
    /// This is used by reports to draw a button and have a method be called when pressed.
    /// </summary>
    public struct ReportAction
    {
        public GUIContent Content;
        public ReportCallback Callback;

        public ReportAction(string name, ReportCallback callback) : this(new GUIContent(name), callback) { }
        public ReportAction(GUIContent content, ReportCallback callback)
        {
            this.Content = content;
            this.Callback = callback;
        }
    }
}
