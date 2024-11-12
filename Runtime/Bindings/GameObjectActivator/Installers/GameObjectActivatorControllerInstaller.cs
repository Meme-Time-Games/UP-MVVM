using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class GameObjectActivatorControllerInstaller : ControllerInstaller
    {
        [Header("Config")] 
        [SerializeField] private bool _isActive;

        [Header("References")] 
        [SerializeField] private GameObject _gameObjectToActive;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new GameObjectActivatorController(eventBindingViewModel, _isActive, _gameObjectToActive);
        }
    }
}