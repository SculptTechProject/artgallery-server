using System;
using System.ComponentModel.DataAnnotations;

namespace artgallery_server.DTO.Exhibition
{
    public sealed record CreateExhibitionDto(
        [Required] string Name,
        string Description,
        string? ImageUrl,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        int Capacity
    );
}
