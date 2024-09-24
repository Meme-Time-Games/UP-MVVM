using UnityEditor;
using UnityEngine;

namespace MVVM.Core
{
    public abstract class ReactiveVariableSO<TValue> : ScriptableObject
    {
        [Header("Config")] 
        [SerializeField] private TValue _defaultValue = default;
        
        private IReactiveVariable<TValue> _reactiveVariable = null;
        
#if UNITY_EDITOR
        private bool _isSubscribedToPlayModeChanged = false;
#endif

        public IReactiveVariable<TValue> GetReactiveVariable()
        {
#if UNITY_EDITOR
            if(!_isSubscribedToPlayModeChanged)
            {
                _isSubscribedToPlayModeChanged = true;
                EditorApplication.playModeStateChanged += ChangePlayMode;
            }
#endif
            
            if (ReferenceEquals(_reactiveVariable, null))
                _reactiveVariable = new ReactiveVariable<TValue>(_defaultValue);
            
            return _reactiveVariable;
        }

#if UNITY_EDITOR
        private void ChangePlayMode(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingEditMode &&
                playModeStateChange != PlayModeStateChange.ExitingPlayMode) 
                return;
            
            _isSubscribedToPlayModeChanged = false;
            EditorApplication.playModeStateChanged += ChangePlayMode;

            _reactiveVariable = null;
        }
#endif
    }
}