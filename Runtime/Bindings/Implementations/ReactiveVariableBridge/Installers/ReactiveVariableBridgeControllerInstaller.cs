using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using MVVM.Core.Installers;
using UnityEngine;

namespace MVVM.Core.Bindings.Implementations
{
    public abstract class ReactiveVariableBridgeControllerInstaller<TTypeIn, TTypeOut> : ReactiveVariableControllerInstaller<TTypeIn>
    {
        [SerializeField] private ReactiveVariableSO<TTypeOut> _outputReactiveVariableSo;

        protected override IController GetController(IReactiveVariable<TTypeIn> inputReactiveVariable)
        {
            return GetBridgeController(inputReactiveVariable, _outputReactiveVariableSo.GetReactiveVariable());
        }

        protected abstract IController GetBridgeController(IReactiveVariable<TTypeIn> inputReactiveVariable, IReactiveVariable<TTypeOut> outputReactiveVariable);
    }
} 