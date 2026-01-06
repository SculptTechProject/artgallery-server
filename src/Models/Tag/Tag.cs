namespace artgallery_server.Models.Tag
{
    public class Tag
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public virtual ICollection<ArtTag> ArtTags { get; set; } = new List<ArtTag>();
    }
}
