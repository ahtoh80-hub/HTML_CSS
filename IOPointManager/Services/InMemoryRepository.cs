using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOPointManager.Interfaces;
using IOPointManager.Models;

namespace IOPointManager.Services
{
    public class InMemoryRepository : IIOPointRepository
    {
        private readonly List<IOPoint> _points = new();
        private readonly HashSet<string> _tags = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        public event EventHandler<LogEventArgs>? LogEvent;

        public Task<IEnumerable<IOPoint>> GetAllAsync()
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted).AsEnumerable());
        }

        public Task<IOPoint?> GetByIdAsync(Guid id)
        {
            lock (_lock)
                return Task.FromResult(_points.FirstOrDefault(p => p.Id == id && !p.IsDeleted));
        }

        public Task<IOPoint?> GetByTagAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return Task.FromResult<IOPoint?>(null);

            lock (_lock)
                return Task.FromResult(_points.FirstOrDefault(p => 
                    string.Equals(p.Tag, tag, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted));
        }

        public Task AddAsync(IOPoint entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Tag))
                throw new ArgumentException("Tag не может быть пустым");

            lock (_lock)
            {
                string? tag = entity.Tag;
                
                if (!string.IsNullOrEmpty(tag) && _tags.Contains(tag))
                    throw new InvalidOperationException($"Точка с тегом '{tag}' уже существует");

                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.Now;
                entity.Version = 1;
                entity.IsDeleted = false;

                _points.Add(entity);
                if (!string.IsNullOrEmpty(tag))
                    _tags.Add(tag);

                LogEvent?.Invoke(this, new LogEventArgs($"Точка '{tag}' добавлена", EventType.Info, "Repository"));
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(IOPoint entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lock)
            {
                var existing = _points.FirstOrDefault(p => p.Id == entity.Id);
                if (existing == null)
                    throw new InvalidOperationException($"Точка с ID '{entity.Id}' не найдена");

                string? oldTag = existing.Tag;
                string? newTag = entity.Tag;

                if (!string.Equals(oldTag, newTag, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(newTag) && _tags.Contains(newTag))
                        throw new InvalidOperationException($"Точка с тегом '{newTag}' уже существует");
                    
                    if (!string.IsNullOrEmpty(oldTag))
                        _tags.Remove(oldTag);
                    
                    if (!string.IsNullOrEmpty(newTag))
                        _tags.Add(newTag);
                }

                var index = _points.IndexOf(existing);
                entity.ModifiedAt = DateTime.Now;
                entity.Version = existing.Version + 1;
                _points[index] = entity;

                LogEvent?.Invoke(this, new LogEventArgs($"Точка '{newTag}' обновлена", EventType.Info, "Repository"));
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            lock (_lock)
            {
                var point = _points.FirstOrDefault(p => p.Id == id);
                if (point != null)
                {
                    point.IsDeleted = true;
                    point.DeletedAt = DateTime.Now;
                    
                    if (!string.IsNullOrEmpty(point.Tag))
                        _tags.Remove(point.Tag);
                        
                    LogEvent?.Invoke(this, new LogEventArgs($"Точка '{point.Tag}' удалена", EventType.Warning, "Repository"));
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return Task.FromResult(false);

            lock (_lock)
                return Task.FromResult(_tags.Contains(tag));
        }

        public Task<int> CountAsync()
        {
            lock (_lock)
                return Task.FromResult(_points.Count(p => !p.IsDeleted));
        }

        public Task<IEnumerable<IOPoint>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllAsync();

            lock (_lock)
            {
                var term = searchTerm.ToLowerInvariant();
                var results = _points.Where(p => !p.IsDeleted && (
                    (p.Tag?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Service?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.ServiceEnglish?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.InstrumentType?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Title?.ToLowerInvariant().Contains(term) ?? false)
                ));
                return Task.FromResult(results.AsEnumerable());
            }
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public Task<IEnumerable<IOPoint>> GetByAreaAsync(int area)
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted && p.Area == area).AsEnumerable());
        }

        public Task<IEnumerable<IOPoint>> GetBySystemAsync(SystemType system)
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted && p.System == system).AsEnumerable());
        }

        public Task<IEnumerable<IOPoint>> GetByStatusAsync(IOPointStatus status)
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted && p.Status == status).AsEnumerable());
        }

        public Task<IEnumerable<IOPoint>> GetByServiceAsync(string service)
        {
            if (string.IsNullOrWhiteSpace(service))
                return GetAllAsync();

            lock (_lock)
            {
                var term = service.ToLowerInvariant();
                return Task.FromResult(_points.Where(p => !p.IsDeleted && 
                    (p.Service?.ToLowerInvariant().Contains(term) ?? false)).AsEnumerable());
            }
        }

        public Task<IEnumerable<IOPoint>> GetInvalidAlarmsAsync()
        {
            lock (_lock)
            {
                var result = new List<IOPoint>();
                foreach (var p in _points.Where(p => !p.IsDeleted))
                {
                    var validation = p.ValidateAlarmHierarchy();
                    if (!validation.IsValid)
                        result.Add(p);
                }
                return Task.FromResult(result.AsEnumerable());
            }
        }

        public Task<IEnumerable<IOPoint>> GetWithDataQualityIssuesAsync(int threshold = 70)
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted && p.DataQualityScore < threshold).AsEnumerable());
        }

        public Task<int> GetCountBySystemAsync(SystemType system)
        {
            lock (_lock)
                return Task.FromResult(_points.Count(p => !p.IsDeleted && p.System == system));
        }

        public Task<int> GetCountByAreaAsync(int area)
        {
            lock (_lock)
                return Task.FromResult(_points.Count(p => !p.IsDeleted && p.Area == area));
        }

        public Task<IEnumerable<IOPoint>> GetPagedAsync(int pageNumber, int pageSize)
        {
            lock (_lock)
            {
                var items = _points.Where(p => !p.IsDeleted)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                return Task.FromResult(items.AsEnumerable());
            }
        }

        public Task<IEnumerable<IOPoint>> GetFilteredAsync(Func<IOPoint, bool> filter)
        {
            lock (_lock)
                return Task.FromResult(_points.Where(p => !p.IsDeleted && filter(p)).AsEnumerable());
        }

        public Task<int> GetCountFilteredAsync(Func<IOPoint, bool> filter)
        {
            lock (_lock)
                return Task.FromResult(_points.Count(p => !p.IsDeleted && filter(p)));
        }

        public void AddRange(IEnumerable<IOPoint> points)
        {
            lock (_lock)
            {
                foreach (var point in points)
                {
                    if (string.IsNullOrWhiteSpace(point.Tag))
                        continue;
                    
                    if (_tags.Contains(point.Tag))
                        continue;

                    point.Id = Guid.NewGuid();
                    point.CreatedAt = DateTime.Now;
                    point.Version = 1;
                    point.IsDeleted = false;

                    _points.Add(point);
                    _tags.Add(point.Tag);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _points.Clear();
                _tags.Clear();
            }
        }
    }
}