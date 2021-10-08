using Amazon.Extensions.CognitoAuthentication;
using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Account
{
    public class ResetPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        public string Email { get; set; }
    }
}
