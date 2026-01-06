namespace artgallery_server.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }
        
        public int ArtId { get; set; }
        public virtual Art? Art { get; set; }
        
        public decimal UnitPriceSnapshot { get; set; }
    }
}
