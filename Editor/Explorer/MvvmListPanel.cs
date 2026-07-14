using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class MvvmListPanel
    {
        private const int RowHeight = 22;

        private readonly VisualElement _root = new VisualElement();
        private readonly ListView _listView = new ListView();
        private readonly ToolbarSearchField _searchField = new ToolbarSearchField();
        private readonly EnumField _filterField = new EnumField(AssetListFilter.All);
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();
        private readonly CreateAssetMenuReader _createAssetMenuReader = new CreateAssetMenuReader();
        private readonly MvvmGroupNameProvider _groupNameProvider = new MvvmGroupNameProvider();

        private readonly List<MvvmListRow> _visibleRows = new List<MvvmListRow>();
        private readonly HashSet<string> _collapsedGroups = new HashSet<string>();

        private IReadOnlyList<MvvmAsset> _allAssets = Array.Empty<MvvmAsset>();
        private ReferenceIndex _referenceIndex;
        private HashSet<string> _duplicateNames = new HashSet<string>();

        public VisualElement Root => _root;
        public Action<MvvmAsset> OnAssetSelected { get; set; }

        public MvvmListPanel()
        {
            _root.AddToClassList("mvvm-list-panel");

            _searchField.AddToClassList("mvvm-search");
            _searchField.RegisterValueChangedCallback(searchChange => RebuildRows());

            _filterField.AddToClassList("mvvm-filter");
            _filterField.RegisterValueChangedCallback(filterChange => RebuildRows());

            VisualElement searchRow = new VisualElement();
            searchRow.AddToClassList("mvvm-search-row");
            searchRow.Add(_searchField);
            searchRow.Add(_filterField);

            _listView.AddToClassList("mvvm-list");
            _listView.fixedItemHeight = RowHeight;
            _listView.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            _listView.selectionType = SelectionType.Single;
            _listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            _listView.itemsSource = _visibleRows;
            _listView.makeItem = CreateRow;
            _listView.bindItem = BindRow;
            _listView.selectionChanged += RaiseAssetSelected;
            _listView.itemsChosen += PingChosenAsset;

            _root.Add(searchRow);
            _root.Add(_listView);
        }

        public void SetAssets(IReadOnlyList<MvvmAsset> assets, ReferenceIndex referenceIndex)
        {
            _allAssets = assets;
            _referenceIndex = referenceIndex;
            _duplicateNames = GetDuplicateNames(assets);

            RebuildRows();
        }

        public void SelectAssetWithGuid(string guid)
        {
            int rowIndex = _visibleRows.FindIndex(row => IsRowForGuid(row, guid));

            if (rowIndex >= 0)
            {
                SelectRowAtIndex(rowIndex);
                return;
            }

            ExpandGroupWithGuidAndSelect(guid);
        }

        private void ExpandGroupWithGuidAndSelect(string guid)
        {
            MvvmAsset asset = _allAssets.FirstOrDefault(candidate => candidate.Guid == guid);

            if (ReferenceEquals(asset, null))
                return;

            string groupName = GetGroupNameWithAsset(asset);
            _collapsedGroups.Remove(groupName);
            RebuildRows();

            int rowIndex = _visibleRows.FindIndex(row => IsRowForGuid(row, guid));

            if (rowIndex < 0)
                return;

            SelectRowAtIndex(rowIndex);
        }

        private void SelectRowAtIndex(int rowIndex)
        {
            _listView.SetSelection(rowIndex);
            _listView.ScrollToItem(rowIndex);
        }

        private bool IsRowForGuid(MvvmListRow row, string guid)
        {
            if (row.IsGroupHeader())
                return false;

            return row.Asset.Guid == guid;
        }

        private void RebuildRows()
        {
            _visibleRows.Clear();

            Dictionary<string, List<MvvmAsset>> groups = GetGroups();
            List<string> groupNames = groups.Keys.ToList();
            groupNames.Sort(CompareGroupNames);

            foreach (string groupName in groupNames)
                AddGroupRows(groupName, groups[groupName]);

            _listView.RefreshItems();
        }

        private void AddGroupRows(string groupName, List<MvvmAsset> assets)
        {
            _visibleRows.Add(MvvmListRow.CreateGroupHeader(groupName));

            if (_collapsedGroups.Contains(groupName))
                return;

            assets.Sort(CompareAssetNames);

            foreach (MvvmAsset asset in assets)
                _visibleRows.Add(MvvmListRow.CreateAssetRow(asset));
        }

        private Dictionary<string, List<MvvmAsset>> GetGroups()
        {
            Dictionary<string, List<MvvmAsset>> groups = new Dictionary<string, List<MvvmAsset>>();

            foreach (MvvmAsset asset in _allAssets)
            {
                if (!IsAssetVisible(asset))
                    continue;

                string groupName = GetGroupNameWithAsset(asset);

                if (!groups.ContainsKey(groupName))
                    groups.Add(groupName, new List<MvvmAsset>());

                groups[groupName].Add(asset);
            }

            return groups;
        }

        private string GetGroupNameWithAsset(MvvmAsset asset)
        {
            if (asset.IsEvent)
                return _groupNameProvider.GetGroupNameWithPath(asset.Path);

            string menuName = _createAssetMenuReader.GetMenuNameWithTypeName(asset.TypeName);

            return _groupNameProvider.GetGroupNameWithMenuNameAndPath(menuName, asset.Path);
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
            row.AddToClassList("list-row");

            Label groupHeader = new Label();
            groupHeader.name = "group-header";
            groupHeader.AddToClassList("group-header");
            groupHeader.RegisterCallback<ClickEvent>(ToggleGroupFromHeader);
            row.Add(groupHeader);

            VisualElement content = new VisualElement();
            content.name = "asset-content";
            content.AddToClassList("asset-row");

            Label nameLabel = new Label();
            nameLabel.name = "name";
            nameLabel.AddToClassList("asset-row__name");
            content.Add(nameLabel);

            Label countLabel = new Label();
            countLabel.name = "count";
            countLabel.AddToClassList("asset-row__count");
            content.Add(countLabel);

            row.Add(content);

            return row;
        }

        private void BindRow(VisualElement row, int rowIndex)
        {
            MvvmListRow listRow = _visibleRows[rowIndex];

            if (listRow.IsGroupHeader())
            {
                BindGroupHeader(row, listRow);
                return;
            }

            BindAssetRow(row, listRow);
        }

        private void BindGroupHeader(VisualElement row, MvvmListRow listRow)
        {
            Label groupHeader = row.Q<Label>("group-header");
            groupHeader.userData = listRow.GroupName;
            groupHeader.text = GetFoldoutArrow(listRow.GroupName) + "  " + listRow.GroupName;
            groupHeader.tooltip = "Click to collapse or expand";
            groupHeader.RemoveFromClassList("is-hidden");

            row.Q("asset-content").AddToClassList("is-hidden");
        }

        private void BindAssetRow(VisualElement row, MvvmListRow listRow)
        {
            row.Q<Label>("group-header").AddToClassList("is-hidden");

            VisualElement content = row.Q("asset-content");
            content.RemoveFromClassList("is-hidden");

            MvvmAsset asset = listRow.Asset;

            Label nameLabel = content.Q<Label>("name");
            nameLabel.text = asset.Name;
            nameLabel.tooltip = asset.Path;

            content.Q<Label>("count").text = GetReferenceCountLabel(asset);
        }

        private string GetFoldoutArrow(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
                return "▶";

            return "▼";
        }

        private string GetReferenceCountLabel(MvvmAsset asset)
        {
            if (ReferenceEquals(_referenceIndex, null))
                return string.Empty;

            return _referenceIndex.GetReferencingPathsWithGuid(asset.Guid).Count.ToString();
        }

        private void ToggleGroupFromHeader(ClickEvent clickEvent)
        {
            Label groupHeader = clickEvent.currentTarget as Label;

            if (ReferenceEquals(groupHeader, null))
                return;

            string groupName = groupHeader.userData as string;

            if (string.IsNullOrEmpty(groupName))
                return;

            ToggleGroup(groupName);
        }

        private void ToggleGroup(string groupName)
        {
            if (_collapsedGroups.Contains(groupName))
            {
                _collapsedGroups.Remove(groupName);
                RebuildRows();
                return;
            }

            _collapsedGroups.Add(groupName);
            RebuildRows();
        }

        private void RaiseAssetSelected(IEnumerable<object> selectedItems)
        {
            MvvmListRow selectedRow = selectedItems.FirstOrDefault() as MvvmListRow;

            if (ReferenceEquals(selectedRow, null))
                return;

            if (selectedRow.IsGroupHeader())
                return;

            OnAssetSelected?.Invoke(selectedRow.Asset);
        }

        private void PingChosenAsset(IEnumerable<object> chosenItems)
        {
            MvvmListRow chosenRow = chosenItems.FirstOrDefault() as MvvmListRow;

            if (ReferenceEquals(chosenRow, null))
                return;

            if (chosenRow.IsGroupHeader())
                return;

            Object asset = AssetDatabase.LoadMainAssetAtPath(chosenRow.Asset.Path);

            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private int CompareGroupNames(string first, string second)
        {
            return string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareAssetNames(MvvmAsset first, MvvmAsset second)
        {
            return string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
