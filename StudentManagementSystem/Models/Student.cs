using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Class { get; set; }

        [Range(1, 60)]
        public int RollNumber { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Range(1, 17)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
