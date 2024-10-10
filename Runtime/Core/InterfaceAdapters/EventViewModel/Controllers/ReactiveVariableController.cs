namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ReactiveVariableController<TType> : IController
    {
        private readonly IReactiveVariable<TType> _reactiveVariable;

        protected ReactiveVariableController(IReactiveVariable<TType> reactiveVariable)
        {
            _reactiveVariable = reactiveVariable;

            _reactiveVariable.OnValueChanged += Execute;
        }

        public void Execute()
        {
            TType value = _reactiveVariable.Value;

            ExecuteWithValue(value);
        }

        protected abstract void ExecuteWithValue(TType value);

        public void Dispose()
        {
            _reactiveVariable.OnValueChanged -= Execute;
        }
    }
}