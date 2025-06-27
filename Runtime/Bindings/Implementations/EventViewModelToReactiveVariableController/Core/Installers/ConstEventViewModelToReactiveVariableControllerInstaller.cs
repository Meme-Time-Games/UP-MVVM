using DependencyInjector.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Core.Bindings.Implementations
{
    public class ConstEventViewModelToReactiveVariableControllerInstaller<TType> : EventViewModelToReactiveVariableControllerInstaller<TType>
    {
        [SerializeField] private TType _valueToSet;

        protected override IController GetEventViewModelToReactiveVariableController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable)
        {
            return new ConstEventViewModelToReactiveVariableController<TType>(eventViewModel, outputReactiveVariable, _valueToSet);
        }

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }
    }
} 