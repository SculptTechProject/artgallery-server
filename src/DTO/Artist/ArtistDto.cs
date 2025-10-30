namespace artgallery_server.DTO.Artist
{
    public sealed record ArtistDto(
        Guid Id,
        string Name,
        string Surname,
        string Biography
    );
}
