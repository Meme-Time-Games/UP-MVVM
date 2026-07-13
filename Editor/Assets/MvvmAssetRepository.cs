using System.Collections.Generic;
using System.Linq;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class MvvmAssetRepository
    {
        private const string EventFilter = "t:EventViewModelSO";
        private const string ReactiveVariableFilter = "t:BaseReactiveVariableSO";

        public IReadOnlyList<MvvmAsset> GetAllAssets()
        {
            List<MvvmAsset> assets = new List<MvvmAsset>();

            assets.AddRange(GetAssetsWithFilter(EventFilter));
            assets.AddRange(GetAssetsWithFilter(ReactiveVariableFilter));

            return assets.OrderBy(asset => asset.Name).ToList();
        }

        private IEnumerable<MvvmAsset> GetAssetsWithFilter(string filter)
        {
            string[] guids = AssetDatabase.FindAssets(filter);

            foreach (string guid in guids)
            {
                MvvmAsset asset = GetAssetWithGuid(guid);

                if (ReferenceEquals(asset, null))
                    continue;

                yield return asset;
            }
        }

        private MvvmAsset GetAssetWithGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (scriptableObject == null)
                return null;

            bool isEvent = scriptableObject is EventViewModelSO;

            return new MvvmAsset(guid, path, scriptableObject.name, scriptableObject.GetType().Name, isEvent);
        }
    }
}
