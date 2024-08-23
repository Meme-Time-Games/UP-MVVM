using System;

namespace MVVM.Core
{
    public interface IEventViewModel
    {
        Action OnEventRaised { get; set; }
        void RaiseEvent();
    }
}