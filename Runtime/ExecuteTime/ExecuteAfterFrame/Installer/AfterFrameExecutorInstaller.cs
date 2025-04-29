using DependencyInjector.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace TimerRaiser
{
    public class AfterFrameExecutorInstaller : SingleMonoInstaller<IAfterFrameExecutor>
    {
        protected override IAfterFrameExecutor GetData()
        {
            GameObject AfterFrameExecutor = new GameObject("AfterFrameExecutor");
            AfterFrameExecutor.transform.SetParent(transform);

            IAfterFrameExecutor afterFrameExecutor = AfterFrameExecutor.AddComponent<AfterFrameExecutor>();

            return afterFrameExecutor;
        }
    }
}