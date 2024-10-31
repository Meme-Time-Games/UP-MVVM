using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class GameObjectActivatorController : Controller
    {
        private readonly bool _isActive;
        private readonly GameObject _gameObject;
        
        public GameObjectActivatorController(IEventViewModel eventViewModel, bool isActive, GameObject gameObject) : base(eventViewModel)
        {
            _isActive = isActive;
            _gameObject = gameObject;
        }

        public override void Execute()
        {
            _gameObject.SetActive(_isActive);
        }
    }
}