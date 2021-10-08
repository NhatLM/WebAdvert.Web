using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Account
{
    public class GenerateCodeToResetPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
