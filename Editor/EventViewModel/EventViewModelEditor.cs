using System;
using System.Collections.Generic;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    [CustomEditor(typeof(EventViewModelSO), true)]
    public class EventViewModelEditor : Editor
    {
        private List<Component> _allComponentReferences;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EventViewModelSO eventViewModelSo = (EventViewModelSO)target;
            IEventViewModel eventViewModel = eventViewModelSo.GetEventViewModel();

            DrawRaiseEventIfIsPlaying(eventViewModel);

            if (GUILayout.Button("Open References Searcher"))
                DrawSearchReferenceInTheScene();
        }

        private void DrawRaiseEventIfIsPlaying(IEventViewModel eventViewModel)
        {
            if (!EditorApplication.isPlaying) 
                return;
            
            if (GUILayout.Button("Raise Event"))
                eventViewModel.RaiseEvent();
        }
        
        private void DrawSearchReferenceInTheScene()
        {
            EventViewModelReferencesEditorWindow.ShowWindow();   
        }
    }
}