namespace artgallery_server.DTO.Artist
{
    public sealed record CreateArtistDto(
        string Name,
        string Surname,
        string Biography
    );
}
