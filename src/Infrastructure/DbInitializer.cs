using artgallery_server.Enum;
using artgallery_server.Models;
using artgallery_server.Models.Exhibitions;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Infrastructure
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Arts.Any()) return;

            // Categories
            var categories = new List<Category>
            {
                new() { Name = "Malarstwo" },
                new() { Name = "Rzeźba" },
                new() { Name = "Fotografia" },
                new() { Name = "Grafika" },
                new() { Name = "Sztuka Cyfrowa" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Exhibitions
            var exhibitions = new List<Exhibition>
            {
                new() { Name = "Światło i Cień", StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(20) },
                new() { Name = "Nowoczesność w PRL", StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-5) },
                new() { Name = "Abstrakcja 2026", StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(40) },
                new() { Name = "Portrety Duszy", StartDate = DateTime.Now.AddDays(-60), EndDate = DateTime.Now.AddDays(10) },
                new() { Name = "Pejzaże Bałtyku", StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(15) }
            };
            context.Exhibitions.AddRange(exhibitions);
            context.SaveChanges();

            // Artists
            var artistNames = new[] { "Jan", "Maria", "Krzysztof", "Anna", "Paweł", "Zofia", "Marek", "Helena", "Tadeusz", "Barbara" };
            var artistSurnames = new[] { "Kowalski", "Nowak", "Wiśniewska", "Wójcik", "Kowalczyk", "Kamińska", "Lewandowski", "Zielińska", "Szymański", "Woźniak" };
            var bios = new[] 
            { 
                "Mistrz surrealizmu, inspiruje się snami.", 
                "Absolwentka ASP, kocha barwy natury.", 
                "Rzeźbiarz pracujący w metalu i szkle.", 
                "Fotografka dokumentująca życie miasta.", 
                "Pionier sztuki generatywnej w Polsce.",
                "Malarka specjalizująca się w akwareli.",
                "Eksperymentator form przestrzennych.",
                "Twórczyni instalacji multimedialnych.",
                "Klasyk polskiego plakatu.",
                "Młoda krew, street art i graffiti."
            };

            var artists = new List<Artist>();
            for (int i = 0; i < 10; i++)
            {
                artists.Add(new Artist
                {
                    Id = Guid.NewGuid(),
                    Name = artistNames[i],
                    Surname = artistSurnames[i],
                    Biography = bios[i]
                });
            }
            context.Artists.AddRange(artists);
            context.SaveChanges();

            // Arts
            var random = new Random();
            var artTitles = new[] { "Cisza", "Krzyk", "Horyzont", "Melancholia", "Błękit", "Chaos", "Harmonia", "Przebudzenie", "Złoty wiek", "Nocny pociąg" };
            var artTypes = System.Enum.GetValues<ArtType>();

            var arts = new List<Art>();
            for (int i = 0; i < 50; i++)
            {
                var seed = Guid.NewGuid();
                arts.Add(new Art
                {
                    Title = $"{artTitles[random.Next(artTitles.Length)]} #{i + 1}",
                    Description = "Unikalne dzieło sztuki pełne głębi i emocji.",
                    Price = random.Next(100, 50001),
                    Type = artTypes[random.Next(artTypes.Length)],
                    ArtistId = artists[random.Next(artists.Count)].Id,
                    CategoryId = categories[random.Next(categories.Count)].Id,
                    ImageUrl = $"https://picsum.photos/seed/{seed}/400/600"
                });
            }
            context.Arts.AddRange(arts);
            context.SaveChanges();
        }
    }
}
