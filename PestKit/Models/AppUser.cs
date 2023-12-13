using Microsoft.AspNetCore.Identity;

namespace PestKit.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public string? ImageUrl { get; set; } = "avatar.png";
        public string? Bio { get; set; }
        public DateTime? Birthday { get; set; }

        public List<BasketItem> BasketItems { get; set; }
        public List<Order> Orders { get; set; }
    }
}
