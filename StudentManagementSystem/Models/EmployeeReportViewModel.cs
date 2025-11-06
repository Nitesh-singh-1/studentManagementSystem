namespace EmployeeManagementSystem.Models
{
    public class EmployeeReportViewModel
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        public List<EmployeeDocumentViewModel> Documents { get; set; } = new List<EmployeeDocumentViewModel>();
    }
    public class EmployeeDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string? FileName { get; set; }
        public byte[]? ImageData { get; set; }
    }
}
