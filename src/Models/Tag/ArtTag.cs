namespace artgallery_server.Models.Tag
{
    public class ArtTag
    {
        public int ArtId { get; set; }
        public Art? Art { get; set; }
        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
