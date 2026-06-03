using CleanrApp.Data;
using CleanrApp.Models;
using CleanrApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Controllers
{
    [Authorize(Roles = "Cleaner")]
    public class CleanerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notify;
        private readonly AuditService _audit;

        public CleanerController(ApplicationDbContext db, INotificationService notify, AuditService audit)
        {
            _db = db;
            _notify = notify;
            _audit = audit;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index()
        {
            var requests = await _db.CleaningRequests
                .Where(r => r.CleanerId == CurrentUserId &&
                    (r.Status == RequestStatus.AssignedCleaner || r.Status == RequestStatus.InProgress || r.Status == RequestStatus.Done))
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Customer)
                .OrderBy(r => r.ScheduledAt ?? r.CreatedAt)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> RequestDetail(int id)
        {
            var request = await _db.CleaningRequests
                .Where(r => r.CleanerId == CurrentUserId && r.Id == id)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Customer)
                .Include(r => r.Photos)
                .FirstOrDefaultAsync();

            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCleaning(int requestId)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.CleanerId == CurrentUserId);
            if (request == null) return NotFound();

            request.Status = RequestStatus.InProgress;
            request.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(CurrentUserId, "بدء التنظيف", $"#{request.RequestNumber}", request.Id);

            TempData["Success"] = "تم تغيير الحالة إلى جاري التنظيف";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishCleaning(int requestId, string? notes)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.CleanerId == CurrentUserId);
            if (request == null) return NotFound();

            request.Status = RequestStatus.Done;
            request.CompletedAt = DateTime.UtcNow;
            request.CleanerNotes = notes;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(CurrentUserId, "إنهاء التنظيف", $"#{request.RequestNumber}", request.Id);

            if (request.SupervisorId != null)
                await _notify.SendToUserAsync(request.SupervisorId, "عامل أنهى التنظيف",
                    $"العامل أنهى طلب #{request.RequestNumber} - برجاء المراجعة",
                    $"/Supervisor/RequestDetail/{request.Id}", request.Id);

            await _notify.SendToRoleAsync("Admin", "تم الانتهاء من التنظيف",
                $"طلب #{request.RequestNumber} تم تنفيذه - في انتظار المراجعة",
                $"/Admin/RequestDetail/{request.Id}");

            TempData["Success"] = "تم الانتهاء من التنظيف، في انتظار المراجعة";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhotos(int requestId, List<IFormFile> photos, PhotoType photoType)
        {
            var request = await _db.CleaningRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.CleanerId == CurrentUserId);
            if (request == null) return NotFound();

            var uploadsPath = Path.Combine("wwwroot", "uploads", "photos", requestId.ToString());
            Directory.CreateDirectory(uploadsPath);

            foreach (var photo in photos)
            {
                if (photo.Length > 0 && photo.Length <= 10 * 1024 * 1024) // max 10MB
                {
                    var ext = Path.GetExtension(photo.FileName).ToLower();
                    if (new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                    {
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadsPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await photo.CopyToAsync(stream);

                        _db.RequestPhotos.Add(new RequestPhoto
                        {
                            CleaningRequestId = requestId,
                            FilePath = $"/uploads/photos/{requestId}/{fileName}",
                            PhotoType = photoType,
                            UploadedById = CurrentUserId
                        });
                    }
                }
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم رفع الصور بنجاح";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        public async Task<IActionResult> History()
        {
            var requests = await _db.CleaningRequests
                .Where(r => r.CleanerId == CurrentUserId && r.Status == RequestStatus.AdminApproved)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();
            return View(requests);
        }
    }
}
