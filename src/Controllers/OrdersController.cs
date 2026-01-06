using artgallery_server.Infrastructure;
using artgallery_server.Models;
using artgallery_server.DTO.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto dto)
        {
            if (dto.ArtIds == null || dto.ArtIds.Count == 0)
            {
                return BadRequest("Order must contain at least one art item.");
            }

            // a) Pobierz Arts na podstawie listy ID z bazy.
            var arts = await _db.Arts
                .Where(a => dto.ArtIds.Contains(a.Id))
                .ToListAsync();

            // b) Sprawdź czy istnieją.
            if (arts.Count != dto.ArtIds.Distinct().Count())
            {
                return BadRequest("One or more Art IDs are invalid.");
            }

            // Sprawdź czy klient istnieje
            var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                return BadRequest($"Customer with ID {dto.CustomerId} does not exist.");
            }

            // c) Utwórz obiekt Order.
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0 // Zostanie obliczone niżej
            };

            // d) Utwórz OrderItems na podstawie cen tych dzieł.
            foreach (var art in arts)
            {
                var orderItem = new OrderItem
                {
                    ArtId = art.Id,
                    UnitPriceSnapshot = art.Price
                };
                order.OrderItems.Add(orderItem);
            }

            // e) Oblicz TotalAmount.
            order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPriceSnapshot);

            // f) Zapisz zmiany w bazie asynchronicznie.
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // g) Zwróć 201 Created z ID zamówienia.
            return Created($"/api/v1/orders/{order.Id}", new { id = order.Id });
        }
    }
}
