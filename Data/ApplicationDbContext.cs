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

        // User assigned to a task
        public DbSet<UserTask> UserTasks { get; set; }

        // Comments
        public DbSet<Comment> Comments { get; set; }

        // Project - Users
        public DbSet<ProjectUser> ProjectUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Many-to-Many for UserTask
            modelBuilder.Entity<UserTask>()
                .HasKey(ut => new { ut.Id, ut.UserId, ut.TaskId });

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(ut => ut.UserTasks)
                .HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.Task)
                .WithMany(ut => ut.UserTasks)
                .HasForeignKey(ut => ut.TaskId);

            //Many-to-Many for ProjectUser
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(pu => pu.ProjectUsers)
                .HasForeignKey(pu => pu.UserId);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany(pu => pu.ProjectUsers)
                .HasForeignKey(pu => pu.ProjectId);
        }
    }
}