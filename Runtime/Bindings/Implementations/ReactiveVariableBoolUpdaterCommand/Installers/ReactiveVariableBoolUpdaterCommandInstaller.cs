using Commands.Core;
using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class ReactiveVariableBoolUpdaterCommandInstaller : SingleMonoInstaller<ICommand>
    {
        [SerializeField] private bool _valueToSet;
        [SerializeField] private ReactiveVariableSO<bool> _boolReactiveVariableSo;

        protected override ICommand GetData()
        {
            return new ReactiveVariableBoolUpdaterCommand(_boolReactiveVariableSo.GetReactiveVariable(), _valueToSet);
        }
    }
}