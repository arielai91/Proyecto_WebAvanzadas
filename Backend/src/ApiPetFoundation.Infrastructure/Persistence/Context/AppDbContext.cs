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
        private const string AuthSchema = "identity";
        private const string DomainSchema = "app";

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
            builder.HasDefaultSchema(AuthSchema);
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users", DomainSchema);
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired();
                entity.Property(u => u.IdentityUserId).IsRequired();
            });

            builder.Entity<Pet>(entity =>
            {
                entity.ToTable("Pets", DomainSchema);
                entity.HasOne(p => p.CreatedBy)
                    .WithMany(u => u.PetsCreated)
                    .HasForeignKey(p => p.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PetImage>(entity =>
            {
                entity.ToTable("PetImages", DomainSchema);
                entity.HasOne(pi => pi.Pet)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AdoptionRequest>(entity =>
            {
                entity.ToTable("AdoptionRequests", DomainSchema);
                entity.HasOne(ar => ar.User)
                    .WithMany(u => u.AdoptionRequests)
                    .HasForeignKey(ar => ar.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ar => ar.Pet)
                    .WithMany(p => p.AdoptionRequests)
                    .HasForeignKey(ar => ar.PetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ar => ar.DecisionBy)
                    .WithMany()
                    .HasForeignKey(ar => ar.DecisionById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications", DomainSchema);
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(n => n.UserId);
            });
        }
    }
}



