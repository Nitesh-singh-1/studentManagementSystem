using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;


namespace EmployeeManagementSystem.Controllers
{

    public class SupervisorController : Controller
    {
        private readonly string _connectionString;
        private readonly ApiService _apiService;

        public SupervisorController(ApiService apiService)
        {

            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? searchTerm)
        {
            var employees = await _apiService.GetAllEmployeesAsync();

            if (employees == null)
            {
                ViewBag.Error = "Unable to fetch employee data from the API.";
                return View(new List<EmployeeResponse>());
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();

                employees = employees
                    .Where(e =>
                        (e.employeeName?.ToLower().Contains(searchTerm) ?? false) ||
                        (e.department?.ToLower().Contains(searchTerm) ?? false) ||
                        (e.designation?.ToLower().Contains(searchTerm) ?? false)
                    )
                    .ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> ViewEmployeeDocumentAsync(int id)
        {
            var employee = await _apiService.getEmployeeById(id);
            var document = employee.employeeDocuments?.FirstOrDefault();

            if (document == null || string.IsNullOrWhiteSpace(document.FilePath))
                return NotFound("No document found for this employee.");

            // ✅ Use the document's actual FilePath
            var filePath = document.FilePath;

            // ✅ Build the physical path from wwwroot
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on server.");

            // ✅ Detect content type
            var extension = Path.GetExtension(fullPath).ToLower();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            // ✅ Make it display inline in browser (not download)
            Response.Headers.Add("Content-Disposition", $"inline; filename={Path.GetFileName(fullPath)}");

            return PhysicalFile(fullPath, contentType);
        }



    }
}
