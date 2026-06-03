using Microsoft.AspNetCore.Identity;

namespace CleanrApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber2 { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfilePhoto { get; set; }
        public string? Notes { get; set; }

        // للعملاء فقط
        public int AllowedVisits { get; set; } = 0;
        public int UsedVisits { get; set; } = 0;

        // للمشرف: هل يمكنه الاعتماد النهائي؟
        public bool CanFinalApprove { get; set; } = false;

        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<CleaningRequest> CustomerRequests { get; set; } = new List<CleaningRequest>();
        public ICollection<CleaningRequest> SupervisorRequests { get; set; } = new List<CleaningRequest>();
        public ICollection<CleaningRequest> CleanerRequests { get; set; } = new List<CleaningRequest>();
    }
}
