using artgallery_server.Enum;

namespace artgallery_server.Models
{
    public class Art
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ArtType Type { get; set; } = ArtType.Unknown;
        public string? ImageUrl { get; set; }
        public Guid ArtistId { get; set; }
        public required Artist Artist { get; set; } = null!;
    }
}
