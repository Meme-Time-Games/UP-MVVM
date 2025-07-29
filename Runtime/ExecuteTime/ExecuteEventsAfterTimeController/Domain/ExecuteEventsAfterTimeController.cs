using System.Collections.Generic;
using TimerRaiser;

namespace MVVM.Core.InterfaceAdapters
{
    public class ExecuteEventsAfterTimeController : Controller
    {
        private readonly ITimer _timer;
        private readonly float _timeToWait;
        private readonly List<IEventViewModel> _eventViewModels; 

        private int _currentIndex;

        public ExecuteEventsAfterTimeController(IEventViewModel eventViewModel, List<IEventViewModel> eventViewModels, ITimer timer, float timeToWait) : base(eventViewModel) 
        {
            _eventViewModels = eventViewModels;
            _timer = timer;
            _timeToWait = timeToWait;
            _currentIndex = 0; 
        }

        public override void Execute()
        {
            if (_eventViewModels != null && _eventViewModels.Count > 0)
            {
                _currentIndex = 0;

                _timer.OnTimerDone += ExecuteNextEvent; 
                _timer.StartTimer(_timeToWait);
            }
        }

        private void ExecuteNextEvent()
        {
            _timer.OnTimerDone -= ExecuteNextEvent;

            if (_currentIndex < _eventViewModels.Count)
            {
                _eventViewModels[_currentIndex].RaiseEvent();

                _currentIndex++;

                if (_currentIndex < _eventViewModels.Count)
                {
                    _timer.OnTimerDone += ExecuteNextEvent;
                    _timer.StartTimer(_timeToWait);
                }
            }
            else
            {
                _currentIndex = 0;
            }
        }
    }
}