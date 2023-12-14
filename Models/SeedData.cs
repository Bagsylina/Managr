using Managr.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Managr.Models
{
    // Class used to create base roles and users in the database
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // The database context; We use it to add data in the database
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()
            ))
            {
                // If the database already has roles then we don't add them again
                if(context.Roles.Any())
                {
                    return;
                }

                // Here we create the roles
                // Because the not registered user and the organizer are not "roles" they are not stored
                // Hence only registered user and admin roles are added
                context.Roles.AddRange(
                    new IdentityRole { Id = "01234567-89ab-cdef-0123-456789abcdef", Name = "User", NormalizedName = "User".ToUpper()},
                    new IdentityRole { Id = "fedcba98-7654-3210-fedc-ba9876543210", Name = "Admin", NormalizedName = "Admin".ToUpper()}
                );

                // Password hasher
                var hasher = new PasswordHasher<ApplicationUser>();

                // Here we create the users
                // The 2 basic users are an admin and a regular user
                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "00112233-4455-6677-8899-aabbccddeeff",
                        UserName = "admin@admin.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@ADMIN.COM",
                        Email = "admin@admin.com",
                        NormalizedUserName = "ADMIN@ADMIN.COM",
                        PasswordHash = hasher.HashPassword(null, "Aa1!")
                    },
                    new ApplicationUser
                    {
                        Id = "ffeeddcc-bbaa-9988-7766-554433221100",
                        UserName = "user@user.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@USER.COM",
                        Email = "user@user.com",
                        NormalizedUserName = "USER@USER.COM",
                        PasswordHash = hasher.HashPassword(null, "Uu1!")
                    }
                );

                // Assign roles to the users
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "01234567-89ab-cdef-0123-456789abcdef",
                        UserId = "ffeeddcc-bbaa-9988-7766-554433221100"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "fedcba98-7654-3210-fedc-ba9876543210",
                        UserId = "00112233-4455-6677-8899-aabbccddeeff"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
