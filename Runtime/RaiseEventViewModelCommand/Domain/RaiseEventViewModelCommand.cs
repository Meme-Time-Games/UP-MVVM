using MVVM.Core;

namespace Commands.Core
{
    public class RaiseEventViewModelCommand : ICommand
    {
        private readonly IEventViewModel _eventViewModel;

        public RaiseEventViewModelCommand(IEventViewModel eventViewModel)
        {
            _eventViewModel = eventViewModel;
        }

        public void Execute()
        {
            _eventViewModel.RaiseEvent();
        }
    }
}