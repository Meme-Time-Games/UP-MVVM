using System;

namespace MVVM.Core
{
    public class EventViewModel : IEventViewModel
    {
        public Action OnEventRaised { get; set; }
        
        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}