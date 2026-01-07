using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Common;

namespace ApiPetFoundation.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<List<Notification>> GetUnreadByUserIdAsync(int userId);
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
