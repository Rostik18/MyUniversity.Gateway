using System.ComponentModel.DataAnnotations;

namespace MyUniversity.Gateway.Models.UserManager.University
{
    public class CreateUniversityModel
    {
        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(maximumLength: 200, MinimumLength = 1)]
        public string Address { get; set; }

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 1)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
