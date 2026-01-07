using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.DTOs.Common;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task AddAsync(Notification entity)
        {
            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Notification entity)
        {
            _context.Notifications.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification entity)
        {
            _context.Notifications.Remove(entity);
            await _context.SaveChangesAsync();
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
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(n => n.Type.ToLower() == type.ToLower());

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            if (createdFrom.HasValue)
                query = query.Where(n => n.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(n => n.CreatedAt <= createdTo.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Notification>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
