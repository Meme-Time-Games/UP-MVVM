using System.Collections.Generic;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class EventViewModelReferencesEditorWindow : EditorWindow
    {
        private EventViewModelSO _eventViewModelSo;
        private List<Component> _sceneReferenceComponents;
        private List<Object> _projectReferenceObjects;
        
        [MenuItem("Tools/MVVM/Event Searcher")]
        public static void ShowWindow()
        {
            GetWindow(typeof(EventViewModelReferencesEditorWindow), false, "Event Searcher");
        }

        private void OnEnable()
        {
            SearchReferencesForSelectedEventViewModelSO();
        }

        private void SearchReferencesForSelectedEventViewModelSO()
        {
            if (Selection.activeObject is not EventViewModelSO)
            {
                return;
            }

            _eventViewModelSo = Selection.activeObject as EventViewModelSO;

            DrawSearchReferenceInTheScene(_eventViewModelSo);
        }

        private void DrawSearchReferenceInTheScene(EventViewModelSO eventViewModelSo)
        {
            _sceneReferenceComponents = GetAllReferencesInScene(eventViewModelSo);
            _projectReferenceObjects = GetAllReferencesInProject(eventViewModelSo);
        }

        private List<Component> GetAllReferencesInScene(EventViewModelSO eventViewModelSo)
        {
            List<Component> allReferencedObjects = new List<Component>();
            List<GameObject> allGameObjects = new List<GameObject>();
            allGameObjects.AddRange(FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));

            foreach (var gameObject in allGameObjects)
            {
                Component[] components = gameObject.GetComponents<Component>();

                foreach (var component in components)
                {
                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty serializedProperty = serializedObject.GetIterator();

                    while (serializedProperty.NextVisible(true))
                    {
                        if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference ||
                            serializedProperty.objectReferenceValue != eventViewModelSo)
                            continue;

                        allReferencedObjects.Add(component);
                    }
                }
            }
            
            return allReferencedObjects;
        }
        
        private List<Object> GetAllReferencesInProject(EventViewModelSO eventViewModelSo)
        {
            List<Object> allReferencedObjects = new List<Object>();
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in allAssetPaths)
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

                if (null == asset) 
                    continue;
                
                if (asset is not GameObject)
                    continue;

                if (HasReference(asset as GameObject, eventViewModelSo))
                {
                    allReferencedObjects.Add(asset);
                }
            }

            return allReferencedObjects;
        }

        private bool HasReference(GameObject gameObject, EventViewModelSO eventViewModelSo)
        {
            Component[] components = gameObject.GetComponents<Component>();

            foreach (var component in components)
            {
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty serializedProperty = serializedObject.GetIterator();

                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference ||
                        serializedProperty.objectReferenceValue != eventViewModelSo)
                        continue;

                    return true;
                }
            }

            return false;
        }

        private void OnGUI()
        {
            if(GUILayout.Button("Search References"))
                SearchReferencesForSelectedEventViewModelSO();
            
            GUILayout.Space(10);
            
            DrawSceneReferences();
            DrawProjectReferences();
        }
        
        private void DrawSceneReferences()
        {
            GUILayout.Label("Scene References");
            
            GUILayout.BeginVertical("box");

            if (ReferenceEquals(_sceneReferenceComponents, null))
            {
                GUILayout.Label("Search again!");
                GUILayout.EndVertical();
                
                return;
            }
            
            if (_sceneReferenceComponents.Count <= 0)
            {
                GUILayout.Label("Nothing Found");
                GUILayout.EndVertical();
                return;
            }

            Dictionary<string, List<Component>> _componentsByScene = new Dictionary<string, List<Component>>();
            
            foreach (var component in _sceneReferenceComponents)
            {
                if(null == component)
                    continue;

                string sceneName = component.gameObject.scene.name;
                if(!_componentsByScene.ContainsKey(sceneName))
                    _componentsByScene.Add(sceneName, new List<Component>());
                
                _componentsByScene[sceneName].Add(component);
            }
            
            foreach (var sceneComponentsKV in _componentsByScene)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label(sceneComponentsKV.Key);

                foreach (var component in sceneComponentsKV.Value)
                {
                    if(null == component)
                        continue;
                    
                    if (GUILayout.Button(component.gameObject.name))
                        Selection.activeGameObject = component.gameObject;
                }
                
                GUILayout.EndVertical();
            }
            
            GUILayout.EndVertical();
        }
        
        private void DrawProjectReferences()
        {
            GUILayout.Label("Prefab References");
            
            GUILayout.BeginVertical("box");
            
            if (_projectReferenceObjects == null || _projectReferenceObjects.Count <= 0)
            {
                GUILayout.Label("Nothing Found");
                GUILayout.EndVertical();
                return;
            }
            
            foreach (var objectReferenced in _projectReferenceObjects)
            {
                if(null == objectReferenced)
                    continue;
                
                if (GUILayout.Button(objectReferenced.name))
                    Selection.activeObject = objectReferenced;
            }
            
            GUILayout.EndVertical();
        }
    }
}