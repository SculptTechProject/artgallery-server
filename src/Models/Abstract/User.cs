namespace artgallery_server.Models.Abstract
{
    public abstract class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
