using DependencyInjector.Installers;
using MVVM.Core;
using UnityEngine;

namespace Commands.Core
{
    public class RaiseEventViewModelCommandInstaller : SingleMonoInstaller<ICommand>
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _eventViewModelSO;
        
        protected override ICommand GetData()
        {
            return new RaiseEventViewModelCommand(_eventViewModelSO.GetEventViewModel());
        }
    }
}