using MVVM.Core.Bindings.Implementations;

namespace MVVM.Core
{
    public class IntRaiseReactiveVariableWithEventViewModelController : RaiseReactiveVariableWithEventViewModelController<int>
    {
        public IntRaiseReactiveVariableWithEventViewModelController(IEventViewModel eventViewModel, IReactiveVariable<int> outputReactiveVariable) : base(eventViewModel, outputReactiveVariable)
        {
        }

        protected override int GetTypeDataOut()
        {
            return _outputReactiveVariable.Value;
        }
    }
}