using TMPro;
using UnityEngine;

namespace MVVM.Bindings
{ 
    public abstract class GenericTextFieldBindingView<T> : GenericBindingView<T>
    {
        [Header("Text")]
        [SerializeField] protected TMP_InputField _text;

        protected override void UpdateText()
        {
            _text.text = _reactiveVariable.Value?.ToString();
        }
    }
}