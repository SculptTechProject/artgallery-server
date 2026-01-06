using artgallery_server.Models.Abstract;

namespace artgallery_server.Models
{
    public class Admin : User
    {
        public required string Role = "Admin";
    }
}
