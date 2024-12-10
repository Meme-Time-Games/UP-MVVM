using DependencyInjector.Installers;
using UnityEngine;

namespace MVVM.Core.InterfaceAdapters
{
    public class RaiseEventViewModelControllerInstaller : MultipleMonoInstaller<IController>
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _subscribeToEventViewModelSO;
        [SerializeField] private EventViewModelSO _raiseEventViewModelSO;
        
        protected override IController GetData()
        {
            return new RaiseEventViewModelController(_subscribeToEventViewModelSO.GetEventViewModel(), _raiseEventViewModelSO.GetEventViewModel());
        }
    }
}