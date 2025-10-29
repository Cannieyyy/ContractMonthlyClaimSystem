using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Verification
    {
        [Key]
        public int VerificationID { get; set; }

        [Required]
        [ForeignKey("Claim")]
        public int ClaimID { get; set; }
        public virtual Claim? Claim { get; set; }

        // Who verified (Employee who verified) - FK to Employee.EmployeeID
        [Required]
        [ForeignKey("VerifiedByEmployee")]
        public int VerifiedBy { get; set; }
        public virtual Employee VerifiedByEmployee { get; set; }

        [Required]
        public DateTime VerificationDate { get; set; } = DateTime.UtcNow;

        // Status of verification (Verified / Rejected / Flagged / etc.)
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Verified";

        // Any optional remarks
        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
