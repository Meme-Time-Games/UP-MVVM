using DependencyInjector.Core;
using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class GameObjectActivatorReactiveControllerInstaller<TValue> : ReactiveVariableControllerInstaller<TValue>
    {
        [Header("Config")] 
        [SerializeField] private bool _isActive;

        [Header("References")] 
        [SerializeField] private GameObject _gameObjectToActive;
        
        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        { }

        protected override IController GetController(IReactiveVariable<TValue> reactiveVariable)
        {
            return new GameObjectActivatorReactiveController<TValue>(reactiveVariable ,_isActive, _gameObjectToActive );
        }
    }
}