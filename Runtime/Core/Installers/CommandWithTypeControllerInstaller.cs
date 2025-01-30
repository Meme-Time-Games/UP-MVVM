using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class CommandWithTypeControllerInstaller<TypeData> : ControllerInstaller
    {
        [SerializeField] private TypeData _typeData;

        [Inject] private ICommandWithType<TypeData> _command;

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new CommandWithTypeController<TypeData>(eventBindingViewModel, _command, _typeData);
        }
    }
}