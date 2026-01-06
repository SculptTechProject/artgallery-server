using artgallery_server.Enum;
using artgallery_server.Models.Abstract;
using artgallery_server.Models.Exhibitions;

namespace artgallery_server.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public TicketType Type { get; set; }
        
        public int ExhibitionId { get; set; }
        public virtual Exhibition Exhibition { get; set; } = null!;
        
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
