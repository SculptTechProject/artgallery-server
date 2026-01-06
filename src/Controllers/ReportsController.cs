using artgallery_server.DTO.Report;
using artgallery_server.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
