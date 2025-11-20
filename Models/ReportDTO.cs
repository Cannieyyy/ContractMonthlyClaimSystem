namespace ContractMonthlyClaimSystem.Models
{
    public class ReportDTO
    {
        public string Department { get; set; } = string.Empty;
        public string Lecturer { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
