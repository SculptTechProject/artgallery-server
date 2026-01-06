using artgallery_server.Infrastructure;
using artgallery_server.Models;
using artgallery_server.Enum;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Services
{
    public class DbSeeder
    {
        private readonly AppDbContext _db;

        public DbSeeder(AppDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            if (await _db.Artists.AnyAsync() || await _db.Arts.AnyAsync())
            {
                return; // Baza nie jest pusta
            }

            // Dodaj kategorię domyślną
            var category = new Category { Name = "General" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var artists = new List<Artist>();
            for (int i = 1; i <= 5; i++)
            {
                artists.Add(new Artist
                {
                    Id = Guid.NewGuid(),
                    Name = $"Artist {i}",
                    Surname = $"Surname {i}",
                    Biography = $"Biography for artist {i}. Lorem ipsum dolor sit amet."
                });
            }

            _db.Artists.AddRange(artists);
            await _db.SaveChangesAsync();

            var random = new Random();
            var arts = new List<Art>();
            var artTypes = System.Enum.GetValues<ArtType>();

            for (int i = 1; i <= 10; i++)
            {
                var artist = artists[random.Next(artists.Count)];
                arts.Add(new Art
                {
                    Title = $"Artwork {i}",
                    Description = $"Description for artwork {i}.",
                    Price = Math.Round((decimal)(random.NextDouble() * 1000 + 50), 2),
                    Type = artTypes[random.Next(artTypes.Length)],
                    ArtistId = artist.Id,
                    Artist = null!, // Navigation property
                    CategoryId = category.Id,
                    ImageUrl = $"https://picsum.photos/seed/art{i}/800/600"
                });
            }

            _db.Arts.AddRange(arts);
            await _db.SaveChangesAsync();
        }
    }
}
