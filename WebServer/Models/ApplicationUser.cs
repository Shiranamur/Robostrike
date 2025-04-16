using Microsoft.AspNetCore.Identity;

namespace BlazorApp1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Salt { get; set; }
        public int Points { get; set; }
    }
}
