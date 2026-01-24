using System;
using System.Collections;
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
        [SerializeField] private float _timeBetweenPress = 0.5f;

        [Header("Debug")] 
        [SerializeField] private bool _wasPressed;
        [SerializeField] private bool _canBePressed = true;

        private IEventViewModel _eventViewModel;
        private WaitForSeconds _timeBetweenPressWaitForSeconds;

        private void Awake()
        {
            _eventViewModel = _eventViewModelSo.GetEventViewModel();
            _timeBetweenPressWaitForSeconds = new WaitForSeconds(_timeBetweenPress);
            
            Subscribe(_button);
        }

        protected abstract void Subscribe(Button button);

        protected void Raise()
        {
            if (!_canBePressed) 
                return;
            
            if (_isOneTimePressed && _wasPressed)
                return;
            
            _wasPressed = true;
            _canBePressed = false;
            
            _eventViewModel.RaiseEvent();

            if(gameObject.activeInHierarchy)
                StartCoroutine(ResetCanBePressed());
        }

        private IEnumerator ResetCanBePressed()
        {
            yield return _timeBetweenPressWaitForSeconds;
            _canBePressed = true;
        }

        private void OnDisable()
        {
            _canBePressed = true;
        }

        private void OnDestroy()
        {
            Dispose(_button);
        }

        protected abstract void Dispose(Button button);
    }
}