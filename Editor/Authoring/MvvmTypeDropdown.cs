using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace MVVM.CoreEditor
{
    public class MvvmTypeDropdown : AdvancedDropdown
    {
        private const string RootName = "Reactive Variable";
        private const string MenuPrefix = "ScriptableObjects/";

        private readonly CreateAssetMenuReader _createAssetMenuReader = new CreateAssetMenuReader();
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _onTypeSelected;

        public MvvmTypeDropdown(
            AdvancedDropdownState state, IReadOnlyList<Type> types, Action<Type> onTypeSelected) : base(state)
        {
            _types = types;
            _onTypeSelected = onTypeSelected;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem(RootName);

            foreach (Type type in _types)
                AddTypeItem(root, type);

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            MvvmTypeDropdownItem typeItem = item as MvvmTypeDropdownItem;

            if (ReferenceEquals(typeItem, null))
                return;

            _onTypeSelected?.Invoke(typeItem.Type);
        }

        private void AddTypeItem(AdvancedDropdownItem root, Type type)
        {
            string[] segments = GetMenuSegmentsWithType(type);
            AdvancedDropdownItem parent = root;

            for (int segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
                parent = GetOrCreateChild(parent, segments[segmentIndex]);

            parent.AddChild(new MvvmTypeDropdownItem(segments[segments.Length - 1], type));
        }

        private string[] GetMenuSegmentsWithType(Type type)
        {
            string menuName = _createAssetMenuReader.GetMenuNameWithTypeName(type.Name);

            if (string.IsNullOrEmpty(menuName))
                return new[] { type.Name };

            return GetMenuNameWithoutPrefix(menuName).Split('/');
        }

        private string GetMenuNameWithoutPrefix(string menuName)
        {
            if (!menuName.StartsWith(MenuPrefix, StringComparison.Ordinal))
                return menuName;

            return menuName.Substring(MenuPrefix.Length);
        }

        private AdvancedDropdownItem GetOrCreateChild(AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem child in parent.children)
            {
                if (child.name == name)
                    return child;
            }

            AdvancedDropdownItem createdChild = new AdvancedDropdownItem(name);
            parent.AddChild(createdChild);

            return createdChild;
        }
    }
}
