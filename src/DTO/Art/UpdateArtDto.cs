using artgallery_server.Enum;

namespace artgallery_server.DTO.Art
{
    public sealed record UpdateArtDto(
        string Title,
        string Description,
        string? ImageUrl,
        Guid ArtistId,
        ArtType Type,
        decimal Price,
        int CategoryId
    );
}
