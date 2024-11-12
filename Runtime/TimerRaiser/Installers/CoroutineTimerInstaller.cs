using DependencyInjector.Installers;
using UnityEngine;

namespace TimerRaiser
{
    public class CoroutineTimerInstaller : SingleMonoInstaller<ITimer>
    {
        protected override ITimer GetData()
        {
            GameObject coroutineTimerGO = new GameObject("CoroutinesTimer");
            coroutineTimerGO.transform.SetParent(transform);
            
            ITimer timer = coroutineTimerGO.AddComponent<CoroutineTimer>();

            return timer;
        }
    }
}