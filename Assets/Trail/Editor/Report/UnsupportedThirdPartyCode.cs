using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Trail
{
    public class UnsupportedThirdPartyCode
    {
        private const string DISABLESTEAMWORKS = "DISABLESTEAMWORKS";

        [InitializeOnLoadMethod]
        private static void CheckForSteamworks()
        {
            var assets = AssetDatabase.FindAssets("Steam t:MonoScript");

            if (assets != null && assets.Any(x => AssetDatabase.GUIDToAssetPath(x).EndsWith("Steam.cs")))
            {
                Report.Create(
                    "Disable Steamworks on WebGL",
                    "Steamworks is not supported on WebGL, only on standalone platforms. You can easily disable it by adding \"DISABLESTEAMWORKS\" to the \"Scripting Define Symbols\" player setting.",
                     ReportCategory.IncompatibleLibraries,
                     @"",
                     () =>
                     {
                         var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
                         return defineSymbols.Split(';').Any(x => x.Equals(DISABLESTEAMWORKS)) ? ReportState.Hidden : ReportState.Required;
                     },
                     new ReportAction(new GUIContent("Fix", ""), () =>
                     {
                         if (EditorUtility.DisplayDialog("Disable Steamworks on WebGL", "After disabling Steamworks, any code that relies on it will probably not compile and will need to be updated.\n\nAre you sure you want to continue?", "Continue", "Cancel"))
                         {
                             var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
                             defineSymbols += string.IsNullOrEmpty(defineSymbols) ? DISABLESTEAMWORKS : ";" + DISABLESTEAMWORKS;
                             PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, defineSymbols);
                         }
                     }));
            }
        }
    }
}
