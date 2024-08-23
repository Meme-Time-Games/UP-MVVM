using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "EventViewModelSO", menuName = "ScriptableObjects/EventViewModelSO")]
    public class EventViewModelSO : ScriptableObject
    {
        private IEventViewModel _eventViewModel = null;

        public IEventViewModel GetEventViewModel()
        {
            if (ReferenceEquals(_eventViewModel, null))
                _eventViewModel = new EventViewModel();

            return _eventViewModel;
        }
    }
}