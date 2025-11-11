namespace artgallery_server.Models
{
    public class Admin
    {
        public Guid Id { get; set; }
        public required string Username { get; set; } = default!;
        public required string PasswordHash { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public required string Role = "Admin";
    }
}
