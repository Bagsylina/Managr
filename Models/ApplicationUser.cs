using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Managr.Models
{
    public class ApplicationUser : IdentityUser
    {
        // File name where we will store the profile picture
        // It is not a priority right now
        // A null value means a default profile picture
        // public string? ProfilePictureFileName { get; set; }

        // The projects owned by the user
        public virtual ICollection<Project>? Projects { get; set; }

        // The projects this user is part of
        // Currently not implemented
    }

}
