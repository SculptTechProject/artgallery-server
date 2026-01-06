namespace artgallery_server.Models.Exhibitions
{
    public class Exhibition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public ICollection<ExhibitionArt> ExhibitionArts { get; set; } = new List<ExhibitionArt>();
    }
}
