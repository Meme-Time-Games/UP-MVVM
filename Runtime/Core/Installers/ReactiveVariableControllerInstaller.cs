using DependencyInjector.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Core.Installers
{
    public abstract class ReactiveVariableControllerInstaller<TType> : MonoInstaller<IController>
    {
        [Header("References")]
        [SerializeField] private ReactiveVariableSO<TType> _reactiveVariableSo;

        protected override IController GetData()
        {
            return GetController(_reactiveVariableSo.GetReactiveVariable());
        }

        protected abstract IController GetController(IReactiveVariable<TType> reactiveVariable);
    }
}