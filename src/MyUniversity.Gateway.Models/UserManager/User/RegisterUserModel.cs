using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyUniversity.Gateway.Models.UserManager.User
{
    public class RegisterUserModel
    {
        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(maximumLength: 36, MinimumLength = 36, ErrorMessage = "User can not exist without university")]
        public string TenantId { get; set; }

        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "User should have at list one role")]
        public IEnumerable<string> Roles { get; set; }
    }
}
