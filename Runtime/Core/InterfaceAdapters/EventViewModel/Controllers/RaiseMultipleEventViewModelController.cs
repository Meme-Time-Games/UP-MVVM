namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseMultipleEventViewModelController : Controller
    {
        private readonly IEventViewModel[] _raiseEventViewModel;
        
        public RaiseMultipleEventViewModelController(IEventViewModel eventViewModel, IEventViewModel[] raiseEventViewModel) : base(eventViewModel)
        {
            _raiseEventViewModel = raiseEventViewModel;
        }

        public override void Execute()
        {
            foreach (IEventViewModel raiseEventViewModel in _raiseEventViewModel)
            {
                raiseEventViewModel.RaiseEvent();
            }
        }
    }
}