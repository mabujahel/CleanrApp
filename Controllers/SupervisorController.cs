using CleanrApp.Data;
using CleanrApp.Models;
using CleanrApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notify;
        private readonly AuditService _audit;

        public SupervisorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
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
            var requests = await _db.CleaningRequests
                .Where(r => r.SupervisorId == CurrentUserId)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Customer)
                .Include(r => r.Cleaner)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Pending = requests.Count(r => r.Status == RequestStatus.Received || r.Status == RequestStatus.AssignedSupervisor);
            ViewBag.InProgress = requests.Count(r => r.Status == RequestStatus.InProgress);
            ViewBag.Done = requests.Count(r => r.Status == RequestStatus.Done);
            ViewBag.Approved = requests.Count(r => r.Status == RequestStatus.SupervisorApproved || r.Status == RequestStatus.AdminApproved);
            return View(requests);
        }

        public async Task<IActionResult> RequestDetail(int id)
        {
            var request = await _db.CleaningRequests
                .Where(r => r.SupervisorId == CurrentUserId && r.Id == id)
                .Include(r => r.Customer)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Cleaner)
                .Include(r => r.Photos).ThenInclude(p => p.UploadedBy)
                .Include(r => r.Notes.Where(n => !n.IsInternal)).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync();

            if (request == null) return NotFound();

            var cleaners = await _userManager.GetUsersInRoleAsync("Cleaner");
            ViewBag.Cleaners = cleaners.Where(u => u.Status == UserStatus.Active).ToList();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCleaner(int requestId, string cleanerId)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.SupervisorId == CurrentUserId);
            if (request == null) return NotFound();

            request.CleanerId = cleanerId;
            request.Status = RequestStatus.AssignedCleaner;
            await _db.SaveChangesAsync();

            await _notify.SendToUserAsync(cleanerId, "طلب تنظيف جديد",
                $"تم تعيين طلب #{request.RequestNumber} لك", $"/Cleaner/RequestDetail/{request.Id}", request.Id);
            await _audit.LogAsync(CurrentUserId, "تعيين عامل", $"الطلب #{request.RequestNumber}", request.Id);

            TempData["Success"] = "تم تعيين العامل";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int requestId, RequestStatus status)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.SupervisorId == CurrentUserId);
            if (request == null) return NotFound();

            var allowedStatuses = new[] { RequestStatus.Received, RequestStatus.AssignedCleaner, RequestStatus.InProgress, RequestStatus.Done, RequestStatus.SupervisorApproved };
            if (!allowedStatuses.Contains(status)) return BadRequest();

            request.Status = status;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(CurrentUserId, "تغيير حالة الطلب", $"#{request.RequestNumber} → {status}", request.Id);

            TempData["Success"] = "تم تحديث الحالة";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreliminaryApprove(int requestId)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.SupervisorId == CurrentUserId);
            if (request == null) return NotFound();

            var supervisor = await _userManager.FindByIdAsync(CurrentUserId);

            request.Status = RequestStatus.SupervisorApproved;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(CurrentUserId, "اعتماد مبدئي", $"#{request.RequestNumber}", request.Id);
            await _notify.SendToRoleAsync("Admin", "طلب يحتاج اعتماداً نهائياً",
                $"الطلب #{request.RequestNumber} تم اعتماده مبدئياً من المشرف {supervisor?.FullName}",
                $"/Admin/RequestDetail/{request.Id}");

            TempData["Success"] = "تم الاعتماد المبدئي، في انتظار الاعتماد النهائي من الإدارة";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int requestId, string content)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.SupervisorId == CurrentUserId);
            if (request == null) return NotFound();

            _db.RequestNotes.Add(new RequestNote
            {
                CleaningRequestId = requestId,
                Content = content,
                AuthorId = CurrentUserId
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة الملاحظة";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }
    }
}
