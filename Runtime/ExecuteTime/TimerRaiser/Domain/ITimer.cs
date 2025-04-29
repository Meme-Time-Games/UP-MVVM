using System;

namespace TimerRaiser
{
    public interface ITimer
    {
        Action OnTimerDone { get; set; }
        void StartTimer(float time);
    }
}