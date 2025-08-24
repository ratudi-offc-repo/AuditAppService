using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AuditTrailApp.Models
{
    public class AuditDTOs
    {
    }

    public enum AuditAction
    {
        Created,
        Updated,
        Deleted
    }

    public class AuditRequest
    {
        [Required]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public object? ObjectBefore { get; set; }

        public object? ObjectAfter { get; set; }
    }

    public class AuditResponse
    {
        public Guid AuditId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<FieldChange> Changes { get; set; } = new List<FieldChange>();
    }

    public class FieldChange
    {
        public string FieldName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }

    public class AuditLogEntry
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ChangesJson { get; set; } = string.Empty;
        public string MetadataJson { get; set; } = string.Empty;
        public string? ObjectBeforeJson { get; set; }
        public string? ObjectAfterJson { get; set; }
    }

    public class AuditQueryRequest
    {
        public string? EntityName { get; set; }
        public string? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
