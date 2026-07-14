using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class OpenSceneReferenceScanner
    {
        public IReadOnlyList<Component> GetReferencingComponentsWithAsset(Object targetAsset)
        {
            List<Component> referencingComponents = new List<Component>();

            foreach (GameObject sceneGameObject in GetAllGameObjectsInOpenScenes())
                AddReferencingComponents(sceneGameObject, targetAsset, referencingComponents);

            return referencingComponents;
        }

        private void AddReferencingComponents(
            GameObject sceneGameObject, Object targetAsset, List<Component> referencingComponents)
        {
            foreach (Component component in sceneGameObject.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                if (!HasReferenceToAsset(component, targetAsset))
                    continue;

                referencingComponents.Add(component);
            }
        }

        private bool HasReferenceToAsset(Component component, Object targetAsset)
        {
            using (SerializedObject serializedComponent = new SerializedObject(component))
            {
                SerializedProperty serializedProperty = serializedComponent.GetIterator();

                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    if (serializedProperty.objectReferenceValue != targetAsset)
                        continue;

                    return true;
                }

                return false;
            }
        }

        private IEnumerable<GameObject> GetAllGameObjectsInOpenScenes()
        {
            for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(sceneIndex);

                if (!scene.isLoaded)
                    continue;

                foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (GameObject gameObject in GetGameObjectWithChildren(rootGameObject))
                        yield return gameObject;
                }
            }
        }

        private IEnumerable<GameObject> GetGameObjectWithChildren(GameObject parentGameObject)
        {
            yield return parentGameObject;

            foreach (Transform childTransform in parentGameObject.transform)
            {
                foreach (GameObject childGameObject in GetGameObjectWithChildren(childTransform.gameObject))
                    yield return childGameObject;
            }
        }
    }
}
