using artgallery_server.Enum;

namespace artgallery_server.DTO.Art
{
    public sealed record ArtMiniDto(
        int Id,
        string Title,
        string Description,
        ArtType Type
    );
}
