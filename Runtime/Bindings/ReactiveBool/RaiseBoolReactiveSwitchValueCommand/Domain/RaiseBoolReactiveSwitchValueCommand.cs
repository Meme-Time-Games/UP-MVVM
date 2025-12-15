using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseBoolReactiveSwitchValueCommand : ICommand
    {
        private readonly IReactiveVariable<bool> _boolReactiveVariable;

        private bool _currentValue;

        public RaiseBoolReactiveSwitchValueCommand(IReactiveVariable<bool> boolReactiveVariable)
        {
            _boolReactiveVariable = boolReactiveVariable;

            _currentValue = boolReactiveVariable.Value;
        }

        public void Execute()
        {
            _currentValue = !_currentValue;
            _boolReactiveVariable.SetValueAndNotify(_currentValue);
        }
    }
}