using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOPointManager.Models;

namespace IOPointManager.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByTagAsync(string tag);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string tag);
        Task<int> CountAsync();
        Task<IEnumerable<T>> SearchAsync(string searchTerm);
        Task SaveChangesAsync();
    }

    public interface IIOPointRepository : IRepository<IOPoint>
    {
        Task<IEnumerable<IOPoint>> GetByAreaAsync(int area);
        Task<IEnumerable<IOPoint>> GetBySystemAsync(SystemType system);
        Task<IEnumerable<IOPoint>> GetByStatusAsync(IOPointStatus status);
        Task<IEnumerable<IOPoint>> GetByServiceAsync(string service);
        Task<IEnumerable<IOPoint>> GetInvalidAlarmsAsync();
        Task<IEnumerable<IOPoint>> GetWithDataQualityIssuesAsync(int threshold = 70);
        Task<int> GetCountBySystemAsync(SystemType system);
        Task<int> GetCountByAreaAsync(int area);
        Task<IEnumerable<IOPoint>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<IOPoint>> GetFilteredAsync(Func<IOPoint, bool> filter);
        Task<int> GetCountFilteredAsync(Func<IOPoint, bool> filter);
    }
}