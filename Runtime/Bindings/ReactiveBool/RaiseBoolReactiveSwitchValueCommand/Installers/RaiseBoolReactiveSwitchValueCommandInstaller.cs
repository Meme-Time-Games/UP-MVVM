using Commands.Core;
using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseBoolReactiveSwitchValueCommandInstaller : SingleMonoInstaller<ICommand>
    {
        [SerializeField] private ReactiveVariableSO<bool> _boolReactiveVariableSo;

        protected override ICommand GetData()
        {
            return new RaiseBoolReactiveSwitchValueCommand(_boolReactiveVariableSo.GetReactiveVariable());
        }
    }
}