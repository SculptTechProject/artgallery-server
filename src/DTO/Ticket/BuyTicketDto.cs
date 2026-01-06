using artgallery_server.Enum;

namespace artgallery_server.DTO.Ticket
{
    public record BuyTicketDto(TicketType Type, int ExhibitionId, int UserId);
}
