using artgallery_server.Models;
using artgallery_server.Models.Abstract;
using artgallery_server.Models.Exhibitions;
using artgallery_server.Models.Tag;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}

        public DbSet<User> Users => Set<User>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Art> Arts => Set<Art>();
        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Exhibition> Exhibitions => Set<Exhibition>();
        public DbSet<ExhibitionArt> ExhibitionArts => Set<ExhibitionArt>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<ArtTag> ArtTags => Set<ArtTag>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // --- HIERARCHIA (Table Per Type - TPT) ---
            // Wspólna tabela Users, osobne dla Admin i Customer
            b.Entity<User>().ToTable("Users");
            b.Entity<Admin>().ToTable("Admins");
            b.Entity<Customer>().ToTable("Customers");

            // --- ARTIST ---
            b.Entity<Artist>(e =>
            {
                e.Property(a => a.Name).HasMaxLength(64).IsRequired();
                e.Property(a => a.Surname).HasMaxLength(64).IsRequired();
                e.Property(a => a.Biography).HasMaxLength(1024);
            });

            // --- CATEGORY (Unary) ---
            b.Entity<Category>(e =>
            {
                e.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- ART ---
            b.Entity<Art>(e =>
            {
                e.Property(a => a.Title).IsRequired().HasMaxLength(64);
                e.Property(a => a.Price).HasColumnType("decimal(18,2)");
                
                // Relacja z Kategorią
                e.HasOne(a => a.Category)
                    .WithMany(c => c.Arts)
                    .HasForeignKey(a => a.CategoryId);
            });

            // --- EXHIBITION ART (Many-to-Many) ---
            b.Entity<ExhibitionArt>(e =>
            {
                e.HasKey(ea => new { ea.ExhibitionId, ea.ArtId }); // Klucz złożony
                e.Property(ea => ea.WallLocation).HasMaxLength(64);
            });

            // --- ART TAG (Many-to-Many) ---
            b.Entity<ArtTag>(e =>
            {
                e.HasKey(at => new { at.ArtId, at.TagId }); // Klucz złożony
            });
            
            // --- ORDERS ---
            b.Entity<Order>(e =>
            {
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            });
            
            b.Entity<OrderItem>(e =>
            {
                e.Property(oi => oi.UnitPriceSnapshot).HasColumnType("decimal(18,2)");
            });
        }
    }
}