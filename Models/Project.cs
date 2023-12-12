using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Managr.Models
{
    public class Project
    {
        // PK compus dintr-un Id si Id-ul organizatorului
        // Valoarea este generata automat de baza de date
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // PK compus dintr-un Id si Id-ul organizatorului
        // FK; Trimite spre ApplicationUser.UserId
        public string OrganizerId { get; set; }

        // Denumirea proiectului
        [Required (ErrorMessage = "Denumirea proiectului este obligatorie")]
        public string? Name { get; set; }

        // Descrierea proiectului
        [Required (ErrorMessage = "Descrierea proiectului este obligatorie")]
        public string? Description { get; set; }

        // Organizatorul proiectului
        public virtual ApplicationUser? Organizer { get; set; }
    }
}
