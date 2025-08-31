using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewProject.Models;

namespace NewProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(50);

            builder.Entity<ApplicationUser>()
                .Property(u => u.LastName)
                .HasMaxLength(50);
        }
    }
}

