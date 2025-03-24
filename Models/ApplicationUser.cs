using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }  // Nullable
    public string? Email { get; set; }     // Nullable
    public DateTime? LockoutEnd { get; set; } // Nullable
}