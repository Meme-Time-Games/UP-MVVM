namespace MVVM.CoreEditor
{
    public class MvvmAsset
    {
        private readonly string _guid;
        private readonly string _path;
        private readonly string _name;
        private readonly string _typeName;
        private readonly bool _isEvent;

        public string Guid => _guid;
        public string Path => _path;
        public string Name => _name;
        public string TypeName => _typeName;
        public bool IsEvent => _isEvent;

        public MvvmAsset(string guid, string path, string name, string typeName, bool isEvent)
        {
            _guid = guid;
            _path = path;
            _name = name;
            _typeName = typeName;
            _isEvent = isEvent;
        }
    }
}
