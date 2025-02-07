using System;
using System.Collections.Generic;
using System.Linq;
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

            DrawSubscribedObjects(eventViewModel);
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
        
        private void DrawSubscribedObjects(IEventViewModel eventViewModel)
        {
            object[] subscribedObjects = GetSubscribers(eventViewModel.OnEventRaised);

            if(subscribedObjects == null || subscribedObjects.Length == 0)
                return;
            
            GUILayout.BeginVertical();
            DrawTitle("Subscribed Objects");
            
            foreach (var subscribedObject in subscribedObjects)
            {
                if (subscribedObject == null)
                    continue;
                
                GUIStyle boldStyle = new GUIStyle(EditorStyles.boldLabel);
                EditorGUILayout.LabelField(subscribedObject.ToString(), boldStyle);
            }
            
            GUILayout.EndVertical();
        }
        
        private object[] GetSubscribers(Delegate delegateValue)
        {
            if (delegateValue == null) 
                return Array.Empty<object>();

            return delegateValue.GetInvocationList()
                .Select(d => d.Target) 
                .ToArray();
        }
        
        private void DrawTitle(string title)
        {
            GUILayout.Space(10);
            
            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontSize = 13;
            boldStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label(title, boldStyle);

            DrawDivider();
        }
        
        private void DrawDivider()
        {
            GUIStyle lineStyle = new GUIStyle();
            lineStyle.normal.background = EditorGUIUtility.whiteTexture;
            lineStyle.margin = new RectOffset(0, 0, 4, 4);
            lineStyle.fixedHeight = 1;
            GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }
    }
}