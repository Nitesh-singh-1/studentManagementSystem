using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;
using System.Data.OleDb;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly string _connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Database\StudentDB.accdb;Persist Security Info=False;";

        public IActionResult Index()
        {
            ViewBag.Classes = GetClasses();
            return View();
        }

        [HttpPost]
        public IActionResult Index(Student student)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Classes = GetClasses();

            if (!ModelState.IsValid)
            {
                return View(student);
            }

            if (IsDuplicate(student.Class, student.RollNumber))
            {
                ModelState.AddModelError("", "A student with the same Class and Roll Number already exists.");
                return View(student);
            }

            SaveStudent(student);
            ModelState.Clear();
            ViewBag.Success = "Student registered successfully!";
            return View(new Student());
        }

        private List<string> GetClasses()
        {
            return new List<string> { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        }

        private bool IsDuplicate(string cls, int roll)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            var cmd = new OleDbCommand("SELECT COUNT(*) FROM Students WHERE UCASE(Class) = UCASE(?) AND RollNumber = ?", conn);
            cmd.Parameters.AddWithValue("?", cls.ToUpper());
            cmd.Parameters.AddWithValue("?", roll);

            int count = (int)cmd.ExecuteScalar();
            Console.WriteLine($"Duplicate Check → Class: {cls}, Roll: {roll}, Count: {count}");
            return count > 0;
        }

        private void SaveStudent(Student student)
        {
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();
            var cmd = new OleDbCommand(
                "INSERT INTO Students ([Class], RollNumber, StudentName, Age, Gender, Address) VALUES (?, ?, ?, ?, ?, ?)", conn);

            cmd.Parameters.AddWithValue("?", student.Class);
            cmd.Parameters.AddWithValue("?", student.RollNumber);
            cmd.Parameters.AddWithValue("?", student.StudentName);
            cmd.Parameters.AddWithValue("?", student.Age);
            cmd.Parameters.AddWithValue("?", student.Gender);
            cmd.Parameters.AddWithValue("?", student.Address);

            cmd.ExecuteNonQuery();
        }
    }
}
