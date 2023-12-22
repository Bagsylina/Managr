using System.ComponentModel.DataAnnotations.Schema;

namespace Managr.Models
{
    public class UserTask
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //User that's assigned to the Task
        public string? UserId { get; set; }

        //Task that's assigned to the User
        public int? TaskId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Models.Task? Task { get; set; }
    }
}
