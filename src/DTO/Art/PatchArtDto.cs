using artgallery_server.Enum;

namespace artgallery_server.DTO.Art
{
    public sealed record PatchArtDto(
        string? Title,
        string? Description,
        ArtType? Type
        );

}
