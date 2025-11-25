namespace MVVM.Core.InterfaceAdapters
{
    public class MultipleEventsToRaiseEventViewModelController : IController
    {
        private readonly IEventViewModel _toRaiseEventViewModel;
        private readonly IEventViewModel[] _toListenEventViewModels;

        public MultipleEventsToRaiseEventViewModelController(IEventViewModel toRaiseEventViewModel, IEventViewModel[] toListenEventViewModels)
        {
            _toRaiseEventViewModel = toRaiseEventViewModel;
            _toListenEventViewModels = toListenEventViewModels;
            
            foreach (IEventViewModel eventViewModel in _toListenEventViewModels)
            {
                eventViewModel.OnEventRaised += Execute;
            }
        }

        public void Dispose()
        {
            foreach (IEventViewModel eventViewModel in _toListenEventViewModels)
            {
                eventViewModel.OnEventRaised -= Execute;
            }
        }

        public void Execute()
        {
            _toRaiseEventViewModel.RaiseEvent();
        }
    }
}