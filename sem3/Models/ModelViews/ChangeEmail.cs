using System.ComponentModel.DataAnnotations;

namespace sem3.Models.ModelViews
{
    public class ChangeEmail
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Confirm Email")]
        [Compare("NewEmail", ErrorMessage = "The emails do not match.")]
        public string ConfirmEmail { get; set; }
    }
}