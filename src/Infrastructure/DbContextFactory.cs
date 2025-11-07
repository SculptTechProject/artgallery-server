using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace artgallery_server.Infrastructure
{
    public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args) 
        {
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json", optional: false);
            configurationBuilder.AddJsonFile($"appsettings.{env}.json", optional: true);
            configurationBuilder.AddEnvironmentVariables();
            var config = configurationBuilder.Build();

            var cs = config.GetConnectionString("Default")
                     ?? "Data Source=./db/artgallery.db";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(cs, x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .Options;

            return new AppDbContext(options);
        }
    }
}
