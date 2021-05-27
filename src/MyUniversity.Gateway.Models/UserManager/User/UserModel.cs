using System.Collections.Generic;
using MyUniversity.Gateway.Models.UserManager.Role;
using MyUniversity.Gateway.Models.UserManager.University;

namespace MyUniversity.Gateway.Models.User
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public IEnumerable<RoleModel> UserRoles { get; set; }
        public UniversityModel University { get; set; }
    }
}
