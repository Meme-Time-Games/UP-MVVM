using System.Collections.Generic;
using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    [CustomEditor(typeof(ReactiveVariableSO<>), true)]
    public class ReactiveVariableEditor : Editor
    {
        private List<Component> _allComponentReferences;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BaseReactiveVariableSO reactiveVariableSo = (BaseReactiveVariableSO)target;
            
            DrawRaiseEventIfIsPlaying(reactiveVariableSo);
        }

        private void DrawRaiseEventIfIsPlaying(BaseReactiveVariableSO baseReactiveVariableSo)
        {
            if (!EditorApplication.isPlaying) 
                return;
            
            if (GUILayout.Button("Raise Event"))
                baseReactiveVariableSo.Raise();
        }
    }
}