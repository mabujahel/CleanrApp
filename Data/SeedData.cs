using CleanrApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager  = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context      = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ── Roles ─────────────────────────────────────────────────────────
            foreach (var role in new[] { "Admin", "Supervisor", "Cleaner", "Customer" })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ── Admin ──────────────────────────────────────────────────────────
            var admin = await EnsureUser(userManager, "admin@cleanrapp.com", "Admin@123456",
                new ApplicationUser {
                    UserName = "admin@cleanrapp.com", Email = "admin@cleanrapp.com",
                    FullName = "مدير النظام", EmailConfirmed = true, PhoneNumber = "0500000001",
                    Status = UserStatus.Active
                }, "Admin");

            // ── Supervisors ────────────────────────────────────────────────────
            var sup1 = await EnsureUser(userManager, "supervisor@cleanrapp.com", "Supervisor@123456",
                new ApplicationUser {
                    UserName = "supervisor@cleanrapp.com", Email = "supervisor@cleanrapp.com",
                    FullName = "أحمد المشرف", EmailConfirmed = true, PhoneNumber = "0501234567",
                    Status = UserStatus.Active, CanFinalApprove = false
                }, "Supervisor");

            var sup2 = await EnsureUser(userManager, "supervisor2@cleanrapp.com", "Supervisor@123456",
                new ApplicationUser {
                    UserName = "supervisor2@cleanrapp.com", Email = "supervisor2@cleanrapp.com",
                    FullName = "سارة المشرفة", EmailConfirmed = true, PhoneNumber = "0501234568",
                    Status = UserStatus.Active, CanFinalApprove = true
                }, "Supervisor");

            // ── Cleaners ───────────────────────────────────────────────────────
            var cl1 = await EnsureUser(userManager, "cleaner@cleanrapp.com", "Cleaner@123456",
                new ApplicationUser {
                    UserName = "cleaner@cleanrapp.com", Email = "cleaner@cleanrapp.com",
                    FullName = "محمد العامل", EmailConfirmed = true, PhoneNumber = "0507654321",
                    Status = UserStatus.Active
                }, "Cleaner");

            var cl2 = await EnsureUser(userManager, "cleaner2@cleanrapp.com", "Cleaner@123456",
                new ApplicationUser {
                    UserName = "cleaner2@cleanrapp.com", Email = "cleaner2@cleanrapp.com",
                    FullName = "علي المنظف", EmailConfirmed = true, PhoneNumber = "0507654322",
                    Status = UserStatus.Active
                }, "Cleaner");

            var cl3 = await EnsureUser(userManager, "cleaner3@cleanrapp.com", "Cleaner@123456",
                new ApplicationUser {
                    UserName = "cleaner3@cleanrapp.com", Email = "cleaner3@cleanrapp.com",
                    FullName = "فاطمة المنظفة", EmailConfirmed = true, PhoneNumber = "0507654323",
                    Status = UserStatus.Active
                }, "Cleaner");

            // ── Customers ──────────────────────────────────────────────────────
            var cus1 = await EnsureUser(userManager, "customer@cleanrapp.com", "Customer@123456",
                new ApplicationUser {
                    UserName = "customer@cleanrapp.com", Email = "customer@cleanrapp.com",
                    FullName = "خالد العميل", EmailConfirmed = true, PhoneNumber = "0509876543",
                    Status = UserStatus.Active, AllowedVisits = 20, UsedVisits = 3
                }, "Customer");

            var cus2 = await EnsureUser(userManager, "customer2@cleanrapp.com", "Customer@123456",
                new ApplicationUser {
                    UserName = "customer2@cleanrapp.com", Email = "customer2@cleanrapp.com",
                    FullName = "نورة الفهد", EmailConfirmed = true, PhoneNumber = "0509876544",
                    Status = UserStatus.Active, AllowedVisits = 15, UsedVisits = 7
                }, "Customer");

            var cus3 = await EnsureUser(userManager, "customer3@cleanrapp.com", "Customer@123456",
                new ApplicationUser {
                    UserName = "customer3@cleanrapp.com", Email = "customer3@cleanrapp.com",
                    FullName = "عمر الزهراني", EmailConfirmed = true, PhoneNumber = "0509876545",
                    Status = UserStatus.Active, AllowedVisits = 10, UsedVisits = 1
                }, "Customer");

            var cus4 = await EnsureUser(userManager, "customer4@cleanrapp.com", "Customer@123456",
                new ApplicationUser {
                    UserName = "customer4@cleanrapp.com", Email = "customer4@cleanrapp.com",
                    FullName = "ريم الشمري", EmailConfirmed = true, PhoneNumber = "0509876546",
                    Status = UserStatus.Active, AllowedVisits = 5, UsedVisits = 4
                }, "Customer");

            // ── Buildings & Units ──────────────────────────────────────────────
            if (await context.Buildings.AnyAsync()) return;

            var b1 = new Building { Name = "برج الياسمين",   Address = "الرياض - حي النخيل",     Description = "مجمع سكني فاخر - 12 طابق", IsActive = true };
            var b2 = new Building { Name = "فندق النجوم",    Address = "جدة - حي الزهراء",        Description = "فندق خمس نجوم - 20 طابق",  IsActive = true };
            var b3 = new Building { Name = "مجمع الورود",    Address = "الدمام - حي الفيصلية",    Description = "مجمع سكني متكامل",          IsActive = true };
            var b4 = new Building { Name = "أبراج المملكة",  Address = "الرياض - حي العليا",      Description = "شقق فندقية راقية",          IsActive = true };

            context.Buildings.AddRange(b1, b2, b3, b4);
            await context.SaveChangesAsync();

            // Units
            var units = new List<Unit>
            {
                // برج الياسمين
                new() { UnitNumber="101", UnitType=UnitType.Apartment, Floor=1,  BuildingId=b1.Id, CustomerId=cus1.Id, IsActive=true },
                new() { UnitNumber="102", UnitType=UnitType.Studio,    Floor=1,  BuildingId=b1.Id, CustomerId=cus1.Id, IsActive=true },
                new() { UnitNumber="201", UnitType=UnitType.Apartment, Floor=2,  BuildingId=b1.Id, CustomerId=cus2.Id, IsActive=true },
                new() { UnitNumber="202", UnitType=UnitType.Suite,     Floor=2,  BuildingId=b1.Id, CustomerId=cus2.Id, IsActive=true },
                new() { UnitNumber="301", UnitType=UnitType.Villa,     Floor=3,  BuildingId=b1.Id, CustomerId=cus3.Id, IsActive=true },
                new() { UnitNumber="401", UnitType=UnitType.Apartment, Floor=4,  BuildingId=b1.Id,                     IsActive=true },
                // فندق النجوم
                new() { UnitNumber="F101", UnitType=UnitType.Room,     Floor=1,  BuildingId=b2.Id, CustomerId=cus3.Id, IsActive=true },
                new() { UnitNumber="F102", UnitType=UnitType.Room,     Floor=1,  BuildingId=b2.Id, CustomerId=cus4.Id, IsActive=true },
                new() { UnitNumber="F201", UnitType=UnitType.Suite,    Floor=2,  BuildingId=b2.Id, CustomerId=cus4.Id, IsActive=true },
                new() { UnitNumber="F301", UnitType=UnitType.Suite,    Floor=3,  BuildingId=b2.Id,                     IsActive=true },
                // مجمع الورود
                new() { UnitNumber="W01",  UnitType=UnitType.Apartment,Floor=1,  BuildingId=b3.Id, CustomerId=cus1.Id, IsActive=true },
                new() { UnitNumber="W02",  UnitType=UnitType.Studio,   Floor=1,  BuildingId=b3.Id, CustomerId=cus2.Id, IsActive=true },
                // أبراج المملكة
                new() { UnitNumber="K501", UnitType=UnitType.Suite,    Floor=5,  BuildingId=b4.Id, CustomerId=cus3.Id, IsActive=true },
                new() { UnitNumber="K502", UnitType=UnitType.Apartment,Floor=5,  BuildingId=b4.Id, CustomerId=cus4.Id, IsActive=true },
            };
            context.Units.AddRange(units);
            await context.SaveChangesAsync();

            // Shortcuts
            var u101  = units[0];  // cus1
            var u102  = units[1];  // cus1
            var u201  = units[2];  // cus2
            var u202  = units[3];  // cus2
            var u301  = units[4];  // cus3
            var uF101 = units[6];  // cus3
            var uF102 = units[7];  // cus4
            var uF201 = units[8];  // cus4
            var uW01  = units[10]; // cus1
            var uK501 = units[12]; // cus3
            var uK502 = units[13]; // cus4

            var now = DateTime.UtcNow;

            // ── Cleaning Requests ──────────────────────────────────────────────
            var requests = new List<CleaningRequest>
            {
                // 1. معتمد نهائياً (منذ أسبوع)
                new() {
                    RequestNumber = "CLN-2025-00001",
                    Status        = RequestStatus.AdminApproved,
                    UnitId        = u101.Id, CustomerId   = cus1.Id,
                    SupervisorId  = sup1.Id, CleanerId    = cl1.Id,
                    ServiceDescription = "تنظيف شامل بعد انتهاء الإجازة",
                    ChangeBedSheet=true, ChangeLinens=true, ChangePillowCases=true, MakeBed=true,
                    CleanBathroom=true,  CleanKitchen=true,
                    ScheduledAt   = now.AddDays(-7),
                    StartedAt     = now.AddDays(-7).AddHours(1),
                    CompletedAt   = now.AddDays(-7).AddHours(3),
                    CleanerNotes  = "تم التنظيف الشامل، الوحدة في حالة ممتازة",
                    ApprovedById  = admin.Id,
                    ApprovedAt    = now.AddDays(-7).AddHours(4),
                    CreatedAt     = now.AddDays(-7).AddMinutes(-30),
                },
                // 2. معتمد نهائياً (منذ 5 أيام)
                new() {
                    RequestNumber = "CLN-2025-00002",
                    Status        = RequestStatus.AdminApproved,
                    UnitId        = u201.Id, CustomerId   = cus2.Id,
                    SupervisorId  = sup2.Id, CleanerId    = cl2.Id,
                    ServiceDescription = "تنظيف روتيني أسبوعي",
                    ChangeBedSheet=true, MakeBed=true, CleanBathroom=true,
                    ScheduledAt   = now.AddDays(-5),
                    StartedAt     = now.AddDays(-5).AddHours(2),
                    CompletedAt   = now.AddDays(-5).AddHours(3),
                    CleanerNotes  = "تم التنظيف بنجاح",
                    ApprovedById  = admin.Id,
                    ApprovedAt    = now.AddDays(-5).AddHours(4),
                    CreatedAt     = now.AddDays(-5).AddMinutes(-60),
                },
                // 3. معتمد نهائياً (منذ 3 أيام)
                new() {
                    RequestNumber = "CLN-2025-00003",
                    Status        = RequestStatus.AdminApproved,
                    UnitId        = uF101.Id, CustomerId = cus3.Id,
                    SupervisorId  = sup1.Id,  CleanerId  = cl3.Id,
                    ServiceDescription = "تنظيف الغرفة الفندقية قبل وصول الضيف",
                    ChangeBedSheet=true, ChangeLinens=true, ChangePillowCases=true,
                    MakeBed=true, CleanBathroom=true,
                    ScheduledAt   = now.AddDays(-3),
                    StartedAt     = now.AddDays(-3).AddHours(1),
                    CompletedAt   = now.AddDays(-3).AddHours(2),
                    CleanerNotes  = "الغرفة جاهزة للضيف",
                    ApprovedById  = admin.Id,
                    ApprovedAt    = now.AddDays(-3).AddHours(3),
                    CreatedAt     = now.AddDays(-3).AddMinutes(-45),
                },
                // 4. اعتماد مبدئي (ينتظر الاعتماد النهائي)
                new() {
                    RequestNumber = "CLN-2025-00004",
                    Status        = RequestStatus.SupervisorApproved,
                    UnitId        = u202.Id, CustomerId  = cus2.Id,
                    SupervisorId  = sup2.Id, CleanerId   = cl1.Id,
                    ServiceDescription = "تنظيف الجناح الرئيسي",
                    ChangeBedSheet=true, ChangeLinens=true, MakeBed=true,
                    CleanBathroom=true,  CleanKitchen=true,
                    ScheduledAt   = now.AddDays(-1),
                    StartedAt     = now.AddDays(-1).AddHours(1),
                    CompletedAt   = now.AddDays(-1).AddHours(3),
                    CleanerNotes  = "تم التنظيف الشامل، لاحظت بعض الأضرار في الحمام",
                    CreatedAt     = now.AddDays(-1).AddMinutes(-30),
                },
                // 5. تم الانتهاء (ينتظر مراجعة المشرف)
                new() {
                    RequestNumber = "CLN-2025-00005",
                    Status        = RequestStatus.Done,
                    UnitId        = u301.Id, CustomerId  = cus3.Id,
                    SupervisorId  = sup1.Id, CleanerId   = cl2.Id,
                    ServiceDescription = "تنظيف عميق للفيلا",
                    ChangeBedSheet=true, ChangeLinens=true, ChangePillowCases=true,
                    MakeBed=true, CleanBathroom=true, CleanKitchen=true,
                    ScheduledAt   = now.AddHours(-5),
                    StartedAt     = now.AddHours(-4),
                    CompletedAt   = now.AddHours(-1),
                    CleanerNotes  = "تم التنظيف الشامل للفيلا. المطبخ احتاج عناية إضافية.",
                    CreatedAt     = now.AddHours(-6),
                },
                // 6. جاري التنفيذ
                new() {
                    RequestNumber = "CLN-2025-00006",
                    Status        = RequestStatus.InProgress,
                    UnitId        = uF102.Id, CustomerId = cus4.Id,
                    SupervisorId  = sup1.Id,  CleanerId  = cl1.Id,
                    ServiceDescription = "تنظيف الغرفة الفندقية",
                    ChangeBedSheet=true, CleanBathroom=true,
                    ScheduledAt   = now.AddHours(-2),
                    StartedAt     = now.AddHours(-1),
                    CreatedAt     = now.AddHours(-3),
                },
                // 7. تم تعيين عامل
                new() {
                    RequestNumber = "CLN-2025-00007",
                    Status        = RequestStatus.AssignedCleaner,
                    UnitId        = uF201.Id, CustomerId = cus4.Id,
                    SupervisorId  = sup2.Id,  CleanerId  = cl3.Id,
                    ServiceDescription = "تنظيف الجناح الكبير",
                    ChangeBedSheet=true, ChangeLinens=true, ChangePillowCases=true,
                    MakeBed=true, CleanBathroom=true,
                    ScheduledAt   = now.AddHours(2),
                    CreatedAt     = now.AddHours(-1),
                },
                // 8. تم تعيين مشرف
                new() {
                    RequestNumber = "CLN-2025-00008",
                    Status        = RequestStatus.AssignedSupervisor,
                    UnitId        = u102.Id,  CustomerId  = cus1.Id,
                    SupervisorId  = sup1.Id,
                    ServiceDescription = "تنظيف استوديو + تغيير مفارش",
                    ChangeBedSheet=true, MakeBed=true,
                    ScheduledAt   = now.AddHours(4),
                    CreatedAt     = now.AddMinutes(-30),
                },
                // 9. قيد الانتظار (لم يعين بعد)
                new() {
                    RequestNumber = "CLN-2025-00009",
                    Status        = RequestStatus.Pending,
                    UnitId        = uW01.Id,  CustomerId = cus1.Id,
                    ServiceDescription = "تنظيف شقة مجمع الورود",
                    ChangeBedSheet=true, CleanBathroom=true, CleanKitchen=true,
                    ScheduledAt   = now.AddDays(1),
                    CreatedAt     = now.AddMinutes(-10),
                },
                // 10. قيد الانتظار
                new() {
                    RequestNumber = "CLN-2025-00010",
                    Status        = RequestStatus.Pending,
                    UnitId        = uK501.Id, CustomerId = cus3.Id,
                    ServiceDescription = "تنظيف الجناح قبل وصول ضيوف VIP",
                    ChangeBedSheet=true, ChangeLinens=true, ChangePillowCases=true,
                    MakeBed=true, CleanBathroom=true, CleanKitchen=true,
                    ScheduledAt   = now.AddDays(1).AddHours(6),
                    CreatedAt     = now.AddMinutes(-5),
                },
                // 11. ملغى من العميل
                new() {
                    RequestNumber = "CLN-2025-00011",
                    Status        = RequestStatus.Cancelled,
                    UnitId        = u201.Id, CustomerId = cus2.Id,
                    ServiceDescription = "تنظيف روتيني",
                    CancellationReason = "إلغاء من العميل - تغيير الخطط",
                    CancelledAt   = now.AddDays(-4),
                    CreatedAt     = now.AddDays(-4).AddMinutes(-20),
                },
                // 12. مرفوض من الإدارة
                new() {
                    RequestNumber = "CLN-2025-00012",
                    Status        = RequestStatus.Rejected,
                    UnitId        = uK502.Id, CustomerId = cus4.Id,
                    ServiceDescription = "طلب تنظيف طارئ",
                    CancellationReason = "الوحدة غير متاحة حالياً - يرجى إعادة الجدولة",
                    CancelledAt   = now.AddDays(-2),
                    CreatedAt     = now.AddDays(-2).AddMinutes(-60),
                },
                // 13. معتمد نهائياً قديم (إحصائيات)
                new() {
                    RequestNumber = "CLN-2025-00013",
                    Status        = RequestStatus.AdminApproved,
                    UnitId        = u101.Id, CustomerId   = cus1.Id,
                    SupervisorId  = sup1.Id, CleanerId    = cl2.Id,
                    ChangeBedSheet=true, MakeBed=true,
                    ScheduledAt   = now.AddDays(-14),
                    StartedAt     = now.AddDays(-14).AddHours(1),
                    CompletedAt   = now.AddDays(-14).AddHours(2),
                    ApprovedById  = admin.Id,
                    ApprovedAt    = now.AddDays(-14).AddHours(3),
                    CreatedAt     = now.AddDays(-14).AddMinutes(-30),
                },
                // 14. معتمد نهائياً قديم
                new() {
                    RequestNumber = "CLN-2025-00014",
                    Status        = RequestStatus.AdminApproved,
                    UnitId        = uW01.Id, CustomerId   = cus1.Id,
                    SupervisorId  = sup2.Id, CleanerId    = cl1.Id,
                    ChangeBedSheet=true, ChangeLinens=true, CleanBathroom=true,
                    ScheduledAt   = now.AddDays(-10),
                    StartedAt     = now.AddDays(-10).AddHours(2),
                    CompletedAt   = now.AddDays(-10).AddHours(4),
                    ApprovedById  = admin.Id,
                    ApprovedAt    = now.AddDays(-10).AddHours(5),
                    CreatedAt     = now.AddDays(-10).AddMinutes(-30),
                },
                // 15. تم استلام الطلب
                new() {
                    RequestNumber = "CLN-2025-00015",
                    Status        = RequestStatus.Received,
                    UnitId        = u102.Id, CustomerId  = cus1.Id,
                    ServiceDescription = "تنظيف سريع للاستوديو",
                    MakeBed=true, CleanBathroom=true,
                    ScheduledAt   = now.AddHours(6),
                    CreatedAt     = now.AddMinutes(-45),
                },
            };

            context.CleaningRequests.AddRange(requests);
            await context.SaveChangesAsync();

            // ── Notes ──────────────────────────────────────────────────────────
            var req1 = requests[0];
            var req4 = requests[3];
            var req5 = requests[4];
            var req6 = requests[5];

            context.RequestNotes.AddRange(
                new RequestNote {
                    CleaningRequestId = req1.Id, AuthorId = sup1.Id, IsInternal = false,
                    Content = "تم مراجعة الطلب والتأكد من اكتمال جميع المهام المطلوبة.",
                    CreatedAt = now.AddDays(-7).AddHours(3)
                },
                new RequestNote {
                    CleaningRequestId = req1.Id, AuthorId = admin.Id, IsInternal = true,
                    Content = "ملاحظة داخلية: العميل راضٍ جداً، يُنصح بتعيين نفس العامل دائماً لهذه الوحدة.",
                    CreatedAt = now.AddDays(-7).AddHours(4)
                },
                new RequestNote {
                    CleaningRequestId = req4.Id, AuthorId = sup2.Id, IsInternal = false,
                    Content = "تم مراجعة الصور، الجناح في حالة ممتازة. مرسل للاعتماد النهائي.",
                    CreatedAt = now.AddDays(-1).AddHours(3)
                },
                new RequestNote {
                    CleaningRequestId = req4.Id, AuthorId = sup2.Id, IsInternal = true,
                    Content = "داخلي: يوجد خدش بسيط على الباب - سيُبلَّغ عنه لفريق الصيانة.",
                    CreatedAt = now.AddDays(-1).AddHours(3).AddMinutes(5)
                },
                new RequestNote {
                    CleaningRequestId = req5.Id, AuthorId = cl2.Id, IsInternal = false,
                    Content = "تم الانتهاء من التنظيف. المطبخ احتاج جهداً إضافياً بسبب الدهون المتراكمة.",
                    CreatedAt = now.AddHours(-1)
                },
                new RequestNote {
                    CleaningRequestId = req6.Id, AuthorId = sup1.Id, IsInternal = false,
                    Content = "تم التأكيد على العامل بالبدء فور وصوله للوحدة.",
                    CreatedAt = now.AddHours(-2)
                }
            );

            // ── Audit Logs ─────────────────────────────────────────────────────
            context.AuditLogs.AddRange(
                new AuditLog { UserId=cus1.Id,  Action="إنشاء طلب تنظيف", Details=$"#{req1.RequestNumber}",          CleaningRequestId=req1.Id, CreatedAt=req1.CreatedAt },
                new AuditLog { UserId=admin.Id, Action="تعيين مشرف",       Details=$"#{req1.RequestNumber} → {sup1.FullName}", CleaningRequestId=req1.Id, CreatedAt=req1.CreatedAt.AddMinutes(10) },
                new AuditLog { UserId=sup1.Id,  Action="تعيين عامل",       Details=$"#{req1.RequestNumber} → {cl1.FullName}",  CleaningRequestId=req1.Id, CreatedAt=req1.CreatedAt.AddMinutes(20) },
                new AuditLog { UserId=cl1.Id,   Action="بدء التنظيف",      Details=$"#{req1.RequestNumber}",          CleaningRequestId=req1.Id, CreatedAt=req1.StartedAt ?? now },
                new AuditLog { UserId=cl1.Id,   Action="إنهاء التنظيف",    Details=$"#{req1.RequestNumber}",          CleaningRequestId=req1.Id, CreatedAt=req1.CompletedAt ?? now },
                new AuditLog { UserId=sup1.Id,  Action="اعتماد مبدئي",     Details=$"#{req1.RequestNumber}",          CleaningRequestId=req1.Id, CreatedAt=(req1.CompletedAt ?? now).AddMinutes(30) },
                new AuditLog { UserId=admin.Id, Action="اعتماد نهائي",     Details=$"#{req1.RequestNumber}",          CleaningRequestId=req1.Id, CreatedAt=req1.ApprovedAt ?? now },

                new AuditLog { UserId=cus2.Id,  Action="إنشاء طلب تنظيف", Details=$"#{req4.RequestNumber}",          CleaningRequestId=req4.Id, CreatedAt=req4.CreatedAt },
                new AuditLog { UserId=sup2.Id,  Action="تعيين عامل",       Details=$"#{req4.RequestNumber} → {cl1.FullName}",  CleaningRequestId=req4.Id, CreatedAt=req4.CreatedAt.AddMinutes(15) },
                new AuditLog { UserId=cl1.Id,   Action="بدء التنظيف",      Details=$"#{req4.RequestNumber}",          CleaningRequestId=req4.Id, CreatedAt=req4.StartedAt ?? now },
                new AuditLog { UserId=cl1.Id,   Action="إنهاء التنظيف",    Details=$"#{req4.RequestNumber}",          CleaningRequestId=req4.Id, CreatedAt=req4.CompletedAt ?? now },
                new AuditLog { UserId=sup2.Id,  Action="اعتماد مبدئي",     Details=$"#{req4.RequestNumber}",          CleaningRequestId=req4.Id, CreatedAt=(req4.CompletedAt ?? now).AddMinutes(20) },

                new AuditLog { UserId=cus3.Id,  Action="إنشاء طلب تنظيف", Details=$"#{req5.RequestNumber}",          CleaningRequestId=req5.Id, CreatedAt=req5.CreatedAt },
                new AuditLog { UserId=cl2.Id,   Action="بدء التنظيف",      Details=$"#{req5.RequestNumber}",          CleaningRequestId=req5.Id, CreatedAt=req5.StartedAt ?? now },
                new AuditLog { UserId=cl2.Id,   Action="إنهاء التنظيف",    Details=$"#{req5.RequestNumber}",          CleaningRequestId=req5.Id, CreatedAt=req5.CompletedAt ?? now }
            );

            await context.SaveChangesAsync();
        }

        // Helper: create user if not exists
        private static async Task<ApplicationUser> EnsureUser(
            UserManager<ApplicationUser> userManager,
            string email, string password,
            ApplicationUser user, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}
