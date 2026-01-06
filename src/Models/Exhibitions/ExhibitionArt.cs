namespace artgallery_server.Models.Exhibitions
{
    public class ExhibitionArt
    {
        public int ExhibitionId { get; set; }
        public Exhibition? Exhibition { get; set; }

        public int ArtId { get; set; }
        public Art? Art { get; set; }

        public string WallLocation { get; set; } = string.Empty; 
    }
}
