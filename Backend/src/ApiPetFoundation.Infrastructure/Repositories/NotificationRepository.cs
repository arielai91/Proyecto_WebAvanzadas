using ApiPetFoundation.Application.Interfaces.Repositories;
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
    }
}