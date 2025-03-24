using System.ComponentModel.DataAnnotations;

namespace Research_Assistant.ViewModels
{
    public class UpdateProfileViewModel 
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
