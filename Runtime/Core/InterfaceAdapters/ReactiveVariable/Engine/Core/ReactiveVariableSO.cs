using UnityEditor;
using UnityEngine;

namespace MVVM.Core
{
    public abstract class ReactiveVariableSO<TValue> : BaseReactiveVariableSO
    {
        [Header("Config")] 
        [SerializeField] private TValue _defaultValue = default;

#if UNITY_EDITOR
        [Header("Debug")] 
        [SerializeField] private TValue _value;
#endif
        
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
            {
                _reactiveVariable = new ReactiveVariable<TValue>(_defaultValue);
                
#if UNITY_EDITOR
                _reactiveVariable.OnValueChangedEditorOnly += UpdateValue;
#endif
            }
            
            return _reactiveVariable;
        }

#if UNITY_EDITOR
        private void UpdateValue()
        {
            _value = _reactiveVariable.Value;
        }
#endif
        
#if UNITY_EDITOR
        private void ChangePlayMode(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingEditMode &&
                playModeStateChange != PlayModeStateChange.ExitingPlayMode) 
                return;
            
            _isSubscribedToPlayModeChanged = false;
            EditorApplication.playModeStateChanged += ChangePlayMode;
            
            if(null != _reactiveVariable)
                _reactiveVariable.OnValueChangedEditorOnly -= UpdateValue;
            
            _reactiveVariable = null;
        }
#endif
    }
}