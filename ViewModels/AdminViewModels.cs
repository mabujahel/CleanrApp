using CleanrApp.Models;
using System.ComponentModel.DataAnnotations;

namespace CleanrApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalCleaners { get; set; }
        public int TotalSupervisors { get; set; }
        public int TodayRequests { get; set; }
        public List<CleaningRequest> RecentRequests { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(8)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "الدور")]
        public string Role { get; set; } = "Customer";

        [Display(Name = "عدد الزيارات المسموح بها")]
        public int AllowedVisits { get; set; } = 0;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "صلاحية الاعتماد النهائي")]
        public bool CanFinalApprove { get; set; } = false;
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "رقم الجوال")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "الحالة")]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [Display(Name = "عدد الزيارات المسموح بها")]
        public int AllowedVisits { get; set; } = 0;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "صلاحية الاعتماد النهائي")]
        public bool CanFinalApprove { get; set; } = false;
    }

    public class CreateUnitViewModel
    {
        [Required(ErrorMessage = "رقم الوحدة مطلوب")]
        [Display(Name = "رقم الوحدة")]
        public string UnitNumber { get; set; } = string.Empty;

        [Display(Name = "نوع الوحدة")]
        public UnitType UnitType { get; set; } = UnitType.Apartment;

        [Display(Name = "الطابق")]
        public int Floor { get; set; } = 1;

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "المبنى مطلوب")]
        [Display(Name = "المبنى")]
        public int BuildingId { get; set; }

        [Display(Name = "العميل")]
        public string? CustomerId { get; set; }
    }

    public class AssignRequestViewModel
    {
        public int RequestId { get; set; }
        public string? SupervisorId { get; set; }
        public string? CleanerId { get; set; }
    }

    public class CancelRequestViewModel
    {
        public int RequestId { get; set; }

        [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
        [Display(Name = "سبب الإلغاء")]
        public string Reason { get; set; } = string.Empty;
    }
}
