using AuditTrailApp.Models;
using System.Collections.Concurrent;

namespace AuditTrailApp.Repositories
{
    public class InMemoryAuditRepository : IAuditRepository
    {
        private readonly List<AuditLogEntry> _auditLogs = new();

        public Task SaveAuditLogAsync(AuditLogEntry entry)
        {
            _auditLogs.Add(entry);
            return Task.CompletedTask;
        }

        public Task<(List<AuditLogEntry> entries, int totalCount)> GetAuditLogsAsync(AuditQueryRequest query)
        {
            var filteredLogs = _auditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.EntityName))
            {
                filteredLogs = filteredLogs.Where(x => x.EntityName.Contains(query.EntityName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(query.UserId))
            {
                filteredLogs = filteredLogs.Where(x => x.UserId == query.UserId);
            }

            if (query.Action.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Action == query.Action.Value);
            }

            if (query.StartDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= query.EndDate.Value);
            }

            // Order by timestamp descending
            filteredLogs = filteredLogs.OrderByDescending(x => x.Timestamp);

            var totalCount = filteredLogs.Count();
            var entries = filteredLogs
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return Task.FromResult((entries, totalCount));
        }
        public Task<AuditLogEntry?> GetAuditLogByIdAsync(Guid id)
        {
            var entry = _auditLogs.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(entry);
        }
    }
}
