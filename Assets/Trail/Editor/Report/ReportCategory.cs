namespace Trail
{
    [System.Flags]
    public enum ReportCategory
    {
        /// <summary>
        /// Unsigned category.
        /// </summary>
        None = 0,

        /// <summary>
        /// Everything revolving Project Settings.
        /// This includes Audio, Graphics, Physics, Player, Quality etc inside the Project Setting window.
        /// </summary>
        ProjectSettings = 1 << 0,


        /// <summary>
        /// Code specific category, this would include code errors that is not possible on webgl as Reflection.Emit.
        /// </summary>
        Code = 1 << 1,

        Audio = 1 << 2,

        Rendering = 1 << 3,

        Physics = 1 << 4,

        Animation = 1 << 5,

        IncompatibleLibraries = 1 << 6,

        /// <summary>
        /// This includes all shader error or potential fixes around shaders and materials.
        /// </summary>
        Shaders = 1 << 29,

        /// <summary>
        /// Category for known unsupported third party assets/plugins.
        /// </summary>
        Plugins = 1 << 30,

        /// <summary>
        /// Everything that does not have a dedicated category due to amount of reports in that area.
        /// </summary>
        Others = 1 << 31,

        /// <summary>
        /// Audio Settings located in project settings. 
        /// </summary>
        AudioSetting = ProjectSettings | Audio,
        /// <summary>
        /// Quality settings located in project settings.
        /// </summary>
        QualitySetting = ProjectSettings | Rendering,
        /// <summary>
        /// Physics settings located in project settings.
        /// </summary>
        PhysicsSetting = ProjectSettings | Physics,



        /// <summary>
        /// All categories.
        /// </summary>
        All = ~0
    }
}
