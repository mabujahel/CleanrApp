using CleanrApp.Data;
using CleanrApp.Hubs;
using CleanrApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(ApplicationDbContext db, IHubContext<NotificationHub> hub, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _hub = hub;
            _userManager = userManager;
        }

        public async Task SendToUserAsync(string userId, string title, string message, string? link = null, int? requestId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                CleaningRequestId = requestId
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _hub.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                title,
                message,
                link,
                createdAt = notification.CreatedAt
            });
        }

        public async Task SendToRoleAsync(string role, string title, string message, string? link = null)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users)
            {
                await SendToUserAsync(user.Id, title, message, link);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 20)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n != null)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
