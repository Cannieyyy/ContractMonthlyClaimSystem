namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimEditViewModel
    {
        public int ClaimID { get; set; }
        public decimal HoursWorked { get; set; }
        public string WorkMonth { get; set; } = ""; // format: "YYYY-MM"
    }
}
