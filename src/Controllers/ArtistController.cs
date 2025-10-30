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
    }
}
