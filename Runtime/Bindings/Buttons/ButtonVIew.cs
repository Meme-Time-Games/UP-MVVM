using MVVM.Core;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public abstract class ButtonView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Button _button;
        [SerializeField] private EventViewModelSO _eventViewModelSo;

        [Header("Config")] 
        [SerializeField] private bool _isOneTimePressed;

        [Header("Debug")] 
        [SerializeField] private bool _wasPressed;

        private IEventViewModel _eventViewModel;

        private void Awake()
        {
            _eventViewModel = _eventViewModelSo.GetEventViewModel();
            
            Subscribe(_button);
        }

        protected abstract void Subscribe(Button button);

        protected void Raise()
        {
            if (_isOneTimePressed && _wasPressed)
                return;
            
            _wasPressed = true;
            
            _eventViewModel.RaiseEvent();
        }

        private void OnDestroy()
        {
            Dispose(_button);
        }

        protected abstract void Dispose(Button button);
    }
}