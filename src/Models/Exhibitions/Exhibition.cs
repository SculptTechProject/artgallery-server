namespace artgallery_server.Models.Exhibitions
{
    public class Exhibition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; } = 100;
        
        public virtual ICollection<ExhibitionArt> ExhibitionArts { get; set; } = new List<ExhibitionArt>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
