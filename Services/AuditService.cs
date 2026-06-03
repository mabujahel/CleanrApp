using CleanrApp.Data;
using CleanrApp.Models;

namespace CleanrApp.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string userId, string action, string? details = null,
            int? requestId = null, string? oldValue = null, string? newValue = null)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                OldValue = oldValue,
                NewValue = newValue,
                CleaningRequestId = requestId,
                IpAddress = ip
            });
            await _db.SaveChangesAsync();
        }
    }
}
