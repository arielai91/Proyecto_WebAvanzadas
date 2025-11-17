using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Persistence.Contexts
{
    public class AppDbContext
        : IdentityDbContext<AppIdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 👉 Tu tabla Users de dominio (NO es Identity)
        public DbSet<User> UsersDomain { get; set; }

        // 👉 Tus tablas originales del dominio
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetImage> PetImages { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 👈 MUY IMPORTANTE: primero configura Identity
            base.OnModelCreating(builder);

            // --------------------------------------------------------------------
            // 🟩 Tu configuración original para User (dominio)
            // --------------------------------------------------------------------
            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name).IsRequired();

                entity.Property(u => u.IdentityUserId)
                      .IsRequired();
            });

            // --------------------------------------------------------------------
            // 🟩 User → Pet (Admin crea mascotas)
            // --------------------------------------------------------------------
            builder.Entity<Pet>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.PetsCreated)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------------------------------------------------------
            // 🟩 Pet → PetImage (1:N)
            // --------------------------------------------------------------------
            builder.Entity<PetImage>()
                .HasOne(pi => pi.Pet)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // --------------------------------------------------------------------
            // 🟩 AdoptionRequest → User (UserId)
            // --------------------------------------------------------------------
            builder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AdoptionRequests)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------------------------------------------------------
            // 🟩 AdoptionRequest → Pet (PetId)
            // --------------------------------------------------------------------
            builder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Pet)
                .WithMany(p => p.AdoptionRequests)
                .HasForeignKey(ar => ar.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // --------------------------------------------------------------------
            // 🟩 AdoptionRequest → Admin (DecisionById)
            // --------------------------------------------------------------------
            builder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.DecisionBy)
                .WithMany()
                .HasForeignKey(ar => ar.DecisionById)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------------------------------------------------------
            // 🟩 User (dominio) → Notification (1:N)
            // --------------------------------------------------------------------
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasIndex(n => n.UserId);
        }
    }
}
