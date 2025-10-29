using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Approval
    {
        [Key]
        public int ApprovalID { get; set; }

        [Required]
        [ForeignKey("Claim")]
        public int ClaimID { get; set; }
        public virtual Claim? Claim { get; set; }

        // Who approved (Employee who approved) - FK to Employee.EmployeeID
        [Required]
        [ForeignKey("VerifiedByEmployee")]
        public int ApprovedBy { get; set; }
        public virtual Employee VerifiedByEmployee { get; set; }

        [Required]
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;

        // Status of approval (Approved / Rejected / Flagged / etc.)
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Approved";

        // Any optional remarks
        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
