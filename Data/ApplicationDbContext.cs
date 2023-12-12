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

        // Aside from the normal logic we must process:
        // Composite primary key for the Project entity
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Function call to the base class (The one we inherit from)
            base.OnModelCreating(modelBuilder);

            // Composite primary key for the Project entity
            modelBuilder.Entity<Project>().HasKey(proj => new { proj.Id, proj.OrganizerId });

            // It is likely we will have to add the foreign key from Task to Project
        }
    }
}