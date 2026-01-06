using artgallery_server.DTO.Exhibition;
using artgallery_server.Infrastructure;
using artgallery_server.Models.Exhibitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/exhibitions")]
    public class ExhibitionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ExhibitionsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Exhibitions.AsNoTracking()
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.ImageUrl,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    e.Capacity,
                    SoldCount = e.Tickets.Count(),
                    IsSoldOut = e.Tickets.Count() >= e.Capacity
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var exhibition = await _db.Exhibitions.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.ImageUrl,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    e.Capacity,
                    SoldCount = e.Tickets.Count(),
                    IsSoldOut = e.Tickets.Count() >= e.Capacity
                })
                .FirstOrDefaultAsync();

            if (exhibition == null)
            {
                return NotFound();
            }

            return Ok(exhibition);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ExhibitionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExhibitionDto>> Create([FromBody] CreateExhibitionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = new Exhibition
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Capacity = dto.Capacity
            };

            _db.Exhibitions.Add(entity);
            await _db.SaveChangesAsync();

            var result = new ExhibitionDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.ImageUrl,
                entity.StartDate,
                entity.EndDate,
                entity.Capacity
            );

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ExhibitionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExhibitionDto>> Update(int id, [FromBody] UpdateExhibitionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _db.Exhibitions.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.ImageUrl = dto.ImageUrl;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Capacity = dto.Capacity;

            await _db.SaveChangesAsync();

            var result = new ExhibitionDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.ImageUrl,
                entity.StartDate,
                entity.EndDate,
                entity.Capacity
            );

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Exhibitions.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _db.Exhibitions.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
