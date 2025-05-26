using MVVM.Core;

namespace Commands.Core
{
    public class RaiseEventWithBoolValueCommand : ICommandWithType<bool>
    {
        private readonly IEventViewModel _onTrueEventViewModel;
        private readonly IEventViewModel _onFalseEventViewModel;

        public RaiseEventWithBoolValueCommand(IEventViewModel onTrueEventViewModel, IEventViewModel onFalseEventViewModel)
        {
            _onTrueEventViewModel = onTrueEventViewModel;
            _onFalseEventViewModel = onFalseEventViewModel;
        }

        public void Execute(bool value)
        {
            if(value)
                _onTrueEventViewModel.RaiseEvent();
            else
                _onFalseEventViewModel.RaiseEvent();
        }
    }
}