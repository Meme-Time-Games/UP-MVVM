using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.ReactiveVariable
{
    public abstract class ReactiveVariableValueInitializerInstaller<TValue> : SingleMonoInstaller<ReactiveVariableValueInitializer<TValue>>
    {
        [Header("References")]
        [SerializeField] private ReactiveVariableSO<TValue> _reactiveVariableSO;
        [SerializeField] private TValue _initialValue;

        protected override ReactiveVariableValueInitializer<TValue> GetData()
        {
            return new ReactiveVariableValueInitializer<TValue>(_reactiveVariableSO.GetReactiveVariable(), _initialValue);
        }
    }
}

