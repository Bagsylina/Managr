using System.ComponentModel.DataAnnotations;

namespace Managr.Models
{
    public class Task
    {
        // PK
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required (ErrorMessage = "Description is required")]
        public string Description { get; set; }
        
        // Task status
        // Will be modified when we add status types in the data base
        [Required (ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [Required (ErrorMessage = "Starting date is required")]
        public DateTime StartDate  { get; set; }

        [Required (ErrorMessage = "Deadline is required")]
        public DateTime Deadline { get; set; }

        public string? Multimedia { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
