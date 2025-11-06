using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class EmployeeDocument
    {
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.Now;

        public byte[]? ImageData { get; set; }
    }
}
