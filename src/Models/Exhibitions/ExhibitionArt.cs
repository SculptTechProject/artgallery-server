namespace artgallery_server.Models.Exhibitions
{
    public class ExhibitionArt
    {
        public int ExhibitionId { get; set; }
        public virtual Exhibition? Exhibition { get; set; }

        public int ArtId { get; set; }
        public virtual Art? Art { get; set; }

        public string WallLocation { get; set; } = string.Empty; 
    }
}
