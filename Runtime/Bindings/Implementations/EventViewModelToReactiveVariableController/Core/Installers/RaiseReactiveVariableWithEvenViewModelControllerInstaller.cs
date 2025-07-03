using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Core.Bindings.Implementations
{
    public abstract class RaiseReactiveVariableWithEvenViewModelControllerInstaller<TType> : ControllerInstaller
    {
        [Header("References")]
        [SerializeField] private ReactiveVariableSO<TType> _outputReactiveVariable;

        protected override IController GetController(IEventViewModel eventViewModel)
        {
            return GetEventViewModelToReactiveVariableController(eventViewModel, _outputReactiveVariable.GetReactiveVariable());
        }

        protected abstract IController GetEventViewModelToReactiveVariableController(IEventViewModel eventViewModel, IReactiveVariable<TType> outputReactiveVariable);
    }
} 