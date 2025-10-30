using artgallery_server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace artgallery_server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // JSON
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // HealthChecks /healthz
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        // DB
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        // OpenAPI (dev)
        builder.Services.AddOpenApi();
        
        // Swagger (dev)
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ArtGallery API",
                Version = "v1",
                Description = "Galeria Sztuki – OpenAPI docs"
            });

            var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme, Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        // make sure the directory exists /db
        var cs = builder.Configuration.GetConnectionString("Default")!;
        var ds = new SqliteConnectionStringBuilder(cs).DataSource;
        var full = Path.IsPathRooted(ds) ? ds : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ds));
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir!);

        // Migrations + PRAGMA
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        // Dev
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArtGallery API v1");
                c.RoutePrefix = "docs"; // UI pod /docs
            });
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