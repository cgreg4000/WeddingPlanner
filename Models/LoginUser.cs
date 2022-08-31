using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class LoginUser
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string LogEmail { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        public string LogPassword { get; set; }
    }
}