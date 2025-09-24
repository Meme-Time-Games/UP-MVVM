using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Bindings
{
    public class PlayAnimationWithTriggerControllerInstaller : ControllerInstaller
    {
        [Header("References")]
        private Animator _animator;

        [Header("Config")]
        private string _triggerName;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new PlayAnimationWithTriggerController(eventBindingViewModel, _animator, _triggerName);
        }
    }
}