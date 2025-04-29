using System;
using System.Collections;
using UnityEngine;

namespace TimerRaiser
{
    public class CoroutineTimer : MonoBehaviour, ITimer
    {
        public Action OnTimerDone { get; set; }
        
        public void StartTimer(float time)
        {
            StopAllCoroutines();
            StartCoroutine(RunTimer(time));
        }

        private IEnumerator RunTimer(float time)
        {
            yield return new WaitForSeconds(time);
            OnTimerDone?.Invoke();
        }
    }
}