using DependencyInjector.Core;
using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core.Installers
{
    public abstract class ReactiveVariableControllerWithUseCaseInstaller<TType, TUseCase> : ReactiveVariableControllerInstaller<TType>
    {
        [Inject] private TUseCase _useCase;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IReactiveVariable<TType> reactiveVariable)
        {
            return GetReactiveController(reactiveVariable, _useCase);
        }

        protected abstract IController GetReactiveController(IReactiveVariable<TType> reactiveVariable, TUseCase useCase);
    }
}