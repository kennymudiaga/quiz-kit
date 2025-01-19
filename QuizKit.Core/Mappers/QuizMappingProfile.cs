using AutoMapper;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Mappers;

public class QuizMappingProfile : Profile
{
    public QuizMappingProfile()
    {
        CreateMap<QuizQuestion, QuestionModel>();

        CreateMap<Quiz, QuizModel>()
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
    }
}
