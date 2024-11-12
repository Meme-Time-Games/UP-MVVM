using MVVM.Core;
using TMPro;
using UnityEngine;

namespace MVVM.Bindings
{
    public class TextBindingView : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private TMP_Text _text;
        [SerializeField] private ReactiveVariableSO<string> _stringReactiveVariableSO;
        
        private ReactiveVariable<string> _stringReactiveVariable;

        private void Awake()
        {
            _stringReactiveVariable = (ReactiveVariable<string>)_stringReactiveVariableSO.GetReactiveVariable();
            _stringReactiveVariable.OnValueChanged += UpdateText;
        }

        private void UpdateText()
        {
            _text.text = _stringReactiveVariable.Value;
        }

        private void OnEnable()
        {
            UpdateText();
        }

        private void OnDestroy()
        {
            _stringReactiveVariable.OnValueChanged -= UpdateText;
        }
    }
}