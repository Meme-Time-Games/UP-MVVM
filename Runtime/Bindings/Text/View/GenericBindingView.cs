using MVVM.Core;
using UnityEngine;

namespace MVVM.Bindings
{
    public abstract class GenericBindingView<T> : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected ReactiveVariableSO<T> _stringReactiveVariableSO;
        [SerializeField] protected bool _updateOnInit = true;

        protected ReactiveVariable<T> _reactiveVariable;

        protected virtual void Awake()
        {
            _reactiveVariable = (ReactiveVariable<T>)_stringReactiveVariableSO.GetReactiveVariable();
            _reactiveVariable.OnValueChanged += UpdateText;

            if (_updateOnInit)
            {
                UpdateText();
            }
        }

        protected abstract void UpdateText();

        protected virtual void OnEnable()
        {
            UpdateText();
        }

        protected virtual void OnDestroy()
        {
            _reactiveVariable.OnValueChanged -= UpdateText;
        }
    }
}