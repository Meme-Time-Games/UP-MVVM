using MVVM.Core;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    [CustomEditor(typeof(BaseReactiveVariableSO), true)]
    public class ReactiveVariableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BaseReactiveVariableSO reactiveVariableSo = (BaseReactiveVariableSO)target;

            DrawRaiseEventIfIsPlaying(reactiveVariableSo);

            if (GUILayout.Button("Open In MVVM Explorer"))
                MvvmExplorerWindow.ShowWindowWithAsset(reactiveVariableSo);
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