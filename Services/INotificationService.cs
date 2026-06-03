using CleanrApp.Models;

namespace CleanrApp.Services
{
    public interface INotificationService
    {
        Task SendToUserAsync(string userId, string title, string message, string? link = null, int? requestId = null);
        Task SendToRoleAsync(string role, string title, string message, string? link = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 20);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
