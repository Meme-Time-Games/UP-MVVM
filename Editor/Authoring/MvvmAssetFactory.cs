using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class MvvmAssetFactory
    {
        public IReadOnlyList<Type> GetCreatableTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(IsCreatable)
                .OrderBy(assetType => assetType.Name)
                .ToList();
        }

        public ScriptableObject Create(Type assetType, string assetName, string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                throw new DirectoryNotFoundException($"The folder '{folderPath}' does not exist.");

            string assetPath = Path.Combine(folderPath, $"{assetName}.asset").Replace("\\", "/");

            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
                throw new InvalidOperationException($"An asset already exists at '{assetPath}'.");

            ScriptableObject createdAsset = ScriptableObject.CreateInstance(assetType);

            AssetDatabase.CreateAsset(createdAsset, assetPath);
            AssetDatabase.SaveAssets();

            return createdAsset;
        }

        private bool IsCreatable(Type assetType)
        {
            if (assetType.IsAbstract)
                return false;

            if (assetType.IsGenericTypeDefinition)
                return false;

            if (typeof(EventViewModelSO).IsAssignableFrom(assetType))
                return true;

            return typeof(BaseReactiveVariableSO).IsAssignableFrom(assetType);
        }
    }
}
