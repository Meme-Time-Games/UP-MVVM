using DependencyInjector.Core;
using MVVM.Core.InterfaceAdapters;

namespace MVVM.Core.Installers
{
    public abstract class ControllerWithUseCaseMultipleInstaller<TUseCase> : ControllerInstaller
    {
        [Inject] private TUseCase _useCase;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return GetControllerInstance(eventBindingViewModel, _useCase);
        }

        protected abstract IController GetControllerInstance(IEventViewModel eventBindingViewModel, TUseCase useCase);
    }
}