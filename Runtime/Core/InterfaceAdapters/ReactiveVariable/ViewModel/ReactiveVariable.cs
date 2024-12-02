using System;
using UnityEngine;

namespace MVVM.Core
{
    public class ReactiveVariable<TValue> : IReactiveVariable<TValue>
    {
        
#if UNITY_EDITOR
        [Header("Debug")] 
        [SerializeField] private bool _isDebugModeActivated = false;
#endif
        
        private TValue _value;
        
        public TValue Value => _value;
        public Action OnValueChanged { get; set; }

        public ReactiveVariable(TValue defaultValue = default)
        {
            _value = defaultValue;
        }
        
        public void SetValue(TValue value)
        {
            _value = value;
            
#if UNITY_EDITOR
            if(_isDebugModeActivated)
                OnValueChanged?.Invoke();
#endif
        }

        public void SetValueAndNotify(TValue value)
        {
            SetValue(value);
            OnValueChanged?.Invoke();
        }
    }
}