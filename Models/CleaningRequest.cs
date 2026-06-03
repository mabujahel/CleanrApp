using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class CleaningRequest
    {
        public int Id { get; set; }

        [Required]
        public string RequestNumber { get; set; } = string.Empty; // CLN-2024-00001

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [MaxLength(1000)]
        public string? ServiceDescription { get; set; }

        // الخدمات الإضافية
        public bool ChangeBedSheet { get; set; } = false;
        public bool ChangeLinens { get; set; } = false;
        public bool ChangePillowCases { get; set; } = false;
        public bool MakeBed { get; set; } = false;
        public bool CleanBathroom { get; set; } = false;
        public bool CleanKitchen { get; set; } = false;

        // المواعيد
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // سبب الإلغاء
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        // ملاحظات بعد الانتهاء من العامل
        public string? CleanerNotes { get; set; }

        // FK Unit
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        // FK Customer
        public string CustomerId { get; set; } = string.Empty;
        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; } = null!;

        // FK Supervisor
        public string? SupervisorId { get; set; }
        [ForeignKey("SupervisorId")]
        public ApplicationUser? Supervisor { get; set; }

        // FK Cleaner
        public string? CleanerId { get; set; }
        [ForeignKey("CleanerId")]
        public ApplicationUser? Cleaner { get; set; }

        // من اعتمد نهائياً
        public string? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public ICollection<RequestPhoto> Photos { get; set; } = new List<RequestPhoto>();
        public ICollection<RequestNote> Notes { get; set; } = new List<RequestNote>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
