using System;
using MVVM.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class RuntimePanel : IDisposable
    {
        private readonly VisualElement _root = new VisualElement();
        private readonly EventRaiseRecorder _eventRaiseRecorder = new EventRaiseRecorder();

        private MvvmAsset _shownAsset;

        public VisualElement Root => _root;

        public RuntimePanel()
        {
            _eventRaiseRecorder.OnLogChanged = RefreshLog;
            EditorApplication.playModeStateChanged += StopWatchingOnExit;
        }

        public void ShowAsset(MvvmAsset asset)
        {
            _shownAsset = asset;
            Refresh();
        }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= StopWatchingOnExit;
            _eventRaiseRecorder.Dispose();
        }

        private void Refresh()
        {
            _root.Clear();

            if (ReferenceEquals(_shownAsset, null))
                return;

            if (!EditorApplication.isPlaying)
            {
                _root.Add(new Label("Enter play mode to inspect runtime state."));
                return;
            }

            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_shownAsset.Path);

            if (scriptableObject is EventViewModelSO eventViewModelSo)
            {
                ShowEvent(eventViewModelSo);
                return;
            }

            ShowReactiveVariable(scriptableObject as BaseReactiveVariableSO);
        }

        private void ShowEvent(EventViewModelSO eventViewModelSo)
        {
            _eventRaiseRecorder.WatchAsset(eventViewModelSo);

            Button raiseButton = new Button(() => eventViewModelSo.GetEventViewModel().RaiseEvent());
            raiseButton.text = "Raise Event";

            _root.Add(raiseButton);
            _root.Add(CreateLogContainer());
        }

        private void ShowReactiveVariable(BaseReactiveVariableSO baseReactiveVariableSo)
        {
            _eventRaiseRecorder.Dispose();

            if (baseReactiveVariableSo == null)
                return;

            InspectorElement inspector = new InspectorElement(baseReactiveVariableSo);

            Button raiseButton = new Button(() => baseReactiveVariableSo.Raise());
            raiseButton.text = "Raise";

            _root.Add(inspector);
            _root.Add(raiseButton);
        }

        private VisualElement CreateLogContainer()
        {
            VisualElement logContainer = new VisualElement();
            logContainer.name = "log";

            foreach (string logEntry in _eventRaiseRecorder.GetLog())
                logContainer.Add(new Label(logEntry));

            return logContainer;
        }

        private void RefreshLog()
        {
            VisualElement logContainer = _root.Q<VisualElement>("log");

            if (logContainer == null)
                return;

            logContainer.Clear();

            foreach (string logEntry in _eventRaiseRecorder.GetLog())
                logContainer.Add(new Label(logEntry));
        }

        private void StopWatchingOnExit(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingPlayMode)
                return;

            _eventRaiseRecorder.Dispose();

            _root.Clear();
            _root.Add(new Label("Enter play mode to inspect runtime state."));
        }
    }
}
