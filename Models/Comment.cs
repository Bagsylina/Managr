using System.ComponentModel.DataAnnotations;

namespace Managr.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage = "Content is required")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? TaskId { get; set; }

        public virtual Task? Task { get; set; }
    }
}
