namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimForVerificationDto
    {
        public int ClaimID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } 
        public string ClaimDate { get; set; }
        public string WorkMonth { get; set; } 
        public decimal HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int? DocumentID { get; set; }
        public string? DocumentName { get; set; }
    }

}
