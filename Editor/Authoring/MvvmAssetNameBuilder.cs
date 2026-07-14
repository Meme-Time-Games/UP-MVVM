using System;

namespace MVVM.CoreEditor
{
    public class MvvmAssetNameBuilder
    {
        public string GetAssetNameWithTypeNameAndName(string typeName, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The asset name cannot be empty.", nameof(name));

            string trimmedName = name.Trim();

            if (trimmedName.EndsWith(typeName, StringComparison.Ordinal))
                return trimmedName;

            return trimmedName + typeName;
        }
    }
}
