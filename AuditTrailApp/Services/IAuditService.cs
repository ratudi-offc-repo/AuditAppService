using AuditTrailApp.Models;

namespace AuditTrailApp.Services
{
    public interface IAuditService
    {
        Task<AuditResponse> CreateAuditLogAsync(AuditRequest request);
        Task<PagedResult<AuditResponse>> GetAuditLogsAsync(AuditQueryRequest query);
        Task<AuditResponse?> GetAuditLogByIdAsync(Guid id);
    }
}
