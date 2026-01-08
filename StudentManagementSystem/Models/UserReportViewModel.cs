namespace EmployeeManagementSystem.Models
{
    public class UserReportViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int EntryMade { get; set; }
        public int ApprovedOrRejected { get; set; }
        public DateTime? LastEntry { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }

    public class SupervisorReportViewModel
    {
        public int OperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int TotalEntries { get; set; }
        public int Approvals { get; set; }
        public int Rejections { get; set; }
        public int Pending { get; set; }
    }

}
