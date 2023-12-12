using System.ComponentModel.DataAnnotations;

namespace Managr.Models
{
    public class Comment
    {
        // PK
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage = "Content is required")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        // FK; The task the comment was left on
        public int? TaskId { get; set; }

        // FK; The user that left the comment
        public string? UserId { get; set; }

        // The task the comment was left on
        public virtual Task? Task { get; set; }

        // The user that left the comment
        public virtual ApplicationUser? User { get; set; }
    }
}
