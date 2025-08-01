using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data.OleDb;

namespace StudentManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly string _connectionString;

        public ReportController(IWebHostEnvironment env)
        {
            var dbPath = Path.Combine(env.ContentRootPath, "Database", "StudentDB.accdb");
            _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
        }

        public IActionResult Index(string selectedClass)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Classes = GetClasses();

            if (string.IsNullOrEmpty(selectedClass))
            {
                ViewBag.SelectedClass = null;
                return View(new List<Student>());
            }

            var students = GetAllStudents()
                .Where(s => s.Class.Equals(selectedClass, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.RollNumber)
                .ToList();

            ViewBag.SelectedClass = selectedClass;
            return View(students);
        }

        private List<string> GetClasses()
        {
            return new List<string> { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        }

        private List<Student> GetAllStudents()
        {
            var students = new List<Student>();

            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            var cmd = new OleDbCommand("SELECT * FROM Students", conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Class = reader["Class"].ToString(),
                    RollNumber = Convert.ToInt32(reader["RollNumber"]),
                    StudentName = reader["StudentName"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),
                    Gender = reader["Gender"].ToString(),
                    Address = reader["Address"].ToString()
                });
            }

            return students;
        }
        public IActionResult Print(string selectedClass)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrEmpty(selectedClass))
                return RedirectToAction("Index");

            var students = GetAllStudents()
                .Where(s => s.Class.Equals(selectedClass, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.RollNumber)
                .ToList();

            ViewBag.Class = selectedClass;
            ViewBag.Date = DateTime.Now.ToString("dd-MM-yyyy");

            return View("Print", students); // View: Views/Report/Print.cshtml
        }

    }
}
