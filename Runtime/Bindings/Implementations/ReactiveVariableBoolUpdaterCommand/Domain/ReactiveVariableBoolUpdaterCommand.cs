using Commands.Core;

namespace MVVM.Core.InterfaceAdapters
{
    public class ReactiveVariableBoolUpdaterCommand : ICommand
    {
        private readonly IReactiveVariable<bool> _boolReactiveVariable;
        private readonly bool _valueToSet;

        public ReactiveVariableBoolUpdaterCommand(IReactiveVariable<bool> boolReactiveVariable, bool valueToSet)
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