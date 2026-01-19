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
                ViewBag.FileUniqueId = result.fileUniqueId;
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
