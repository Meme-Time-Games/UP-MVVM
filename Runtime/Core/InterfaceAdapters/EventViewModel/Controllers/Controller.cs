namespace MVVM.Core.InterfaceAdapters
{
    public abstract class Controller : IController
    {
        private readonly IEventViewModel _eventViewModel;

        protected Controller(IEventViewModel eventViewModel)
        {
            _eventViewModel = eventViewModel;

            _eventViewModel.OnEventRaised += Execute;
        }

        public abstract void Execute();

        public void Dispose()
        {
            _eventViewModel.OnEventRaised -= Execute;
        }
    }
}