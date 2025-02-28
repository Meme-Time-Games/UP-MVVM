using MVVM.Core;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class ToggleReactiveVariableNotifier : MonoBehaviour
    {
        [SerializeField] private Toggle _toggleToNotify;
        [SerializeField] private ReactiveVariableSO<bool> _toggleToNotifyReactiveVariableSO;

        private void Awake()
        {
            _toggleToNotify.onValueChanged.AddListener(HandleToggle);
        }

        private void HandleToggle(bool toggleValue)
        {
            _toggleToNotifyReactiveVariableSO.GetReactiveVariable().SetValueAndNotify(toggleValue);
        }

        private void OnDestroy()
        {
            _toggleToNotify.onValueChanged.RemoveListener(HandleToggle);
        }
    }
}