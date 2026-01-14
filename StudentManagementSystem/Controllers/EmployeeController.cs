using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace EmployeeManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string _connectionString;

        private readonly ApiService _apiService;

        public EmployeeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" ||
                HttpContext.Session.GetString("UserRole") != "Operator")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Departments = GetDepartments();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(EmployeeResponse model, List<IFormFile>? documents)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" ||
                HttpContext.Session.GetString("UserRole") != "Operator")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Departments = GetDepartments();

            //if (!ModelState.IsValid)
            //    return View(model);
            var createdBy = HttpContext.Session.GetInt32("UserId");
            var result = await _apiService.AddEmployee(Convert.ToInt32(createdBy),model, documents);

            if (result != null)
            {
                ViewBag.Success = "Registered successfully!";
                ModelState.Clear();
                return View(new EmployeeResponse());
            }

            ViewBag.Error = "Failed to register.";
            return View(model);
        }

        private List<string> GetDepartments()
        {
            return new List<string> { "HR", "IT", "Finance", "Operations", "Sales", "Marketing" };
        }

        private bool IsDuplicate(string name, string dept)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT COUNT(*) FROM Employees WHERE EmployeeName = @name AND Department = @dept", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@dept", dept);
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        private int SaveEmployee(Employee employee)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "INSERT INTO Employees (EmployeeName, Department, Designation, Age, Gender, Address) " +
                "OUTPUT INSERTED.Id VALUES (@name, @dept, @desig, @age, @gender, @address)", conn);

            cmd.Parameters.AddWithValue("@name", employee.EmployeeName);
            cmd.Parameters.AddWithValue("@dept", employee.Department);
            cmd.Parameters.AddWithValue("@desig", employee.Designation);
            cmd.Parameters.AddWithValue("@age", employee.Age);
            cmd.Parameters.AddWithValue("@gender", employee.Gender);
            cmd.Parameters.AddWithValue("@address", employee.Address);

            int newId = (int)cmd.ExecuteScalar();
            return newId;
        }

        private void SaveEmployeeDocument(int employeeId, string? fileName, string? filePath)
        {
            // Determine the full file path on disk (convert from relative to absolute)
            string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            byte[]? imageBytes = null;
            if (System.IO.File.Exists(physicalPath))
            {
                imageBytes = System.IO.File.ReadAllBytes(physicalPath);
            }

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(
                "INSERT INTO EmployeeDocuments (EmployeeId, FileName, FilePath, ImageData) VALUES (@empId, @name, @path, @img)", conn);

            cmd.Parameters.AddWithValue("@empId", employeeId);
            cmd.Parameters.AddWithValue("@name", fileName ?? "");
            cmd.Parameters.AddWithValue("@path", filePath ?? "");
            cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = (object?)imageBytes ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }

        [HttpGet]
        public IActionResult ViewDocument(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT ImageData, FileName FROM EmployeeDocuments WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var imageData = reader["ImageData"] as byte[];
                var fileName = reader["FileName"].ToString();

                if (imageData == null)
                    return NotFound("No image data found.");

                // Detect content type (default to jpeg)
                string contentType = "image/jpeg";
                if (fileName != null && fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";

                return File(imageData, contentType);
            }

            return NotFound();
        }

        private string SaveFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }

        // View Employee Details
        public async Task<IActionResult> View(int id)
        {
            var employees = await _apiService.getEmployeeById(id);

            if (employees == null)
            {
                ViewBag.Error = "Unable to fetch employee data from the API.";
                return View(new List<EmployeeResponse>());
            }

            return View(employees);
        }

       
        public async Task<IActionResult> Edit(int id)
        {
            var employees = await _apiService.getEmployeeById(id);
            if (employees == null)
            {
                ViewBag.Error = "Unable to fetch employee data from the API.";
                return View(new List<EmployeeResponse>());
            }
            Console.WriteLine(JsonSerializer.Serialize(employees));
            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeResponse model, List<IFormFile>? documents)
        {
            if (documents != null && documents.Any())
            {
                model.employeeDocuments = new List<EmployeeDocument>();

                foreach (var file in documents)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    var bytes = ms.ToArray();

                    model.employeeDocuments.Add(new EmployeeDocument
                    {
                        EmployeeId = model.Id,
                        FileName = file.FileName,
                        ImageData = bytes
                    });
                }
            }
            var createdBy = HttpContext.Session.GetInt32("UserId");
            var result = await _apiService.UpdateEmployee(model,documents);

            if (result)
                return RedirectToAction("Index", "Report");

            ViewBag.Error = "Failed to update employee.";
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var result = await _apiService.DeleteDocument(id);

            if (result != null)
            {
                TempData["Message"] = "Document deleted successfully.";

                
                return RedirectToAction("Edit", new { id = result.employeeId });
            }
            else
            {
                TempData["Error"] = "Failed to delete document.";
                
                return RedirectToAction("Edit");
            }
        }

        

    }
}
