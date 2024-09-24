using System;

namespace MVVM.Core
{
    public interface IReactiveVariable<TValue>
    {
        TValue Value { get; }
        Action OnValueChanged { get; set; }
        void SetValue(TValue value);
        void SetValueAndNotify(TValue value);
    }
}