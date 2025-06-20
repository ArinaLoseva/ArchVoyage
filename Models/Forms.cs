using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArchVoyage.Models
{
    public class Forms
    {
        [Key]
        public string? UserEmail { get; set; }

        [Required]
        public string? FullName { get; set; }

        public string? RegistrationDate { get; set; }

        [Required]
        public string? City { get; set; }

        public string? Transport { get; set; }

        [Required]
        public string? Contacts { get; set; }

        public string? PersonalInformation { get; set; }

        public string? PhotoLink { get; set; }

    }
}
