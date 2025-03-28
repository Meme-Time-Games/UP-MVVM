using DependencyInjector.Installers;
using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace Core.Installers
{
    public class DisposeEventViewModelControllerInstaller : SingleMonoInstaller<RaiseDisposeEventViewModel>
    {
        [SerializeField] private EventViewModelSO _eventViewModelSO;
        protected override RaiseDisposeEventViewModel GetData()
        {
            return new RaiseDisposeEventViewModel(_eventViewModelSO.GetEventViewModel());
        }
    }
}