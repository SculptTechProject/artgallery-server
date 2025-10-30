using artgallery_server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Text.Json.Serialization;

namespace artgallery_server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // JSON (enumy jako stringi) + kontrolery
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // HealthChecks (bo mapujesz /healthz)
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(); // opcjonalne, ale przydatne

        // DB
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        // OpenAPI (dev)
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Upewnij się, że katalog dla SQLite istnieje (np. /data)
        var cs = builder.Configuration.GetConnectionString("Default")!;
        var ds = new SqliteConnectionStringBuilder(cs).DataSource;
        var full = Path.IsPathRooted(ds) ? ds : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ds));
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir!);

        // Migracje + PRAGMA
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        // Dev Swagger
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // W kontenerze zwykle NIE wymuszaj https (chyba że masz cert)
        // app.UseHttpsRedirection();

        app.UseAuthorization();

        // Healthz
        app.MapHealthChecks("/healthz");

        app.MapControllers();
        app.Run();
    }
}