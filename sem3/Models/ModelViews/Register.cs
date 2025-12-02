using System;
using System.ComponentModel.DataAnnotations;

namespace sem3.Models.ModelViews
{
    public class Register
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^(0|\+84)[1-9][0-9]{8}$", ErrorMessage = "Invalid Vietnamese phone number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public string OTP { get; set; }
    }
}