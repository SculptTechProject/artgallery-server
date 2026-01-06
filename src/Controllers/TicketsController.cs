using artgallery_server.Infrastructure;
using artgallery_server.Models;
using artgallery_server.Enum;
using artgallery_server.DTO.Ticket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketDto dto)
        {
            // Sprawdź czy wystawa istnieje
            var exhibition = await _db.Exhibitions.FirstOrDefaultAsync(e => e.Id == dto.ExhibitionId);
            if (exhibition == null)
            {
                return BadRequest("Exhibition not found.");
            }

            // Sprawdź czy użytkownik istnieje
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return BadRequest("User not found.");
            }

            // Ustal cenę na podstawie typu
            decimal price = dto.Type == TicketType.Normalny ? 30.00m : 15.00m;

            var ticket = new Ticket
            {
                Type = dto.Type,
                ExhibitionId = dto.ExhibitionId,
                UserId = dto.UserId,
                Price = price
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return Created($"/api/v1/tickets/{ticket.Id}", new { id = ticket.Id, price = ticket.Price });
        }
    }
}
