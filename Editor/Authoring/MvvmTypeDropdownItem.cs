using System;
using UnityEditor.IMGUI.Controls;

namespace MVVM.CoreEditor
{
    public class MvvmTypeDropdownItem : AdvancedDropdownItem
    {
        private readonly Type _type;

        public Type Type => _type;

        public MvvmTypeDropdownItem(string name, Type type) : base(name)
        {
            _type = type;
        }
    }
}
