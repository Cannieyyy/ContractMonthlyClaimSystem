namespace ContractMonthlyClaimSystem.Models
{
    public class CoordinatorDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int InProgress { get; set; }
        public int VerifiedCount { get; set; }
        public int RejectedCount { get; set; }

        // Optional: if you also pass claims to the view
        public List<Claim> Claims { get; set; }
    }

}
