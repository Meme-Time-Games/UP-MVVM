using System;

namespace TimerRaiser
{
    public interface ITimer
    {
        Action OnTimerDone { get; set; }
        void StartTimer(int time);
    }
}