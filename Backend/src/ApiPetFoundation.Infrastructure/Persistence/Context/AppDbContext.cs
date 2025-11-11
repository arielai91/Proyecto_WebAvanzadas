using Microsoft.EntityFrameworkCore;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Infrastructure.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 🗂️ Tablas del dominio
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetImage> PetImages { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // ⚙️ Configuración de relaciones y restricciones
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User → Pet (Admin crea mascotas)
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.PetsCreated)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Pet → PetImage (1:N)
            modelBuilder.Entity<PetImage>()
                .HasOne(pi => pi.Pet)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // AdoptionRequest → User (UserId)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AdoptionRequests)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //  AdoptionRequest → Pet (PetId)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany(p => p.AdoptionRequests)
                .HasForeignKey(ar => ar.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // AdoptionRequest → Admin (DecisionById)
            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.DecisionBy)
                .WithMany()
                .HasForeignKey(ar => ar.DecisionById)
                .OnDelete(DeleteBehavior.Restrict);

            // User → Notification (1:N)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId);
        }

    }
}
