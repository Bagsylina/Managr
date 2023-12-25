﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Managr.Models
{
    public class Project
    {
        // PK
        // The Id is automatically generated by the database
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        // FK; References ApplicationUser.UserId
        public string? OrganizerId { get; set; }

        // Project name/title
        [Required (ErrorMessage = "Project name is required")]
        public string? Name { get; set; }

        // Project description
        [Required (ErrorMessage = "Project description is required")]
        public string? Description { get; set; }

        // Creation date
        public DateTime? CreationDate { get; set; }

        // The organizer (The admin of the project)
        public virtual ApplicationUser? Organizer { get; set; }

        // The tasks of the project
        public virtual ICollection<Task>? Tasks { get; set; }

        // The ApplicationUsers that have view access to the project
        public virtual ICollection<ProjectUser>? ProjectUsers { get; set; }

        // Used for AddMember drop down
        [NotMapped]
        public IEnumerable<SelectListItem>? DropDownMembers { get; set; }

        // Used for RemoveMember drop down
        [NotMapped]
        public IEnumerable<SelectListItem>? DropDownNonMembers { get; set; }
    }
}
