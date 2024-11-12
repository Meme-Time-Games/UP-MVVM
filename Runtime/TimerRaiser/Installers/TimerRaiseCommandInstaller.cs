using Commands.Core;
using DependencyInjector.Core;
using MVVM.Core;
using UnityEngine;

namespace TimerRaiser
{
    public class TimerRaiseCommandInstaller : CommandInstaller
    {
        [Header("Config")] 
        [SerializeField] private  int _time;

        [Header("References")]
        [SerializeField] private EventViewModelSO _onTimerDoneEventViewModelSO;

        [Inject] private ITimer _timer;

        
        protected override ICommand GetData()
        {
            return new TimerRaiseCommand(_timer, _time, _onTimerDoneEventViewModelSO.GetEventViewModel());
        }
    }
}