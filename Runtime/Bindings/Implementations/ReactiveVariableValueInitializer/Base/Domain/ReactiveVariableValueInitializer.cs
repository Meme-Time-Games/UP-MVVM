namespace MVVM.Core.ReactiveVariable
{
    public class ReactiveVariableValueInitializer<TValue>
    {
        public ReactiveVariableValueInitializer(IReactiveVariable<TValue> reactiveVariable, TValue initialValue)
        {
            reactiveVariable.SetValueAndNotify(initialValue);
        }
    }
}