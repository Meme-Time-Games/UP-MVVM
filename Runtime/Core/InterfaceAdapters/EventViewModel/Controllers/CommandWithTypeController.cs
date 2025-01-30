using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandWithTypeController<TypeData> : ControllerWithUseCase<TypeData>
    {
        private readonly ICommandWithType<TypeData> _command;

        public CommandWithTypeController(IEventViewModel eventViewModel, ICommandWithType<TypeData> command,
            TypeData typeData) : base(eventViewModel, typeData)
        {
            _command = command;
        }

        protected override void ExecuteUseCase(TypeData useCase)
        {
            _command.Execute(useCase);
        }
    }
}