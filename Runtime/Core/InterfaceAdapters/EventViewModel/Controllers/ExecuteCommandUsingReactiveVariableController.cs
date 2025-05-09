using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class ExecuteCommandUsingReactiveVariableController<TType> : Controller
    {
        private readonly IReactiveVariable<TType> _reactiveVariable;
        private readonly ICommandWithType<TType> _commandWithType;

        public ExecuteCommandUsingReactiveVariableController(IEventViewModel eventViewModel, IReactiveVariable<TType> reactiveVariable, 
            ICommandWithType<TType> commandWithType) : base(eventViewModel)
        {
            _reactiveVariable = reactiveVariable;
            _commandWithType = commandWithType;
        }

        public override void Execute()
        {
            TType value = _reactiveVariable.Value;

            _commandWithType.Execute(value);
        }
    }
}