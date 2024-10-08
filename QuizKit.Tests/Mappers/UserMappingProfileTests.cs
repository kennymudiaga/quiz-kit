using AutoMapper;
using QuizKit.Core.Mappers;

namespace QuizKit.Tests.Mappers;

public class UserMappingProfileTests
{
    private readonly IMapper mapper = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<UserMappingProfile>();
    }).CreateMapper();
}

