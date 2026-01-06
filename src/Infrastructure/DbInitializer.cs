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

            // --- KATEGORIE ---
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

            // --- WYSTAWY (Z logicznym podziałem Capacity) ---
            var exhibitions = new List<Exhibition>
            {
                new() { Name = "Światło i Cień", StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(20) },
                new() { Name = "Nowoczesność w PRL", StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-5) },
                new() { Name = "Abstrakcja 2026", StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(40) },
                new() { Name = "Portrety Duszy", StartDate = DateTime.Now.AddDays(-60), EndDate = DateTime.Now.AddDays(10) },
                new() { Name = "Pejzaże Bałtyku", StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(15) }
            };

            // Tutaj magia: ustawiamy pojemność tak, żeby część była wyprzedana
            for (int i = 0; i < exhibitions.Count; i++)
            {
                // Co druga wystawa ma małą pojemność (500), reszta dużą (1000)
                exhibitions[i].Capacity = (i % 2 == 0) ? 1000 : 500;
                
                // Dla celów testowych, możemy też dodać losowe zdjęcia wystaw, jeśli masz takie pole
                exhibitions[i].ImageUrl = $"https://picsum.photos/seed/exhib{i}/1200/600";
            }

            context.Exhibitions.AddRange(exhibitions);
            context.SaveChanges();

            // --- ARTYŚCI ---
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

            // --- DZIEŁA SZTUKI (ARTS) ---
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
                    Price = random.Next(500, 5001),
                    Type = artTypes[random.Next(artTypes.Length)],
                    ArtistId = artists[random.Next(artists.Count)].Id,
                    CategoryId = categories[random.Next(categories.Count)].Id,
                    // Zmiana zdjęcia na większe i z unikalnym seedem 'art{i}'
                    ImageUrl = $"https://picsum.photos/seed/art{i}/800/600"
                });
            }
            context.Arts.AddRange(arts);
            context.SaveChanges();

            // --- MOCK ORDERS & TICKETS ---
            var rng = new Random();

            // 1. Generuj zamówienia (Orders)
            if (context.Orders.Count() < 1000)
            {
                var customers = context.Customers.ToList();
                var artsList = context.Arts.ToList();

                // Jeśli nie ma klientów, utwórz kilku testowych
                if (!customers.Any())
                {
                    var testCustomers = new List<Customer>();
                    var customerNames = new[] { "Jan", "Anna", "Piotr", "Maria", "Tomasz", "Katarzyna", "Marcin", "Magdalena" };
                    var customerSurnames = new[] { "Kowalski", "Nowak", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamińska", "Lewandowski", "Zielińska" };
                    
                    for (int i = 0; i < 8; i++)
                    {
                        testCustomers.Add(new Customer
                        {
                            Email = $"customer{i + 1}@example.com",
                            Username = $"customer{i + 1}",
                            PasswordHash = "GUEST_NO_PASSWORD",
                            Name = customerNames[i],
                            Surname = customerSurnames[i],
                            ShippingAdress = $"ul. Testowa {i + 1}, Warszawa",
                            PhoneNumber = $"+4812345678{i}"
                        });
                    }
                    context.Customers.AddRange(testCustomers);
                    context.SaveChanges();
                    customers = context.Customers.ToList();
                }

                if (artsList.Any() && customers.Any())
                {
                    for (int i = 0; i < 1500; i++)
                    {
                        var orderDate = DateTime.UtcNow.AddDays(-rng.Next(60));
                        var customer = customers[rng.Next(customers.Count)];
                        
                        var artCount = rng.Next(1, 4);
                        var selectedArts = artsList.OrderBy(x => Guid.NewGuid()).Take(artCount).ToList();
                        
                        var totalAmount = selectedArts.Sum(a => a.Price);
                        
                        var order = new Order
                        {
                            CustomerId = customer.Id,
                            OrderDate = orderDate,
                            TotalAmount = totalAmount
                        };
                        
                        foreach (var art in selectedArts)
                        {
                            order.OrderItems.Add(new OrderItem
                            {
                                ArtId = art.Id,
                                UnitPriceSnapshot = art.Price
                            });
                        }
                        
                        context.Orders.Add(order);
                    }
                    context.SaveChanges();
                }
            }

            // 2. Generuj bilety (Tickets)
            if (context.Tickets.Count() < 3000)
            {
                var exhibitionsList = context.Exhibitions.ToList();
                var customers = context.Customers.ToList();

                if (exhibitionsList.Any())
                {
                    for (int i = 0; i < 3000; i++)
                    {
                        var exhibition = exhibitionsList[rng.Next(exhibitionsList.Count)];
                        var purchaseDate = DateTime.UtcNow.AddDays(-rng.Next(60));
                        
                        var ticketType = (TicketType)rng.Next(2);
                        var price = ticketType == TicketType.Normalny ? 50.00m : 25.00m;
                        
                        int? userId = null;
                        if (customers.Any())
                        {
                            if (rng.Next(10) < 7)
                            {
                                userId = customers[rng.Next(customers.Count)].Id;
                            }
                        }
                        
                        var ticket = new Ticket
                        {
                            ExhibitionId = exhibition.Id,
                            PurchaseDate = purchaseDate,
                            Type = ticketType,
                            Price = price,
                            Email = userId.HasValue 
                                ? customers.First(c => c.Id == userId.Value).Email 
                                : $"guest{i}@example.com",
                            PaymentMethod = rng.Next(2) == 0 ? "Karta" : "Gotówka",
                            UserId = userId
                        };
                        
                        context.Tickets.Add(ticket);
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}