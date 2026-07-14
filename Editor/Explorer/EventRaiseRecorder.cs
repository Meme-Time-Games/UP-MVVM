using System;
using System.Collections.Generic;
using MVVM.Core;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class EventRaiseRecorder : IDisposable
    {
        private const int MaximumLogEntries = 100;

        private readonly List<string> _log = new List<string>();

        private EventViewModelSO _watchedEventViewModelSo;
        private IEventViewModel _watchedEventViewModel;

        public Action OnLogChanged { get; set; }

        public IReadOnlyList<string> GetLog()
        {
            return _log;
        }

        public void WatchAsset(EventViewModelSO eventViewModelSo)
        {
            Dispose();

            _watchedEventViewModelSo = eventViewModelSo;
            _watchedEventViewModel = eventViewModelSo.GetEventViewModel();
            _watchedEventViewModel.OnEventRaised += RecordRaise;
        }

        public void Dispose()
        {
            if (ReferenceEquals(_watchedEventViewModel, null))
                return;

            _watchedEventViewModel.OnEventRaised -= RecordRaise;
            _watchedEventViewModel = null;
            _watchedEventViewModelSo = null;
            _log.Clear();
        }

        private void RecordRaise()
        {
            _log.Insert(0, $"[frame {Time.frameCount}] {_watchedEventViewModelSo.name} raised");

            if (_log.Count > MaximumLogEntries)
                _log.RemoveAt(_log.Count - 1);

            OnLogChanged?.Invoke();
        }
    }
}
