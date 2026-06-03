using CleanrApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanrApp.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notify;

        public NotificationsController(INotificationService notify) => _notify = notify;

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        [HttpGet]
        public async Task<IActionResult> GetUnread()
        {
            var count = await _notify.GetUnreadCountAsync(CurrentUserId);
            var notifications = await _notify.GetUserNotificationsAsync(CurrentUserId, 10);
            return Json(new { count, notifications });
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notify.MarkAsReadAsync(id, CurrentUserId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notify.MarkAllAsReadAsync(CurrentUserId);
            return Ok();
        }
    }
}
