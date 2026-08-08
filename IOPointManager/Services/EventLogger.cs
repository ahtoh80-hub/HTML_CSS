using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IOPointManager.Interfaces;

namespace IOPointManager.Services
{
    public class EventLogger : IEventLogger
    {
        private readonly ObservableCollection<LogEventArgs> _logs = new();
        private readonly object _lock = new();
        private const int MaxLogs = 10000;

        public event EventHandler<LogEventArgs>? LogAdded;

        public void LogInfo(string message, string? source = null)
            => Log(new LogEventArgs(message, EventType.Info, source));

        public void LogWarning(string message, string? source = null)
            => Log(new LogEventArgs(message, EventType.Warning, source));

        public void LogError(string message, string? source = null)
            => Log(new LogEventArgs(message, EventType.Error, source));

        public void Log(LogEventArgs args)
        {
            lock (_lock)
            {
                _logs.Insert(0, args);
                if (_logs.Count > MaxLogs)
                    _logs.RemoveAt(_logs.Count - 1);
                LogAdded?.Invoke(this, args);
            }
        }

        public IEnumerable<LogEventArgs> GetLogs()
        {
            lock (_lock) { return _logs.ToList(); }
        }

        public void Clear()
        {
            lock (_lock) { _logs.Clear(); }
        }

        public ObservableCollection<LogEventArgs> GetObservableLogs() => _logs;
    }
}