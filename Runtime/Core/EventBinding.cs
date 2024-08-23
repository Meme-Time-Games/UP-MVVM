using System;

namespace Core
{
    public interface IEventViewModel
    {
        Action OnEventRaised { get; set; }
        void RaiseEvent();
    }
}