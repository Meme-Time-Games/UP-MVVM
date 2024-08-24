using DependencyInjector.Installers;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Core.Installers
{
    public abstract class ControllerInstaller : MonoInstaller<IController>
    {
        [Header("References")]
        [SerializeField] private EventViewModelSO _eventViewModelSO;

        protected override IController GetData()
        {
            return GetController(_eventViewModelSO.GetEventViewModel());
        }

        protected abstract IController GetController(IEventViewModel eventBindingViewModel);
    }
}