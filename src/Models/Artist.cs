namespace artgallery_server.Models
{
    public class Artist
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Biography { get; set; } = null!;
        public virtual ICollection<Art> Arts { get; set; } = new List<Art>();
    }
}
