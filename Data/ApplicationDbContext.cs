using Managr.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Managr.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // Projects
        public DbSet<Project> Projects { get; set; } 

        // Tasks
        public DbSet<Models.Task> Tasks { get; set; }

        // Comments
        public DbSet<Comment> Comments { get; set; }
    }
}