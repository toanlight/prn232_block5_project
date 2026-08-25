using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetUserNotificationsAsync(int userId, string userRole, int top = 50);
        Task<int> GetUnreadCountAsync(int userId, string userRole);
        Task<Notification?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId, string userRole);
        Task AddAsync(Notification notification);
        Task SaveChangesAsync();
    }
}
