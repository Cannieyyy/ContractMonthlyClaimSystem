// Models/Domain/Department.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // New: Hourly rate used to calculate payments
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
