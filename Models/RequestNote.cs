using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class RequestNote
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = false; // ملاحظات داخلية لا تظهر للعميل
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string AuthorId { get; set; } = string.Empty;
        [ForeignKey("AuthorId")]
        public ApplicationUser Author { get; set; } = null!;

        public int CleaningRequestId { get; set; }
        public CleaningRequest CleaningRequest { get; set; } = null!;
    }
}
