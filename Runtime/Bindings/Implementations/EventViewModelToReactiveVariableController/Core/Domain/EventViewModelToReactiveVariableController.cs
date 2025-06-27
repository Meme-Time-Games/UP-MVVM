using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core.Bindings.Implementations
{
    public abstract class EventViewModelToReactiveVariableController<TType> : Controller
    {
        private readonly IReactiveVariable<TType> _outputReactiveVariable;

        public EventViewModelToReactiveVariableController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable) : base(eventViewModel)
        {
            _outputReactiveVariable = outputReactiveVariable;
        }

        public override void Execute()
        {
            _outputReactiveVariable.SetValueAndNotify(GetTypeDataOut());
        }

        protected abstract TType GetTypeDataOut();
    }
} 