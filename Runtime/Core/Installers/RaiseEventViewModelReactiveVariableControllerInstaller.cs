using DependencyInjector.Core;
using MVVM.Core.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseEventViewModelReactiveVariableControllerInstaller<Type> : ReactiveVariableControllerInstaller<Type>
    {
        [Header("References")]
        [SerializeField] private EventViewModelSO _eventToRaiseEventViewModelSO;

        protected override IController GetController(IReactiveVariable<Type> reactiveVariable)
        {
            return new RaiseEventViewModelReactiveVariableController<Type>(reactiveVariable, _eventToRaiseEventViewModelSO.GetEventViewModel());
        }

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }
    }
}