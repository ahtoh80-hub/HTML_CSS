using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOPointManager.Models;

namespace IOPointManager.Interfaces
{
    public interface IAccessService
    {
        event EventHandler<LogEventArgs>? ProgressReport;
        bool Connect(string connectionString);
        bool IsConnected { get; }
        void Disconnect();
        Task<bool> CreateTableAsync(string tableName);
        Task<bool> ClearTableAsync(string tableName);
        Task<bool> BackupTableAsync(string tableName);
        Task<int> WritePointsAsync(string tableName, IEnumerable<IOPoint> points);
        Task<IEnumerable<IOPoint>> ReadPointsAsync(string tableName);
        Task<IEnumerable<string>> GetTableNamesAsync();
        Task<bool> TableExistsAsync(string tableName);
        Task<bool> DropTableAsync(string tableName);
    }
}