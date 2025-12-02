using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace sem3.Models.Helpers
{
    public static class CaptchaHelper
    {
        public static string GenerateCaptchaCode()
        {
            var random = new Random();
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var captchaCode = new char[5];

            for (int i = 0; i < 5; i++)
            {
                captchaCode[i] = chars[random.Next(chars.Length)];
            }

            return new string(captchaCode);
        }

        public static byte[] GenerateCaptchaImage(string captchaCode)
        {
            using (var bitmap = new Bitmap(150, 50))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var ms = new MemoryStream())
            {
                graphics.Clear(Color.White);

                var random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    var pen = new Pen(Color.LightGray, 1);
                    graphics.DrawLine(pen,
                        random.Next(0, 150), random.Next(0, 50),
                        random.Next(0, 150), random.Next(0, 50));
                }

                var font = new Font("Arial", 24, FontStyle.Bold);
                var brush = new SolidBrush(Color.DarkBlue);
                graphics.DrawString(captchaCode, font, brush, 10, 10);

                for (int i = 0; i < 100; i++)
                {
                    var x = random.Next(0, 150);
                    var y = random.Next(0, 50);
                    bitmap.SetPixel(x, y, Color.LightGray);
                }

                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }

}