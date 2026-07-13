using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class AssetListPanel
    {
        private const int RowHeight = 20;

        private readonly VisualElement _root = new VisualElement();
        private readonly ListView _listView = new ListView();
        private readonly ToolbarSearchField _searchField = new ToolbarSearchField();
        private readonly EnumField _filterField = new EnumField(AssetListFilter.All);
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();

        private IReadOnlyList<MvvmAsset> _allAssets = Array.Empty<MvvmAsset>();
        private List<MvvmAsset> _visibleAssets = new List<MvvmAsset>();
        private ReferenceIndex _referenceIndex;
        private HashSet<string> _duplicateNames = new HashSet<string>();

        public VisualElement Root => _root;
        public Action<MvvmAsset> OnAssetSelected { get; set; }

        public AssetListPanel()
        {
            _root.style.flexGrow = 1;

            _searchField.RegisterValueChangedCallback(searchChange => RefreshVisibleAssets());
            _filterField.RegisterValueChangedCallback(filterChange => RefreshVisibleAssets());

            _listView.fixedItemHeight = RowHeight;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            _listView.selectionType = SelectionType.Single;
            _listView.makeItem = CreateRow;
            _listView.bindItem = BindRow;
            _listView.style.flexGrow = 1;
            _listView.selectionChanged += RaiseAssetSelected;

            _root.Add(_searchField);
            _root.Add(_filterField);
            _root.Add(_listView);
        }

        public void SetAssets(IReadOnlyList<MvvmAsset> assets, ReferenceIndex referenceIndex)
        {
            _allAssets = assets;
            _referenceIndex = referenceIndex;
            _duplicateNames = GetDuplicateNames(assets);

            RefreshVisibleAssets();
        }

        public void SelectAssetWithGuid(string guid)
        {
            int assetIndex = _visibleAssets.FindIndex(asset => asset.Guid == guid);

            if (assetIndex < 0)
                return;

            _listView.SetSelection(assetIndex);
            _listView.ScrollToItem(assetIndex);
        }

        private HashSet<string> GetDuplicateNames(IReadOnlyList<MvvmAsset> assets)
        {
            List<string> names = assets.Select(asset => asset.Name).ToList();
            HashSet<string> duplicateNames = new HashSet<string>();

            foreach (IReadOnlyList<string> duplicateGroup in _duplicateNameFinder.GetDuplicateGroups(names))
            {
                foreach (string duplicateName in duplicateGroup)
                    duplicateNames.Add(duplicateName);
            }

            return duplicateNames;
        }

        private void RefreshVisibleAssets()
        {
            _visibleAssets = _allAssets.Where(IsAssetVisible).ToList();

            _listView.itemsSource = _visibleAssets;
            _listView.Rebuild();
        }

        private bool IsAssetVisible(MvvmAsset asset)
        {
            if (!HasSearchMatch(asset))
                return false;

            return HasFilterMatch(asset);
        }

        private bool HasSearchMatch(MvvmAsset asset)
        {
            if (string.IsNullOrEmpty(_searchField.value))
                return true;

            return asset.Name.IndexOf(_searchField.value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasFilterMatch(MvvmAsset asset)
        {
            AssetListFilter filter = (AssetListFilter)_filterField.value;

            if (filter == AssetListFilter.All)
                return true;

            if (filter == AssetListFilter.Events)
                return asset.IsEvent;

            if (filter == AssetListFilter.ReactiveVariables)
                return !asset.IsEvent;

            if (filter == AssetListFilter.Unused)
                return IsAssetUnused(asset);

            return _duplicateNames.Contains(asset.Name);
        }

        private bool IsAssetUnused(MvvmAsset asset)
        {
            if (ReferenceEquals(_referenceIndex, null))
                return false;

            return !_referenceIndex.HasReferencesWithGuid(asset.Guid);
        }

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            Label nameLabel = new Label();
            nameLabel.name = "name";
            nameLabel.style.flexGrow = 1;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            Label referenceCountLabel = new Label();
            referenceCountLabel.name = "referenceCount";
            referenceCountLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            referenceCountLabel.style.opacity = 0.6f;

            row.Add(nameLabel);
            row.Add(referenceCountLabel);

            return row;
        }

        private void BindRow(VisualElement row, int assetIndex)
        {
            MvvmAsset asset = _visibleAssets[assetIndex];

            row.Q<Label>("name").text = asset.Name;
            row.Q<Label>("referenceCount").text = GetReferenceCountLabel(asset);
        }

        private string GetReferenceCountLabel(MvvmAsset asset)
        {
            if (ReferenceEquals(_referenceIndex, null))
                return string.Empty;

            return _referenceIndex.GetReferencingPathsWithGuid(asset.Guid).Count.ToString();
        }

        private void RaiseAssetSelected(IEnumerable<object> selectedItems)
        {
            MvvmAsset selectedAsset = selectedItems.FirstOrDefault() as MvvmAsset;

            if (ReferenceEquals(selectedAsset, null))
                return;

            OnAssetSelected?.Invoke(selectedAsset);
        }
    }
}
