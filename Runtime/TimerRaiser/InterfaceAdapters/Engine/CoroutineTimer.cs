using System;
using System.Collections;
using UnityEngine;

namespace TimerRaiser
{
    public class CoroutineTimer : MonoBehaviour, ITimer
    {
        public Action OnTimerDone { get; set; }
        
        public void StartTimer(int time)
        {
            StopAllCoroutines();
            StartCoroutine(RunTimer(time));
        }

        public IEnumerator RunTimer(int time)
        {
            yield return new WaitForSeconds(time);
            OnTimerDone?.Invoke();
        }
    }
}