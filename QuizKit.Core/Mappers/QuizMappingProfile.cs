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

        CreateMap<Quiz, QuizPreviewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateTime.MinValue))
            .ForMember(dest => dest.TimeLimit, opt => opt.MapFrom(src => src.TimeLimit))
            .ForMember(dest => dest.QuestionsCount, opt => opt.MapFrom(src => src.Questions.Count))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
