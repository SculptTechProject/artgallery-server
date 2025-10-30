using artgallery_server.DTO.Artist;
using artgallery_server.Enum;

namespace artgallery_server.DTO.Art
{
    public sealed record ArtDto(
        Guid Id,
        string Title,
        string Description,
        ArtistDto Artist,
        ArtType Type
    );
}
