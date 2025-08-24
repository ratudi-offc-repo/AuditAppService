using AuditTrailApp.Models;

namespace AuditTrailApp.Repositories
{
    public interface IAuditRepository
    {
        Task SaveAuditLogAsync(AuditLogEntry entry);
        Task<(List<AuditLogEntry> entries, int totalCount)> GetAuditLogsAsync(AuditQueryRequest query);
        Task<AuditLogEntry?> GetAuditLogByIdAsync(Guid id);
    }
}
