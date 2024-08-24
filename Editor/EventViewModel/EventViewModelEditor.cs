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

            if (_allComponentReferences != null)
                DrawReferenceComponents();

            if (GUILayout.Button("Search references"))
                DrawSearchReferenceInTheScene(eventViewModelSo);
        }

        private void DrawReferenceComponents()
        {
            foreach (var component in _allComponentReferences)
            {
                if (GUILayout.Button(component.gameObject.name))
                    Selection.activeGameObject = component.gameObject;
            }
        }

        private static void DrawRaiseEventIfIsPlaying(IEventViewModel eventViewModel)
        {
            if (!EditorApplication.isPlaying) 
                return;
            
            if (GUILayout.Button("Raise Event"))
                eventViewModel.RaiseEvent();
        }
        
        private void DrawSearchReferenceInTheScene(EventViewModelSO eventViewModelSo)
        {
            EventViewModelReferencesEditorWindow.ShowWindow();   
            // _allComponentReferences = GetAllReferences(eventViewModelSo);
        }

        private List<Component> GetAllReferences(EventViewModelSO eventViewModelSo)
        {
            List<Component> allReferencedObjects = new List<Component>();
            List<GameObject> allGameObjects = new List<GameObject>();
            allGameObjects.AddRange(FindObjectsOfType<GameObject>());

            foreach (var go in allGameObjects)
            {
                Component[] components = go.GetComponents<Component>();

                foreach (var component in components)
                {
                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty prop = so.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType != SerializedPropertyType.ObjectReference ||
                            prop.objectReferenceValue != eventViewModelSo)
                            continue;

                        allReferencedObjects.Add(component);
                        Debug.Log($"Found reference to {eventViewModelSo.name} in component {component.GetType().Name} on GameObject {go.name}", go);
                    }
                }
            }

            Debug.Log($"Total references found: {allReferencedObjects.Count}");
            return allReferencedObjects;
        }
    }
}