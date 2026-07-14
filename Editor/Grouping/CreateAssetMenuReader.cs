using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class CreateAssetMenuReader
    {
        private static Dictionary<string, string> _menuNamesByTypeName;

        public string GetMenuNameWithTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return string.Empty;

            Dictionary<string, string> menuNames = GetMenuNames();

            if (!menuNames.TryGetValue(typeName, out string menuName))
                return string.Empty;

            return menuName;
        }

        private Dictionary<string, string> GetMenuNames()
        {
            if (!ReferenceEquals(_menuNamesByTypeName, null))
                return _menuNamesByTypeName;

            _menuNamesByTypeName = new Dictionary<string, string>();

            foreach (Type scriptableObjectType in TypeCache.GetTypesDerivedFrom<ScriptableObject>())
                AddMenuNameWithType(scriptableObjectType);

            return _menuNamesByTypeName;
        }

        private void AddMenuNameWithType(Type scriptableObjectType)
        {
            object[] attributes = scriptableObjectType.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false);

            if (attributes.Length == 0)
                return;

            CreateAssetMenuAttribute createAssetMenu = (CreateAssetMenuAttribute)attributes[0];

            if (string.IsNullOrEmpty(createAssetMenu.menuName))
                return;

            string typeName = scriptableObjectType.Name;

            if (_menuNamesByTypeName.ContainsKey(typeName))
            {
                WarnOnConflictingMenuName(typeName, scriptableObjectType, createAssetMenu.menuName);
                return;
            }

            _menuNamesByTypeName[typeName] = createAssetMenu.menuName;
        }

        private void WarnOnConflictingMenuName(string typeName, Type scriptableObjectType, string menuName)
        {
            if (_menuNamesByTypeName[typeName] == menuName)
                return;

            Debug.LogWarning(
                $"MVVM Explorer found two ScriptableObject types named '{typeName}' with different " +
                $"CreateAssetMenu paths ('{_menuNamesByTypeName[typeName]}' and '{menuName}', the second from " +
                $"'{scriptableObjectType.FullName}'). The first is used for grouping.");
        }
    }
}
