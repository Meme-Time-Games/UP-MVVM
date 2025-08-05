namespace MVVM.Core.Bindings.Implementations
{
    public class ConstRaiseReactiveVariableWithEventViewModelController<TType> : RaiseReactiveVariableWithEventViewModelController<TType>
    {
        private readonly TType _valueToSet;

        public ConstRaiseReactiveVariableWithEventViewModelController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable, TType valueToSet) 
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