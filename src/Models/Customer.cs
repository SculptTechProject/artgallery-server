using artgallery_server.Models.Abstract;

namespace artgallery_server.Models
{
    public class Customer : User
    {
        public string ShippingAdress { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
