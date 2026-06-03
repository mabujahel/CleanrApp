using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        public int? CleaningRequestId { get; set; }
        public CleaningRequest? CleaningRequest { get; set; }
    }
}
