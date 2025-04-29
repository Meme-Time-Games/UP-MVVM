using System;
using Commands.Core;
using MVVM.Core;

namespace TimerRaiser
{
    public class TimerRaiseCommand : ICommand, IDisposable
    {
        private readonly ITimer _timer;
        private readonly float _time;
        private readonly IEventViewModel _onTimerDoneEventViewModel;

        public TimerRaiseCommand(ITimer timer, float time, IEventViewModel onTimerDoneEventViewModel)
        {
            _timer = timer;
            _time = time;
            _onTimerDoneEventViewModel = onTimerDoneEventViewModel;

            _timer.OnTimerDone += RaiseTimeDone;
        }
        
        public void Execute()
        {
            _timer.StartTimer(_time);
        }

        private void RaiseTimeDone()
        {
            _onTimerDoneEventViewModel.RaiseEvent();
        }

        public void Dispose()
        {
            _timer.OnTimerDone -= RaiseTimeDone;
        }
    }
}