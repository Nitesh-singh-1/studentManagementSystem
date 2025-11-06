using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EmployeeManagementSystem.Services
{
    public static class CaptchaService
    {
        public static string GenerateCaptchaText(int length = 6)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GenerateCaptchaImage(string captchaText)
        {
            using Bitmap bitmap = new Bitmap(150, 50);
            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            using Font font = new Font("Arial", 20, FontStyle.Bold);
            g.DrawString(captchaText, font, Brushes.Blue, new PointF(10, 10));

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
