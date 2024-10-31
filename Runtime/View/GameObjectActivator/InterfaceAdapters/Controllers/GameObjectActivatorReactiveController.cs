using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace View.GameObjectActivator
{
    public class GameObjectActivatorReactiveController<TValue> : ReactiveVariableController<TValue>
    {
        private readonly bool _isActive;
        private readonly GameObject _gameObject;
        
        public GameObjectActivatorReactiveController(IReactiveVariable<TValue> reactiveVariable, bool isActive, GameObject gameObject) : base(reactiveVariable)
        {
            _isActive = isActive;
            _gameObject = gameObject;
        }

        protected override void ExecuteWithValue(TValue value)
        {
            _gameObject.SetActive(_isActive);
        }
    }
}