using Managr.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Managr.Models
{
    // Clasa folosita pentru crearea rolurilor si a utilizatorilor de test
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // Ne folosim de variabila context pentru a ne conecta cu baza de date
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()
            ))
            {
                // Daca deja exista roluri in baza de date atunci nu mai
                // cream altele
                if(context.Roles.Any())
                {
                    return;
                }

                // Creez rolurile in baza de date
                // Deoarece rolul de utilizator neinregistrat si organizator nu trebuie stocate vom avea
                // doar roluri de utilizator inregistrat si administrator
                context.Roles.AddRange(
                    new IdentityRole { Id = "01234567-89ab-cdef-0123-456789abcdef", Name = "User", NormalizedName = "User".ToUpper()},
                    new IdentityRole { Id = "fedcba98-7654-3210-fedc-ba9876543210", Name = "Admin", NormalizedName = "Admin".ToUpper()}
                );

                // Hasher pentru parole
                var hasher = new PasswordHasher<ApplicationUser>();

                // Creez utilizatori in baza de date
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
