using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseBoolReactiveVariableCommand : ICommand
    {
        private readonly IReactiveVariable<bool> _boolReactiveVariable;
        private readonly bool _valueToSet;

        public RaiseBoolReactiveVariableCommand(IReactiveVariable<bool> boolReactiveVariable, bool valueToSet)
        {
            _boolReactiveVariable = boolReactiveVariable;
            _valueToSet = valueToSet;
        }

        public void Execute()
        {
            _boolReactiveVariable.SetValue(_valueToSet);
        }
    }
}