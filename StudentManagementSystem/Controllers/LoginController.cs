using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using Microsoft.AspNetCore.Http;

namespace StudentManagementSystem.Controllers
{
    public class LoginController : Controller
    {
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
        public IActionResult Index(LoginViewModel model)
        {
            var storedCaptcha = HttpContext.Session.GetString("Captcha");
            if (model.CaptchaCode != storedCaptcha)
            {
                ModelState.AddModelError("CaptchaCode", "Invalid CAPTCHA.");
                ViewBag.Captcha = CaptchaService.GenerateCaptchaText();
                
                HttpContext.Session.SetString("Captcha", (string)ViewBag.Captcha);
                return View(model);
            }

            if (model.Username == "admin" && model.Password == "admin123")
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                return RedirectToAction("Index", "Student"); // next step
            }

            ModelState.AddModelError("", "Invalid credentials");
            ViewBag.Captcha = CaptchaService.GenerateCaptchaText();
            HttpContext.Session.SetString("Captcha", (string)ViewBag.Captcha);
            return View(model);
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
