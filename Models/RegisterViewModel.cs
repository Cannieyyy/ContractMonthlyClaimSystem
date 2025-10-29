using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Name must be at most 200 characters.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Role { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).+$",
         ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "OTP Code")]
        [StringLength(20)]
        public string OtpCode { get; set; }

    }
}
