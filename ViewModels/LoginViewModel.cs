//using Research_Assistant.ViewModels.LoginViewModel;


using System.ComponentModel.DataAnnotations;

namespace Research_Assistant.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
