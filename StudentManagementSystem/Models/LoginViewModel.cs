namespace StudentManagementSystem.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CaptchaCode { get; set; } 
        public string GeneratedCaptcha { get; set; }
    }
}
