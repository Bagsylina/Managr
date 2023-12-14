﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Managr.Models
{
    public class Project
    {
        // Composite PK: Id and OrganizerId
        // The Id is automatically generated by the database
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Composite PK: Id and OrganizerId
        // FK; References ApplicationUser.UserId
        public string OrganizerId { get; set; }

        // Project name/title
        [Required (ErrorMessage = "Project name is required")]
        public string? Name { get; set; }

        // Project description
        [Required (ErrorMessage = "Project description is required")]
        public string? Description { get; set; }

        // Creation date
        [Required]
        public DateTime? CreationDate { get; set; }

        // The organizer (The admin of the project)
        public virtual ApplicationUser? Organizer { get; set; }
    }
}
