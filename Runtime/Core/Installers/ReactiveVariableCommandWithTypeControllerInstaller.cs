using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core.Installers
{
    public class ReactiveVariableCommandWithTypeControllerInstaller<TType> : ReactiveVariableControllerInstaller<TType>
    {
        [Inject] private ICommandWithType<TType> _command;

        protected override IController GetController(IReactiveVariable<TType> reactiveVariable)
        {
            return new ReactiveVariableCommandWithTypeController<TType>(reactiveVariable, _command);
        }

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }
    }
}