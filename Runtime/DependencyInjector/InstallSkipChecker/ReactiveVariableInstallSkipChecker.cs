using System;
using DependencyInjector.Installers;
using MVVM.Core;
using UnityEngine;

namespace InstallSkipChecker
{
    public abstract class ReactiveVariableInstallSkipChecker<TType> : MonoInstallSkipChecker
    {
        [Header("References")]
        [SerializeField] private ReactiveVariableSO<TType> _reactiveVariable;
        
        [Header("Config")] 
        [SerializeField] private TType _hasToSkipIfConditionIs;
        
        public override bool HasToSkip()
        {
            TType reactiveVariableValue = _reactiveVariable.GetReactiveVariable().Value;
            bool result = Compare(_hasToSkipIfConditionIs, reactiveVariableValue);
            return result;
        }

        protected abstract bool Compare(TType conditionValue, TType reactiveVariableValue);
    }
}