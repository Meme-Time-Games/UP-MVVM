using MVVM.Core;
using MVVM.Core.InterfaceAdapters;

namespace MVVM
{
    public class StringEventRaiseReactiveVariableController  : ReactiveVariableController<string>
    {
        private readonly IEventViewModel _eventViewModel;
    
        public StringEventRaiseReactiveVariableController(IReactiveVariable<string> reactiveVariable, IEventViewModel eventViewModel) : base(reactiveVariable)
        {
            _eventViewModel = eventViewModel;
        }

        protected override void ExecuteWithValue(string _)
        {
            _eventViewModel.RaiseEvent();
        }
    }
}