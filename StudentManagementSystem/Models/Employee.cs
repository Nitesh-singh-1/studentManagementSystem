using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Designation { get; set; }

        [Range(18, 65)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }

        // Navigation property
        public List<EmployeeDocument>? Documents { get; set; }
    }
}
