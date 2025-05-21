namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseEventViewModelReactiveVariableController<TType> : ReactiveVariableController<TType>
    {
        private readonly IEventViewModel _raiseEventViewModel;

        public RaiseEventViewModelReactiveVariableController(IReactiveVariable<TType> reactiveVariable, IEventViewModel raiseEventViewModel) : base(reactiveVariable)
        {
            _raiseEventViewModel = raiseEventViewModel;
        }

        protected override void ExecuteWithValue(TType value)
        {
            _raiseEventViewModel.RaiseEvent();
        }
    }
}