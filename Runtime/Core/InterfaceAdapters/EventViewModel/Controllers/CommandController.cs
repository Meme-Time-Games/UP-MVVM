using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandController : Controller
    {
        protected readonly ICommand _command;
        
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