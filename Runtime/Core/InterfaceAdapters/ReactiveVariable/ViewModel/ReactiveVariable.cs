using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MVVM.Core
{
    public class ReactiveVariable<TValue> : IReactiveVariable<TValue>
    {
        private TValue _value;
        
        public TValue Value => _value;
        public Action OnValueChanged { get; set; }
        
#if UNITY_EDITOR
        public Action OnValueChangedEditorOnly { get; set; }
#endif
        
        public ReactiveVariable(TValue defaultValue = default)
        {
            _value = defaultValue;
        }
        
        public void SetValueAndNotify(TValue value)
        {
            SetValue(value);
            OnValueChanged?.Invoke();
        }
        
        public void SetValue(TValue value)
        {
            _value = value;
            
#if UNITY_EDITOR
            OnValueChangedEditorOnly?.Invoke();
#endif
        }
    }
}