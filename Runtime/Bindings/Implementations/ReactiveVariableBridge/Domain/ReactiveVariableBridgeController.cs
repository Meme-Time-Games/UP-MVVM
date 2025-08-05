using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core.Bindings.Implementations
{
    public abstract class ReactiveVariableBridgeController<TTypeIn, TTypeOut> : ReactiveVariableController<TTypeIn>
    {
        private readonly IReactiveVariable<TTypeOut> _outputReactiveVariable;

        protected ReactiveVariableBridgeController(
            IReactiveVariable<TTypeIn> inputReactiveVariable,
            IReactiveVariable<TTypeOut> outputReactiveVariable)
            : base(inputReactiveVariable)
        {
            _outputReactiveVariable = outputReactiveVariable;
        }

        protected override void ExecuteWithValue(TTypeIn value)
        {
            _outputReactiveVariable.SetValueAndNotify(GetTypeDataOut(value));
        }

        protected abstract TTypeOut GetTypeDataOut(TTypeIn value);
    }
} 