using AuditTrailApp.Models;
using AuditTrailApp.Repositories;
using System.Text.Json;

namespace AuditTrailApp.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AuditResponse> CreateAuditLogAsync(AuditRequest request)
        {
            try
            {
                var auditId = Guid.NewGuid();
                var timestamp = DateTime.UtcNow;

                var changes = CalculateChanges(request.ObjectBefore, request.ObjectAfter, request.Action);

                var auditEntry = new AuditLogEntry
                {
                    Id = auditId,
                    EntityName = request.EntityName,
                    Action = request.Action,
                    UserId = request.UserId,
                    Timestamp = timestamp,
                    ChangesJson = JsonSerializer.Serialize(changes),
                    ObjectBeforeJson = request.ObjectBefore != null ? JsonSerializer.Serialize(request.ObjectBefore) : null,
                    ObjectAfterJson = request.ObjectAfter != null ? JsonSerializer.Serialize(request.ObjectAfter) : null
                };

                await _repository.SaveAuditLogAsync(auditEntry);

                var response = new AuditResponse
                {
                    AuditId = auditId,
                    EntityName = request.EntityName,
                    Action = request.Action.ToString(),
                    UserId = request.UserId,
                    Timestamp = timestamp,
                    Changes = changes,
                };

                _logger.LogInformation("Audit log created successfully for {EntityName} by user {UserId}",
                    request.EntityName, request.UserId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log for {EntityName}", request.EntityName);
                throw;
            }
        }

        public async Task<PagedResult<AuditResponse>> GetAuditLogsAsync(AuditQueryRequest query)
        {
            try
            {
                var (entries, totalCount) = await _repository.GetAuditLogsAsync(query);

                var responses = entries.Select(entry => new AuditResponse
                {
                    AuditId = entry.Id,
                    EntityName = entry.EntityName,
                    Action = entry.Action.ToString(),
                    UserId = entry.UserId,
                    Timestamp = entry.Timestamp,
                    Changes = JsonSerializer.Deserialize<List<FieldChange>>(entry.ChangesJson) ?? new List<FieldChange>(),
                }).ToList();

                return new PagedResult<AuditResponse>
                {
                    Items = responses,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                throw;
            }
        }

        public async Task<AuditResponse?> GetAuditLogByIdAsync(Guid id)
        {
            try
            {
                var entry = await _repository.GetAuditLogByIdAsync(id);
                if (entry == null) return null;

                return new AuditResponse
                {
                    AuditId = entry.Id,
                    EntityName = entry.EntityName,
                    Action = entry.Action.ToString(),
                    UserId = entry.UserId,
                    Timestamp = entry.Timestamp,
                    Changes = JsonSerializer.Deserialize<List<FieldChange>>(entry.ChangesJson) ?? new List<FieldChange>(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {AuditId}", id);
                throw;
            }
        }

        private List<FieldChange> CalculateChanges(object? objectBefore, object? objectAfter, AuditAction action)
        {
            var changes = new List<FieldChange>();

            switch (action)
            {
                case AuditAction.Created:
                    if (objectAfter != null)
                    {
                        changes.AddRange(GetAllFieldsAsChanges(objectAfter, isCreation: true));
                    }
                    break;

                case AuditAction.Updated:
                    if (objectBefore != null && objectAfter != null)
                    {
                        changes.AddRange(CompareObjects(objectBefore, objectAfter));
                    }
                    break;

                case AuditAction.Deleted:
                    if (objectBefore != null)
                    {
                        changes.AddRange(GetAllFieldsAsChanges(objectBefore, isCreation: false));
                    }
                    break;
            }

            return changes;
        }

        private List<FieldChange> GetAllFieldsAsChanges(object obj, bool isCreation)
        {
            var changes = new List<FieldChange>();
            var properties = GetPropertiesFromObject(obj);

            foreach (var property in properties)
            {
                changes.Add(new FieldChange
                {
                    FieldName = property.Key,
                    OldValue = isCreation ? null : property.Value,
                    NewValue = isCreation ? property.Value : null,
                });
            }

            return changes;
        }

        private List<FieldChange> CompareObjects(object objectBefore, object objectAfter)
        {
            var changes = new List<FieldChange>();
            var beforeProperties = GetPropertiesFromObject(objectBefore);
            var afterProperties = GetPropertiesFromObject(objectAfter);

            // Get all unique keys
            var allKeys = beforeProperties.Keys.Union(afterProperties.Keys).Distinct();

            foreach (var key in allKeys)
            {
                beforeProperties.TryGetValue(key, out var oldValue);
                afterProperties.TryGetValue(key, out var newValue);

                if (!AreValuesEqual(oldValue, newValue))
                {
                    changes.Add(new FieldChange
                    {
                        FieldName = key,
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
            }

            return changes;
        }

        private Dictionary<string, object?> GetPropertiesFromObject(object obj)
        {
            var properties = new Dictionary<string, object?>();

            if (obj is JsonElement jsonElement)
            {
                // Handle JsonElement
                foreach (var property in jsonElement.EnumerateObject())
                {
                    properties[property.Name] = ConvertJsonElementToObject(property.Value);
                }
            }
            else
            {
                // Handle regular object
                var objectProperties = obj.GetType().GetProperties();
                foreach (var property in objectProperties)
                {
                    properties[property.Name] = property.GetValue(obj);
                }
            }

            return properties;
        }

        private object? ConvertJsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElementToObject(item));
                    }
                    return list;
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElementToObject(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        private bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Equals(value2);
        }
    }
}
