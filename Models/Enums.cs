namespace CleanrApp.Models
{
    public enum RequestStatus
    {
        Pending = 1,           // قيد الانتظار
        Received = 2,          // تم استلام الطلب
        AssignedSupervisor = 3, // تم تعيين مشرف
        AssignedCleaner = 4,   // تم تعيين عامل
        InProgress = 5,        // جاري التنفيذ
        Done = 6,              // تم الانتهاء (من العامل)
        SupervisorApproved = 7, // اعتماد مبدئي من المشرف
        AdminApproved = 8,     // اعتماد نهائي
        Cancelled = 9,         // ملغى
        Rejected = 10          // مرفوض
    }

    public enum UnitType
    {
        Apartment = 1,   // شقة
        Room = 2,        // غرفة فندقية
        Villa = 3,       // فيلا
        Studio = 4,      // استوديو
        Suite = 5        // جناح
    }

    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3
    }

    public enum PhotoType
    {
        BeforeCleaning = 1,  // قبل التنظيف
        AfterCleaning = 2,   // بعد التنظيف
        Bed = 3,             // السرير
        Bathroom = 4,        // الحمام
        Kitchen = 5,         // المطبخ
        Other = 6
    }
}
