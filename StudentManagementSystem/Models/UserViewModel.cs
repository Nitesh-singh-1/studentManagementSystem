namespace EmployeeManagementSystem.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public int? ParentUserId { get; set; }
        public string? ReportingPerson { get; set; }
    }
}
