using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Research_Assistant.Models;

namespace Research_Assistant.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
            {
                
            }

        // Add DbSet if you have custom roles (Optional)
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public DbSet<ApplicationRole> Roles { get; set; }
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize Identity tables if needed
        }
         public DbSet<Paper> Papers { get; set; }
        // Define your database tables here
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
            {
                var user = entry.Entity;
            
                // Ensure email fields are not null
                if (string.IsNullOrEmpty(user.Email))
                {
                    user.Email = "default@example.com";
                }
                if (string.IsNullOrEmpty(user.NormalizedEmail))
                {
                    user.NormalizedEmail = user.Email?.ToUpper();
                }
            }   

            return base.SaveChanges();
        }

    }
}