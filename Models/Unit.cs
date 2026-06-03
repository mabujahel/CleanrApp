using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanrApp.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UnitNumber { get; set; } = string.Empty;

        public UnitType UnitType { get; set; } = UnitType.Apartment;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Floor { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK Building
        public int BuildingId { get; set; }
        public Building Building { get; set; } = null!;

        // FK Customer (المالك/العميل)
        public string? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser? Customer { get; set; }

        public ICollection<CleaningRequest> CleaningRequests { get; set; } = new List<CleaningRequest>();
    }
}
