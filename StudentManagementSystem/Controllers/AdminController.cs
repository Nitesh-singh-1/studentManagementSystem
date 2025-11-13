using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }
        public IActionResult AdminDashboard()
        {
            // check login
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            var role = HttpContext.Session.GetString("UserRole");

            if (isLoggedIn != "true" || role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public async Task<IActionResult> Supervisors(string role = "All")
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Index", "Login");

            var adminId = HttpContext.Session.GetInt32("UserId");

            // Fetch filtered users based on dropdown role
            var users = await _apiService.GetUsersAsync(adminId ?? 0, role);
            ViewBag.Supervisors = users.Where(x => x.Role == "Supervisor").ToList();
            ViewBag.SelectedRole = role; // for dropdown default
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserViewModel model)
        {
            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null)
                return Json(new { success = false, message = "Session expired." });

            // If Operator is being created by Supervisor, store that supervisor ID in CreatedBy
            model.CreatedBy = adminId.Value;
            model.CreatedOn = DateTime.Now;

            var result = await _apiService.CreateUserAsync(model);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromForm] UserViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Session expired." });

            model.CreatedBy = userId.Value;
            var result = await _apiService.UpdateUserAsync(model);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _apiService.DeleteUserAsync(id);
            return Json(result);
        }



    }
}
