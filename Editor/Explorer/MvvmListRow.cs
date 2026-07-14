namespace MVVM.CoreEditor
{
    public class MvvmListRow
    {
        private readonly MvvmAsset _asset;
        private readonly string _groupName;

        public MvvmAsset Asset => _asset;
        public string GroupName => _groupName;

        private MvvmListRow(MvvmAsset asset, string groupName)
        {
            _asset = asset;
            _groupName = groupName;
        }

        public static MvvmListRow CreateAssetRow(MvvmAsset asset)
        {
            return new MvvmListRow(asset, null);
        }

        public static MvvmListRow CreateGroupHeader(string groupName)
        {
            return new MvvmListRow(null, groupName);
        }

        public bool IsGroupHeader()
        {
            return !string.IsNullOrEmpty(_groupName);
        }
    }
}
