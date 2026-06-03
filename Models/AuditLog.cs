using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        public int? CleaningRequestId { get; set; }
        public CleaningRequest? CleaningRequest { get; set; }
    }
}
