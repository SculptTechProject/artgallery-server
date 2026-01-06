using artgallery_server.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using artgallery_server.Utils;
using artgallery_server.Services;

namespace artgallery_server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var signingKey = ValidJwtKey.ValidateJwtKey();
        builder.Services.AddSingleton(signingKey);
        
        // JSON
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
                    policy.WithOrigins(
                            "http://localhost:3000",
                            "http://127.0.0.1:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            );
        });

        // HealthChecks /healthz
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        // DB
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<DbSeeder>();

        // Admin policy (Auth)
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "ArtGalleryBackend",
                    ValidAudience = "artgallery_api",
                    IssuerSigningKey = signingKey,

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    RoleClaimType = ClaimTypes.Role
                };
                opt.RequireHttpsMetadata = false;
            });
        
        builder.Services.AddAuthorization(o =>
        {
            o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        });


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

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Wpisz: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme
                        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>() }
            });

            var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });

        var app = builder.Build();
        
        app.Services.GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();

        // make sure the directory exists /db and /wwwroot/uploads
        var cs = builder.Configuration.GetConnectionString("Default")!;
        var ds = new SqliteConnectionStringBuilder(cs).DataSource;
        var full = Path.IsPathRooted(ds) ? ds : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ds));
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir!);

        var uploadsPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

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
        
        // Serwowanie statycznych plików z wwwroot
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx => ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800",
            ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings = { [".webp"] = "image/webp", [".avif"] = "image/avif" }
            }
        });

        // W kontenerze zwykle NIE wymuszaj https (chyba że masz cert)
        // app.UseHttpsRedirection();

        var logger = app.Logger;
        
        try
        {
            AdminSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();
            logger.LogInformation("AdminSeeder: zakończono pomyślnie.");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DbInitializer.Seed(db);
                
                var dbSeeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                dbSeeder.SeedAsync().GetAwaiter().GetResult();
                logger.LogInformation("DbSeeder: zakończono pomyślnie.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AdminSeeder: błąd inicjalizacji admina.");
        }

        app.UseRouting();
        
        app.UseCors("DevCors");

        app.UseAuthentication();
        app.UseAuthorization();

        // Healthz
        app.MapHealthChecks("/healthz");

        app.MapControllers();
        
        app.Run();
    }
}