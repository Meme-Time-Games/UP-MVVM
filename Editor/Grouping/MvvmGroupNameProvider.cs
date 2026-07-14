namespace MVVM.CoreEditor
{
    public class MvvmGroupNameProvider
    {
        private const string UngroupedName = "Ungrouped";

        private static readonly string[] GenericMenuSegments =
        {
            "scriptableobjects",
            "reactivevariable",
            "reactivevariables"
        };

        private static readonly string[] GenericFolderNames =
        {
            "assets",
            "packages",
            "data",
            "scriptableobject",
            "scriptableobjects",
            "so",
            "sos",
            "resources",
            "prefabs",
            "scene",
            "scenes",
            "settings",
            "config",
            "configs",
            "runtime",
            "editor",
            "event",
            "events",
            "reactivevariable",
            "reactivevariables",
            "variables"
        };

        public string GetGroupNameWithMenuNameAndPath(string menuName, string assetPath)
        {
            string featureFromMenu = GetFeatureFromMenuName(menuName);

            if (!string.IsNullOrEmpty(featureFromMenu))
                return featureFromMenu;

            return GetFeatureFromAssetPath(assetPath);
        }

        private string GetFeatureFromMenuName(string menuName)
        {
            if (string.IsNullOrEmpty(menuName))
                return string.Empty;

            string[] segments = menuName.Split('/');

            for (int segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
            {
                if (IsGenericName(segments[segmentIndex], GenericMenuSegments))
                    continue;

                return segments[segmentIndex];
            }

            return string.Empty;
        }

        private string GetFeatureFromAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return UngroupedName;

            string[] segments = assetPath.Split('/');

            for (int segmentIndex = segments.Length - 2; segmentIndex >= 0; segmentIndex--)
            {
                if (IsGenericName(segments[segmentIndex], GenericFolderNames))
                    continue;

                return segments[segmentIndex];
            }

            return UngroupedName;
        }

        private bool IsGenericName(string name, string[] genericNames)
        {
            string normalizedName = name.ToLowerInvariant();

            foreach (string genericName in genericNames)
            {
                if (normalizedName == genericName)
                    return true;
            }

            return false;
        }
    }
}
