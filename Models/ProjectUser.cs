using System.ComponentModel.DataAnnotations;

namespace Managr.Models
{
    public class ProjectUser
    {
        // FK; References Project.Id
        [Required(ErrorMessage="Project to assign to is required")]
        public int? ProjectId{ get; set; }

        // FK; References ApplicationUser.UserId
        [Required(ErrorMessage="User to assign is required")]
        public string? UserId{ get; set; }

        // Project entity
        public virtual Project? Project{ get; set; }

        // ApplicationUser entity
        public virtual ApplicationUser? User{ get; set; }
    }
}
