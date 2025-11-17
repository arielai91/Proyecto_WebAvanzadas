using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiPetFoundation.Infrastructure.Persistence.Contexts
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var connectionString =
            "Host=aws-1-us-east-2.pooler.supabase.com;" +
            "Port=5432;" +
            "Database=postgres;" +
            "Username=postgres.gucoxlyyznglrfliwlze;" +
            "Password=I8uTgfh4FWfi4Vld;" +
            "Ssl Mode=Require;Trust Server Certificate=true;";

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
