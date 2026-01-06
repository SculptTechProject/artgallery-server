namespace artgallery_server.Models.Tag
{
    public class ArtTag
    {
        public int ArtId { get; set; }
        public virtual Art? Art { get; set; }
        public int TagId { get; set; }
        public virtual Tag? Tag { get; set; }
    }
}
