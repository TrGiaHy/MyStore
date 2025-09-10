using System.ComponentModel.DataAnnotations;

namespace Repository.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string repassword { get; set; }

        [Required(ErrorMessage = "Please confirm your FullName.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please confirm your Address.")]
        public string Address { get; set; }
    }
}
