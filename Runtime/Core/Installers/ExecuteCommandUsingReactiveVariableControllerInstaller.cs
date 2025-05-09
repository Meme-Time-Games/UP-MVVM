using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Core.Installers
{
    public class ExecuteCommandUsingReactiveVariableControllerInstaller<TUseCase> : ControllerInstaller
    {
        [SerializeField] private ReactiveVariableSO<TUseCase> _reactiveVariableSO;
        [Inject] private ICommandWithType<TUseCase> _commandWithType;

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new ExecuteCommandUsingReactiveVariableController<TUseCase>(eventBindingViewModel, _reactiveVariableSO.GetReactiveVariable(), _commandWithType);
        }
    }
}