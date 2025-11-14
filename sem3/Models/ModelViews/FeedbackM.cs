using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace sem3.Models.ModelViews
{
    public class FeedbackM
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Please select a rating.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write your feedback.")]
        [StringLength(2000, ErrorMessage = "Feedback cannot exceed 2000 characters.")]
        public string FeedbackText { get; set; }
    }
}