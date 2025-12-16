using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class MultipleGameObjectActivatorControllerInstaller : ControllerInstaller
    {
        [Header("Config")] 
        [SerializeField] private bool _isActive;

        [Header("References")] 
        [SerializeField] private GameObject[] _gameObjectsToActive;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            return new MultipleGameObjectActivatorController(eventBindingViewModel, _isActive, _gameObjectsToActive);
        }
    }
}