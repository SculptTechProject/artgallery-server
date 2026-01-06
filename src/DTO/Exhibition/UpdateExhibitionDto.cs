using System;
using System.ComponentModel.DataAnnotations;

namespace artgallery_server.DTO.Exhibition
{
    public sealed record UpdateExhibitionDto(
        [Required] string Name,
        string Description,
        string? ImageUrl,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        int Capacity
    );
}
