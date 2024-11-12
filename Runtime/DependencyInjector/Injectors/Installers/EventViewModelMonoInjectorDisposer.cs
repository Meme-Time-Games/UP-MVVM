using DependencyInjector.Installers;
using MVVM.Core;
using UnityEngine;

namespace MVVM
{
    public class EventViewModelMonoInjectorDisposer : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _eventViewModelSo;
        [SerializeField] private BaseMonoInjector _monoInjector;

        private IEventViewModel _eventViewModel;
        
        private void Awake()
        {
            _eventViewModel = _eventViewModelSo.GetEventViewModel();
            _eventViewModel.OnEventRaised += Dispose;
        }

        private void Dispose()
        {
            _monoInjector.Dispose();
        }

        private void OnDestroy()
        {
            _eventViewModel.OnEventRaised -= Dispose;
        }
    }
}