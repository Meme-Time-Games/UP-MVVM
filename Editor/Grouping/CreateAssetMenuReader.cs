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

            if (!menuNames.ContainsKey(typeName))
                return string.Empty;

            return menuNames[typeName];
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

            _menuNamesByTypeName[scriptableObjectType.Name] = createAssetMenu.menuName;
        }
    }
}
