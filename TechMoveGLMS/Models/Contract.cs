using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace TechMoveGLMS.Models
{
    public enum ContractStatus { Draft, Active, OnHold, Expired }
    public enum ServiceLevel { Standard, Express, Hazardous }

    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required]
        public ServiceLevel Level { get; set; }

        // Stores the path to the PDF on the server disk
        public string? SignedAgreementFilePath { get; set; }

        // Navigation Property
        public ICollection<ServiceRequest>? ServiceRequests { get; set; }
    }
}