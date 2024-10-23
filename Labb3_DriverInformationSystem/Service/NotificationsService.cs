using Labb3_DriverInformationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem.Services
{
    public class NotificationsService
    {
        private readonly ApplicationDbContext _context;

        public NotificationsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hämta antalet olästa notifikationer för användaren
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.UserNotifications
                .Where(un => un.UserId == userId && !un.IsRead)
                .CountAsync();
        }

        //// Markera alla notifikationer som lästa för användaren
        //public async Task MarkNotificationsAsReadAsync(string userId)
        //{
        //    var notifications = await _context.UserNotifications
        //        .Where(un => un.UserId == userId && !un.IsRead)
        //        .ToListAsync();

        //    foreach (var notification in notifications)
        //    {
        //        notification.IsRead = true;
        //    }

        //    await _context.SaveChangesAsync();
        //}
    }
}
