using AutoMapper;
using MyUniversity.Gateway.Models.UserManager.User;

namespace MyUniversity.Gateway.Api.MapperProfiles
{
    public class ControllerMapperProfile : Profile
    {
        public ControllerMapperProfile()
        {
            CreateMap<RegisterUserModel, RegistrationRequest>()
                .ForMember(x => x.Roles, x => x.MapFrom(xx => xx.Roles))
                .ForMember(x => x.UniversityId, x => x.MapFrom(xx => xx.TenantId));
        }
    }
}
