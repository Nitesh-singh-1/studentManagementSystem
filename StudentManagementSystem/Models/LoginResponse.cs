namespace EmployeeManagementSystem.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
    }
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string employeeName { get; set; }
        public string department { get; set; }
        public string designation { get; set; }
        public int age { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public bool? isApproved { get; set; }
        public string? Remarks { get; set; }
        public int? createdBy { get; set; }
        public int? modifiedBy { get; set; }
        public int documentId { get; set; }
        public string fileName { get; set; }
        public string filePath { get; set; }
        public string createdon { get; set; }
        public string enteryMadeBy { get; set; }

        public List<EmployeeDocument> employeeDocuments { get; set; } = new();
    }

    public class DeleteResponse
    {
        public string Message { get; set; }
        public int DocumentId { get; set; }

        public int employeeId { get; set; }
    }

    //del response of empList
    public class delResponse
    {
        public string Message { get; set; }

        public int employeeId { get; set; }
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class EmployeeResponseforAdmin
    {
        public int Id { get; set; }
        public string employeeName { get; set; }
        public string department { get; set; }
        public string designation { get; set; }
        public int age { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public bool? isApproved { get; set; }
        public string? Remarks { get; set; }
        public int documentId { get; set; }
        public string fileName { get; set; }
        public string filePath { get; set; }
        public string createdon { get; set; }
        public string enteryMadeBy { get; set; }

    }
}
