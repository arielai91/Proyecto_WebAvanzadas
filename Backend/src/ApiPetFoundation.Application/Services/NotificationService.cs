using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _notificationRepository.GetAllAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _notificationRepository.GetByIdAsync(id);
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _notificationRepository.AddAsync(notification);
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task DeleteNotificationAsync(Notification notification)
        {
            await _notificationRepository.DeleteAsync(notification);
        }
    }
}
