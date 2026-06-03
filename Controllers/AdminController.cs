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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notify;
        private readonly AuditService _audit;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notify, AuditService audit)
        {
            _db = db;
            _userManager = userManager;
            _notify = notify;
            _audit = audit;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalRequests = await _db.CleaningRequests.CountAsync(),
                PendingRequests = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.Pending),
                InProgressRequests = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.InProgress),
                CompletedRequests = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.AdminApproved),
                CancelledRequests = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.Cancelled || r.Status == RequestStatus.Rejected),
                TodayRequests = await _db.CleaningRequests.CountAsync(r => r.CreatedAt.Date == DateTime.Today),
                RecentRequests = await _db.CleaningRequests
                    .Include(r => r.Customer)
                    .Include(r => r.Unit).ThenInclude(u => u.Building)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10).ToListAsync()
            };

            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var cleaners = await _userManager.GetUsersInRoleAsync("Cleaner");
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            vm.TotalCustomers = customers.Count;
            vm.TotalCleaners = cleaners.Count;
            vm.TotalSupervisors = supervisors.Count;
            return View(vm);
        }

        // ===== CUSTOMERS =====
        public async Task<IActionResult> Customers()
        {
            var users = await _userManager.GetUsersInRoleAsync("Customer");
            return View(users.OrderBy(u => u.FullName).ToList());
        }

        public IActionResult CreateCustomer() => View(new CreateUserViewModel { Role = "Customer" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber,
                AllowedVisits = vm.AllowedVisits,
                Notes = vm.Notes,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _audit.LogAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
                    "إضافة عميل", $"تم إضافة العميل: {user.FullName}");
                TempData["Success"] = "تم إضافة العميل بنجاح";
                return RedirectToAction("Customers");
            }
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View(vm);
        }

        public async Task<IActionResult> EditCustomer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(new EditUserViewModel
            {
                Id = user.Id, FullName = user.FullName, PhoneNumber = user.PhoneNumber,
                Status = user.Status, AllowedVisits = user.AllowedVisits, Notes = user.Notes
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(EditUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();
            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;
            user.Status = vm.Status;
            user.AllowedVisits = vm.AllowedVisits;
            user.Notes = vm.Notes;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "تم تحديث بيانات العميل";
            return RedirectToAction("Customers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.Status = UserStatus.Inactive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "تم إيقاف العميل";
            return RedirectToAction("Customers");
        }

        // ===== SUPERVISORS =====
        public async Task<IActionResult> Supervisors()
        {
            var users = await _userManager.GetUsersInRoleAsync("Supervisor");
            return View(users.OrderBy(u => u.FullName).ToList());
        }

        public IActionResult CreateSupervisor() => View(new CreateUserViewModel { Role = "Supervisor" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupervisor(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = new ApplicationUser
            {
                UserName = vm.Email, Email = vm.Email, FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber, Notes = vm.Notes, CanFinalApprove = vm.CanFinalApprove,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Supervisor");
                TempData["Success"] = "تم إضافة المشرف بنجاح";
                return RedirectToAction("Supervisors");
            }
            foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
            return View(vm);
        }

        // ===== CLEANERS =====
        public async Task<IActionResult> Cleaners()
        {
            var users = await _userManager.GetUsersInRoleAsync("Cleaner");
            return View(users.OrderBy(u => u.FullName).ToList());
        }

        public IActionResult CreateCleaner() => View(new CreateUserViewModel { Role = "Cleaner" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCleaner(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = new ApplicationUser
            {
                UserName = vm.Email, Email = vm.Email, FullName = vm.FullName,
                PhoneNumber = vm.PhoneNumber, Notes = vm.Notes, EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Cleaner");
                TempData["Success"] = "تم إضافة العامل بنجاح";
                return RedirectToAction("Cleaners");
            }
            foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
            return View(vm);
        }

        // ===== BUILDINGS =====
        public async Task<IActionResult> Buildings()
        {
            var buildings = await _db.Buildings.Include(b => b.Units).ToListAsync();
            return View(buildings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBuilding(string name, string? address)
        {
            _db.Buildings.Add(new Building { Name = name, Address = address });
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة المبنى";
            return RedirectToAction("Buildings");
        }

        // ===== UNITS =====
        public async Task<IActionResult> Units()
        {
            var units = await _db.Units
                .Include(u => u.Building)
                .Include(u => u.Customer)
                .ToListAsync();
            return View(units);
        }

        public async Task<IActionResult> CreateUnit()
        {
            ViewBag.Buildings = await _db.Buildings.Where(b => b.IsActive).ToListAsync();
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            ViewBag.Customers = customers;
            return View(new CreateUnitViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUnit(CreateUnitViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Buildings = await _db.Buildings.Where(b => b.IsActive).ToListAsync();
                var customers = await _userManager.GetUsersInRoleAsync("Customer");
                ViewBag.Customers = customers;
                return View(vm);
            }
            _db.Units.Add(new Unit
            {
                UnitNumber = vm.UnitNumber, UnitType = vm.UnitType, Floor = vm.Floor,
                Description = vm.Description, BuildingId = vm.BuildingId, CustomerId = vm.CustomerId
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة الوحدة";
            return RedirectToAction("Units");
        }

        // ===== REQUESTS =====
        public async Task<IActionResult> Requests(RequestStatus? status = null)
        {
            var query = _db.CleaningRequests
                .Include(r => r.Customer)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Supervisor)
                .Include(r => r.Cleaner)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            ViewBag.CurrentStatus = status;
            return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> RequestDetail(int id)
        {
            var request = await _db.CleaningRequests
                .Include(r => r.Customer)
                .Include(r => r.Unit).ThenInclude(u => u.Building)
                .Include(r => r.Supervisor)
                .Include(r => r.Cleaner)
                .Include(r => r.Photos).ThenInclude(p => p.UploadedBy)
                .Include(r => r.Notes).ThenInclude(n => n.Author)
                .Include(r => r.AuditLogs).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            var cleaners = await _userManager.GetUsersInRoleAsync("Cleaner");
            ViewBag.Supervisors = supervisors.Where(u => u.Status == UserStatus.Active).ToList();
            ViewBag.Cleaners = cleaners.Where(u => u.Status == UserStatus.Active).ToList();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRequest(AssignRequestViewModel vm)
        {
            var request = await _db.CleaningRequests.FindAsync(vm.RequestId);
            if (request == null) return NotFound();

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            if (!string.IsNullOrEmpty(vm.SupervisorId))
            {
                request.SupervisorId = vm.SupervisorId;
                request.Status = RequestStatus.AssignedSupervisor;
                await _notify.SendToUserAsync(vm.SupervisorId, "طلب تنظيف جديد",
                    $"تم تعيين طلب #{request.RequestNumber} لك", $"/Supervisor/RequestDetail/{request.Id}", request.Id);
            }
            if (!string.IsNullOrEmpty(vm.CleanerId))
            {
                request.CleanerId = vm.CleanerId;
                request.Status = RequestStatus.AssignedCleaner;
                await _notify.SendToUserAsync(vm.CleanerId, "طلب تنظيف جديد",
                    $"تم تعيين طلب #{request.RequestNumber} لك", $"/Cleaner/RequestDetail/{request.Id}", request.Id);
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "تعيين طلب", $"تم تعيين الطلب #{request.RequestNumber}", request.Id);
            TempData["Success"] = "تم التعيين بنجاح";
            return RedirectToAction("RequestDetail", new { id = vm.RequestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = await _db.CleaningRequests
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null) return NotFound();

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            request.Status = RequestStatus.AdminApproved;
            request.ApprovedById = adminId;
            request.ApprovedAt = DateTime.UtcNow;

            // خصم زيارة
            var customer = await _userManager.FindByIdAsync(request.CustomerId);
            if (customer != null)
            {
                customer.UsedVisits++;
                await _userManager.UpdateAsync(customer);
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "اعتماد نهائي", $"تم الاعتماد النهائي للطلب #{request.RequestNumber}", request.Id);
            await _notify.SendToUserAsync(request.CustomerId, "تم اعتماد طلبك",
                $"تم الانتهاء من تنظيف وحدتك وإعتماده نهائياً - طلب #{request.RequestNumber}",
                $"/Customer/RequestDetail/{request.Id}", request.Id);

            TempData["Success"] = "تم الاعتماد النهائي وخصم الزيارة";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(CancelRequestViewModel vm)
        {
            var request = await _db.CleaningRequests.FindAsync(vm.RequestId);
            if (request == null) return NotFound();

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            request.Status = RequestStatus.Cancelled;
            request.CancellationReason = vm.Reason;
            request.CancelledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "إلغاء طلب", $"الطلب #{request.RequestNumber} - السبب: {vm.Reason}", request.Id);
            await _notify.SendToUserAsync(request.CustomerId, "تم إلغاء طلبك",
                $"تم إلغاء طلب التنظيف #{request.RequestNumber} - السبب: {vm.Reason}",
                $"/Customer/RequestDetail/{request.Id}", request.Id);

            TempData["Success"] = "تم إلغاء الطلب";
            return RedirectToAction("Requests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int requestId, string content, bool isInternal)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            _db.RequestNotes.Add(new RequestNote
            {
                CleaningRequestId = requestId,
                Content = content,
                IsInternal = isInternal,
                AuthorId = adminId
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة الملاحظة";
            return RedirectToAction("RequestDetail", new { id = requestId });
        }

        // ===== REPORTS =====
        public async Task<IActionResult> Reports()
        {
            ViewBag.TotalRequests = await _db.CleaningRequests.CountAsync();
            ViewBag.TodayRequests = await _db.CleaningRequests.CountAsync(r => r.CreatedAt.Date == DateTime.Today);
            ViewBag.MonthRequests = await _db.CleaningRequests.CountAsync(r => r.CreatedAt.Month == DateTime.Today.Month && r.CreatedAt.Year == DateTime.Today.Year);
            ViewBag.Completed = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.AdminApproved);
            ViewBag.Cancelled = await _db.CleaningRequests.CountAsync(r => r.Status == RequestStatus.Cancelled);

            var cleanerGroups = await _db.CleaningRequests
                .Where(r => r.CleanerId != null && r.Status == RequestStatus.AdminApproved)
                .Include(r => r.Cleaner)
                .GroupBy(r => new { r.CleanerId, r.Cleaner!.FullName })
                .Select(g => new { g.Key.CleanerId, g.Key.FullName, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToListAsync();
            ViewBag.CleanerPerformance = cleanerGroups;

            var allCleaners = await _userManager.GetUsersInRoleAsync("Cleaner");
            ViewBag.AllCleaners = allCleaners;

            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            ViewBag.Customers = customers;

            return View();
        }
    }
}
