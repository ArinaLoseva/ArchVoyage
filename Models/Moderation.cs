using System.ComponentModel.DataAnnotations;

namespace ArchVoyage.Models
{
    public class Moderation
    {
        [Key]
        public string UserEmail { get; set; }

        public bool Checking { get; set; }

        public bool Blocking { get; set; }

    }
}
