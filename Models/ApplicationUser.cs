using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Managr.Models
{
    public class ApplicationUser : IdentityUser
    {
        // UserName
        // Obligatoriu
        [Required (ErrorMessage = "Numele de utilizator este obligatoriu")]
        public string? Name { get; set; }
        
        // Numele fisierului unde va fi incarcata poza de profil.
        // Nu este o prioritate sa adaugam poze de profil dar o vom face curand
        // Lipsa valorii reprezinta faptul ca poza de profil este una implicita
        // public string? ProfilePictureFileName { get; set; }

        // Proiectele detinute de acest utilizator
        public virtual ICollection<Project>? Projects { get; set; }

        // Proiectele din care acest utilizator face parte
        // Momentan nu este implementat, urmeaza
    }

}
