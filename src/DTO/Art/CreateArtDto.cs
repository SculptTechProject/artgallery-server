using artgallery_server.Enum;

namespace artgallery_server.DTO.Art
{
    public sealed record CreateArtDto(
        string Title,
        string Description,
        string? ImageUrl,
        Guid ArtistId,
        ArtType Type
    );
}
