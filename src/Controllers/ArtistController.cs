using artgallery_server.DTO.Art;
using artgallery_server.DTO.Artist;
using artgallery_server.Infrastructure;
using artgallery_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ArtistController : ControllerBase
    {
        // Connection to database
        private readonly AppDbContext _db;
        public ArtistController(AppDbContext db) => _db = db;
        
        // Add artist endpoint
        // POST api/v1/artists
        [HttpPost]
        public async Task<ActionResult<ArtistDto>> AddArtist([FromBody] CreateArtistDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Surname))
            {
                return BadRequest("Name and surname are required");
            }

            var Entity = new Artist
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Biography = dto.Biography?.Trim() ?? string.Empty
            };
            
            _db.Artists.Add(Entity);
            await _db.SaveChangesAsync();

            var result = new ArtistDto(Entity.Id, Entity.Name, Entity.Surname, Entity.Biography);
            return Ok(result);
        }
        
        // Get artists endpoint
        // GET api/v1/artist
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtistDto>>> List()
        {
            var list = await _db.Artists.AsNoTracking()
                .OrderBy(x => x.Surname).ThenBy(x => x.Name)
                .Select(a => new ArtistDto(a.Id, a.Name, a.Surname, a.Biography))
                .ToListAsync();
            return list;
        }
        
        // Get artist by Id endpoint
        // GET api/v1/artist/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ArtistDto>> GetById(Guid id, [FromQuery] bool expandArts = false)
        {
            if (!expandArts)
            {
                var artist = await _db.Artists.AsNoTracking()
                    .Where(a => a.Id == id)
                    .Select(a => new ArtistDto(a.Id, a.Name, a.Surname, a.Biography))
                    .FirstOrDefaultAsync();

                return artist is null ? NotFound() : Ok(artist);
            }

            var withArts = await _db.Artists.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new ArtistWithArtsDto(
                    a.Id,
                    a.Name,
                    a.Surname,
                    a.Biography,
                    a.Arts
                        .OrderBy(x => x.Title)
                        .Select(x => new ArtMiniDto(x.Id, x.Title, x.Description, x.Type))
                        .ToList()
                ))
                .FirstOrDefaultAsync();

            return withArts is null ? NotFound() : Ok(withArts);
        }
        
        // Get artist by Id endpoint
        // GET api/v1/artist/{id}/grouped
        [HttpGet("{id:guid}/grouped")]
        public async Task<ActionResult<object>> GetByIdGrouped([FromRoute] Guid id)
        {
            var data = await _db.Artists.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new {
                    a.Id,
                    a.Name,
                    a.Surname,
                    a.Biography,
                    Arts = a.Arts
                        .Select(x => new ArtMiniDto(x.Id, x.Title, x.Description, x.Type))
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data is null) return NotFound();

            var grouped = data.Arts
                .GroupBy(x => x.Type)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtMiniDto>)g.ToList());

            return Ok(new {
                data.Id,
                data.Name,
                data.Surname,
                data.Biography,
                ArtsByType = grouped
            });
        }

        
        // Patch artist by Id endpoint
        // PATCH api/v1/artist/{id}
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ArtistDto>> PatchArtist([FromRoute] Guid id, [FromBody] PatchArtistDto dto)
        {
            var artist = await _db.Artists.FirstOrDefaultAsync(a => a.Id == id);
            
            if (artist is null) return NotFound();
            
            if (dto.Name is not null) artist.Name = dto.Name;
            if (dto.Surname is not null) artist.Surname = dto.Surname;
            if (dto.Biography is not null) artist.Biography = dto.Biography;
            
            await _db.SaveChangesAsync();

            var result = new ArtistDto(artist.Id, artist.Name, artist.Surname, artist.Biography);
            
            return Ok(result);
        }
        
        // Delete artist by Id endpoint
        // DELETE api/v1/artist/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ArtistDto>> DeleteArtist([FromRoute] Guid id)
        {
            var artist = await _db.Artists.FindAsync(id);

            if (artist is null) return NotFound();

            var hasArts = await _db.Arts.AsNoTracking().AnyAsync(x => x.ArtistId == id);

            if (hasArts)
            {
                return Conflict("Cannot delete artist with arts.");
            }

            _db.Artists.Remove(artist);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
