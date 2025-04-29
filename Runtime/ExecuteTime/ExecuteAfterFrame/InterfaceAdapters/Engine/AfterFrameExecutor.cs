using System;
using System.Collections;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class AfterFrameExecutor : MonoBehaviour, IAfterFrameExecutor
    {
        public Action OnExecute { get; set; }

        public void Execute()
        {
            StopAllCoroutines();
            StartCoroutine(ExecuteAfterTime());
        }

        private IEnumerator ExecuteAfterTime()
        { 
            yield return new WaitForEndOfFrame();
            OnExecute?.Invoke();
        }
    }
}