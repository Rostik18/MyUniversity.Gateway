
using System.ComponentModel.DataAnnotations;

namespace MyUniversity.Gateway.Models.UserManager.User
{
    public class LoginUserModel
    {
        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
