using System;
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

        private IEventViewModel _eventViewModel;

        private void Awake()
        {
            _eventViewModel = _eventViewModelSo.GetEventViewModel();
            
            Subscribe(_button);
        }

        protected abstract void Subscribe(Button button);

        protected void Raise()
        {
            _eventViewModel.RaiseEvent();
        }

        protected abstract void Dispose(Button button);
    }
}