using MVVM.Core;
using UnityEngine;

namespace Commands.Core
{
    public class RaiseEventWithBoolValueCommandInstaller : CommandWithTypeInstaller<bool>
    {
        [Header("References")] 
        [SerializeField] private EventViewModelSO _onTrueEventViewModelSO;
        [SerializeField] private EventViewModelSO _onFalseEventViewModelSO;
        
        protected override ICommandWithType<bool> GetData()
        {
            return new RaiseEventWithBoolValueCommand(_onTrueEventViewModelSO.GetEventViewModel(),
                _onFalseEventViewModelSO.GetEventViewModel());
        }
    }
}