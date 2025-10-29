using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class Employee
    {

        
        public int EmployeeID { get; set; }

        [Required]
        [StringLength(200)]               // adjust max length if you want
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        public int DepartmentID { get; set; }
        public Department Department { get; set; }

        [Required]
        [StringLength(100)]
        public string Role { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Claim>? Claims { get; set; }
        public virtual ICollection<Verification>? Verifications { get; set; }
        public virtual UserAccount? UserAccount { get; set; }
    }
}
