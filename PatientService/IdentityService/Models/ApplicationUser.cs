using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public class RegisterModel
        {
            public new string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}