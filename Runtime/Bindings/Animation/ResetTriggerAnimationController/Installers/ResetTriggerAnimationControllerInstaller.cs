using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Bindings
{
    public class ResetTriggerAnimationControllerInstaller : ControllerInstaller
    {
        [Header("References")]
        [SerializeField] private Animator _animator;

        [Header("Config")]
        [SerializeField] private string _triggerName;

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new ResetTriggerAnimationController(eventBindingViewModel, _animator, _triggerName);
        }
    }
}