// Models/Domain/Claim.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }

        public int EmployeeID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }

        public DateTime ClaimDate { get; set; }

        public string? Status { get; set; }

        public DateTime DateCreated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Navigation property
        public virtual Employee Employee { get; set; }
        public ICollection<SupportingDocument>? SupportingDocuments { get; set; }
        
        
        public virtual ICollection<Verification>? Verifications { get; set; }
        public virtual ICollection<Approval>? Approvals { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
