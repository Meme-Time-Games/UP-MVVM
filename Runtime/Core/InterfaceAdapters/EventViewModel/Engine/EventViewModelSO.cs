using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MVVM.Core
{
    [CreateAssetMenu(fileName = "EventViewModelSO", menuName = "ScriptableObjects/MVVM/EventViewModelSO")]
    public class EventViewModelSO : ScriptableObject
    {
        private IEventViewModel _eventViewModel = null;
        
#if UNITY_EDITOR
        private bool _isSubscribedToPlayModeChanged = false;
#endif

        public IEventViewModel GetEventViewModel()
        {
#if UNITY_EDITOR
            if(!_isSubscribedToPlayModeChanged)
            {
                _isSubscribedToPlayModeChanged = true;
                EditorApplication.playModeStateChanged += Dispose;
            }
#endif
            
            if (ReferenceEquals(_eventViewModel, null))
                _eventViewModel = new EventViewModel();

            return _eventViewModel;
        }

#if UNITY_EDITOR
        private void Dispose(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingEditMode &&
                playModeStateChange != PlayModeStateChange.ExitingPlayMode) 
                return;
            
            EditorApplication.playModeStateChanged -= Dispose;
            
            _isSubscribedToPlayModeChanged = false;
            _eventViewModel = null;
        }
#endif
    }
}