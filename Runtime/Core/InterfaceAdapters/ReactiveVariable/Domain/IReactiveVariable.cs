using System;

namespace MVVM.Core
{
    public interface IReactiveVariable<TValue>
    {
        TValue Value { get; }
        Action OnValueChanged { get; set; }
        
#if UNITY_EDITOR
        public Action OnValueChangedEditorOnly { get; set; }
#endif
        
        void SetValue(TValue value);
        void SetValueAndNotify(TValue value);
    }
}