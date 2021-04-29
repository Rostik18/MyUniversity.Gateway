using System.Collections.Generic;
using AutoMapper;
using MyUniversity.Gateway.Models.User;
using MyUniversity.Gateway.Models.UserManager.Role;
using MyUniversity.Gateway.Models.UserManager.University;
using MyUniversity.Gateway.Models.UserManager.User;
using MyUniversity.Gateway.Role;
using MyUniversity.Gateway.University;
using MyUniversity.Gateway.User;

namespace MyUniversity.Gateway.Api.MapperProfiles
{
    public class ControllerMapperProfile : Profile
    {
        public ControllerMapperProfile()
        {
            #region User

            CreateMap<RegisterUserModel, RegistrationRequest>()
                .ForMember(x => x.Roles, x => x.MapFrom(xx => xx.Roles))
                .ForMember(x => x.UniversityId, x => x.MapFrom(xx => string.IsNullOrWhiteSpace(xx.TenantId) ? string.Empty : xx.TenantId));

            CreateMap<LoginUserModel, LoginRequest>();

            CreateMap<UpdateUserModel, UpdateUserRequest>()
                .ForMember(x => x.FirstName, x => x.MapFrom(xx => xx.FirstName ?? string.Empty))
                .ForMember(x => x.LastName, x => x.MapFrom(xx => xx.LastName ?? string.Empty))
                .ForMember(x => x.EmailAddress, x => x.MapFrom(xx => xx.EmailAddress ?? string.Empty))
                .ForMember(x => x.PhoneNumber, x => x.MapFrom(xx => xx.PhoneNumber ?? string.Empty))
                .ForMember(x => x.UniversityId, x => x.MapFrom(xx => xx.UniversityId ?? string.Empty))
                .ForMember(x => x.Password, x => x.MapFrom(xx => xx.Password ?? string.Empty))
                .ForMember(x => x.Roles, x => x.MapFrom(xx => xx.Roles ?? new List<int>()));

            CreateMap<RoleReply, RoleModel>();
            CreateMap<User.UniversityModelReply, UniversityModel>();
            CreateMap<UserModelReply, UserModel>()
                .ForMember(x => x.UserRoles, x => x.MapFrom(xx => xx.Roles))
                .ForMember(x => x.University, x => x.MapFrom(xx => xx.University));

            #endregion

            #region University

            CreateMap<CreateUniversityModel, CreateUniversityRequest>();
            CreateMap<UpdateUniversityModel, UpdateUniversityRequest>()
                .ForMember(x => x.Name, x => x.MapFrom(xx => xx.Name ?? string.Empty))
                .ForMember(x => x.Address, x => x.MapFrom(xx => xx.Address ?? string.Empty))
                .ForMember(x => x.EmailAddress, x => x.MapFrom(xx => xx.EmailAddress ?? string.Empty))
                .ForMember(x => x.PhoneNumber, x => x.MapFrom(xx => xx.PhoneNumber ?? string.Empty));

            CreateMap<University.UniversityModelReply, UniversityModel>();

            #endregion

            #region Role

            CreateMap<RoleModelReply, RoleModel>();

            #endregion
        }
    }
}
