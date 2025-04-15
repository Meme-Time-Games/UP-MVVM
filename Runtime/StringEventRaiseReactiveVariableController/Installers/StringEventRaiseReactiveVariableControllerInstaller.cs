using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM
{
    public class StringEventRaiseReactiveVariableControllerInstaller : ReactiveVariableControllerInstaller<string>
    {
        [Header("References")]
        [SerializeField] private EventViewModelSO _eventViewModel;

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IReactiveVariable<string> reactiveVariable)
        {
            return new StringEventRaiseReactiveVariableController(reactiveVariable, _eventViewModel.GetEventViewModel());
        }
    }
}