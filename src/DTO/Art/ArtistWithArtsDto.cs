namespace artgallery_server.DTO.Art
{
    public sealed record ArtistWithArtsDto(
        Guid Id,
        string Name,
        string Surname,
        string Biography,
        IReadOnlyList<ArtMiniDto> Arts
    );
}
