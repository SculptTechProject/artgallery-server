using artgallery_server.Enum;
using artgallery_server.Models.Exhibitions;
using artgallery_server.Models.Tag;

namespace artgallery_server.Models
{
    public class Art
    {
        public int Id { get; set; }
        public required string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ArtType Type { get; set; } = ArtType.Unknown;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public required Guid ArtistId { get; set; }
        public virtual Artist Artist { get; set; } = null!;
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public virtual ICollection<ExhibitionArt> ExhibitionArts { get; set; } = new List<ExhibitionArt>();
        public virtual ICollection<ArtTag> ArtTags { get; set; } = new List<ArtTag>();
    }
}
