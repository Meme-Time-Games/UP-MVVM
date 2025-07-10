using MVVM.Core;
using TMPro;
using UnityEngine;

namespace MVVM.Bindings
{
    public class GenericTextBindingView<T> : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected TMP_Text _text;
        [SerializeField] protected ReactiveVariableSO<T> _stringReactiveVariableSO;
        
        protected ReactiveVariable<T> _reactiveVariable;

        protected virtual void Awake()
        {
            _reactiveVariable = (ReactiveVariable<T>)_stringReactiveVariableSO.GetReactiveVariable();
            _reactiveVariable.OnValueChanged += UpdateText;
        }

        protected virtual void UpdateText()
        {
            _text.text = _reactiveVariable.Value?.ToString();
        }

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