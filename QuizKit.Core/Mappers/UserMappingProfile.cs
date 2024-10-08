using AutoMapper;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Mappers;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<SignUpCommand, SignUpCommand>();
        CreateMap<InviteUserCommand, InviteUserCommand>();
        CreateMap<LoginCommand, LoginCommand>();
    }
}
