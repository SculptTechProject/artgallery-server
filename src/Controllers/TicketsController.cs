using artgallery_server.Infrastructure;
using artgallery_server.Models;
using artgallery_server.Enum;
using artgallery_server.DTO.Ticket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TicketsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("buy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketDto dto)
        {
            var exhibition = await _db.Exhibitions
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == dto.ExhibitionId);
            
            if (exhibition == null)
            {
                return NotFound("Exhibition not found.");
            }

            int soldCount = exhibition.Tickets.Count;

            if (soldCount >= exhibition.Capacity)
            {
                return BadRequest("Brak wolnych miejsc");
            }

            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (customer == null)
            {
                customer = new Customer
                {
                    Email = dto.Email,
                    Username = dto.Email,
                    PasswordHash = "GUEST_NO_PASSWORD",
                    Name = "Guest",
                    Surname = "Guest",
                    ShippingAdress = string.Empty,
                    PhoneNumber = string.Empty
                };
                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            var ticketType = (TicketType)dto.Type;
            decimal price = ticketType == TicketType.Normalny ? 30.00m : 15.00m;

            var ticket = new Ticket
            {
                ExhibitionId = dto.ExhibitionId,
                Email = dto.Email,
                PaymentMethod = dto.PaymentMethod,
                Type = ticketType,
                Price = price,
                UserId = customer.Id,
                PurchaseDate = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Zakup zakończony sukcesem", ticketId = ticket.Id, customerId = customer.Id });
        }

        [HttpGet("/api/v1/exhibitions/{id}/availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailability(int id)
        {
            var exhibition = await _db.Exhibitions
                .Select(e => new 
                {
                    e.Id,
                    e.Capacity,
                    SoldCount = e.Tickets.Count
                })
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exhibition == null)
            {
                return NotFound("Exhibition not found.");
            }

            return Ok(new 
            {
                capacity = exhibition.Capacity,
                sold = exhibition.SoldCount,
                remaining = exhibition.Capacity - exhibition.SoldCount
            });
        }

        /// <summary>
        /// Pobiera wszystkie bilety z pełnymi danymi (Wystawa + Klient).
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<Ticket>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _db.Tickets
                // 1. To dociąga wystawę (w JSON będzie jako pole "exhibition")
                .Include(t => t.Exhibition) 
                
                // 2. TEGO BRAKOWAŁO: To dociąga dane klienta (imię, nazwisko, email)
                .Include(t => t.User)       
                
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            return Ok(tickets);
        }
    }
}
