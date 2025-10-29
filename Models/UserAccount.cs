using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class UserAccount
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
