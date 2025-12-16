using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class MultipleGameObjectActivatorController : Controller
    {
        private readonly bool _isActive;
        private readonly GameObject[] _gameObjects;
        
        public MultipleGameObjectActivatorController(IEventViewModel eventViewModel, bool isActive, GameObject[] gameObjects) : base(eventViewModel)
        {
            _isActive = isActive;
            _gameObjects = gameObjects;
        }

        public override void Execute()
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.SetActive(_isActive);
            }
           
        }
    }
}