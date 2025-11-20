using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class HRDashboardViewModel
    {
        public List<Department> Departments { get; set; }
        public List<Employee> Employees { get; set; }
        [NotMapped]
        public bool IsActive { get; set; }



    }
}
