using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core.Installers;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandControllerInstaller : ControllerInstaller
    {
        [Inject] public ICommand _command;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsSingle(diContainer);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new CommandController(eventBindingViewModel, _command);
        }
    }
}