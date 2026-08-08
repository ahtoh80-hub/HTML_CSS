using System;
using System.Collections.Generic;

namespace IOPointManager.Interfaces
{
    public enum EventType { Info, Warning, Error }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
        public EventType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Source { get; set; }

        public LogEventArgs(string message, EventType type = EventType.Info, string? source = null)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now;
            Source = source;
        }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{Type}] {(Source != null ? $"[{Source}] " : "")}{Message}";
    }

    public interface IEventLogger
    {
        event EventHandler<LogEventArgs> LogAdded;
        void LogInfo(string message, string? source = null);
        void LogWarning(string message, string? source = null);
        void LogError(string message, string? source = null);
        void Log(LogEventArgs args);
        IEnumerable<LogEventArgs> GetLogs();
        void Clear();
    }
}