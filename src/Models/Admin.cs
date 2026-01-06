using artgallery_server.Models.Abstract;

namespace artgallery_server.Models
{
    public class Admin : User
    {
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public required string Role = "Admin";
    }
}
