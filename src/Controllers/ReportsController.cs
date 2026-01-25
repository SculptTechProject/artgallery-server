using artgallery_server.DTO.Report;
using artgallery_server.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Pobiera statystyki dashboardu: całkowity przychód, liczba zamówień, sprzedane bilety, liczba dzieł sztuki.
        /// </summary>
        [HttpGet("dashboard-stats")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalOrdersRevenue = await _db.Orders.SumAsync(o => o.TotalAmount);
            var totalTicketsRevenue = await _db.Tickets.SumAsync(t => t.Price);
            var totalRevenue = totalOrdersRevenue + totalTicketsRevenue;

            var totalOrders = await _db.Orders.CountAsync();
            var ticketsSold = await _db.Tickets.CountAsync();
            var totalArts = await _db.Arts.CountAsync();

            var stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TicketsSold = ticketsSold,
                TotalArts = totalArts
            };

            return Ok(stats);
        }

        /// <summary>
        /// Pobiera dane do wykresu przychodu z ostatnich 30 dni, grupując po dacie (bez godziny).
        /// </summary>
        [HttpGet("revenue-chart")]
        [ProducesResponseType(typeof(List<ChartDataDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueChart()
        {
            var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
            var today = DateTime.UtcNow.Date;

            // Pobierz zamówienia z ostatnich 30 dni i zgrupuj po dacie
            var ordersByDate = await _db.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo && o.OrderDate <= today.AddDays(1).AddTicks(-1))
                .ToListAsync();

            var ordersGrouped = ordersByDate
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                .ToList();

            // Pobierz bilety z ostatnich 30 dni i zgrupuj po dacie
            var ticketsByDate = await _db.Tickets
                .Where(t => t.PurchaseDate >= thirtyDaysAgo && t.PurchaseDate <= today.AddDays(1).AddTicks(-1))
                .ToListAsync();

            var ticketsGrouped = ticketsByDate
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(t => t.Price) })
                .ToList();

            // Połącz dane z zamówień i biletów
            var combinedData = ordersGrouped
                .Concat(ticketsGrouped)
                .GroupBy(x => x.Date)
                .Select(g => new ChartDataDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(x => x.Revenue)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Upewnij się, że wszystkie dni z zakresu są reprezentowane (z zerowym przychodem jeśli brak danych)
            var allDates = Enumerable.Range(0, (today - thirtyDaysAgo).Days + 1)
                .Select(offset => thirtyDaysAgo.AddDays(offset))
                .ToList();

            var result = allDates
                .Select(date => new ChartDataDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Revenue = combinedData.FirstOrDefault(c => c.Date == date.ToString("yyyy-MM-dd"))?.Revenue ?? 0
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Pobiera Top 5 wystaw pod względem liczby sprzedanych biletów wraz z przychodem.
        /// </summary>
        [HttpGet("top-exhibitions")]
        [ProducesResponseType(typeof(List<TopExhibitionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopExhibitions()
        {
            var topExhibitions = await _db.Exhibitions
                .Select(e => new TopExhibitionDto
                {
                    ExhibitionName = e.Name,
                    TicketCount = e.Tickets.Count,
                    Revenue = e.Tickets.Sum(t => t.Price)
                })
                .OrderByDescending(e => e.TicketCount)
                .Take(5)
                .ToListAsync();

            return Ok(topExhibitions);
        }

        /// <summary>
        /// Raport magazynu dzieł: grupowanie po kategorii lub artyście.
        /// Filtry: dateFrom/dateTo (opcjonalne, obecnie nie zawężają danych dzieł), groupBy (category|artist), status (all|available|sold|reserved|onExhibition), category (opcjonalnie).
        /// Uwaga: w aktualnym modelu DB statusy "reserved" i "onExhibition" nie są jawnie przechowywane, więc są traktowane jak "all".
        /// "sold" jest wyznaczane po obecności pozycji w OrderItems.
        /// </summary>
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport(
            [FromQuery] string? dateFrom,
            [FromQuery] string? dateTo,
            [FromQuery] string groupBy = "category",
            [FromQuery] string status = "all",
            [FromQuery] string? category = null)
        {
            // Daty zostawiamy w parametrach (wymóg 2 kryteriów), ale obecnie nie zawężamy po nich danych dzieł.
            // Jeśli chcesz, możemy później zawężać po CreatedAt/AddedAt, ale w modelu Arts tego pola nie widać.
            _ = dateFrom;
            _ = dateTo;

            var soldArtIds = await _db.OrderItems
                .Select(oi => oi.ArtId)
                .Distinct()
                .ToListAsync();

            var artsQuery = _db.Arts
                .Include(a => a.Category)
                .Include(a => a.Artist)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                var cat = category.Trim();
                artsQuery = artsQuery.Where(a => a.Category != null && a.Category.Name.Contains(cat));
            }

            // Status filter
            if (status.Equals("sold", StringComparison.OrdinalIgnoreCase))
            {
                artsQuery = artsQuery.Where(a => soldArtIds.Contains(a.Id));
            }
            else if (status.Equals("available", StringComparison.OrdinalIgnoreCase))
            {
                artsQuery = artsQuery.Where(a => !soldArtIds.Contains(a.Id));
            }
            // reserved / onExhibition / all -> no additional filtering

            var arts = await artsQuery.ToListAsync();

            var grouped = groupBy.Equals("artist", StringComparison.OrdinalIgnoreCase)
                ? arts.GroupBy(a => a.Artist != null ? ($"{a.Artist.Name} {a.Artist.Surname}").Trim() : "Nieznany artysta")
                : arts.GroupBy(a => a.Category != null ? a.Category.Name : "Bez kategorii");

            var result = grouped
                .Select(g => new InventoryGroupRowDto
                {
                    GroupName = g.Key,
                    ArtsCount = g.Count(),
                    TotalValue = g.Sum(x => x.Price),
                    AvgPrice = g.Count() == 0 ? 0 : g.Average(x => x.Price)
                })
                .OrderByDescending(r => r.TotalValue)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Raport pojedynczego zamówienia (formularz).
        /// Query: variant=receipt|invoice (obecnie wpływa tylko na nazwę wariantu; formatowanie robisz po stronie frontu/PDF).
        /// </summary>
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetOrderReport([FromRoute] int orderId, [FromQuery] string variant = "receipt")
        {
            _ = variant;

            var order = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var artIds = order.OrderItems.Select(oi => oi.ArtId).Distinct().ToList();
            var arts = await _db.Arts
                .Where(a => artIds.Contains(a.Id))
                .ToListAsync();

            var items = order.OrderItems
                .Select(oi =>
                {
                    var art = arts.FirstOrDefault(a => a.Id == oi.ArtId);
                    var name = art?.Title ?? $"Dzieło #{oi.ArtId}";
                    var unit = oi.UnitPriceSnapshot;
                    return new OrderReportItemDto
                    {
                        Name = name,
                        Quantity = 1,
                        UnitPrice = unit,
                        Total = unit
                    };
                })
                .ToList();

            var subtotal = items.Sum(i => i.Total);

            var dto = new OrderReportDto
            {
                OrderId = order.Id.ToString(CultureInfo.InvariantCulture),
                CreatedAt = order.OrderDate.ToString("O", CultureInfo.InvariantCulture),
                Status = "OK",
                CustomerName = (order.Customer != null ? ($"{order.Customer.Name} {order.Customer.Surname}").Trim() : null),
                CustomerEmail = order.Customer?.Email,
                PaymentMethod = null,
                Currency = "PLN",
                Subtotal = subtotal,
                Tax = null,
                Total = order.TotalAmount,
                Items = items
            };

            return Ok(dto);
        }

        // DTOs lokalne dla raportów (żeby nie rozbijać teraz na osobne pliki)
        public class InventoryGroupRowDto
        {
            public string GroupName { get; set; } = string.Empty;
            public int ArtsCount { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AvgPrice { get; set; }
        }

        public class OrderReportItemDto
        {
            public string Name { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }

        public class OrderReportDto
        {
            public string OrderId { get; set; } = string.Empty;
            public string CreatedAt { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? CustomerName { get; set; }
            public string? CustomerEmail { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Currency { get; set; }
            public decimal Subtotal { get; set; }
            public decimal? Tax { get; set; }
            public decimal Total { get; set; }
            public List<OrderReportItemDto> Items { get; set; } = new();
        }
    }
}
