using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Notifications;
using ApiPetFoundation.Application.DTOs.Common;

namespace ApiPetFoundation.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadByUserAsync(int userId);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task AddNotificationAsync(Notification notification);
        Task MarkAsReadAsync(Notification notification);
        Task<int> MarkAllAsReadAsync(int userId);
        NotificationResponse MapToResponse(Notification notification);
        Task<PagedResult<Notification>> GetPagedAsync(
            int userId,
            int page,
            int pageSize,
            string? type,
            bool? isRead,
            DateTime? createdFrom,
            DateTime? createdTo);
    }
}
