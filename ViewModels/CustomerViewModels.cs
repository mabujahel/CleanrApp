using CleanrApp.Models;
using System.ComponentModel.DataAnnotations;

namespace CleanrApp.ViewModels
{
    public class CreateRequestViewModel
    {
        [Required(ErrorMessage = "الوحدة مطلوبة")]
        [Display(Name = "الوحدة")]
        public int UnitId { get; set; }

        [Display(Name = "وصف الخدمة المطلوبة")]
        public string? ServiceDescription { get; set; }

        [Display(Name = "موعد التنظيف")]
        public DateTime? ScheduledAt { get; set; }

        // الخدمات الإضافية
        [Display(Name = "تغيير مفرش السرير")]
        public bool ChangeBedSheet { get; set; }

        [Display(Name = "تغيير الملايات")]
        public bool ChangeLinens { get; set; }

        [Display(Name = "تغيير أكياس المخدات")]
        public bool ChangePillowCases { get; set; }

        [Display(Name = "ترتيب السرير")]
        public bool MakeBed { get; set; }

        [Display(Name = "تنظيف دورة المياه")]
        public bool CleanBathroom { get; set; }

        [Display(Name = "تنظيف المطبخ")]
        public bool CleanKitchen { get; set; }
    }

    public class CustomerDashboardViewModel
    {
        public ApplicationUser Customer { get; set; } = null!;
        public int RemainingVisits => Customer.AllowedVisits - Customer.UsedVisits;
        public List<Unit> Units { get; set; } = new();
        public List<CleaningRequest> ActiveRequests { get; set; } = new();
        public List<CleaningRequest> RecentRequests { get; set; } = new();
    }
}
