using EmployeeManagementSystem.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _connectionString;

        private readonly ApiService _apiService;

        public LoginController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            var captcha = CaptchaService.GenerateCaptchaText();
            HttpContext.Session.SetString("Captcha", captcha);
            ViewBag.Captcha = captcha;
            return View(new LoginViewModel());
        }

        public IActionResult CaptchaImage()
        {
            var captcha = HttpContext.Session.GetString("Captcha");
            if (string.IsNullOrEmpty(captcha))
                captcha = CaptchaService.GenerateCaptchaText();

            var imageBytes = CaptchaService.GenerateCaptchaImage(captcha);
            return File(imageBytes, "image/png");
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            var storedCaptcha = HttpContext.Session.GetString("Captcha");
            if (model.CaptchaCode != storedCaptcha)
            {
                ModelState.AddModelError("CaptchaCode", "Invalid CAPTCHA.");
                var newCaptcha = CaptchaService.GenerateCaptchaText();
                HttpContext.Session.SetString("Captcha", newCaptcha);
                ViewBag.Captcha = newCaptcha;
                ModelState.SetModelValue("CaptchaCode", new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(string.Empty));
                return View(model);
            }

            //if (!ModelState.IsValid)
            //    return View(model);

            var (success, role, error) = await _apiService.LoginAsync(model.Username, model.Password);

            if (!success)
            {
                ModelState.AddModelError("", error ?? "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("UserRole", role!);

            return role switch
            {
                "Operator" => RedirectToAction("Index", "Employee"),
                "Supervisor" => RedirectToAction("Dashboard", "Supervisor"),
                _ => RedirectToAction("Index")
            };
        }

        public IActionResult RefreshCaptcha()
        {
            var captcha = CaptchaService.GenerateCaptchaText();
            HttpContext.Session.SetString("Captcha", captcha);
            return Json(new { captcha });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
