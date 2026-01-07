using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Application.DTOs.Notifications;
using ApiPetFoundation.Application.DTOs.Common;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId)
        {
            return await _notificationRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(int userId)
        {
            return await _notificationRepository.GetUnreadByUserIdAsync(userId);
        }

        public NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<PagedResult<Notification>> GetPagedAsync(
            int userId,
            int page,
            int pageSize,
            string? type,
            bool? isRead,
            DateTime? createdFrom,
            DateTime? createdTo)
        {
            return await _notificationRepository.GetPagedAsync(
                userId,
                page,
                pageSize,
                type,
                isRead,
                createdFrom,
                createdTo);
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _notificationRepository.GetByIdAsync(id);
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _notificationRepository.AddAsync(notification);
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unread = await _notificationRepository.GetUnreadByUserIdAsync(userId);
            if (unread.Count == 0)
                return 0;

            foreach (var notification in unread)
            {
                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification);
            }

            return unread.Count;
        }

        public async Task DeleteNotificationAsync(Notification notification)
        {
            await _notificationRepository.DeleteAsync(notification);
        }
    }
}
