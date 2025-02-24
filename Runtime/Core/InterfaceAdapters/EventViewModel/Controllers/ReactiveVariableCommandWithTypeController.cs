using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class ReactiveVariableCommandWithTypeController<TType> : ReactiveVariableController<TType>
    {
        private readonly ICommandWithType<TType> _command;

        public ReactiveVariableCommandWithTypeController(IReactiveVariable<TType> reactiveVariable, ICommandWithType<TType> command) : base(reactiveVariable)
        {
            _command = command;
        }

        protected override void ExecuteWithValue(TType value)
        {
            _command.Execute(value);
        }
    }
}