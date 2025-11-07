using artgallery_server.Enum;
using artgallery_server.Models;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}
        public DbSet<Art> Arts => Set<Art>();
        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Admin> Admins => Set<Admin>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Art
            b.Entity<Art>(e =>
            {
                e.ToTable("arts");
                e.HasKey(a => a.Id);
                e.Property(a => a.Id).ValueGeneratedOnAdd();
                e.Property(a => a.Title).IsRequired().HasMaxLength(64);
                e.Property(a => a.Description).HasMaxLength(1000);
                e.Property(a => a.ImageUrl).IsRequired().HasMaxLength(256);
                e.Property(a => a.ArtistId).IsRequired();
                
                e.Property(a => a.Type).IsRequired()
                    .HasConversion<String>()
                    .HasMaxLength(32);
                
                e.HasOne(a => a.Artist)
                    .WithMany(x => x.Arts)
                    .HasForeignKey(a => a.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Artist
            b.Entity<Artist>(e =>
            {
                e.ToTable("artists");
                e.HasKey(a => a.Id);
                e.Property(a => a.Id).ValueGeneratedOnAdd();
                e.Property(a => a.Name)
                    .HasMaxLength(64)
                    .IsRequired();
                e.Property(a => a.Surname)
                    .HasMaxLength(64)
                    .IsRequired();

                e.Property(a => a.Biography)
                    .HasMaxLength(1024)
                    .IsRequired();
               
                e.HasMany(a => a.Arts)
                    .WithOne(a => a.Artist)
                    .HasForeignKey(a => a.ArtistId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Admin
            b.Entity<Admin>(e =>
            {
                e.ToTable("admin");
                e.HasKey(a => a.Id);
                e.Property(a => a.Id).ValueGeneratedOnAdd();
                e.HasIndex(a => a.Username).IsUnique();
                e.Property(a => a.PasswordHash).IsRequired();
                e.Property(a => a.IsActive);
                e.Property(a => a.CreatedAtUtc);
                e.Property(a => a.Role).IsRequired();
            });
        }
    }
}
