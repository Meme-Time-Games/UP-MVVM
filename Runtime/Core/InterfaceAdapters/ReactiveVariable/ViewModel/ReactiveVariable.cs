using System;

namespace MVVM.Core
{
    public class ReactiveVariable<TValue> : IReactiveVariable<TValue>
    {
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
        }

        public void SetValueAndNotify(TValue value)
        {
            SetValue(value);
            OnValueChanged?.Invoke();
        }
    }
}