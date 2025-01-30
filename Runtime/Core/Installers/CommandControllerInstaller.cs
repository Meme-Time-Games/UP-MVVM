using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core.Installers;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandControllerInstaller : ControllerInstaller
    {
        [Inject] private ICommand _command;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new CommandController(eventBindingViewModel, _command);
        }
    }
}