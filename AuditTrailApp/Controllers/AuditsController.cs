using AuditTrailApp.Models;
using AuditTrailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuditTrailApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditsController> _logger;

        public AuditsController(IAuditService auditService, ILogger<AuditsController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AuditResponse>> CreateAuditLog([FromBody] AuditRequest request)
        {
            try
            {
                var result = await _auditService.CreateAuditLogAsync(request);
                return CreatedAtAction(nameof(GetAuditLog), new { id = result.AuditId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log");
                return StatusCode(500, "An error occurred while creating the audit log");
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditResponse>>> GetAuditLogs([FromQuery] AuditQueryRequest query)
        {
            try
            {
                var result = await _auditService.GetAuditLogsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, "An error occurred while retrieving audit logs");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditResponse>> GetAuditLog(Guid id)
        {
            try
            {
                var result = await _auditService.GetAuditLogByIdAsync(id);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {AuditId}", id);
                return StatusCode(500, "An error occurred while retrieving the audit log");
            }
        }
    }
}
