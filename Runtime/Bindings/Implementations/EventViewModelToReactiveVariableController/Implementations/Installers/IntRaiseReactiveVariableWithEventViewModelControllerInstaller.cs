using DependencyInjector.Core;
using MVVM.Core.Bindings.Implementations;
using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core
{
    public class IntRaiseReactiveVariableWithEventViewModelControllerInstaller : RaiseReactiveVariableWithEventViewModelControllerInstaller<int>
    {
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetEventViewModelToReactiveVariableController(IEventViewModel eventViewModel,
            IReactiveVariable<int> outputReactiveVariable)
        {
            return new IntRaiseReactiveVariableWithEventViewModelController(eventViewModel, outputReactiveVariable);
        }
    }
}