using Microsoft.AspNetCore.Identity;

namespace Model
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
