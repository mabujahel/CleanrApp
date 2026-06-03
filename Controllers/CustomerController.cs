using CleanrApp.Data;
using CleanrApp.Models;
using CleanrApp.Services;
using CleanrApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notify;
        private readonly AuditService _audit;

        public CustomerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notify, AuditService audit)
        {
            _db = db;
            _userManager = userManager;
            _notify = notify;
            _audit = audit;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index()
        {
            var customer = await _userManager.FindByIdAsync(CurrentUserId);
            if (customer == null) return NotFound();

            var units = await _db.Units
                .Where(u => u.CustomerId == CurrentUserId && u.IsActive)
                .Include(u => u.Building)
                .ToListAsync();

            var activeRequests = await _db.CleaningRequests
                .Where(r => r.CustomerId == CurrentUserId &&
                    r.Status != RequestStatus.AdminApproved &&
                    r.Status != RequestStatus.Cancelled &&
                    r.Status != RequestStatus.Rejected)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var recentRequests = await _db.CleaningRequests
                .Where(r => r.CustomerId == CurrentUserId)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(new CustomerDashboardViewModel
            {
                Customer = customer,
                Units = units,
                ActiveRequests = activeRequests,
                RecentRequests = recentRequests
            });
        }

        public async Task<IActionResult> NewRequest(int? unitId = null)
        {
            var units = await _db.Units
                .Where(u => u.CustomerId == CurrentUserId && u.IsActive)
                .Include(u => u.Building)
                .ToListAsync();
            ViewBag.Units = units;

            return View(new CreateRequestViewModel
            {
                UnitId = unitId ?? 0,
                ScheduledAt = DateTime.Now.AddHours(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewRequest(CreateRequestViewModel vm)
        {
            var customer = await _userManager.FindByIdAsync(CurrentUserId);
            if (customer == null) return NotFound();

            // التحقق من رصيد الزيارات
            if (customer.AllowedVisits > 0 && customer.UsedVisits >= customer.AllowedVisits)
            {
                ModelState.AddModelError("", "لا يوجد رصيد زيارات كافٍ. يرجى التواصل مع الإدارة.");
            }

            // التحقق من عدم وجود طلب مفتوح لنفس الوحدة
            var hasOpenRequest = await _db.CleaningRequests.AnyAsync(r =>
                r.UnitId == vm.UnitId && r.CustomerId == CurrentUserId &&
                r.Status != RequestStatus.AdminApproved &&
                r.Status != RequestStatus.Cancelled &&
                r.Status != RequestStatus.Rejected);

            if (hasOpenRequest)
                ModelState.AddModelError("UnitId", "يوجد طلب مفتوح بالفعل لهذه الوحدة");

            if (!ModelState.IsValid)
            {
                ViewBag.Units = await _db.Units
                    .Where(u => u.CustomerId == CurrentUserId && u.IsActive)
                    .Include(u => u.Building).ToListAsync();
                return View(vm);
            }

            var count = await _db.CleaningRequests.CountAsync() + 1;
            var request = new CleaningRequest
            {
                RequestNumber = $"CLN-{DateTime.Now:yyyy}-{count:D5}",
                CustomerId = CurrentUserId,
                UnitId = vm.UnitId,
                ServiceDescription = vm.ServiceDescription,
                ScheduledAt = vm.ScheduledAt,
                ChangeBedSheet = vm.ChangeBedSheet,
                ChangeLinens = vm.ChangeLinens,
                ChangePillowCases = vm.ChangePillowCases,
                MakeBed = vm.MakeBed,
                CleanBathroom = vm.CleanBathroom,
                CleanKitchen = vm.CleanKitchen,
                Status = RequestStatus.Pending
            };

            _db.CleaningRequests.Add(request);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(CurrentUserId, "إنشاء طلب تنظيف", $"#{request.RequestNumber}", request.Id);
            await _notify.SendToRoleAsync("Admin", "طلب تنظيف جديد",
                $"عميل {customer.FullName} أنشأ طلباً جديداً #{request.RequestNumber}",
                $"/Admin/RequestDetail/{request.Id}");

            TempData["Success"] = $"تم إنشاء طلبك بنجاح برقم {request.RequestNumber}";
            return RedirectToAction("RequestDetail", new { id = request.Id });
        }

        public async Task<IActionResult> RequestDetail(int id)
        {
            var request = await _db.CleaningRequests
                .Where(r => r.Id == id && r.CustomerId == CurrentUserId)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Photos)
                .Include(r => r.Notes.Where(n => !n.IsInternal)).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync();

            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.CustomerId == CurrentUserId);

            if (request == null) return NotFound();
            if (request.Status != RequestStatus.Pending)
            {
                TempData["Error"] = "لا يمكن إلغاء الطلب بعد استلامه من الإدارة";
                return RedirectToAction("RequestDetail", new { id = requestId });
            }

            request.Status = RequestStatus.Cancelled;
            request.CancellationReason = "إلغاء من العميل";
            request.CancelledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = "تم إلغاء الطلب";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> History()
        {
            var requests = await _db.CleaningRequests
                .Where(r => r.CustomerId == CurrentUserId)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(requests);
        }
    }
}
