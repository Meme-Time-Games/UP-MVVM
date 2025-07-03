namespace MVVM.Core.Bindings.Implementations
{
    public class ConstRaiseReactiveVariableWithEvenViewModelController<TType> : RaiseReactiveVariableWithEvenViewModelController<TType>
    {
        private readonly TType _valueToSet;

        public ConstRaiseReactiveVariableWithEvenViewModelController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable, TType valueToSet) 
        : base(eventViewModel, outputReactiveVariable)
        {
            _valueToSet = valueToSet;
        }

        protected override TType GetTypeDataOut()
        {
            return _valueToSet;
        }
    }
} 