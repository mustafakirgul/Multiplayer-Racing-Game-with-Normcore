using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System.Linq;

namespace Trail
{
    public static class AddressablesLinkFixes
    {
        private const string AddressablesName = "com.unity.addressables";
        private static string FullLinkPath = System.IO.Path.GetFullPath("Assets/link.xml");

        private static UnityEditor.PackageManager.Requests.ListRequest listRequest = null;

        [InitializeOnLoadMethod]
        private static void Load()
        {
            listRequest = UnityEditor.PackageManager.Client.List(true);
            EditorApplication.update += Update;
        }

        public static void CreateReport()
        {
            Report.Create(
                "Prevent code-stripping Addressables",
                "Prevent Unity from code-stripping Addressables related code by adding them into the Link.xml whitelist file.",
                ReportCategory.Code,
                @"https://forum.unity.com/threads/addressables-and-code-stripping-il2cpp.700883/#post-5202515",
                GetLinkState,
                "Fix",
                FixAddressableLink,
                threaded: true);
        }

        private static void Update()
        {
            if (listRequest == null)
            {
                EditorApplication.update -= Update;
                return;
            }
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Failure)
                {
                    Debug.Log(listRequest.Error.message);
                }
                else if (listRequest.Result.Any(x =>x.status == UnityEditor.PackageManager.PackageStatus.Available && x.name == AddressablesName))
                {
                    CreateReport();
                }
                EditorApplication.update -= Update;
            }
        }

        private static ReportState GetLinkState()
        {
            if (!System.IO.File.Exists(FullLinkPath))
                return ReportState.Required;

            var linkDoc = XDocument.Load(FullLinkPath);
            var linkDocNodes = linkDoc.Descendants();
            // Skip first element to discard the <linker> tag in comparison.
            var addressableDocNodes = GetAddressableDocument().Descendants().Skip(1);
            foreach (var node in addressableDocNodes)
            {
                if (!linkDocNodes.Any(x => XElement.EqualityComparer.Equals(x, node)))
                {
                    return ReportState.Required;
                }
            }

            return ReportState.Hidden;
        }

        private static void FixAddressableLink()
        {
            if (!System.IO.File.Exists(FullLinkPath))
            {
                GetAddressableDocument().Save(FullLinkPath);
                AssetDatabase.ImportAsset("Assets/link.xml", ImportAssetOptions.ForceUpdate);
                return;
            }

            var linkDoc = XDocument.Load(FullLinkPath);
            var combined = linkDoc.Descendants().Union(GetAddressableDocument().Descendants());
            combined.First().Save(FullLinkPath);
        }

        private static XDocument GetAddressableDocument()
        {
            return XDocument.Parse(
@"<linker>
    <assembly fullname=""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"" preserve=""all"">
    <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.LegacyResourcesProvider"" preserve=""all"" />
    <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider"" preserve=""all"" />
    <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider"" preserve=""all"" />
    <type fullname=""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider"" preserve=""all"" />
    <type fullname=""UnityEngine.ResourceManagement.AsyncOperations"" preserve=""all"" />
    </assembly>
    <assembly fullname=""Unity.Addressables, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"" preserve=""all"">
    <type fullname=""UnityEngine.AddressableAssets.Addressables"" preserve=""all"" />
    </assembly>
</linker>"
);
        }
    }
}
