using Commands.Core;
using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseBoolReactiveVariableCommandInstaller : SingleMonoInstaller<ICommand>
    {
        [SerializeField] private bool _valueToSet;
        [SerializeField] private ReactiveVariableSO<bool> _boolReactiveVariableSo;

        protected override ICommand GetData()
        {
            return new RaiseBoolReactiveVariableCommand(_boolReactiveVariableSo.GetReactiveVariable(), _valueToSet);
        }
    }
}