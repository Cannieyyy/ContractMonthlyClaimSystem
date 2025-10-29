// Models/Domain/SupportingDocument.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int DocumentID { get; set; }

        // FK to Claim
        [Required]
        public int ClaimID { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = null!;

        public DateTime UploadDate { get; set; }

        // Navigation
        [ForeignKey("ClaimID")]
        public Claim? Claim { get; set; }
    }
}
