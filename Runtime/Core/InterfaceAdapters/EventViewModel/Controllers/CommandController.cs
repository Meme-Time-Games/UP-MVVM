using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandController : Controller
    {
        private readonly ICommand _command;
        
        public CommandController(IEventViewModel eventViewModel, ICommand command) : base(eventViewModel)
        {
            _command = command;
        }

        public override void Execute()
        {
            _command.Execute();
        }
    }
}