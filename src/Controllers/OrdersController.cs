using artgallery_server.Infrastructure;
using artgallery_server.Models;
using artgallery_server.DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            // LOGIKA KLIENTA
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (customer == null)
            {
                customer = new Customer
                {
                    Email = dto.Email,
                    Username = dto.Email,
                    PasswordHash = "GUEST_NO_PASSWORD",
                    Name = "Guest",
                    Surname = "Order",
                    ShippingAdress = string.Empty,
                    PhoneNumber = string.Empty
                };
                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            // c) Utwórz obiekt Order.
            var order = new Order
            {
                CustomerId = customer.Id,
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

        /// <summary>
        /// Pobiera zamówienia. Admin widzi wszystkie, zwykły użytkownik tylko swoje.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders()
        {
            var isAdmin = User.IsInRole("Admin");
            
            IQueryable<Order> query = _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Art)
                .Include(o => o.Customer);

            if (!isAdmin)
            {
                // Zwykły użytkownik - tylko jego zamówienia
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }

                var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Username == username || c.Email == username);
                if (customer == null)
                {
                    return Ok(new List<Order>()); // Brak zamówień dla tego użytkownika
                }

                query = query.Where(o => o.CustomerId == customer.Id);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Pobiera wszystkie zamówienia.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }
    }
}
