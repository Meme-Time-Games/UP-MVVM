using DependencyInjector.Installers;
using MVVM.Core;
using UnityEngine;

namespace MVVM
{
    public class BoolReactiveVariableInjectorInitializer : MonoBehaviour
    {
        [SerializeField] private BaseMonoInjector _baseMonoInjector;
        [SerializeField] private ReactiveVariableSO<bool> _reactiveVariable;
        [SerializeField] private bool _activateCondition;

        private void Awake()
        {
            Inject();
        }

        private void Inject()
        {
            if (_activateCondition != _reactiveVariable.GetReactiveVariable().Value)
            {
                return;
            }
            
            _baseMonoInjector.InjectAll();
        }
    }
}