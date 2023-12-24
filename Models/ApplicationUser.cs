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

        //Comments posted by a User
        public virtual ICollection<Comment>? Comments { get; set; }

        //Tasks assigned to the User
        public virtual ICollection<UserTask>? UserTasks { get; set; }

        // The Projects that the ApplicationUser has view access to
        public virtual ICollection<ProjectUser>? ProjectUsers { get; set; }
    }

}
