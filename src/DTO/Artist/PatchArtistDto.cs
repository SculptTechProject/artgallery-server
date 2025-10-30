namespace artgallery_server.DTO.Artist
{
    public sealed record PatchArtistDto(
        string? Name,
        string? Surname,
        string? Biography
    );
}
