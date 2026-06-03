using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class RequestPhoto
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public PhotoType PhotoType { get; set; } = PhotoType.AfterCleaning;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string UploadedById { get; set; } = string.Empty;
        [ForeignKey("UploadedById")]
        public ApplicationUser UploadedBy { get; set; } = null!;

        public int CleaningRequestId { get; set; }
        public CleaningRequest CleaningRequest { get; set; } = null!;
    }
}
