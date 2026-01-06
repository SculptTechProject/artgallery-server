using System.ComponentModel.DataAnnotations;

namespace artgallery_server.DTO.Ticket
{
    public record BuyTicketDto(
        [Required] int ExhibitionId,
        [Required][EmailAddress] string Email,
        [Required] string PaymentMethod,
        [Required] int Type // 0 = Normal, 1 = Reduced
    );
}
