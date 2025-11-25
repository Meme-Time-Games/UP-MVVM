using DependencyInjector.Installers;
using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace Core.Installers
{
    public class MultipleEventsToRaiseEventViewModelControllerInstaller : MultipleMonoInstaller<IController>
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _toRaiseEventViewModelSO;
        [SerializeField] private EventViewModelSO[] _toListenEventViewModelSO;
        protected override IController GetData()
        {
            IEventViewModel[] toListenEventViewModels = new IEventViewModel[_toListenEventViewModelSO.Length];

            for (int i = 0; i < toListenEventViewModels.Length; i++)
            {
                toListenEventViewModels[i] = _toListenEventViewModelSO[i].GetEventViewModel();
            }
            
            return new MultipleEventsToRaiseEventViewModelController(_toRaiseEventViewModelSO.GetEventViewModel(), toListenEventViewModels);
        }
    }
}