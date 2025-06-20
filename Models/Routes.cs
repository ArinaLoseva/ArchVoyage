using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArchVoyage.Models
{
    public class Routes
    {
        [Key]
        public int? RouteID { get; set; }

        public string? UserEmail { get; set; }

        public string? RouteName { get; set; }

    }
}
