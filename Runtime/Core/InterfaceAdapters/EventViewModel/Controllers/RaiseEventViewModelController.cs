namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseEventViewModelController : Controller
    {
        private readonly IEventViewModel _raiseEventViewModel;
        
        public RaiseEventViewModelController(IEventViewModel eventViewModel, IEventViewModel raiseEventViewModel) : base(eventViewModel)
        {
            _raiseEventViewModel = raiseEventViewModel;
        }

        public override void Execute()
        {
            _raiseEventViewModel.RaiseEvent();
        }
    }
}