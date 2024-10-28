using System;
using DependencyInjector.Installers;
using MVVM.Core;
using UnityEngine;

namespace MVVM
{
    public class EventViewModelInjectorInitializer : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _eventViewModelSo; 
        [SerializeField] private BaseMonoInjector _monoInjector;
        
        private IEventViewModel _eventViewModel;
        
        private void Awake()
        {
            _eventViewModel = _eventViewModelSo.GetEventViewModel();
            _eventViewModel.OnEventRaised += Inject;
        }

        private void Inject()
        {
            _monoInjector.InjectAll();
        }

        private void OnDestroy()
        {
            _eventViewModel.OnEventRaised -= Inject;
        }
    }
}