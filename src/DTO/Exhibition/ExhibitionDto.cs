using System;

namespace artgallery_server.DTO.Exhibition
{
    public sealed record ExhibitionDto(
        int Id,
        string Name,
        string Description,
        string? ImageUrl,
        DateTime StartDate,
        DateTime EndDate,
        int Capacity
    );
}
