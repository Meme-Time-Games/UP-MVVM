namespace MVVM.Core.Bindings.Implementations
{
    public class ConstEventViewModelToReactiveVariableController<TType> : EventViewModelToReactiveVariableController<TType>
    {
        private readonly TType _valueToSet;

        public ConstEventViewModelToReactiveVariableController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable, TType valueToSet) 
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