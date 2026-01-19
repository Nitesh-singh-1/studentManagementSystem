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

        
        public string Designation { get; set; }

        
        public int Age { get; set; }

        
        public string Gender { get; set; }

       
        public string Address { get; set; }

        // Navigation property
        public List<EmployeeDocument>? Documents { get; set; }
    }
}
