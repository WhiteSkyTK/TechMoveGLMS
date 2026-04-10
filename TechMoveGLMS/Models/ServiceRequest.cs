using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // The original USD amount entered by the user
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalCostUSD { get; set; }

        // The converted ZAR amount stored in the DB
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConvertedCostZAR { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}