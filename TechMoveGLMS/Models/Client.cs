using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace TechMoveGLMS.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        public string Region { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Contract>? Contracts { get; set; }
    }
}