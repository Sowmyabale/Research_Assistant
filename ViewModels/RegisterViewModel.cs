using System.ComponentModel.DataAnnotations;

namespace Research_Assistant.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }  

        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }  
    }
}
