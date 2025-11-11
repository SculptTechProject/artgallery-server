using artgallery_server.DTO.Art;
using artgallery_server.DTO.Artist;
using artgallery_server.Enum;
using artgallery_server.Infrastructure;
using artgallery_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/arts")]
    public class ArtController : ControllerBase
    {
        // Connection to database
        private readonly AppDbContext _db;
        
        // Constructor
        public ArtController(AppDbContext db) => _db = db;
        
        // POST api/v1/arts
        [HttpPost]
        [ProducesResponseType(typeof(ArtDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArtDto>> AddArt([FromBody] CreateArtDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest("Title and description are required.");

            var artistDto = await _db.Artists.AsNoTracking()
                .Where(a => a.Id == dto.ArtistId)
                .Select(a => new ArtistDto(a.Id, a.Name, a.Surname, a.Biography))
                .FirstOrDefaultAsync();

            if (artistDto is null)
                return NotFound($"Artist {dto.ArtistId} not found.");

            var entity = new Art
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                ArtistId = dto.ArtistId,
                ImageUrl = dto.ImageUrl,
                Type = dto.Type,
                Artist = null!
            };

            _db.Arts.Add(entity);
            await _db.SaveChangesAsync();

            var result = new ArtDto(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.ImageUrl,
                artistDto,
                entity.Type
            );

            return CreatedAtAction(nameof(GetArtById), new { id = entity.Id }, result);
        }

        // Get art by Id endpoint
        // GET api/v1/art/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ArtDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArtDto>> GetArtById([FromRoute] Guid id)
        {
            var art = await _db.Arts.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new ArtDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.ImageUrl,
                    new ArtistDto(a.Artist.Id, a.Artist.Name, a.Artist.Surname, a.Artist.Biography),
                    a.Type))
                .FirstOrDefaultAsync();

            return art is null ? NotFound() : Ok(art);
        }
        
        // Get arts
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ArtDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArtDto>>> GetArts(
            [FromQuery] ArtType? type,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var q = _db.Arts.AsNoTracking();

            if (type.HasValue)
                q = q.Where(a => a.Type == type.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(a => a.Title.Contains(s) || a.Description.Contains(s));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArtDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.ImageUrl,
                    new ArtistDto(a.Artist.Id, a.Artist.Name, a.Artist.Surname, a.Artist.Biography),
                    a.Type))
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }
        
        [HttpGet("random")]
        [ProducesResponseType(typeof(IEnumerable<ArtDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArtDto>>> GetRandomArts(
            [FromQuery] int limit = 20,
            [FromQuery] ArtType? type = null)
        {
            limit = Math.Clamp(limit, 1, 100);

            var q = _db.Arts.AsNoTracking();
            if (type.HasValue)
                q = q.Where(a => a.Type == type.Value);

            var items = await q
                .OrderBy(a => EF.Functions.Random())
                .Take(limit)
                .Select(a => new ArtDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.ImageUrl,
                    new ArtistDto(a.Artist.Id, a.Artist.Name, a.Artist.Surname, a.Artist.Biography),
                    a.Type))
                .ToListAsync();

            return Ok(items);
        }
        
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(IEnumerable<ArtDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArtDto>>> GetByType([FromRoute] ArtType type)
        {
            var items = await _db.Arts.AsNoTracking()
                .Where(a => a.Type == type)
                .OrderBy(a => a.Title)
                .Select(a => new ArtDto(
                    a.Id,
                    a.Title,
                    a.Description,
                    a.ImageUrl,
                    new ArtistDto(a.Artist.Id, a.Artist.Name, a.Artist.Surname, a.Artist.Biography),
                    a.Type))
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
            [FromQuery] bool includeUnknown = false,
            [FromQuery] int minCount = 1,
            CancellationToken ct = default)
        {
            IQueryable<Art> q = _db.Arts.AsNoTracking();

            if (!includeUnknown)
                q = q.Where(a => a.Type != ArtType.Unknown);

            var raw = await q
                .GroupBy(a => a.Type)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .Where(x => x.Count >= minCount)
                .ToListAsync(ct);

            var data = raw
                .Select(x => new CategoryDto(
                    (int)x.Key,
                    x.Key.ToDisplayName(),
                    x.Key.ToSlug(),
                    x.Count
                ))
                .OrderBy(x => x.Name)
                .ToList();

            return Ok(data);
        }
    }
}
