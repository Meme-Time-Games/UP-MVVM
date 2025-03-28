using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseMultipleEventViewModelControllerInstaller : SingleMonoInstaller<IController>
    {
            [Header("References")] 
            [SerializeField] private EventViewModelSO _subscribeToEventViewModelSO;
            [SerializeField] private EventViewModelSO[] _raiseEventsViewModelSO;
        
            protected override IController GetData()
            {
                IEventViewModel[] eventsViewModel = new IEventViewModel[_raiseEventsViewModelSO.Length];
                
                for (int i = 0; i < _raiseEventsViewModelSO.Length; i++)
                {
                    eventsViewModel[i] = _raiseEventsViewModelSO[i].GetEventViewModel();
                }
                
                return new RaiseMultipleEventViewModelController(_subscribeToEventViewModelSO.GetEventViewModel(), eventsViewModel);
            }
        }
}
