using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly CloneEbayDbContext _context;

        public NotificationRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, string userRole, int top = 50)
        {
            var roleFilter = string.IsNullOrWhiteSpace(userRole) ? "Buyer" : userRole.Trim();

            return await _context.Notifications
                .Where(n => (n.UserId == userId 
                          || n.UserRole == roleFilter 
                          || (n.UserId == null && (n.UserRole == "All" || n.UserRole == null)))
                         && (n.Status == "Sent" || n.Status == "Pending"))
                .OrderByDescending(n => n.CreatedAt)
                .Take(top)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId, string userRole)
        {
            var roleFilter = string.IsNullOrWhiteSpace(userRole) ? "Buyer" : userRole.Trim();

            var notifications = await _context.Notifications
                .Where(n => (n.UserId == userId 
                          || n.UserRole == roleFilter 
                          || (n.UserId == null && (n.UserRole == "All" || n.UserRole == null)))
                         && (n.Status == "Sent" || n.Status == "Pending"))
                .ToListAsync();

            var readNotificationIds = await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            int unread = 0;
            foreach (var n in notifications)
            {
                if (n.UserId == userId)
                {
                    if (!n.IsRead) unread++;
                }
                else
                {
                    if (!readNotificationIds.Contains(n.Id)) unread++;
                }
            }

            return unread;
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null) return;

            if (notification.UserId == userId)
            {
                notification.IsRead = true;
            }
            else
            {
                var alreadyRead = await _context.UserNotificationReads
                    .AnyAsync(r => r.NotificationId == notificationId && r.UserId == userId);

                if (!alreadyRead)
                {
                    _context.UserNotificationReads.Add(new UserNotificationRead
                    {
                        NotificationId = notificationId,
                        UserId = userId,
                        ReadAt = DateTime.UtcNow
                    });
                }
            }

            await SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId, string userRole)
        {
            var roleFilter = string.IsNullOrWhiteSpace(userRole) ? "Buyer" : userRole.Trim();

            var notifications = await _context.Notifications
                .Where(n => (n.UserId == userId 
                          || n.UserRole == roleFilter 
                          || (n.UserId == null && (n.UserRole == "All" || n.UserRole == null)))
                         && (n.Status == "Sent" || n.Status == "Pending"))
                .ToListAsync();

            var readNotificationIds = await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            foreach (var n in notifications)
            {
                if (n.UserId == userId)
                {
                    n.IsRead = true;
                }
                else
                {
                    if (!readNotificationIds.Contains(n.Id))
                    {
                        _context.UserNotificationReads.Add(new UserNotificationRead
                        {
                            NotificationId = n.Id,
                            UserId = userId,
                            ReadAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await SaveChangesAsync();
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
