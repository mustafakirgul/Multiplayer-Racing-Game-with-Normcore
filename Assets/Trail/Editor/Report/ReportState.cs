namespace Trail
{
    [System.Flags]
    public enum ReportState : byte
    {
        /// <summary>
        /// Unknown state, it does not know if it should be hidden or be shown.
        /// Report window will do a new report check every frame until it changes to something else.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Used to hide anything that does not need a fix or tweak.
        /// </summary>
        Hidden = 1 << 0,
        /// <summary>
        /// Recommended but not required for uploading to Trail.
        /// Usually performance fixes or shaders
        /// </summary>
        Recommended = 1 << 1,
        /// <summary>
        /// Required fixes for uploading to Trail.
        /// </summary>
        Required = 1 << 2
    }
}
