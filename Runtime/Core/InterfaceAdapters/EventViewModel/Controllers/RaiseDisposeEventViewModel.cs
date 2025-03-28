using System;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseDisposeEventViewModel : IDisposable
    {
        private readonly IEventViewModel _eventViewModel;

        public RaiseDisposeEventViewModel(IEventViewModel eventViewModel)
        {
            _eventViewModel = eventViewModel;
        }
        public void Dispose()
        {
            _eventViewModel.RaiseEvent();
        }
    }
}