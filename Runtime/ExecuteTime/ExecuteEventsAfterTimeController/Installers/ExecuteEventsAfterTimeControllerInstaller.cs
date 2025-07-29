using DependencyInjector.Core;
using MVVM.Core.Installers;
using System.Collections.Generic;
using TimerRaiser;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class ExecuteEventsAfterTimeControllerInstaller : ControllerInstaller
    {
        [SerializeField] private float _timeToWait;
        [SerializeField] private EventViewModelSO[] _eventViewModelsToExecute;

        [Inject] private ITimer _timer;

        protected override IController GetController(IEventViewModel eventBindingViewModel)
        {
            List<IEventViewModel> eventsToExecute = new List<IEventViewModel>();

            for (int i = 0; i < _eventViewModelsToExecute.Length; i++)
            {
                eventsToExecute.Add(_eventViewModelsToExecute[i].GetEventViewModel());
            }

            return new ExecuteEventsAfterTimeController(eventBindingViewModel, eventsToExecute, _timer, _timeToWait);
        }

        protected override void InstallServiceInContainer(IDIContainer diContainer, IController serviceInstance)
        {
            diContainer.RegisterAsMultiple(serviceInstance);
        }
    }
}