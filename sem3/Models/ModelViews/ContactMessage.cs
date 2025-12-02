using System.ComponentModel.DataAnnotations;

namespace sem3.Models.ModelViews
{
    public class ContactMessage
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; }

        [Required(ErrorMessage = "CAPTCHA code is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "CAPTCHA code must be 5 characters")]
        public string CaptchaCode { get; set; }
    }
}